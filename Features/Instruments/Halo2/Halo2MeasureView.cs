using System.Globalization;
using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Halo 2 live measurement view: simulated readings, charts, and mode toggles.
/// Binds to <see cref="Halo2MeasureViewModel"/>; live timer and chart drawing remain in the view layer.
///
/// ──────────────── Layout map (top → bottom) ────────────────
///   BuildLabHeader        : Device card (icon · name · disconnect on row 1;
///                          Battery + Probe Condition clusters on row 2)
///   BuildLiveReadingsCard : Big pH / mV / temperature panel with stability pill + settings cog
///   BuildModeBar          : Data / Trends / Calibrate segmented control
///   BuildTable / BuildGraph / BuildCalibrationSummary : swapped based on _mode and _showCalibration
///
/// ──────────────── State you'll usually want when debugging ──
///   _mode                : Table | Graph (segmented control)
///   _showCalibration     : true when "Calibrate" segment is selected
///   _tagged              : header probe row toggled — also changes the demo probe % to 50
///   _showMvPrimary       : mirrors Halo2Preferences ("ph" vs "mv" as the big number)
///   _useFahrenheit       : mirrors Halo2Preferences
///   _isStable            : drives the green/amber stability pill
///   _history             : last N readings (used by table + chart drawable)
///   _sparkPh/_sparkMv/_sparkTemp : rolling buffers for the small sparkline graphs
///
/// ──────────────── Lifecycle ────────────────────────────────
///   - Singleton (created lazily by <see cref="Halo2MeasureModule"/>).
///   - <see cref="OnLoaded"/> starts the ~1Hz simulation timer; <see cref="OnUnloaded"/> stops it.
///   - <see cref="OnHandlerChanged"/> also stops the timer when the native handler detaches,
///     so hidden tab pages don't burn CPU.
///   - <see cref="Rebuild"/> is the single point that re-creates the visual tree; call after
///     any state change that affects which sub-view is shown.
/// </summary>
public sealed class Halo2MeasureView : ContentView
{
	readonly Halo2MeasureViewModel _viewModel;
	readonly Core.Localization.LocalizationService _loc;

	/// <summary>Local enum mirroring <see cref="Halo2MeasureViewModel.DisplayMode"/> — view-private so VM stays UI-toolkit-agnostic.</summary>
	enum HaloMode { Table, Graph }

	readonly VerticalStackLayout _root = new() { Spacing = 0 };
	readonly HaloChartDrawable _chart;

	/// <summary>Reverse-chronological history of demo samples (newest first). Capped at <see cref="MaxHistory"/>.</summary>
	readonly List<HaloReading> _history = [];

	// Sparkline ring buffers (kept at fixed size = SparkPointCount; older values fall off the front).
	readonly List<double> _sparkPh = [];
	readonly List<double> _sparkMv = [];
	readonly List<double> _sparkTemp = [];
	readonly Random _random = new();

	/// <summary>~1Hz timer driving the demo data. Created when shown, disposed when hidden/unloaded.</summary>
	IDispatcherTimer? _liveTimer;
	HaloMode _mode = HaloMode.Table;
	bool _showCalibration;
	bool _tagged;
	bool _showMvPrimary;
	bool _useFahrenheit;
	bool _isStable = true;

	double _lastPh = 4.49;
	double _lastMv = -127.4;
	double _lastTemp = 37.7;
	int _batteryPercent = 44;

	Label? _phPrimaryValue;
	Label? _phStatusLabel;
	Label? _tempValueLabel;
	Label? _tempStatusLabel;
	Label? _stabilityPillText;
	Border? _stabilityPill;
	BoxView? _stabilityDot;
	Label? _primaryChannelLabel;
	Label? _tempUnitHeaderLabel;
	Label? _switchChannelLabel;
	Label? _switchTempUnitLabel;
	GraphicsView? _phSparkline;
	GraphicsView? _tempSparkline;
	VerticalStackLayout? _tableDataRows;
	GraphicsView? _graphView;

	const int TableRowCount = 10;
	const int MaxHistory = 120;

	// ── Heart-rate-style sparkline (mini live trend on the readings card) ──────────
	// Clean modern monitor look: plain line — no dots, no value labels.
	const int SparkPointCount = 14;
	const float SparklineHeight = 52;
	const float SparklineStroke = 2.0f;

	const float GraphLinePh = 1.8f;
	const float GraphLineTemp = 1.5f;
	const double GraphHeight = 280;

	// Trends-graph (large chart) viewing window. Matches the sparkline clinical range so
	// the big chart doesn't clip when the live simulator produces wide swings.
	const double PhSpanMin = 0;
	const double PhSpanMax = 14;
	const double TempSpanMin = 0;
	const double TempSpanMax = 120;

	// ── Sparkline fixed Y-axis ranges & alarm thresholds ──────────────────────────
	// Sparkline uses *clinical* bounds (not the simulation clamps) so a quiet reading
	// near pH 7 renders near the midline instead of filling the whole strip.
	// pH: 0–14 chemistry range; red alarm outside [4, 12].
	const double PhSparkMin = 0;
	const double PhSparkMax = 14;
	const double PhSparkMidline = 7;
	const double PhDangerLow = 4;
	const double PhDangerHigh = 12;

	// mV: ±500 covers any pH-probe ORP reading. Thresholds derived from Nernst
	// (~ -59.16 mV / pH unit at 25 °C, isopotential ≈ 0 mV at pH 7):
	//   pH 4  ≈ +177 mV  → +180 mV alarm
	//   pH 12 ≈ -296 mV  → -300 mV alarm
	const double MvSparkMin = -500;
	const double MvSparkMax = 500;
	const double MvSparkMidline = 0;
	const double MvDangerLow = -300;
	const double MvDangerHigh = 180;

	// Temperature stored in °C; bounds are converted to °F at draw time when needed.
	// Range 0–120 °C, alarm outside [18, 80] °C (typical aquaculture / lab safe band).
	const double TempSparkMinC = 0;
	const double TempSparkMaxC = 120;
	const double TempSparkMidlineC = 25;
	const double TempDangerLowC = 18;
	const double TempDangerHighC = 80;

	static Color LabCanvas => ThemeColors.LabCanvas;
	static Color LabCard => ThemeColors.LabCard;
	static Color LabBorder => ThemeColors.LabBorder;
	static Color LabMuted => ThemeColors.LabMuted;
	static Color LabPrimaryText => ThemeColors.LabPrimaryText;
	static Color LabTableHeaderBackground => ThemeColors.LabTableHeaderBackground;
	static Color LabTableHeaderText => ThemeColors.LabTableHeaderText;
	static Color LabModeChipActive => ThemeColors.LabModeChipActive;
	static Color LabGradientEnd => ThemeColors.LabGradientEnd;
	static Color LabWarning => ThemeColors.LabWarning;
	static Color LabWarningMuted => ThemeColors.LabWarningMuted;
	static Color LabDanger => ThemeColors.LabDanger;
	static Color LabDangerSoft => ThemeColors.LabDangerSoft;
	static Color LabDangerMuted => ThemeColors.LabDangerMuted;
	static Color CyanAccent => ThemeColors.LabAccentCyan;
	static Color OrangeAccent => ThemeColors.LabAccentOrange;
	static Color Emerald => ThemeColors.LabEmerald;
	static Color EmeraldMuted => ThemeColors.LabEmeraldMuted;

	public Halo2MeasureView()
	{
		_viewModel = AppServices.Get<Halo2MeasureViewModel>();
		_loc = _viewModel.Loc;
		BindingContext = _viewModel;
		_chart = new HaloChartDrawable(() => _history)
		{
			PhAxisLabel = _loc.T("Halo_Mode_Ph"),
			TempAxisLabel = _loc.T("Halo_Settings_Graph_Temp"),
		};
		SyncSettingsFromPreferences();
		_showMvPrimary = Halo2Preferences.GetPrimaryDisplay().Equals("mv", StringComparison.OrdinalIgnoreCase);
		_useFahrenheit = _viewModel.UseFahrenheit;
		SetDynamicResource(BackgroundColorProperty, "LabCanvas");
		Content = new ScrollView { Content = _root };
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
		Rebuild();
	}

