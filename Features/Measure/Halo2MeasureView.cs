using System.Globalization;
using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Core.Helpers;

namespace HannaUIDemo.Features.Measure;

public sealed class Halo2MeasureView : ContentView
{
	readonly Halo2MeasureViewModel _viewModel;
	enum HaloMode { Table, Graph }

	readonly VerticalStackLayout _root = new() { Spacing = 0 };
	readonly HaloChartDrawable _chart;
	readonly List<HaloReading> _history = [];
	readonly List<double> _sparkPh = [];
	readonly List<double> _sparkMv = [];
	readonly List<double> _sparkTemp = [];
	readonly Random _random = new();

	IDispatcherTimer? _liveTimer;
	HaloMode _mode = HaloMode.Table;
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

	const double PhSpanMin = 4;
	const double PhSpanMax = 12;
	const double TempSpanMin = 0;
	const double TempSpanMax = 120;

	static readonly Color LabCanvas = Color.FromArgb("#0A0F1C");
	static readonly Color LabCard = Color.FromArgb("#18181B");
	static readonly Color LabBorder = Color.FromArgb("#FFFFFF").MultiplyAlpha(0.10f);
	static readonly Color LabMuted = Color.FromArgb("#A1A1AA");
	static readonly Color CyanAccent = Color.FromArgb("#22D3EE");
	static readonly Color OrangeAccent = Color.FromArgb("#FB923C");
	static readonly Color Emerald = Color.FromArgb("#34D399");
	static readonly Color EmeraldMuted = Color.FromArgb("#34D399").MultiplyAlpha(0.12f);

