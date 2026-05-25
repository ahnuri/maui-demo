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
///   BuildLabHeader        : Device card (probe-condition hero icon + name/battery/% rows)
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
	const int SparkPointCount = 25;
	const float SparklineHeight = 26;
	const float SparklineStroke = 1.4f;
	const float GraphLinePh = 1.8f;
	const float GraphLineTemp = 1.5f;
	const double GraphHeight = 280;

	const double PhSpanMin = 4;
	const double PhSpanMax = 12;
	const double TempSpanMin = 0;
	const double TempSpanMax = 120;

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
		BindingContext = _viewModel;
		_chart = new HaloChartDrawable(() => _history);
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

	void AppendLiveSample()
	{
		_lastPh = Math.Round(Math.Clamp(_lastPh + (_random.NextDouble() - 0.5) * 0.07, PhSpanMin, PhSpanMax), 2);
		_lastTemp = Math.Round(Math.Clamp(_lastTemp + (_random.NextDouble() - 0.5) * 0.45, TempSpanMin, TempSpanMax), 1);
		_lastMv = Math.Round(_lastPh * -59.16 - 400 + (_random.NextDouble() * 4 - 2), 1);
		_isStable = _random.NextDouble() > 0.15;

		PushSpark(_sparkPh, _lastPh);
		PushSpark(_sparkMv, _lastMv);
		PushSpark(_sparkTemp, _lastTemp);

		var stamp = DateTime.Now;
		_history.Insert(0, new HaloReading(_lastPh, _lastMv, _lastTemp, stamp));
		while (_history.Count > MaxHistory)
			_history.RemoveAt(_history.Count - 1);
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

		if (_primaryChannelLabel is not null)
		{
			_primaryChannelLabel.Text = showMvOnPrimary ? "mV" : "pH";
			_primaryChannelLabel.TextColor = showMvOnPrimary ? CyanAccent : CyanAccent;
		}

		if (_phPrimaryValue is not null)
		{
			_phPrimaryValue.Text = showMvOnPrimary
				? _lastMv.ToString("0.0", c)
				: _lastPh.ToString("0.00", c);
			_phPrimaryValue.TextColor = showMvOnPrimary ? CyanAccent : phStatus.Color;
		}

		if (_phStatusLabel is not null)
		{
			_phStatusLabel.Text = showMvOnPrimary ? "ORP · glass electrode" : phStatus.Label;
			_phStatusLabel.TextColor = showMvOnPrimary ? CyanAccent.MultiplyAlpha(0.85f) : phStatus.Color;
		}

		if (_tempUnitHeaderLabel is not null)
		{
			_tempUnitHeaderLabel.Text = TempUnitSymbol;
			_tempUnitHeaderLabel.TextColor = OrangeAccent;
		}

		if (_tempValueLabel is not null)
		{
			_tempValueLabel.Text = displayTemp.ToString("0.0", c);
			_tempValueLabel.TextColor = tempStatus.Color;
		}

		if (_tempStatusLabel is not null)
		{
			_tempStatusLabel.Text = $"{tempStatus.Label} • ATC";
			_tempStatusLabel.TextColor = tempStatus.Color;
		}

		if (_switchChannelLabel is not null)
		{
			_switchChannelLabel.IsVisible = allowSwitch;
			if (allowSwitch)
				_switchChannelLabel.Text = showMvOnPrimary ? "Switch to pH" : "Switch to mV";
		}

		if (_switchTempUnitLabel is not null)
			_switchTempUnitLabel.Text = _useFahrenheit ? "Switch to °C" : "Switch to °F";

		if (_stabilityPillText is not null && _stabilityPill is not null && _stabilityDot is not null)
		{
			if (_isStable)
			{
				_stabilityPillText.Text = "Stable";
				_stabilityPillText.TextColor = Emerald;
				_stabilityPill.BackgroundColor = EmeraldMuted;
				_stabilityPill.Stroke = Emerald.MultiplyAlpha(0.35f);
				_stabilityDot.Color = Emerald;
			}
			else
			{
				_stabilityPillText.Text = "Drifting";
				_stabilityPillText.TextColor = LabWarning;
				_stabilityPill.BackgroundColor = LabWarningMuted;
				_stabilityPill.Stroke = LabWarning.MultiplyAlpha(0.35f);
				_stabilityDot.Color = LabWarning;
			}
		}

		_phSparkline?.Invalidate();
		_tempSparkline?.Invalidate();
	}

	static (string Label, Color Color) GetPhStatus(double ph) => ph switch
	{
		< 5.5 => ("Strong acidic", ThemeColors.LabPhAcidic),
		< 6.5 => ("Acidic", ThemeColors.LabPhAcidicMid),
		< 7.5 => ("Neutral", ThemeColors.LabPhNeutral),
		< 9.0 => ("Basic", ThemeColors.LabPhBasic),
		_ => ("Strong alkaline", ThemeColors.LabPhAlkaline)
	};

	static (string Label, Color Color) GetTempStatus(double temp) => temp switch
	{
		> 80 => ("Critical", ThemeColors.LabPhAcidic),
		> 60 => ("High", ThemeColors.LabPhAcidicMid),
		_ => ("Optimal", ThemeColors.LabPhNeutral)
	};

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
		for (var i = 0; i < SparkPointCount; i++)
		{
			_sparkPh.Add(_lastPh);
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

	static Label CreateTrendCaptionLabel() =>
		new()
		{
			Text = "Trend",
			FontSize = 10,
			FontAttributes = FontAttributes.Bold,
			TextColor = LabMuted,
			CharacterSpacing = 0.6,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(0, 6, 0, 2)
		};



	const string Halo2DeviceName = "HI12322 Probe 2";

	static Label CreateDeviceCardValueLabel(string text, Color color, double fontSize = 15, FontAttributes fontAttributes = FontAttributes.Bold) =>
		new()
		{
			Text = text,
			FontSize = fontSize,
			FontAttributes = fontAttributes,
			TextColor = color,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};

	static Label CreateDeviceCardCaptionLabel(string text) =>
		new()
		{
			Text = " ",
			FontSize = 11,
			FontAttributes = FontAttributes.Bold,
			TextColor = LabMuted,
			CharacterSpacing = 0.4
		};

	Border CreateDisconnectButton()
	{
		var label = new Label
		{
			Text = "Disconnect",
			FontSize = 13,
			FontAttributes = FontAttributes.Bold,
			TextColor = LabDangerSoft,
			VerticalOptions = LayoutOptions.Center
		};
		var button = new Border
		{
			Padding = new Thickness(12, 6),
			BackgroundColor = LabDangerMuted,
			Stroke = LabDanger.MultiplyAlpha(0.35f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			VerticalOptions = LayoutOptions.Center,
			Content = label
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) =>
		{
			if (ViewNavigation.FindHostPage(this) is MeasureTabPage measureTab)
				await measureTab.DisconnectAndOpenDevicesAsync();
		};
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button, "Disconnect device");
		return button;
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
		_viewModel.StabilityLabel = _isStable ? "Stable" : "Drifting";
	}

	int GetProbeConditionPercent() => _tagged ? 50 : 94;

	Border BuildLabHeader()
	{
		var conditionPercent = GetProbeConditionPercent();
		var batteryColor = _batteryPercent <= 35 ? LabWarning : LabPrimaryText;
		var probeSummaryColor = conditionPercent >= 80 ? LabPrimaryText : conditionPercent >= 50 ? LabWarning : LabDangerSoft;

		var probeIcon = Halo2ProbeConditionIcons.CreateGlyph(conditionPercent, width: 32, height: 80);
		probeIcon.VerticalOptions = LayoutOptions.Center;

		var deviceBlock = new VerticalStackLayout
		{
			Spacing = 0,
			Children =
			{
				//CreateDeviceCardCaptionLabel("Device"),
				CreateDeviceCardValueLabel(Halo2DeviceName, LabPrimaryText, fontSize: 20)
			}
		};

		var batteryLabel = CreateDeviceCardValueLabel($"Battery · {_batteryPercent}%", batteryColor);
		var probeText = _tagged ? "50%" : $"Excellent · {conditionPercent}%";
		var probeBlock = new VerticalStackLayout
		{
			Spacing = 0,
			Children =
			{
				CreateDeviceCardValueLabel(probeText, probeSummaryColor),
				CreateDeviceCardCaptionLabel("Probe Condition")
			}
		};

		var disconnect = CreateDisconnectButton();
		var deviceRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 12,
			Children = { deviceBlock, disconnect }
		};
		Grid.SetColumn(disconnect, 1);

		var textRows = new VerticalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children = { deviceRow, batteryLabel, probeBlock }
		};

		var probeTap = new TapGestureRecognizer();
		probeTap.Tapped += (_, _) => { _tagged = !_tagged; Rebuild(); };
		probeIcon.GestureRecognizers.Add(probeTap);
		probeBlock.GestureRecognizers.Add(probeTap);

		var cardBody = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 14,
			Children = { probeIcon, textRows }
		};
		Grid.SetColumn(textRows, 1);

		var metricsChrome = new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			BackgroundColor = LabCard.MultiplyAlpha(0.65f),
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(14, 12),
				Children = { cardBody }
			}
		};

		var shell = new VerticalStackLayout
		{
			Spacing = 0,
			Padding = new Thickness(20, 12, 20, 0),
			Children = { metricsChrome }
		};

		var header = new Border
		{
			StrokeThickness = 0,
			BackgroundColor = LabCanvas,
			StrokeShape = new RoundRectangle { CornerRadius = 0 },
			Content = shell
		};

		return header;
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

		var settings = IconActionButton(HaloMeasureModeIconKind.Settings, "Settings", () => _ = OpenHaloSettingsAsync());

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
			Drawable = new HaloSparklineDrawable(GetPrimarySparkData, () =>
			{
				var pref = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
				var showMv = (pref is "ph" or "mv") && _showMvPrimary;
				return showMv ? CyanAccent : GetPhStatus(_lastPh).Color;
			}),
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
			Drawable = new HaloSparklineDrawable(GetTempSparkData, () => OrangeAccent),
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
		grid.Children.Add(ModeChip("Data", HaloMode.Table, HaloMeasureModeIconKind.Table));
		var graph = ModeChip("Trends", HaloMode.Graph, HaloMeasureModeIconKind.Chart);
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
						Text = "Calibrate",
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
		AddCell(header, "pH", 0, true, LabTableHeaderText);
		AddCell(header, "mV", 1, true, LabTableHeaderText);
		AddCell(header, $"Temp ({TempUnitSymbol})", 2, true, LabTableHeaderText);
		AddCell(header, "Timestamp", 3, true, LabTableHeaderText);
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
				CreateLegendSwatch("pH", CyanAccent),
				CreateLegendSwatch($"Temp ({TempUnitSymbol})", OrangeAccent)
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

	sealed class HaloSparklineDrawable : IDrawable
	{
		readonly Func<IReadOnlyList<double>> _getData;
		readonly Func<Color> _getColor;

		public HaloSparklineDrawable(Func<IReadOnlyList<double>> getData, Func<Color> getColor)
		{
			_getData = getData;
			_getColor = getColor;
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			var data = _getData();
			if (data.Count < 2)
				return;

			var min = data.Min();
			var max = data.Max();
			var range = max - min;
			if (range < 0.0001)
				range = 1;

			var path = new PathF();
			for (var i = 0; i < data.Count; i++)
			{
				var x = dirtyRect.Left + i * dirtyRect.Width / Math.Max(1, data.Count - 1);
				var norm = (float)((data[i] - min) / range);
				var y = dirtyRect.Bottom - Math.Clamp(norm, 0, 1) * dirtyRect.Height;
				if (i == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);
			}

			var color = _getColor();
			canvas.StrokeColor = color;
			canvas.StrokeSize = SparklineStroke;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.StrokeLineJoin = LineJoin.Round;
			canvas.DrawPath(path);
		}
	}

	sealed class HaloChartDrawable : IDrawable
	{
		readonly Func<IReadOnlyList<HaloReading>> _getHistory;

		public bool Tagged { get; set; }

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
				DrawSeries(canvas, plot, ph, PhSpanMin, PhSpanMax, ThemeColors.LabAccentCyan, GraphLinePh);
			if (temp.Length > 1)
				DrawSeries(canvas, plot, temp, TempSpanMin, TempSpanMax, ThemeColors.LabAccentOrange, GraphLineTemp);

			if (Tagged)
			{
				canvas.StrokeColor = ThemeColors.LabEmerald;
				canvas.StrokeSize = 2;
				var x = plot.Left + plot.Width * 0.72f;
				canvas.DrawLine(x, plot.Top, x, plot.Bottom);
			}

			canvas.FontColor = ThemeColors.LabMuted;
			canvas.FontSize = 12;
			DrawRotatedAxisTitle(canvas, "pH", 12, plot.Center.Y);
			DrawRotatedAxisTitle(canvas, "Temp", dirtyRect.Width - 12, plot.Center.Y);
		}

		static void DrawRotatedAxisTitle(ICanvas canvas, string title, float centerX, float centerY)
		{
			canvas.SaveState();
			canvas.Rotate(-90, centerX, centerY);
			canvas.DrawString(title, centerX - 40, centerY - 10, 80, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.RestoreState();
		}

		static void DrawSeries(ICanvas canvas, RectF plot, double[] values, double min, double max, Color color, float width)
		{
			var path = new PathF();
			for (var i = 0; i < values.Length; i++)
			{
				var x = plot.Left + i * plot.Width / Math.Max(1, values.Length - 1);
				var normalized = (float)((values[i] - min) / (max - min));
				var y = plot.Bottom - Math.Clamp(normalized, 0, 1) * plot.Height;
				if (i == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);
			}

			canvas.StrokeColor = color;
			canvas.StrokeSize = width;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.StrokeLineJoin = LineJoin.Round;
			canvas.DrawPath(path);
		}
	}
}