	void OnLoaded(object? sender, EventArgs e) => TryStartLiveTimer();

	void OnUnloaded(object? sender, EventArgs e) => StopLiveTimer();

	protected override void OnPropertyChanged(string? propertyName = null)
	{
		base.OnPropertyChanged(propertyName);
		if (propertyName == nameof(IsVisible))
			TryStartLiveTimer();
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		if (Handler is null)
			StopLiveTimer();
		else if (IsVisible)
			TryStartLiveTimer();
	}

	public void ApplyTheme() => Rebuild();

	void TryStartLiveTimer()
	{
		if (!IsVisible || Handler is null)
		{
			StopLiveTimer();
			return;
		}

		if (_liveTimer is not null)
			return;

		if (Dispatcher.CreateTimer() is not { } timer)
			return;

		_liveTimer = timer;
		_liveTimer.Interval = TimeSpan.FromMilliseconds(950);
		_liveTimer.Tick += OnLiveTimerTick;
		_liveTimer.Start();
	}

	void StopLiveTimer()
	{
		if (_liveTimer is null)
			return;

		_liveTimer.Tick -= OnLiveTimerTick;
		_liveTimer.Stop();
		_liveTimer = null;
	}

	void OnLiveTimerTick(object? sender, EventArgs e)
	{
		if (!IsVisible || Handler is null)
			return;

		MainThread.BeginInvokeOnMainThread(() =>
		{
			AppendLiveSample();
			UpdateLiveReadingsUi();
			PushReadingsToViewModel();
			SyncTableRows();
			RefreshChart();
		});
	}

	/// <summary>
	/// Generates one fresh demo sample per tick (called by <see cref="OnLiveTimerTick"/> ~1Hz).
	///
	/// Each channel rolls an *independent* uniform-random value across its full clinical range
	/// so the heart-rate sparkline shows visibly varied data on every tick (e.g. pH might be
	/// 4, 12, 3, 14, 1, 9, 4, 7, 12, 13, 6, 12, …). <see cref="RollNextDistinct"/> guarantees
	/// no two consecutive samples are identical so the line never flatlines.
	///
	/// Note: ranges intentionally cover 0..14 pH / ±500 mV / 0..120 °C, so the simulator
	/// regularly crosses the alarm thresholds — useful for demoing the red-segment behavior
	/// on the sparkline. Replace this whole method with the BLE feed for real hardware.
	/// </summary>
	void AppendLiveSample()
	{
		_lastPh = RollNextDistinct(_lastPh, PhSparkMin, PhSparkMax, decimals: 2);
		_lastMv = RollNextDistinct(_lastMv, MvSparkMin, MvSparkMax, decimals: 1);
		_lastTemp = RollNextDistinct(_lastTemp, TempSparkMinC, TempSparkMaxC, decimals: 1);
		_isStable = _random.NextDouble() > 0.15;

		PushSpark(_sparkPh, _lastPh);
		PushSpark(_sparkMv, _lastMv);
		PushSpark(_sparkTemp, _lastTemp);

		var stamp = DateTime.Now;
		_history.Insert(0, new HaloReading(_lastPh, _lastMv, _lastTemp, stamp));
		while (_history.Count > MaxHistory)
			_history.RemoveAt(_history.Count - 1);
	}

	/// <summary>
	/// Uniform random value in <c>[min, max]</c> rounded to <paramref name="decimals"/> places,
	/// re-rolled (up to a few tries) if it matches the previous value — keeps consecutive
	/// samples visibly different so the trend chart never draws a flat segment.
	/// </summary>
	double RollNextDistinct(double previous, double min, double max, int decimals)
	{
		var epsilon = Math.Pow(10, -decimals) / 2;
		double next;
		var attempts = 0;
		do
		{
			next = Math.Round(min + _random.NextDouble() * (max - min), decimals);
			attempts++;
		}
		while (Math.Abs(next - previous) < epsilon && attempts < 5);
		return next;
	}

	static void PushSpark(List<double> buffer, double value)
	{
		buffer.Add(value);
		while (buffer.Count > SparkPointCount)
			buffer.RemoveAt(0);
	}

	void UpdateLiveReadingsUi()
	{
		var c = CultureInfo.CurrentCulture;
		var phStatus = GetPhStatus(_lastPh);
		var tempStatus = GetTempStatus(_lastTemp);
		var pref = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		var allowSwitch = pref is "ph" or "mv";
		var showMvOnPrimary = allowSwitch && _showMvPrimary;
		var displayTemp = ToDisplayTemp(_lastTemp);

		// Default = theme's primary text color (reads as black on light theme / white on dark).
		// Out-of-range = red alarm. Channel-tinted accents are intentionally dropped so that
		// color carries one and only one meaning on the readings card: "in range vs alarm".
		var primaryColor = GetPrimaryReadingColor();
		var tempColor = GetTempReadingColor(_lastTemp);

		if (_primaryChannelLabel is not null)
		{
			_primaryChannelLabel.Text = showMvOnPrimary ? _loc.T("Halo_Mode_Mv") : _loc.T("Halo_Mode_Ph");
			_primaryChannelLabel.TextColor = primaryColor;
		}

		if (_phPrimaryValue is not null)
		{
			_phPrimaryValue.Text = showMvOnPrimary
				? _lastMv.ToString("0.0", c)
				: _lastPh.ToString("0.00", c);
			_phPrimaryValue.TextColor = primaryColor;
		}

		if (_phStatusLabel is not null)
		{
			_phStatusLabel.Text = showMvOnPrimary ? _loc.T("Halo_PhStatus_Orp") : phStatus.Label;
			_phStatusLabel.TextColor = primaryColor;
		}

		if (_tempUnitHeaderLabel is not null)
		{
			_tempUnitHeaderLabel.Text = TempUnitSymbol;
			_tempUnitHeaderLabel.TextColor = tempColor;
		}

		if (_tempValueLabel is not null)
		{
			_tempValueLabel.Text = displayTemp.ToString("0.0", c);
			_tempValueLabel.TextColor = tempColor;
		}

		if (_tempStatusLabel is not null)
		{
			_tempStatusLabel.Text = _loc.T("Halo_Temp_AtcFormat", tempStatus.Label);
			_tempStatusLabel.TextColor = tempColor;
		}

		if (_switchChannelLabel is not null)
		{
			_switchChannelLabel.IsVisible = allowSwitch;
			if (allowSwitch)
				_switchChannelLabel.Text = showMvOnPrimary
					? _loc.T("Halo_Mode_SwitchToPh")
					: _loc.T("Halo_Mode_SwitchToMv");
		}

		if (_switchTempUnitLabel is not null)
			_switchTempUnitLabel.Text = _useFahrenheit
				? _loc.T("Halo_Mode_SwitchToCelsius")
				: _loc.T("Halo_Mode_SwitchToFahrenheit");

		if (_stabilityPillText is not null && _stabilityPill is not null && _stabilityDot is not null)
		{
			if (_isStable)
			{
				_stabilityPillText.Text = _loc.T("Halo_Stability_Stable");
				_stabilityPillText.TextColor = Emerald;
				_stabilityPill.BackgroundColor = EmeraldMuted;
				_stabilityPill.Stroke = Emerald.MultiplyAlpha(0.35f);
				_stabilityDot.Color = Emerald;
			}
			else
			{
				_stabilityPillText.Text = _loc.T("Halo_Stability_Drifting");
				_stabilityPillText.TextColor = LabWarning;
				_stabilityPill.BackgroundColor = LabWarningMuted;
				_stabilityPill.Stroke = LabWarning.MultiplyAlpha(0.35f);
				_stabilityDot.Color = LabWarning;
			}
		}

		_phSparkline?.Invalidate();
		_tempSparkline?.Invalidate();
	}