	public Halo2MeasureView()
	{
		_viewModel = AppServices.Get<Halo2MeasureViewModel>();
		BindingContext = _viewModel;
		_chart = new HaloChartDrawable(() => _history);
		SyncSettingsFromPreferences();
		_showMvPrimary = Halo2Preferences.GetPrimaryDisplay().Equals("mv", StringComparison.OrdinalIgnoreCase);
		_useFahrenheit = _viewModel.UseFahrenheit;
		SetDynamicResource(BackgroundColorProperty, "PageBackground");
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
			_phStatusLabel.Text = showMvOnPrimary ? "ORP / glass" : phStatus.Label;
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
				_stabilityPillText.Text = "STABLE";
				_stabilityPillText.TextColor = Emerald;
				_stabilityPill.BackgroundColor = EmeraldMuted;
				_stabilityPill.Stroke = Emerald.MultiplyAlpha(0.35f);
				_stabilityDot.Color = Emerald;
			}
			else
			{
				_stabilityPillText.Text = "DRIFTING";
				_stabilityPillText.TextColor = Color.FromArgb("#FBBF24");
				_stabilityPill.BackgroundColor = Color.FromArgb("#FBBF24").MultiplyAlpha(0.12f);
				_stabilityPill.Stroke = Color.FromArgb("#FBBF24").MultiplyAlpha(0.35f);
				_stabilityDot.Color = Color.FromArgb("#FBBF24");
			}
		}

		_phSparkline?.Invalidate();
		_tempSparkline?.Invalidate();
	}

	static (string Label, Color Color) GetPhStatus(double ph) => ph switch
	{
		< 5.5 => ("STRONG ACIDIC", Color.FromArgb("#EF4444")),
		< 6.5 => ("ACIDIC", Color.FromArgb("#F97316")),
		< 7.5 => ("NEUTRAL", Color.FromArgb("#22C55E")),
		< 9.0 => ("BASIC", Color.FromArgb("#A855F7")),
		_ => ("STRONG ALKALINE", Color.FromArgb("#C026D3"))
	};

	static (string Label, Color Color) GetTempStatus(double temp) => temp switch
	{
		> 80 => ("CRITICAL", Color.FromArgb("#EF4444")),
		> 60 => ("HIGH", Color.FromArgb("#F97316")),
		_ => ("OPTIMAL", Color.FromArgb("#22C55E"))
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
			? Color.FromArgb("#27272A").MultiplyAlpha(0.5f)
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
				_mode == HaloMode.Graph ? BuildGraph() : BuildTable()
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
			FontSize = 12,
			TextColor = LabMuted,
			HorizontalTextAlignment = TextAlignment.Center,
			TextDecorations = TextDecorations.Underline,
			Margin = new Thickness(0, 4, 0, 0)
		};
	}



	const string Halo2DeviceIcon = "halo2_device_icon.png";
	const string Halo2DeviceName = "HI12322 • Probe 2";

	static VerticalStackLayout CreateMetricGroup(string caption, View glyph, string value, Color valueColor)
	{
		var row = new HorizontalStackLayout
		{
			Spacing = 10,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				glyph,
				new Label
				{
					Text = value,
					FontSize = 15,
					FontAttributes = FontAttributes.Bold,
					TextColor = valueColor,
					VerticalOptions = LayoutOptions.Center
				}
			}
		};

		return new VerticalStackLayout
		{
			Spacing = 4,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = caption, FontSize = 11, TextColor = LabMuted, CharacterSpacing = 0.5 },
				row
			}
		};
	}

	Border CreateDisconnectButton()
	{
		var label = new Label
		{
			Text = "Disconnect",
			FontSize = 13,
			FontAttributes = FontAttributes.Bold,
			TextColor = Color.FromArgb("#FCA5A5"),
			VerticalOptions = LayoutOptions.Center
		};
		var button = new Border
		{
			Padding = new Thickness(12, 6),
			BackgroundColor = Color.FromArgb("#EF4444").MultiplyAlpha(0.12f),
			Stroke = Color.FromArgb("#EF4444").MultiplyAlpha(0.35f),
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
		_viewModel.StabilityLabel = _isStable ? "STABLE" : "DRIFTING";
	}

	static View CreateBatteryGlyph(int percent)
	{
		percent = Math.Clamp(percent, 0, 100);
		var fill = percent switch
		{
			<= 15 => Color.FromArgb("#EF4444"),
			<= 35 => Color.FromArgb("#FBBF24"),
			_ => Emerald
		};

		var shell = new Border
		{
			WidthRequest = 22,
			HeightRequest = 12,
			Padding = 2,
			Stroke = LabMuted,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 2 },
			VerticalOptions = LayoutOptions.Center,
			Content = new BoxView
			{
				Color = fill,
				WidthRequest = Math.Max(2, 16 * percent / 100.0),
				HeightRequest = 6,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center
			}
		};

		return new HorizontalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				shell,
				new BoxView { WidthRequest = 2, HeightRequest = 5, Color = LabMuted, VerticalOptions = LayoutOptions.Center }
			}
		};
	}

	static View CreateConditionGlyph(int percent)
	{
		var color = percent >= 80 ? Emerald : percent >= 50 ? Color.FromArgb("#FBBF24") : Color.FromArgb("#EF4444");
		return new Border
		{
			WidthRequest = 18,
			HeightRequest = 18,
			StrokeThickness = 0,
			BackgroundColor = color.MultiplyAlpha(0.18f),
			StrokeShape = new RoundRectangle { CornerRadius = 9 },
			Content = new Label
			{
				Text = "\u2714",
				FontSize = 11,
				TextColor = color,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
	}

	int GetProbeConditionPercent() => _tagged ? 50 : 94;

	Border BuildLabHeader()
	{
		var deviceIcon = new Image
		{
			Source = Halo2DeviceIcon,
			WidthRequest = 40,
			HeightRequest = 40,
			Aspect = Aspect.AspectFit,
			VerticalOptions = LayoutOptions.Center
		};
		var deviceName = new Label
		{
			Text = Halo2DeviceName,
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		var disconnect = CreateDisconnectButton();

		var deviceRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 12,
			Padding = new Thickness(0, 0, 0, 12),
			Children = { deviceIcon, deviceName, disconnect }
		};
		Grid.SetColumn(deviceName, 1);
		Grid.SetColumn(disconnect, 2);

		var conditionPercent = GetProbeConditionPercent();
		var batteryColor = _batteryPercent <= 35 ? Color.FromArgb("#FBBF24") : Colors.White;
		var probeSummaryColor = conditionPercent >= 80 ? Colors.White : conditionPercent >= 50 ? Color.FromArgb("#FBBF24") : Color.FromArgb("#FCA5A5");

		var probeTap = new TapGestureRecognizer();
		probeTap.Tapped += (_, _) => { _tagged = !_tagged; Rebuild(); };

		var batteryGroup = CreateMetricGroup(
			"Battery",
			CreateBatteryGlyph(_batteryPercent),
			$"{_batteryPercent}%",
			batteryColor);

		var probeGroup = CreateMetricGroup(
			"Probe Condition",
			CreateConditionGlyph(conditionPercent),
			_tagged ? "Tagged • 50%" : $"Excellent • {conditionPercent}%",
			probeSummaryColor);
		probeGroup.GestureRecognizers.Add(probeTap);

		var metricsDivider = new BoxView
		{
			WidthRequest = 1,
			Color = LabBorder,
			Margin = new Thickness(0, 2),
			VerticalOptions = LayoutOptions.Fill
		};

		var metricsStrip = new Grid
		{
			Padding = new Thickness(0, 12, 0, 0),
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(1),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12,
			Children = { batteryGroup, metricsDivider, probeGroup }
		};
		Grid.SetColumn(metricsDivider, 1);
		Grid.SetColumn(probeGroup, 2);

		var metricsPanel = new VerticalStackLayout
		{
			Spacing = 0,
			Padding = new Thickness(14, 12),
			Children = { deviceRow, metricsStrip }
		};

		var metricsChrome = new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			BackgroundColor = LabCard.MultiplyAlpha(0.65f),
			Content = metricsPanel
		};

		var shell = new VerticalStackLayout
		{
			Spacing = 0,
			Padding = new Thickness(20, 12, 20, 14),
			Children = { metricsChrome }
		};

		var header = new Border
		{
			StrokeThickness = 0,
			BackgroundColor = LabCanvas,
			StrokeShape = new RoundRectangle { CornerRadius = 0 },
			Content = shell
		};

		var divider = new BoxView { HeightRequest = 1, Color = LabBorder, HorizontalOptions = LayoutOptions.Fill };
		return new Border
		{
			StrokeThickness = 0,
			Content = new VerticalStackLayout { Spacing = 0, Children = { header, divider } }
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

		var settings = ActionButton("\u2699", "Settings", () => _ = OpenHaloSettingsAsync(), Colors.White);

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
			FontSize = 52,
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
			HeightRequest = 44,
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
			Spacing = 2,
			HorizontalOptions = LayoutOptions.Fill,
			Children =
			{
				_primaryChannelLabel,
				_phPrimaryValue,
				_phStatusLabel,
				_phSparkline,
				_switchChannelLabel
			}
		};

		_tempValueLabel = new Label
		{
			FontSize = 52,
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
			HeightRequest = 44,
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
			Spacing = 2,
			HorizontalOptions = LayoutOptions.Fill,
			Children =
			{
				_tempUnitHeaderLabel,
				_tempValueLabel,
				_tempStatusLabel,
				_tempSparkline,
				_switchTempUnitLabel
			}
		};

		var midDivider = new BoxView { WidthRequest = 1, Color = LabBorder, Margin = new Thickness(0, 8) };

		var dualGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(1), new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 16,
			Padding = new Thickness(20, 20, 20, 20),
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
				new GradientStop(Color.FromArgb("#27272A"), 0),
				new GradientStop(Color.FromArgb("#09090B"), 1)
			}
		};

		var panel = new VerticalStackLayout { Spacing = 0, Children = { titleSection, dualGrid } };

		return new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 24 },
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

	Border ActionButton(string glyph, string text, Action action, Color color)
	{
		var button = new Border
		{
			WidthRequest = 36,
			HeightRequest = 36,
			BackgroundColor = Colors.White.MultiplyAlpha(0.05f),
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = new Label { Text = glyph, FontSize = 18, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button, text);
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
		grid.Children.Add(ModeChip("Table", HaloMode.Table, "\u2637"));
		var graph = ModeChip("Graph", HaloMode.Graph, "\u223F");
		grid.Children.Add(graph);
		Grid.SetColumn(graph, 1);
		var calibrate = ModeAction("Calibrate", "\u25F0", async () =>
		{
			if (Shell.Current is not null)
				await Shell.Current.GoToAsync(Halo2Routes.Calibration);
		});
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

	Border ModeChip(string text, HaloMode mode, string glyph)
	{
		var active = _mode == mode;
		var chip = new Border
		{
			Padding = new Thickness(12, 10),
			BackgroundColor = active ? LabCanvas : Colors.Transparent,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new HorizontalStackLayout
			{
				Spacing = 6,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label { Text = glyph, TextColor = active ? CyanAccent : LabMuted },
					new Label { Text = text, FontAttributes = active ? FontAttributes.Bold : FontAttributes.None, TextColor = active ? Colors.White : LabMuted }
				}
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			_mode = mode;
			Rebuild();
		};
		chip.GestureRecognizers.Add(tap);
		return chip;
	}

	static Border ModeAction(string text, string glyph, Func<Task> action)
	{
		var chip = new Border
		{
			Padding = new Thickness(12, 10),
			BackgroundColor = Colors.Transparent,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new HorizontalStackLayout
			{
				Spacing = 6,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label { Text = glyph, TextColor = CyanAccent },
					new Label { Text = text, TextColor = Colors.White }
				}
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await action();
		chip.GestureRecognizers.Add(tap);
		return chip;
	}

	Border BuildTable()
	{
		var stack = new VerticalStackLayout { Spacing = 0 };
		var header = new Grid
		{
			ColumnDefinitions = TableColumns(),
			Padding = new Thickness(10, 12),
			BackgroundColor = Color.FromArgb("#0F172A")
		};
		AddCell(header, "pH", 0, true, Colors.White);
		AddCell(header, "mV", 1, true, Colors.White);
		AddCell(header, $"Temp ({TempUnitSymbol})", 2, true, Colors.White);
		AddCell(header, "Timestamp", 3, true, Colors.White);
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
			TextColor = textColor ?? Colors.White,
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
		_graphView = new GraphicsView
		{
			Drawable = _chart,
			HeightRequest = 420
		};
		return new Border
		{
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			BackgroundColor = LabCard,
			Content = _graphView,
			Padding = 8
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
			canvas.StrokeSize = 2.5f;
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

			var plot = new RectF(54, 18, dirtyRect.Width - 116, dirtyRect.Height - 70);
			canvas.FillColor = Color.FromArgb("#09090B");
			canvas.FillRoundedRectangle(plot, 6);

			canvas.StrokeColor = LabBorder;
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
				DrawSeries(canvas, plot, ph, PhSpanMin, PhSpanMax, CyanAccent, 2.6f);
			if (temp.Length > 1)
				DrawSeries(canvas, plot, temp, TempSpanMin, TempSpanMax, OrangeAccent, 2.2f);

			if (Tagged)
			{
				canvas.StrokeColor = Emerald;
				canvas.StrokeSize = 4;
				var x = plot.Left + plot.Width * 0.72f;
				canvas.DrawLine(x, plot.Top, x, plot.Bottom);
			}

			canvas.FontColor = LabMuted;
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
			canvas.DrawPath(path);
		}
	}
}