	(string Label, Color Color) GetPhStatus(double ph) => ph switch
	{
		< 5.5 => (_loc.T("Halo_Ph_Status_StrongAcidic"), ThemeColors.LabPhAcidic),
		< 6.5 => (_loc.T("Halo_Ph_Status_Acidic"), ThemeColors.LabPhAcidicMid),
		< 7.5 => (_loc.T("Halo_Ph_Status_Neutral"), ThemeColors.LabPhNeutral),
		< 9.0 => (_loc.T("Halo_Ph_Status_Basic"), ThemeColors.LabPhBasic),
		_ => (_loc.T("Halo_Ph_Status_StrongAlkaline"), ThemeColors.LabPhAlkaline)
	};

	(string Label, Color Color) GetTempStatus(double temp) => temp switch
	{
		> 80 => (_loc.T("Halo_Temp_Status_Critical"), ThemeColors.LabPhAcidic),
		> 60 => (_loc.T("Halo_Temp_Status_High"), ThemeColors.LabPhAcidicMid),
		_ => (_loc.T("Halo_Temp_Status_Optimal"), ThemeColors.LabPhNeutral)
	};

	// ── Alarm helpers ─────────────────────────────────────────────────────────────
	// One source of truth for "is this reading inside the safe band?" — used by every
	// label and graph stroke so the alarm state is always visually consistent.
	static bool IsPhInRange(double ph) => ph >= PhDangerLow && ph <= PhDangerHigh;
	static bool IsMvInRange(double mv) => mv >= MvDangerLow && mv <= MvDangerHigh;
	static bool IsTempInRangeC(double tempC) => tempC >= TempDangerLowC && tempC <= TempDangerHighC;

	/// <summary>Theme primary text when in range, alarm red when out of range.</summary>
	static Color ReadingColor(bool inRange) => inRange ? ThemeColors.LabPrimaryText : ThemeColors.LabDanger;

	/// <summary>
	/// Color for the primary live-readings cluster (pH/mV value, status text, channel header).
	/// Picks the threshold set matching whichever channel is currently being displayed as primary.
	/// </summary>
	Color GetPrimaryReadingColor()
	{
		var pref = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		var showMv = (pref is "ph" or "mv") && _showMvPrimary;
		return showMv ? ReadingColor(IsMvInRange(_lastMv)) : ReadingColor(IsPhInRange(_lastPh));
	}

	/// <summary>Color for the temperature live-readings cluster. Thresholds are checked in °C (storage unit).</summary>
	static Color GetTempReadingColor(double tempC) => ReadingColor(IsTempInRangeC(tempC));

	void SyncTableRows()
	{
		if (_tableDataRows is null)
			return;

		_tableDataRows.Children.Clear();
		var rows = _history.Take(TableRowCount).ToList();
		for (var i = 0; i < rows.Count; i++)
		{
			var reading = rows[i];
			var taggedRow = _tagged && i == 3;
			var row = new Grid
			{
				ColumnDefinitions = TableColumns(),
				Padding = new Thickness(10, 10),
				BackgroundColor = RowBackground(i, taggedRow)
			};
			var c = CultureInfo.CurrentCulture;
			AddCell(row, reading.Ph.ToString("0.00", c), 0);
			AddCell(row, reading.Mv.ToString("0.0", c), 1);
			AddCell(row, reading.Temp.ToString("0.0", c), 2);
			AddCell(row, reading.DateDisplay, 3);
			_tableDataRows.Children.Add(row);
		}
	}

	Color RowBackground(int index, bool taggedRow)
	{
		if (taggedRow)
			return Emerald.MultiplyAlpha(0.22f);

		return index % 2 == 0
			? ThemeColors.LabRowStripe
			: LabCard;
	}

	void RefreshChart()
	{
		_chart.Tagged = _tagged;
		_graphView?.Invalidate();
	}

	void Rebuild()
	{
		_useFahrenheit = Halo2Preferences.UseFahrenheit();
		_root.Children.Clear();
		ClearUiRefs();

		_root.BackgroundColor = LabCanvas;
		_root.Padding = new Thickness(0, 0, 0, 24);

		_root.Children.Add(BuildLabHeader());
		_root.Children.Add(BuildContentSection());

		if (_history.Count == 0)
			SeedInitialHistory();

		UpdateLiveReadingsUi();
		SyncTableRows();
		RefreshChart();
	}

	void ClearUiRefs()
	{
		_phPrimaryValue = _phStatusLabel = _tempValueLabel = _tempStatusLabel = null;
		_primaryChannelLabel = _tempUnitHeaderLabel = null;
		_stabilityPillText = _switchChannelLabel = _switchTempUnitLabel = null;
		_stabilityPill = null;
		_stabilityDot = null;
		_phSparkline = _tempSparkline = null;
		_tableDataRows = null;
		_graphView = null;
	}

	void SeedInitialHistory()
	{
		_sparkPh.Clear();
		_sparkMv.Clear();
		_sparkTemp.Clear();
		// Pre-fill all three buffers so the sparkline is full-length from the very first frame
		// (otherwise mV would render with only ~TableRowCount samples after start-up).
		for (var i = 0; i < SparkPointCount; i++)
		{
			_sparkPh.Add(_lastPh);
			_sparkMv.Add(_lastMv);
			_sparkTemp.Add(_lastTemp);
		}

		for (var i = 0; i < TableRowCount; i++)
			AppendLiveSample();
	}

	View BuildContentSection()
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(20, 16, 20, 8),
			Children =
			{
				BuildLiveReadingsCard(),
				BuildModeBar(),
				_showCalibration ? BuildCalibrationSummary() : _mode == HaloMode.Graph ? BuildGraph() : BuildTable()
			}
		};
		return stack;
	}

	double ToDisplayTemp(double celsius) => _useFahrenheit ? celsius * 9.0 / 5.0 + 32.0 : celsius;

	string TempUnitSymbol => _useFahrenheit ? "°F" : "°C";

	IReadOnlyList<double> GetPrimarySparkData()
	{
		var pref = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		var showMv = (pref is "ph" or "mv") && _showMvPrimary;
		return showMv ? _sparkMv : _sparkPh;
	}

	IReadOnlyList<double> GetTempSparkData()
	{
		if (!_useFahrenheit)
			return _sparkTemp;

		var converted = new double[_sparkTemp.Count];
		for (var i = 0; i < _sparkTemp.Count; i++)
			converted[i] = ToDisplayTemp(_sparkTemp[i]);
		return converted;
	}

	/// <summary>
	/// Returns the fixed Y-axis bounds + alarm thresholds for the primary (pH or mV) sparkline.
	/// Switches between pH (0..14, alarm outside 4..12) and mV (±500, alarm outside -300..+180)
	/// based on the current <c>Halo2Preferences.GetPrimaryDisplay()</c> and the in-card toggle.
	/// </summary>
	SparklineBounds GetPrimarySparkBounds()
	{
		var pref = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		var showMv = (pref is "ph" or "mv") && _showMvPrimary;
		return showMv
			? new SparklineBounds(MvSparkMin, MvSparkMax, MvSparkMidline, MvDangerLow, MvDangerHigh)
			: new SparklineBounds(PhSparkMin, PhSparkMax, PhSparkMidline, PhDangerLow, PhDangerHigh);
	}

	/// <summary>
	/// Temperature sparkline bounds in the *display* unit (auto-converts °C → °F).
	/// Source bounds: 0..120 °C with alarm outside 18..80 °C; midline at 25 °C.
	/// </summary>
	SparklineBounds GetTempSparkBounds() => _useFahrenheit
		? new SparklineBounds(
			ToDisplayTemp(TempSparkMinC),
			ToDisplayTemp(TempSparkMaxC),
			ToDisplayTemp(TempSparkMidlineC),
			ToDisplayTemp(TempDangerLowC),
			ToDisplayTemp(TempDangerHighC))
		: new SparklineBounds(TempSparkMinC, TempSparkMaxC, TempSparkMidlineC, TempDangerLowC, TempDangerHighC);

	static Label CreateChannelSwitchLabel()
	{
		return new Label
		{
			FontSize = 11,
			TextColor = LabMuted,
			HorizontalTextAlignment = TextAlignment.Center,
			TextDecorations = TextDecorations.Underline,
			Margin = new Thickness(0, 2, 0, 0)
		};
	}

	Label CreateTrendCaptionLabel() =>
		new()
		{
			Text = _loc.T("Halo_Tab_Trend"),
			FontSize = 10,
			FontAttributes = FontAttributes.Bold,
			TextColor = LabMuted,
			CharacterSpacing = 0.6,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(0, 6, 0, 2)
		};



	string Halo2DeviceName => _loc.T("Halo_Device_Name");
	const string Halo2DeviceIcon = "halo2_device_icon.png";

	static Label CreateDeviceCardValueLabel(string text, Color color, double fontSize = 15, FontAttributes fontAttributes = FontAttributes.None) =>
		new()
		{
			Text = text,
			FontSize = fontSize,
			FontAttributes = fontAttributes,
			TextColor = color,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};

	/// <summary>Small caption used above each metric (e.g. "Battery:", "Probe Condition:").</summary>
	static Label CreateDeviceCardCaptionLabel(string text, TextAlignment alignment = TextAlignment.Start) =>
		new()
		{
			Text = text,
			FontSize = 13,
			TextColor = LabMuted,
			CharacterSpacing = 0.2,
			HorizontalTextAlignment = alignment,
			HorizontalOptions = alignment == TextAlignment.End ? LayoutOptions.End : LayoutOptions.Start
		};

	/// <summary>Maps probe condition % to a short human label (matches the design's "Excellent / Good / Fair / Poor" rubric).</summary>
	string GetProbeConditionLabel(int percent) => percent switch
	{
		>= 80 => _loc.T("Halo_Probe_Excellent"),
		>= 50 => _loc.T("Halo_Probe_Good"),
		>= 30 => _loc.T("Halo_Probe_Fair"),
		_ => _loc.T("Halo_Probe_Poor")
	};

	static Color GetProbeConditionColor(int percent) => percent switch
	{
		>= 80 => LabPrimaryText,
		>= 50 => LabPrimaryText,
		>= 30 => LabWarning,
		_ => LabDanger
	};

	/// <summary>
	/// Vector battery glyph (rounded body + nub). Inner fill color tracks the percent:
	/// green when healthy, amber when low (&lt; 35%), red when critical (&lt; 20%).
	/// Drawn with Border + BoxView so it scales with theme and pixel density.
	/// </summary>
	static View CreateBatteryGlyph(int percent)
	{
		const double bodyWidth = 26;
		const double bodyHeight = 12;
		const double bodyPadding = 1.5;

		var fillColor = percent <= 20 ? LabDanger : percent <= 35 ? LabWarning : Emerald;
		var safePercent = Math.Clamp(percent, 0, 100);
		var fillWidth = (bodyWidth - bodyPadding * 2) * safePercent / 100.0;

		var fillBar = new BoxView
		{
			Color = fillColor,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Fill,
			WidthRequest = fillWidth,
			CornerRadius = 1
		};

		var body = new Border
		{
			WidthRequest = bodyWidth,
			HeightRequest = bodyHeight,
			Stroke = LabMuted,
			StrokeThickness = 1.2,
			StrokeShape = new RoundRectangle { CornerRadius = 2 },
			BackgroundColor = Colors.Transparent,
			Padding = new Thickness(bodyPadding),
			Content = fillBar,
			VerticalOptions = LayoutOptions.Center
		};

		var nub = new BoxView
		{
			Color = LabMuted,
			WidthRequest = 2,
			HeightRequest = 6,
			CornerRadius = 1,
			VerticalOptions = LayoutOptions.Center
		};

		return new HorizontalStackLayout
		{
			Spacing = 1,
			VerticalOptions = LayoutOptions.Center,
			Children = { body, nub }
		};
	}

	/// <summary>
	/// Round status glyph for probe condition. Green tinted disc with a "✓" when the
	/// probe is healthy (≥ 50%), warning "!" otherwise.
	/// </summary>
	static View CreateProbeStatusGlyph(int percent)
	{
		var healthy = percent >= 50;
		var background = healthy ? EmeraldMuted : LabDangerMuted;
		var glyphColor = healthy ? Emerald : LabDanger;
		var glyph = healthy ? "✓" : "!";

		var symbol = new Label
		{
			Text = glyph,
			FontSize = 15,
			FontAttributes = FontAttributes.Bold,
			TextColor = glyphColor,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		return new Border
		{
			WidthRequest = 26,
			HeightRequest = 26,
			StrokeThickness = 0,
			StrokeShape = new Ellipse(),
			BackgroundColor = background,
			Content = symbol,
			VerticalOptions = LayoutOptions.Center
		};
	}

	/// <summary>
	/// Icon-only chip showing a Bluetooth glyph. Tap → confirmation dialog → if the user
	/// confirms, the host <see cref="MeasureTabPage"/> performs the disconnect and re-opens
	/// the device picker. Destructive intent communicated via a soft-red tint.
	/// </summary>
	Border CreateDisconnectButton()
	{
		var icon = new GraphicsView
		{
			WidthRequest = 18,
			HeightRequest = 18,
			Drawable = new HaloBluetoothIconDrawable(() => LabDanger),
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			InputTransparent = true
		};

		var button = new Border
		{
			WidthRequest = 38,
			HeightRequest = 38,
			Padding = new Thickness(0),
			BackgroundColor = LabDangerMuted,
			Stroke = LabDanger.MultiplyAlpha(0.3f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 19 },
			VerticalOptions = LayoutOptions.Center,
			Content = icon
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await ConfirmAndDisconnectAsync();
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button, _loc.T("Halo_Disconnect_DialogTitle"));
		return button;
	}

	/// <summary>
	/// Shows a confirmation alert before disconnecting from the probe. Routed through the
	/// host page so it works whether we're embedded in <see cref="MeasureTabPage"/> or any
	/// other ContentPage in the future.
	/// </summary>
	async Task ConfirmAndDisconnectAsync()
	{
		var host = ViewNavigation.FindHostPage(this);
		if (host is null)
			return;

		var confirmed = await host.DisplayAlertAsync(
			_loc.T("Halo_Disconnect_DialogTitle"),
			_loc.T("Halo_Disconnect_DialogMessage", Halo2DeviceName),
			_loc.T("Toolbar_Disconnect"),
			_loc.T("Common_Cancel"));

		if (confirmed && host is MeasureTabPage measureTab)
			await measureTab.DisconnectAndOpenDevicesAsync();
	}

	/// <summary>
	/// Bluetooth "B-rune" glyph drawn as a single continuous stroke. Path normalized to a
	/// [-1, 1] coordinate space and scaled to the rect at draw time. Color is fetched via
	/// lambda so theme switches re-evaluate it.
	/// </summary>
	sealed class HaloBluetoothIconDrawable(Func<Color> getColor) : IDrawable
	{
		public void Draw(ICanvas canvas, RectF rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
				return;

			var color = getColor();
			canvas.StrokeColor = color;
			canvas.StrokeSize = Math.Max(1.4f, rect.Width * 0.12f);
			canvas.StrokeLineCap = LineCap.Round;
			canvas.StrokeLineJoin = LineJoin.Round;

			var cx = rect.Center.X;
			var cy = rect.Center.Y;
			var u = Math.Min(rect.Width, rect.Height) * 0.4f;

			// Iconic Bluetooth trace (single stroke):
			//   (-0.42, -0.42) → ( 0.58,  0.42) → ( 0, 1) → ( 0,-1) → ( 0.58,-0.42) → (-0.42, 0.42)
			var path = new PathF();
			path.MoveTo(cx - 0.42f * u, cy - 0.42f * u);
			path.LineTo(cx + 0.58f * u, cy + 0.42f * u);
			path.LineTo(cx + 0.00f * u, cy + 1.00f * u);
			path.LineTo(cx + 0.00f * u, cy - 1.00f * u);
			path.LineTo(cx + 0.58f * u, cy - 0.42f * u);
			path.LineTo(cx - 0.42f * u, cy + 0.42f * u);
			canvas.DrawPath(path);
		}
	}

	async Task OpenHaloSettingsAsync()
	{
		if (Shell.Current is null)
			return;

		await Shell.Current.GoToAsync(Halo2Routes.Settings);
		SyncSettingsFromPreferences();
	}

	public void SyncSettingsFromPreferences()
	{
		_viewModel.SyncFromPreferences();
		_showMvPrimary = Halo2Preferences.GetPrimaryDisplay().Equals("mv", StringComparison.OrdinalIgnoreCase);
		_useFahrenheit = _viewModel.UseFahrenheit;
		Rebuild();
	}

	void PushReadingsToViewModel()
	{
		_viewModel.Ph = _lastPh;
		_viewModel.Millivolts = _lastMv;
		_viewModel.TemperatureC = _lastTemp;
		_viewModel.BatteryPercent = _batteryPercent;
		_viewModel.ProbeConditionPercent = GetProbeConditionPercent();
		_viewModel.IsTagged = _tagged;
		_viewModel.StabilityLabel = _isStable
			? _loc.T("Halo_Stability_Stable")
			: _loc.T("Halo_Stability_Drifting");
	}

	int GetProbeConditionPercent() => _tagged ? 50 : 94;

	/// <summary>
	/// Top device card — two rows separated by uniform vertical rhythm:
	///   Row 1: small device icon · device name · Bluetooth-disconnect icon chip
	///   Row 2: Battery cluster | thin vertical divider | Probe Condition cluster (right-aligned)
	///
	/// Tapping the Bluetooth chip prompts a confirmation dialog before disconnecting.
	/// Tapping the probe-condition row toggles a demo "tagged" state so reviewers can see the
	/// warning visuals without waiting on data.
	/// </summary>
	Border BuildLabHeader()
	{
		var conditionPercent = GetProbeConditionPercent();
		var batteryWarning = _batteryPercent <= 35;
		var batteryValueColor = batteryWarning ? LabWarning : LabPrimaryText;
		var probeColor = GetProbeConditionColor(conditionPercent);

		// ── Row 1: tight icon · device name · disconnect chip ───────────────
		// Small frame + zero margin keeps the icon visually adjacent to the name
		// (no extra whitespace baked in by AspectFit padding around the PNG).
		var deviceIcon = new Image
		{
			Source = Halo2DeviceIcon,
			Aspect = Aspect.AspectFit,
			WidthRequest = 36,
			HeightRequest = 44,
			Margin = new Thickness(0),
			VerticalOptions = LayoutOptions.Center
		};

		var deviceName = CreateDeviceCardValueLabel(
			Halo2DeviceName,
			LabPrimaryText,
			fontSize: 20,
			fontAttributes: FontAttributes.Bold);

		var disconnect = CreateDisconnectButton();

		var topRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 10,
			VerticalOptions = LayoutOptions.Center,
			Children = { deviceIcon, deviceName, disconnect }
		};
		Grid.SetColumn(deviceName, 1);
		Grid.SetColumn(disconnect, 2);

		// ── Row 2: battery (left) | divider | probe condition (right-aligned) ─
		var batteryCluster = new VerticalStackLayout
		{
			Spacing = 6,
			HorizontalOptions = LayoutOptions.Start,
			Children =
			{
				CreateDeviceCardCaptionLabel("Battery:"),
				new HorizontalStackLayout
				{
					Spacing = 8,
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
						CreateBatteryGlyph(_batteryPercent),
						CreateDeviceCardValueLabel(
							$"{_batteryPercent}%",
							batteryValueColor,
							fontSize: 15,
							fontAttributes: FontAttributes.Bold)
					}
				}
			}
		};

		var probeText = $"{GetProbeConditionLabel(conditionPercent)} · {conditionPercent}%";
		var probeCluster = new VerticalStackLayout
		{
			Spacing = 6,
			HorizontalOptions = LayoutOptions.End,
			Children =
			{
				CreateDeviceCardCaptionLabel("Probe Condition:", TextAlignment.End),
				new HorizontalStackLayout
				{
					Spacing = 8,
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
						CreateProbeStatusGlyph(conditionPercent),
						CreateDeviceCardValueLabel(
							probeText,
							probeColor,
							fontSize: 15,
							fontAttributes: FontAttributes.Bold)
					}
				}
			}
		};

		var probeTap = new TapGestureRecognizer();
		probeTap.Tapped += (_, _) => { _tagged = !_tagged; Rebuild(); };
		probeCluster.GestureRecognizers.Add(probeTap);

		var divider = new BoxView
		{
			Color = LabBorder,
			WidthRequest = 1,
			VerticalOptions = LayoutOptions.Fill,
			Margin = new Thickness(0, 4)
		};

		var bottomRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 14,
			Children = { batteryCluster, divider, probeCluster }
		};
		Grid.SetColumn(divider, 1);
		Grid.SetColumn(probeCluster, 2);

		var cardBody = new VerticalStackLayout
		{
			Spacing = 14,
			Padding = new Thickness(16),
			Children = { topRow, bottomRow }
		};

		var card = new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			BackgroundColor = LabCard,
			Content = cardBody
		};

		var shell = new VerticalStackLayout
		{
			Spacing = 0,
			Padding = new Thickness(20, 12, 20, 0),
			Children = { card }
		};

		return new Border
		{
			StrokeThickness = 0,
			BackgroundColor = LabCanvas,
			StrokeShape = new RoundRectangle { CornerRadius = 0 },
			Content = shell
		};
	}

	Border BuildLiveReadingsCard()
	{
		_stabilityDot = new BoxView { WidthRequest = 8, HeightRequest = 8, CornerRadius = 4, Color = Emerald };
		_stabilityPillText = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
		_stabilityPill = new Border
		{
			Padding = new Thickness(16, 8),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = new HorizontalStackLayout
			{
				Spacing = 8,
				Children = { _stabilityDot, _stabilityPillText }
			}
		};

		_stabilityPill.HorizontalOptions = LayoutOptions.Start;
		_stabilityPill.VerticalOptions = LayoutOptions.Center;

		var settings = IconActionButton(HaloMeasureModeIconKind.Settings, _loc.T("Toolbar_Settings"), () => _ = OpenHaloSettingsAsync());

		var topStatusRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 12,
			VerticalOptions = LayoutOptions.Center,
			Children = { _stabilityPill, settings }
		};
		Grid.SetColumn(settings, 1);

		var titleSection = new VerticalStackLayout
		{
			Padding = new Thickness(20, 14, 20, 0),
			Children = { topStatusRow }
		};

		_phPrimaryValue = new Label
		{
			FontSize = 46,
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.None
		};
		_phStatusLabel = new Label
		{
			FontSize = 13,
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
			CharacterSpacing = 1
		};
		_phSparkline = new GraphicsView
		{
			HeightRequest = SparklineHeight,
			Margin = new Thickness(4, 0),
			Drawable = new HaloHeartRateSparkline(
				GetPrimarySparkData,
				// Modern monitor look: neutral grey by default (LabMuted ≈ slate-500 light /
				// #A1A1AA dark). The drawable flips individual segments to red whenever the
				// data crosses an alarm threshold.
				getNormalColor: () => ThemeColors.LabMuted,
				getBounds: GetPrimarySparkBounds),
			HorizontalOptions = LayoutOptions.Fill
		};

		_primaryChannelLabel = new Label
		{
			FontSize = 13,
			TextColor = CyanAccent,
			CharacterSpacing = 2,
			HorizontalTextAlignment = TextAlignment.Center,
			FontAttributes = FontAttributes.Bold
		};

		_switchChannelLabel = CreateChannelSwitchLabel();
		var switchTap = new TapGestureRecognizer();
		switchTap.Tapped += (_, _) =>
		{
			_showMvPrimary = !_showMvPrimary;
			UpdateLiveReadingsUi();
			_phSparkline?.Invalidate();
		};
		_switchChannelLabel.GestureRecognizers.Add(switchTap);

		var phColumn = new VerticalStackLayout
		{
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Fill,
			Children =
			{
				_primaryChannelLabel,
				_phPrimaryValue,
				_phStatusLabel,
				CreateTrendCaptionLabel(),
				_phSparkline,
				_switchChannelLabel
			}
		};

		_tempValueLabel = new Label
		{
			FontSize = 46,
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.None
		};
		_tempStatusLabel = new Label
		{
			FontSize = 13,
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
			CharacterSpacing = 0.5
		};
		_tempSparkline = new GraphicsView
		{
			HeightRequest = SparklineHeight,
			Margin = new Thickness(4, 0),
			Drawable = new HaloHeartRateSparkline(
				GetTempSparkData,
				getNormalColor: () => ThemeColors.LabMuted,
				getBounds: GetTempSparkBounds),
			HorizontalOptions = LayoutOptions.Fill
		};

		_tempUnitHeaderLabel = new Label
		{
			FontSize = 13,
			TextColor = OrangeAccent,
			CharacterSpacing = 1,
			HorizontalTextAlignment = TextAlignment.Center,
			FontAttributes = FontAttributes.Bold
		};

		_switchTempUnitLabel = CreateChannelSwitchLabel();
		var tempSwitchTap = new TapGestureRecognizer();
		tempSwitchTap.Tapped += (_, _) =>
		{
			_useFahrenheit = !_useFahrenheit;
			Halo2Preferences.SetTemperatureUnit(_useFahrenheit);
			UpdateLiveReadingsUi();
			_tempSparkline?.Invalidate();
		};
		_switchTempUnitLabel.GestureRecognizers.Add(tempSwitchTap);

		var tempColumn = new VerticalStackLayout
		{
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Fill,
			Children =
			{
				_tempUnitHeaderLabel,
				_tempValueLabel,
				_tempStatusLabel,
				CreateTrendCaptionLabel(),
				_tempSparkline,
				_switchTempUnitLabel
			}
		};

		var midDivider = new BoxView { WidthRequest = 1, Color = LabBorder, Margin = new Thickness(0, 8) };

		var dualGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(1), new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 16,
			Padding = new Thickness(16, 12, 16, 16),
			Children = { phColumn, midDivider, tempColumn }
		};
		Grid.SetColumn(midDivider, 1);
		Grid.SetColumn(tempColumn, 2);

		var inner = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1),
			GradientStops =
			{
				new GradientStop(ThemeColors.LabGradientStop, 0),
				new GradientStop(LabGradientEnd, 1)
			}
		};

		var panel = new VerticalStackLayout { Spacing = 0, Children = { titleSection, dualGrid } };

		return new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 20 },
			Background = inner,
			Content = panel,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(CyanAccent.MultiplyAlpha(0.15f)),
				Offset = new Point(0, 6),
				Radius = 16,
				Opacity = 1
			}
		};
	}

	Border IconActionButton(HaloMeasureModeIconKind iconKind, string accessibilityText, Action action)
	{
		var button = new Border
		{
			WidthRequest = 36,
			HeightRequest = 36,
			BackgroundColor = ThemeColors.LabIconButtonFill,
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = Halo2MeasureModeIcons.Create(iconKind, () => LabPrimaryText, 22)
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button, accessibilityText);
		return button;
	}

	Border BuildModeBar()
	{
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)),
			Padding = 4,
			BackgroundColor = LabCard
		};
		grid.Children.Add(ModeChip(_loc.T("Halo_Tab_Data"), HaloMode.Table, HaloMeasureModeIconKind.Table));
		var graph = ModeChip(_loc.T("Halo_Tab_Trends"), HaloMode.Graph, HaloMeasureModeIconKind.Chart);
		grid.Children.Add(graph);
		Grid.SetColumn(graph, 1);
		var calibrate = CalibrationModeChip();
		grid.Children.Add(calibrate);
		Grid.SetColumn(calibrate, 2);
		return new Border
		{
			StrokeThickness = 1,
			Stroke = LabBorder,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			BackgroundColor = LabCard,
			Content = grid
		};
	}

	Border ModeChip(string text, HaloMode mode, HaloMeasureModeIconKind iconKind)
	{
		var active = !_showCalibration && _mode == mode;
		var chip = new Border
		{
			Padding = new Thickness(12, 10),
			BackgroundColor = active ? LabModeChipActive : Colors.Transparent,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new HorizontalStackLayout
			{
				Spacing = 6,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					Halo2MeasureModeIcons.Create(iconKind, () => active ? CyanAccent : LabMuted),
					new Label
					{
						Text = text,
						FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
						TextColor = active ? LabPrimaryText : LabMuted,
						VerticalOptions = LayoutOptions.Center
					}
				}
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			_mode = mode;
			_showCalibration = false;
			Rebuild();
		};
		chip.GestureRecognizers.Add(tap);
		return chip;
	}

	Border CalibrationModeChip()
	{
		var active = _showCalibration;
		var chip = new Border
		{
			Padding = new Thickness(12, 10),
			BackgroundColor = active ? LabModeChipActive : Colors.Transparent,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new HorizontalStackLayout
			{
				Spacing = 6,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					Halo2MeasureModeIcons.Create(HaloMeasureModeIconKind.Calibration, () => active ? CyanAccent : LabMuted),
					new Label
					{
						Text = _loc.T("Halo_Tab_Calibrate"),
						FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
						TextColor = LabPrimaryText,
						VerticalOptions = LayoutOptions.Center
					}
				}
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			_showCalibration = !_showCalibration;
			Rebuild();
		};
		chip.GestureRecognizers.Add(tap);
		return chip;
	}

	Border BuildCalibrationSummary()
	{
		var header = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 8,
			Padding = new Thickness(14, 14),
			BackgroundColor = LabCardElevated
		};
		AddCalibrationSummaryMetric(header, "Last Calibration:", Halo2CalibrationDemoData.LastCalibrationDisplay, 0);
		AddCalibrationSummaryMetric(header, "Offset:", Halo2CalibrationDemoData.OffsetDisplay, 1);
		AddCalibrationSummaryMetric(header, "Average Slope:", Halo2CalibrationDemoData.AverageSlopeDisplay, 2);

		var pointsGrid = new Grid
		{
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			],
			RowSpacing = 4,
			Padding = new Thickness(10, 16, 10, 18),
			HorizontalOptions = LayoutOptions.Center
		};

		var columnDefs = new ColumnDefinitionCollection();
		var points = Halo2CalibrationDemoData.Points;
		var slopes = Halo2CalibrationDemoData.SegmentSlopes;
		for (var i = 0; i < points.Count; i++)
		{
			columnDefs.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
			if (i < slopes.Count)
				columnDefs.Add(new ColumnDefinition(GridLength.Auto));
		}

		pointsGrid.ColumnDefinitions = columnDefs;

		var col = 0;
		for (var i = 0; i < points.Count; i++)
		{
			if (i > 0)
			{
				var slope = CalibrationSlopeLabel(slopes[i - 1]);
				pointsGrid.Children.Add(slope);
				Grid.SetColumn(slope, col);
				Grid.SetRow(slope, 0);
				col++;
			}

			var point = CalibrationPointColumn(points[i]);
			pointsGrid.Children.Add(point);
			Grid.SetColumn(point, col);
			Grid.SetRow(point, 1);
			col++;
		}

		var body = new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			Content = pointsGrid
		};

		var stack = new VerticalStackLayout
		{
			Spacing = 0,
			Children =
			{
				header,
				new BoxView { HeightRequest = 1, Color = LabBorder },
				body
			}
		};

		return new Border
		{
			StrokeThickness = 1,
			Stroke = LabBorder,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			BackgroundColor = LabCard,
			Content = stack
		};
	}

	static void AddCalibrationSummaryMetric(Grid header, string caption, string value, int column)
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 4,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = caption,
					FontSize = 12,
					TextColor = LabMuted,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = value,
					FontSize = 13,
					FontAttributes = FontAttributes.Bold,
					TextColor = LabPrimaryText,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.WordWrap
				}
			}
		};
		header.Children.Add(stack);
		Grid.SetColumn(stack, column);
	}

	static View CalibrationSlopeLabel(string slopePercent)
	{
		return new VerticalStackLayout
		{
			WidthRequest = 40,
			Spacing = 0,
			VerticalOptions = LayoutOptions.End,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Slope:",
					FontSize = 11,
					TextColor = LabMuted,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = slopePercent,
					FontSize = 12,
					FontAttributes = FontAttributes.Bold,
					TextColor = LabPrimaryText,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};
	}

	static View CalibrationPointColumn(Halo2CalibrationPoint point)
	{
		return new VerticalStackLayout
		{
			Spacing = 6,
			MinimumWidthRequest = 68,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				Halo2CalibrationUi.BufferBeaker(point.Ph, 54, 42, calibrated: true),
				new Label
				{
					Text = point.Millivolts,
					FontSize = 12,
					TextColor = LabPrimaryText,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = point.Temperature,
					FontSize = 12,
					TextColor = LabPrimaryText,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = Halo2CalibrationDemoData.PointDateDisplay,
					FontSize = 11,
					TextColor = LabMuted,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = Halo2CalibrationDemoData.PointTimeDisplay,
					FontSize = 11,
					TextColor = LabMuted,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};
	}

	static Color LabCardElevated => ThemeColors.LabCardElevated;

	Border BuildTable()
	{
		var stack = new VerticalStackLayout { Spacing = 0 };
		var header = new Grid
		{
			ColumnDefinitions = TableColumns(),
			Padding = new Thickness(10, 12),
			BackgroundColor = LabTableHeaderBackground
		};
		AddCell(header, _loc.T("Halo_Table_PhHeader"), 0, true, LabTableHeaderText);
		AddCell(header, _loc.T("Halo_Table_MvHeader"), 1, true, LabTableHeaderText);
		AddCell(header, _loc.T("Halo_Table_TempHeaderFormat", TempUnitSymbol), 2, true, LabTableHeaderText);
		AddCell(header, _loc.T("Halo_Table_TimestampHeader"), 3, true, LabTableHeaderText);
		stack.Children.Add(header);

		_tableDataRows = new VerticalStackLayout { Spacing = 0 };
		stack.Children.Add(_tableDataRows);

		return new Border
		{
			StrokeThickness = 1,
			Stroke = LabBorder,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			BackgroundColor = LabCard.MultiplyAlpha(0.85f),
			Content = stack
		};
	}

	static ColumnDefinitionCollection TableColumns() => new(
		new ColumnDefinition(new GridLength(0.72, GridUnitType.Star)),
		new ColumnDefinition(new GridLength(0.78, GridUnitType.Star)),
		new ColumnDefinition(new GridLength(0.95, GridUnitType.Star)),
		new ColumnDefinition(new GridLength(1.55, GridUnitType.Star)));

	static void AddCell(Grid rowGrid, string text, int column, bool bold = false, Color? textColor = null)
	{
		var label = new Label
		{
			Text = text,
			FontSize = bold ? 13 : 14,
			FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
			TextColor = textColor ?? LabPrimaryText,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation,
			MaxLines = 1
		};
		rowGrid.Children.Add(label);
		Grid.SetColumn(label, column);
	}

	Border BuildGraph()
	{
		_chart.Tagged = _tagged;
		var legend = new HorizontalStackLayout
		{
			Spacing = 16,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 0, 0, 6),
			Children =
			{
				CreateLegendSwatch(_loc.T("Halo_Mode_Ph"), CyanAccent),
				CreateLegendSwatch(_loc.T("Halo_Table_TempHeaderFormat", TempUnitSymbol), OrangeAccent)
			}
		};

		_graphView = new GraphicsView
		{
			Drawable = _chart,
			HeightRequest = GraphHeight
		};

		var panel = new VerticalStackLayout
		{
			Spacing = 4,
			Padding = new Thickness(10, 10, 10, 8),
			Children = { legend, _graphView }
		};

		return new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			BackgroundColor = LabCard,
			Content = panel
		};
	}

	static View CreateLegendSwatch(string label, Color color)
	{
		return new HorizontalStackLayout
		{
			Spacing = 6,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new BoxView { WidthRequest = 14, HeightRequest = 2, Color = color, VerticalOptions = LayoutOptions.Center },
				new Label
				{
					Text = label,
					FontSize = 11,
					TextColor = LabMuted,
					VerticalOptions = LayoutOptions.Center
				}
			}
		};
	}

	readonly record struct HaloReading(double Ph, double Mv, double Temp, DateTime Timestamp)
	{
		public string DateDisplay => Timestamp.ToString("yy-MM-dd, HH:mm:ss", CultureInfo.CurrentCulture);
	}

	/// <summary>Fixed Y-axis bounds + alarm thresholds for a heart-rate-style sparkline.</summary>
	readonly record struct SparklineBounds(
		double Min,
		double Max,
		double Midline,
		double DangerLow,
		double DangerHigh);

	/// <summary>
	/// ECG/heart-rate-monitor inspired mini chart for the live readings card.
	///
	/// Visual layers (back-to-front):
	///   1. Subtle horizontal grid lines.
	///   2. Faint midline at the channel's "neutral" value (pH 7, 0 mV, 25 °C).
	///   3. Faint dashed red lines at the low/high alarm thresholds.
	///   4. Plain polyline through the samples — each segment turns red if either
	///      endpoint is outside the alarm band, so an excursion is instantly visible.
	///
	/// Y-axis is fixed via <see cref="SparklineBounds"/>; values that exceed the range
	/// are clamped to the edge of the plot so off-scale conditions still draw a line.
	/// </summary>
	sealed class HaloHeartRateSparkline : IDrawable
	{
		readonly Func<IReadOnlyList<double>> _getData;
		readonly Func<Color> _getNormalColor;
		readonly Func<SparklineBounds> _getBounds;

		public HaloHeartRateSparkline(
			Func<IReadOnlyList<double>> getData,
			Func<Color> getNormalColor,
			Func<SparklineBounds> getBounds)
		{
			_getData = getData;
			_getNormalColor = getNormalColor;
			_getBounds = getBounds;
		}

		public void Draw(ICanvas canvas, RectF rect)
		{
			var data = _getData();
			if (data.Count == 0)
				return;

			var b = _getBounds();
			var range = Math.Max(0.0001, b.Max - b.Min);

			// Small inset so the line + stroke caps don't crop against the card border
			// at value extremes (top/bottom of the plot).
			const float verticalPad = 4f;
			const float sidePad = 6f;
			var plot = new RectF(rect.X + sidePad, rect.Y + verticalPad, rect.Width - sidePad * 2, rect.Height - verticalPad * 2);
			if (plot.Width <= 0 || plot.Height <= 0)
				return;

			var normalColor = _getNormalColor();
			var dangerColor = ThemeColors.LabDanger;

			DrawGrid(canvas, plot);
			DrawMidline(canvas, plot, b, range, normalColor);
			DrawThresholds(canvas, plot, b, range, dangerColor);

			var points = ComputePoints(data, plot, b, range);
			DrawSegments(canvas, data, points, b, normalColor, dangerColor);
		}

		static void DrawGrid(ICanvas canvas, RectF plot)
		{
			// Very subtle "graph paper" so the chart still feels like an instrument
			// readout without competing with the data line.
			canvas.StrokeColor = ThemeColors.LabBorder.MultiplyAlpha(0.18f);
			canvas.StrokeSize = 0.5f;
			canvas.StrokeDashPattern = null;
			for (var i = 1; i <= 3; i++)
			{
				var y = plot.Y + plot.Height * i / 4f;
				canvas.DrawLine(plot.X, y, plot.Right, y);
			}
		}

		static void DrawMidline(ICanvas canvas, RectF plot, SparklineBounds b, double range, Color normalColor)
		{
			var midRatio = (float)Math.Clamp((b.Midline - b.Min) / range, 0, 1);
			var midY = plot.Bottom - midRatio * plot.Height;
			canvas.StrokeColor = normalColor.MultiplyAlpha(0.35f);
			canvas.StrokeSize = 0.8f;
			canvas.StrokeDashPattern = null;
			canvas.DrawLine(plot.X, midY, plot.Right, midY);
		}

		static void DrawThresholds(ICanvas canvas, RectF plot, SparklineBounds b, double range, Color dangerColor)
		{
			canvas.StrokeColor = dangerColor.MultiplyAlpha(0.38f);
			canvas.StrokeSize = 0.6f;
			canvas.StrokeDashPattern = [3, 4];

			DrawHorizontalAt(canvas, plot, b.DangerLow, b.Min, range);
			DrawHorizontalAt(canvas, plot, b.DangerHigh, b.Min, range);

			canvas.StrokeDashPattern = null;
		}

		static PointF[] ComputePoints(IReadOnlyList<double> data, RectF plot, SparklineBounds b, double range)
		{
			var step = data.Count > 1 ? plot.Width / (data.Count - 1) : 0;
			var points = new PointF[data.Count];
			for (var i = 0; i < data.Count; i++)
			{
				var x = plot.X + i * step;
				var norm = (float)Math.Clamp((data[i] - b.Min) / range, 0, 1);
				var y = plot.Bottom - norm * plot.Height;
				points[i] = new PointF(x, y);
			}
			return points;
		}

		static void DrawSegments(ICanvas canvas, IReadOnlyList<double> data, PointF[] points, SparklineBounds b, Color normalColor, Color dangerColor)
		{
			canvas.StrokeSize = SparklineStroke;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.StrokeLineJoin = LineJoin.Round;

			for (var i = 1; i < points.Length; i++)
			{
				var inDanger = IsDanger(data[i], b) || IsDanger(data[i - 1], b);
				canvas.StrokeColor = inDanger ? dangerColor : normalColor;
				canvas.DrawLine(points[i - 1], points[i]);
			}
		}

		static bool IsDanger(double value, SparklineBounds b) => value < b.DangerLow || value > b.DangerHigh;

		static void DrawHorizontalAt(ICanvas canvas, RectF plot, double value, double min, double range)
		{
			var ratio = (float)Math.Clamp((value - min) / range, 0, 1);
			var y = plot.Bottom - ratio * plot.Height;
			canvas.DrawLine(plot.X, y, plot.Right, y);
		}
	}

	sealed class HaloChartDrawable : IDrawable
	{
		readonly Func<IReadOnlyList<HaloReading>> _getHistory;

		public bool Tagged { get; set; }
		public string PhAxisLabel { get; set; } = "pH";
		public string TempAxisLabel { get; set; } = "Temp";

		public HaloChartDrawable(Func<IReadOnlyList<HaloReading>> getHistory) => _getHistory = getHistory;

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			var history = _getHistory();
			var ordered = history.AsEnumerable().Reverse().ToArray();
			var take = Math.Min(36, ordered.Length);
			var slice = take == 0 ? Array.Empty<HaloReading>() : ordered[^take..];
			var ph = slice.Select(r => r.Ph).ToArray();
			var temp = slice.Select(r => r.Temp).ToArray();

			var plot = new RectF(48, 14, dirtyRect.Width - 100, dirtyRect.Height - 52);
			canvas.FillColor = ThemeColors.LabGraphPlotFill;
			canvas.FillRoundedRectangle(plot, 6);

			canvas.StrokeColor = ThemeColors.LabBorder;
			canvas.StrokeSize = 1f;
			const float phAxisMin = 4f;
			const float phAxisMax = 12f;
			for (var i = 0; i <= 8; i++)
			{
				var phVal = phAxisMin + i * (phAxisMax - phAxisMin) / 8f;
				var y = plot.Bottom - (phVal - phAxisMin) / (phAxisMax - phAxisMin) * plot.Height;
				canvas.DrawLine(plot.Left, y, plot.Right, y);
			}

			for (var i = 0; i <= 6; i++)
			{
				var t = i * 20;
				var y = plot.Bottom - (t / 120f) * plot.Height;
				canvas.DrawLine(plot.Right, y, plot.Right + 10, y);
			}

			if (ph.Length > 1)
				DrawSeries(canvas, plot, ph,
					PhSpanMin, PhSpanMax,
					PhDangerLow, PhDangerHigh,
					ThemeColors.LabMuted, ThemeColors.LabDanger,
					GraphLinePh);
			if (temp.Length > 1)
				DrawSeries(canvas, plot, temp,
					TempSpanMin, TempSpanMax,
					TempDangerLowC, TempDangerHighC,
					ThemeColors.LabMuted, ThemeColors.LabDanger,
					GraphLineTemp);

			if (Tagged)
			{
				canvas.StrokeColor = ThemeColors.LabEmerald;
				canvas.StrokeSize = 2;
				var x = plot.Left + plot.Width * 0.72f;
				canvas.DrawLine(x, plot.Top, x, plot.Bottom);
			}

			canvas.FontColor = ThemeColors.LabMuted;
			canvas.FontSize = 12;
			DrawRotatedAxisTitle(canvas, PhAxisLabel, 12, plot.Center.Y);
			DrawRotatedAxisTitle(canvas, TempAxisLabel, dirtyRect.Width - 12, plot.Center.Y);
		}

		static void DrawRotatedAxisTitle(ICanvas canvas, string title, float centerX, float centerY)
		{
			canvas.SaveState();
			canvas.Rotate(-90, centerX, centerY);
			canvas.DrawString(title, centerX - 40, centerY - 10, 80, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.RestoreState();
		}

		/// <summary>
		/// Renders a series with per-segment alarm coloring. A segment is drawn in
		/// <paramref name="dangerColor"/> when either endpoint is outside
		/// <c>[dangerLow, dangerHigh]</c>; otherwise it uses <paramref name="normalColor"/>.
		/// Y-axis is fixed by <paramref name="min"/>/<paramref name="max"/> so out-of-range
		/// samples pin to the plot edges instead of being silently rescaled.
		/// </summary>
		static void DrawSeries(
			ICanvas canvas,
			RectF plot,
			double[] values,
			double min,
			double max,
			double dangerLow,
			double dangerHigh,
			Color normalColor,
			Color dangerColor,
			float width)
		{
			var points = new PointF[values.Length];
			for (var i = 0; i < values.Length; i++)
			{
				var x = plot.Left + i * plot.Width / Math.Max(1, values.Length - 1);
				var normalized = (float)((values[i] - min) / (max - min));
				var y = plot.Bottom - Math.Clamp(normalized, 0, 1) * plot.Height;
				points[i] = new PointF(x, y);
			}

			canvas.StrokeSize = width;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.StrokeLineJoin = LineJoin.Round;

			for (var i = 1; i < points.Length; i++)
			{
				var inDanger = values[i] < dangerLow || values[i] > dangerHigh
					|| values[i - 1] < dangerLow || values[i - 1] > dangerHigh;
				canvas.StrokeColor = inDanger ? dangerColor : normalColor;
				canvas.DrawLine(points[i - 1], points[i]);
			}
		}
	}
}
