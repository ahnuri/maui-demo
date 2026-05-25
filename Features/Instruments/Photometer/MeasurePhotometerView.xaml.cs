using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Features.Measure;
using HannaUIDemo;
using Microsoft.Extensions.DependencyInjection;
using PhotometerState = HannaUIDemo.Features.Instruments.Photometer.PhotometerMeasureViewModel.MeasureState;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>
/// Photometer measurement UI (HI97115 demo). Presentation-heavy view; state machine and
/// <see cref="PhotometerMeasureViewModel"/> hold navigation commands — extract further into VM for production BLE.
/// </summary>
public partial class MeasurePhotometerView : ContentView
{
	enum MethodStatus { Pending, Active, Done }

	readonly PhotometerMeasureViewModel _viewModel;

	class MethodItem
	{
		public string Title { get; set; } = "";
		public string Unit { get; set; } = "";
		public string Value { get; set; } = "";
		public MethodStatus Status { get; set; }
	}

	List<MethodItem> _selectedMethods = [];
	CancellationTokenSource? _measurementLoopCts;
	const int MeasurementStepDelayMs = 2400;

	static readonly Color[] InitialPalette =
	[
		Color.FromRgb(14, 165, 198),
		Color.FromRgb(99, 102, 241),
		Color.FromRgb(236, 72, 153),
		Color.FromRgb(34, 197, 94),
		Color.FromRgb(245, 158, 11),
		Color.FromRgb(168, 85, 247),
		Color.FromRgb(239, 68, 68),
		Color.FromRgb(20, 184, 166),
	];

	static string MethodInitials(string title)
	{
		var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length >= 2)
			return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
		if (parts.Length == 1 && parts[0].Length >= 2)
			return parts[0][..2].ToUpperInvariant();
		return parts.Length == 1 ? parts[0].ToUpperInvariant() : "?";
	}

	static Color InitialDiskBg(string methodTitle) =>
		InitialPalette[Math.Abs(methodTitle.GetHashCode(StringComparison.Ordinal) % InitialPalette.Length)].MultiplyAlpha(0.18f);

	static Color InitialDiskFg(string methodTitle) =>
		InitialPalette[Math.Abs(methodTitle.GetHashCode(StringComparison.Ordinal) % InitialPalette.Length)];

	static Image AssetIcon(string fileBase, double size = 22) => new()
	{
		Source = fileBase,
		WidthRequest = size,
		HeightRequest = size,
		Aspect = Aspect.AspectFit,
		HorizontalOptions = LayoutOptions.Center,
		VerticalOptions = LayoutOptions.Center
	};

	/// <summary>Connected HI97115 from the Devices screen (demo catalog until real BLE).</summary>
	static DeviceListItem? TryGetConnectedPhotometer()
	{
		try
		{
			return AppServices.Get<DeviceViewModel>()
				.ConnectedDevices.FirstOrDefault(d => d.InstrumentKind == InstrumentKind.Photometer);
		}
		catch
		{
			return null;
		}
	}

	static string GetPhotometerHeaderName(DeviceListItem? device)
	{
		if (!string.IsNullOrWhiteSpace(device?.Serial))
			return $"HI97115 · {device.Serial}";
		return "HI97115";
	}

	static View BuildPhotometerBatteryGlyph(int percent, bool known = true)
	{
		if (!known)
			percent = 0;
		else
			percent = Math.Clamp(percent, 0, 100);

		var fill = !known
			? ThemeColors.OnSurfaceVariant.MultiplyAlpha(0.2f)
			: percent switch
			{
				<= 15 => AppConstants.Error,
				<= 35 => Color.FromRgb(234, 88, 12),
				_ => AppConstants.Success
			};
		var stroke = ThemeColors.OnSurfaceVariant.MultiplyAlpha(0.55f);
		var innerW = known ? Math.Max(2.0, 16.0 * percent / 100.0) : 0;

		var shell = new Border
		{
			WidthRequest = 22,
			HeightRequest = 12,
			Padding = 2,
			Stroke = stroke,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 2 },
			VerticalOptions = LayoutOptions.Center,
			Content = new BoxView
			{
				Color = fill,
				WidthRequest = innerW,
				HeightRequest = 6,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center
			}
		};
		var nub = new BoxView { WidthRequest = 2, HeightRequest = 5, Color = stroke, VerticalOptions = LayoutOptions.Center };
		return new HorizontalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { shell, nub } };
	}

	static VerticalStackLayout CreatePhotometerMetricGroup(string caption, View glyph, string value, Color valueColor)
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
				new Label
				{
					Text = caption,
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceMuted,
					CharacterSpacing = 0.5
				},
				row
			}
		};
	}

	static VerticalStackLayout CreatePhotometerTextMetric(string caption, string value, Color valueColor, bool showChevron = false)
	{
		var valueRow = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
		valueRow.Children.Add(new Label
		{
			Text = value,
			FontSize = 15,
			FontAttributes = FontAttributes.Bold,
			TextColor = valueColor,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		});
		if (showChevron)
		{
			valueRow.Children.Add(new Label
			{
				Text = "\u203A",
				FontSize = 18,
				TextColor = AppConstants.Primary,
				VerticalOptions = LayoutOptions.Center
			});
		}

		return new VerticalStackLayout
		{
			Spacing = 4,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = caption,
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceMuted,
					CharacterSpacing = 0.5
				},
				valueRow
			}
		};
	}

	Border CreatePhotometerBeginButton()
	{
		var ctaChevron = new Label
		{
			Text = "\u203A",
			FontSize = 20,
			TextColor = Colors.White.MultiplyAlpha(0.9f),
			VerticalOptions = LayoutOptions.Center
		};
		var ctaRow = new Grid
		{
			HeightRequest = 50,
			Padding = new Thickness(16, 0),
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto))
		};
		ctaRow.Children.Add(new Label
		{
			Text = "Begin measurement",
			FontAttributes = FontAttributes.Bold,
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			FontSize = 16,
			TextColor = Colors.White,
			//VerticalOptions = LayoutOptions.Center
		});
		//ctaRow.Children.Add(ctaChevron);
		//Microsoft.Maui.Controls.Grid.SetColumn(ctaChevron, 1);

		var button = new Border
		{
			BackgroundColor = AppConstants.Primary,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = (float)AppConstants.RadiusButton },
			Content = ctaRow,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(AppConstants.Primary.MultiplyAlpha(0.3f)),
				Offset = new Point(0, 3),
				Radius = 8,
				Opacity = 0.8f
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => BeginMeasurement();
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button,
			$"Begin measurement for {_viewModel.SelectedTankDisplay}. Opens the full method queue.");
		return button;
	}

	Border CreatePhotometerDisconnectButton()
	{
		var label = new Label
		{
			Text = "Disconnect",
			FontSize = 13,
			FontAttributes = FontAttributes.Bold,
			TextColor = ThemeColors.LabDangerSoft,
			VerticalOptions = LayoutOptions.Center
		};
		var button = new Border
		{
			Padding = new Thickness(12, 6),
			BackgroundColor = ThemeColors.LabDangerMuted,
			Stroke = ThemeColors.LabDanger.MultiplyAlpha(0.35f),
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
		SemanticProperties.SetDescription(button, "Disconnect photometer");
		return button;
	}

	/// <summary>Device card: settings, name, disconnect; battery and active tank; begin measurement on overview.</summary>
	Border BuildPhotometerDeviceHeader()
	{
		var device = TryGetConnectedPhotometer();
		var battNullable = device?.BatteryPercent;
		var battKnown = battNullable is int b && b >= 0 && b <= 100;
		var battPct = battKnown ? Math.Clamp(battNullable!.Value, 0, 100) : 0;

		var settingsIcon = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.1f),
			StrokeThickness = 0,
			Content = AssetIcon("app_settings_icon", 22),
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			VerticalOptions = LayoutOptions.Center
		};
		var settingsIconTap = new TapGestureRecognizer();
		settingsIconTap.Tapped += async (_, _) => await OpenSettings();
		settingsIcon.GestureRecognizers.Add(settingsIconTap);
		SemanticProperties.SetDescription(settingsIcon, "Photometer instrument settings");

		var deviceName = new Label
		{
			Text = GetPhotometerHeaderName(device),
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			TextColor = ThemeColors.OnSurface,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		var disconnect = CreatePhotometerDisconnectButton();

		var deviceRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 10,
			VerticalOptions = LayoutOptions.Start,
			Children = { settingsIcon, deviceName, disconnect }
		};
		Microsoft.Maui.Controls.Grid.SetColumn(deviceName, 1);
		Microsoft.Maui.Controls.Grid.SetColumn(disconnect, 2);

		var rowDivider = new BoxView
		{
			HeightRequest = 1,
			Color = ThemeColors.Divider,
			HorizontalOptions = LayoutOptions.Fill
		};

		var batteryColor = !battKnown
			? ThemeColors.OnSurfaceMuted
			: battPct <= 35
				? Color.FromRgb(234, 88, 12)
				: ThemeColors.OnSurface;

		var batteryGroup = CreatePhotometerMetricGroup(
			"Battery:",
			BuildPhotometerBatteryGlyph(battPct, battKnown),
			battKnown ? $"{battPct}%" : "—",
			batteryColor);

		var tankGroup = CreatePhotometerTextMetric(
			"Active tank:",
			_viewModel.SelectedTankDisplay,
			AppConstants.Primary,
			showChevron: true);
		var tankTap = new TapGestureRecognizer();
		tankTap.Tapped += async (_, _) => await OpenTankPickerAsync();
		tankGroup.GestureRecognizers.Add(tankTap);
		SemanticProperties.SetDescription(tankGroup, "Change active tank");
		SemanticProperties.SetHint(tankGroup, "Opens tank selection");

		var metricsDivider = new BoxView
		{
			WidthRequest = 1,
			Color = ThemeColors.Divider,
			Margin = new Thickness(0, 2),
			VerticalOptions = LayoutOptions.Fill
		};

		var metricsStrip = new Grid
		{
			Padding = new Thickness(0, 4, 0, 0),
			RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto)),
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(1),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 10,
			VerticalOptions = LayoutOptions.Start,
			Children = { batteryGroup, metricsDivider, tankGroup }
		};
		Microsoft.Maui.Controls.Grid.SetColumn(metricsDivider, 1);
		Microsoft.Maui.Controls.Grid.SetColumn(tankGroup, 2);

		const double cardGap = 12;
		var metricsSection = new VerticalStackLayout { Spacing = 0, Children = { rowDivider, metricsStrip } };

		var panelChildren = new List<View> { deviceRow, metricsSection };

		if (_viewModel.State == PhotometerState.NewAnalysis)
		{
			panelChildren.Add(new Label
			{
				Text = "Tap Active tank to change it, then run the full method queue. Quick measurement presets below use the same tank.",
				FontSize = 12,
				LineHeight = 1.35,
				TextColor = ThemeColors.OnSurfaceVariant,
				LineBreakMode = LineBreakMode.WordWrap
			});
			panelChildren.Add(CreatePhotometerBeginButton());
		}

		var metricsPanel = new VerticalStackLayout
		{
			Spacing = cardGap,
			Padding = new Thickness(14, 12)
		};
		foreach (var child in panelChildren)
			metricsPanel.Children.Add(child);

		var metricsChrome = new Border
		{
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			BackgroundColor = ThemeColors.Surface,
			Content = metricsPanel,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 2),
				Radius = 10,
				Opacity = 1
			}
		};

		return new Border
		{
			StrokeThickness = 0,
			Margin = new Thickness(0, 0, 0, 14),
			Content = metricsChrome
		};
	}

	static Border FlowStepHeader(string title, string subtitle)
	{
		var accent = new BoxView
		{
			WidthRequest = 4,
			Color = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Fill
		};
		var stack = new VerticalStackLayout { Spacing = 6 };
		stack.Children.Add(new Label
		{
			Text = title,
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			CharacterSpacing = 0.4,
			TextColor = AppConstants.Primary
		});
		stack.Children.Add(new Label
		{
			Text = subtitle,
			FontSize = 14,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});

		var inner = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12,
			Padding = new Thickness(14, 14, 14, 14)
		};
		inner.Children.Add(accent);
		inner.Children.Add(stack);
		Grid.SetColumn(stack, 1);

		return new Border
		{
			Margin = new Thickness(0, 0, 0, 12),
			BackgroundColor = ThemeColors.SurfaceSecondary,
			StrokeThickness = 1,
			Stroke = AppConstants.Primary.MultiplyAlpha(0.15f),
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = inner
		};
	}

	static readonly Dictionary<string, List<MethodItem>> Presets = new()
	{
		["Daily Check"] =
		[
			new MethodItem { Title = "Alkalinity Marine", Unit = "dKH", Value = "9.65", Status = MethodStatus.Done },
			new MethodItem { Title = "pH Marine", Unit = "pH", Value = "\u2014", Status = MethodStatus.Active },
			new MethodItem { Title = "Phosphate Marine", Unit = "ppm", Value = "\u2014", Status = MethodStatus.Pending },
		],
		["Weekly Check"] =
		[
			new MethodItem { Title = "Alkalinity Marine", Unit = "dKH", Value = "9.65", Status = MethodStatus.Done },
			new MethodItem { Title = "Calcium Marine", Unit = "ppm", Value = "446", Status = MethodStatus.Done },
			new MethodItem { Title = "Magnesium Marine", Unit = "ppm", Value = "1389", Status = MethodStatus.Done },
			new MethodItem { Title = "Nitrate Marine", Unit = "ppm", Value = "\u2014", Status = MethodStatus.Active },
			new MethodItem { Title = "Phosphate Marine", Unit = "ppm", Value = "\u2014", Status = MethodStatus.Pending },
			new MethodItem { Title = "pH Marine", Unit = "pH", Value = "\u2014", Status = MethodStatus.Pending },
		],
		["All Methods"] =
		[
			new MethodItem { Title = "Alkalinity Marine", Unit = "dKH", Value = "9.65", Status = MethodStatus.Done },
			new MethodItem { Title = "Calcium Marine", Unit = "ppm", Value = "446", Status = MethodStatus.Done },
			new MethodItem { Title = "Magnesium Marine", Unit = "ppm", Value = "1389", Status = MethodStatus.Done },
			new MethodItem { Title = "Nitrate Marine LR", Unit = "ppm", Value = "3.84", Status = MethodStatus.Done },
			new MethodItem { Title = "Nitrate Marine HR", Unit = "ppm", Value = "73.3", Status = MethodStatus.Done },
			new MethodItem { Title = "Nitrite Marine ULR", Unit = "ppb", Value = "\u2014", Status = MethodStatus.Active },
			new MethodItem { Title = "pH Marine", Unit = "pH", Value = "\u2014", Status = MethodStatus.Pending },
			new MethodItem { Title = "Phosphate Marine ULR", Unit = "ppm", Value = "\u2014", Status = MethodStatus.Pending },
			new MethodItem { Title = "Ammonia Marine", Unit = "ppm", Value = "\u2014", Status = MethodStatus.Pending },
		],
	};

	public MeasurePhotometerView()
	{
		_viewModel = AppServices.Get<PhotometerMeasureViewModel>();
		BindingContext = _viewModel;
		InitializeComponent();
		_viewModel.StateChanged += (_, _) => Rebuild();
		_viewModel.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != nameof(PhotometerMeasureViewModel.SelectedTankNumber)
			    && e.PropertyName != nameof(PhotometerMeasureViewModel.SelectedTankDisplay))
				return;
			Rebuild();
		};
		Rebuild();
		Unloaded += (_, _) => StopAutoMeasurementLoop();
	}

	void SetState(PhotometerState s)
	{
		var wasRunning = _viewModel.State == PhotometerState.Running;
		if (s != PhotometerState.Running)
			StopAutoMeasurementLoop();
		else if (!wasRunning)
			PrepareQueueForNewRun();

		_viewModel.SetState(s);

		if (s == PhotometerState.Running)
			_ = StartAutoMeasurementLoopAsync();
	}

	bool AllRunningMethodsDone() =>
		_selectedMethods.Count > 0 && _selectedMethods.All(m => m.Status == MethodStatus.Done);

	static string? ResolveDemoValueForMethod(string title)
	{
		foreach (var preset in Presets.Values)
		{
			var match = preset.FirstOrDefault(p => p.Title == title);
			if (match is not null && match.Value is not "\u2014" and not "—")
				return match.Value;
		}
		return null;
	}

	void PrepareQueueForNewRun()
	{
		foreach (var method in _selectedMethods)
		{
			method.Status = MethodStatus.Pending;
			method.Value = "\u2014";
		}

		if (_selectedMethods.Count > 0)
			_selectedMethods[0].Status = MethodStatus.Active;
	}

	void EnsureHasActiveParameter()
	{
		if (_selectedMethods.Any(m => m.Status == MethodStatus.Active))
			return;

		var nextIdx = _selectedMethods.FindIndex(m => m.Status == MethodStatus.Pending);
		if (nextIdx >= 0)
			_selectedMethods[nextIdx].Status = MethodStatus.Active;
	}

	void RecordActiveParameterFromMeter()
	{
		var activeIdx = _selectedMethods.FindIndex(m => m.Status == MethodStatus.Active);
		if (activeIdx < 0)
			return;

		var active = _selectedMethods[activeIdx];
		if (active.Value is "\u2014" or "—")
		{
			var demo = ResolveDemoValueForMethod(active.Title);
			if (!string.IsNullOrEmpty(demo))
				active.Value = demo;
		}

		active.Status = MethodStatus.Done;
		var nextIdx = _selectedMethods.FindIndex(m => m.Status == MethodStatus.Pending);
		if (nextIdx >= 0)
			_selectedMethods[nextIdx].Status = MethodStatus.Active;
	}

	void StopAutoMeasurementLoop()
	{
		if (_measurementLoopCts is null)
			return;
		_measurementLoopCts.Cancel();
		_measurementLoopCts.Dispose();
		_measurementLoopCts = null;
	}

	async Task StartAutoMeasurementLoopAsync()
	{
		StopAutoMeasurementLoop();
		var cts = new CancellationTokenSource();
		_measurementLoopCts = cts;
		var token = cts.Token;

		try
		{
			while (!token.IsCancellationRequested
			       && _viewModel.State == PhotometerState.Running
			       && !AllRunningMethodsDone())
			{
				EnsureHasActiveParameter();
				await MainThread.InvokeOnMainThreadAsync(Rebuild);

				await Task.Delay(MeasurementStepDelayMs, token);

				if (token.IsCancellationRequested || _viewModel.State != PhotometerState.Running)
					break;

				RecordActiveParameterFromMeter();
			}

			if (!token.IsCancellationRequested && _viewModel.State == PhotometerState.Running)
				await MainThread.InvokeOnMainThreadAsync(Rebuild);
		}
		catch (TaskCanceledException)
		{
			// Loop stopped when leaving Running or starting a new loop.
		}
	}

	void ReRunSelectedMethods(IReadOnlyList<int> indices)
	{
		if (indices.Count == 0)
			return;

		var ordered = indices.OrderBy(i => i).ToList();
		foreach (var idx in ordered)
		{
			if (idx < 0 || idx >= _selectedMethods.Count)
				continue;
			_selectedMethods[idx].Status = MethodStatus.Pending;
			_selectedMethods[idx].Value = "\u2014";
		}

		var firstActive = ordered.FirstOrDefault(i => i >= 0 && i < _selectedMethods.Count);
		if (firstActive >= 0)
			_selectedMethods[firstActive].Status = MethodStatus.Active;

		Rebuild();
		if (_viewModel.State == PhotometerState.Running)
			_ = StartAutoMeasurementLoopAsync();
	}

	async Task OnFinishSequenceAsync()
	{
		StopAutoMeasurementLoop();

		var page = ViewNavigation.FindHostPage(this);
		if (page is null)
			return;

		var wantsRerun = await page.DisplayAlertAsync(
			"Finish sequence",
			"Do you want to re-run the measurement?",
			"Yes",
			"No");

		if (!wantsRerun)
		{
			FinalizeRunForCompletion();
			SetState(PhotometerState.Completed);
			return;
		}

		await OpenRerunPickerAsync();
	}

	async Task OpenRerunPickerAsync()
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is null)
			return;

		var options = new List<RerunMethodsPickerPage.RerunMethodOption>();
		for (var i = 0; i < _selectedMethods.Count; i++)
		{
			if (_selectedMethods[i].Status == MethodStatus.Done)
			{
				options.Add(new RerunMethodsPickerPage.RerunMethodOption(
					i,
					_selectedMethods[i].Title,
					FormatCompletionValue(_selectedMethods[i])));
			}
		}

		if (options.Count == 0)
		{
			await page.DisplayAlertAsync(
				"Re-run measurements",
				"No completed parameters are available to re-run.",
				"OK");
			return;
		}

		var picker = new RerunMethodsPickerPage(options, ReRunSelectedMethods);
		await page.Navigation.PushModalAsync(new NavigationPage(picker));
	}

	void Rebuild()
	{
		BodyStack.Children.Clear();
		FooterHost.Children.Clear();
		FooterHost.IsVisible = false;

		if (_viewModel.State == PhotometerState.NewAnalysis)
			BodyStack.Children.Add(BuildPhotometerDeviceHeader());

		switch (_viewModel.State)
		{
			case PhotometerState.NewAnalysis:
				BuildNewAnalysis();
				break;
			case PhotometerState.Setup:
				BuildSetup();
				break;
			case PhotometerState.StartMeasurement:
				BuildStartMeasurement();
				break;
			case PhotometerState.Running:
				BuildRunning();
				break;
			case PhotometerState.Completed:
				BuildCompleted();
				break;
		}

		SyncNavigationChrome();
	}

	/// <summary>Asks the measure tab host to refresh Shell chrome via the photometer module.</summary>
	public void SyncNavigationChrome()
	{
		if (ViewNavigation.FindHostPage(this) is MeasureTabPage measureTab)
			measureTab.RefreshShellNavigation();
	}

	public PhotometerMeasureViewModel PhotometerViewModel => _viewModel;

	void BuildNewAnalysis()
	{
		BodyStack.Children.Add(SectionHeaderWithGlyph(PhotometerActionIconKind.QuickBolt, "Quick Measurement Presets"));
		BodyStack.Children.Add(new BoxView { HeightRequest = 12 });
		BodyStack.Children.Add(BuildPresetGrid());
		BodyStack.Children.Add(new BoxView { HeightRequest = 24 });
		BodyStack.Children.Add(SectionHeaderWithAsset("log_history", "Recent Measurements"));
		BodyStack.Children.Add(new BoxView { HeightRequest = 12 });
		BodyStack.Children.Add(RecentTile("Alkalinity Marine", "11/12/25 • 11:33:56 AM", "9.65 dKH"));
		BodyStack.Children.Add(new BoxView { HeightRequest = 10 });
		BodyStack.Children.Add(RecentTile("pH Marine", "10/11/25 • 1:10:59 AM", "8.2 pH"));
	}

	Grid BuildPresetGrid()
	{
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)),
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12,
			RowSpacing = 12
		};

		var c1 = MakePresetCard("Daily Check", "3 methods", PhotometerActionIconKind.DailySun, () => SelectPreset("Daily Check"));
		grid.Children.Add(c1);
		Grid.SetRow(c1, 0);
		Grid.SetColumn(c1, 0);

		var c2 = MakePresetCard("Weekly Check", "6 methods", PhotometerActionIconKind.WeeklyCalendar, () => SelectPreset("Weekly Check"));
		grid.Children.Add(c2);
		Grid.SetRow(c2, 0);
		Grid.SetColumn(c2, 1);

		var c3 = MakePresetCard("All Methods", "9 methods", PhotometerActionIconKind.AllMethodsGrid, () => SelectPreset("All Methods"));
		grid.Children.Add(c3);
		Grid.SetRow(c3, 1);
		Grid.SetColumn(c3, 0);

		var c4 = MakeAddCard();
		grid.Children.Add(c4);
		Grid.SetRow(c4, 1);
		Grid.SetColumn(c4, 1);

		return grid;
	}

	void SelectPreset(string name)
	{
		_selectedMethods = Presets[name].Select(m => new MethodItem
		{
			Title = m.Title,
			Unit = m.Unit,
			Value = m.Value,
			Status = m.Status
		}).ToList();
		SetState(PhotometerState.Setup);
	}

	Border MakePresetCard(string title, string subtitle, PhotometerActionIconKind iconKind, Action onTap)
	{
		var iconHost = new Border
		{
			Padding = 8,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
			StrokeThickness = 0,
			HorizontalOptions = LayoutOptions.Start,
			Content = PhotometerActionIcons.Create(iconKind, () => AppConstants.Primary, 26),
			StrokeShape = new RoundRectangle { CornerRadius = 12 }
		};

		var inner = new VerticalStackLayout
		{
			Padding = 18,
			Spacing = 14,
			Children =
			{
				iconHost,
				new Label { Text = title, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 },
				new Label { Text = subtitle, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		var b = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			Content = inner,
			StrokeShape = new RoundRectangle { CornerRadius = 18 }
		};
		b.Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 4), Radius = 12, Opacity = 1 };
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => onTap();
		b.GestureRecognizers.Add(tap);
		return b;
	}

	Border MakeAddCard()
	{
		var inner = new VerticalStackLayout
		{
			Padding = new Thickness(8, 12),
			VerticalOptions = LayoutOptions.Center,
			Spacing = 8,
			Children =
			{
				new Border
				{
					WidthRequest = 44,
					HeightRequest = 44,
					HorizontalOptions = LayoutOptions.Center,
					BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
					StrokeThickness = 0,
					Content = PhotometerActionIcons.Create(PhotometerActionIconKind.QuickBolt, () => AppConstants.Primary, 24),
					StrokeShape = new RoundRectangle { CornerRadius = 12 }
				},
				new Label
				{
					Text = "Custom sequence",
					FontSize = 12,
					HorizontalTextAlignment = TextAlignment.Center,
					FontAttributes = FontAttributes.Bold,
					TextColor = ThemeColors.OnSurface,
					LineBreakMode = LineBreakMode.WordWrap,
					MaxLines = 2
				},
				new Label
				{
					Text = "Build your own queue",
					FontSize = 12,
					HorizontalTextAlignment = TextAlignment.Center,
					TextColor = ThemeColors.OnSurfaceVariant
				}
			}
		};

		var b = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = AppConstants.Primary.MultiplyAlpha(0.25f),
			StrokeThickness = 1.5,
			Content = inner,
			StrokeShape = new RoundRectangle { CornerRadius = 18 }
		};
		b.Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 10, Opacity = 1 };
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => BeginMeasurement();
		b.GestureRecognizers.Add(tap);
		return b;
	}

	Border RecentTile(string method, string date, string value)
	{
		var row = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 14,
			Padding = 16
		};

		var initials = MethodInitials(method);
		var icon = new Border
		{
			WidthRequest = 48,
			HeightRequest = 48,
			BackgroundColor = InitialDiskBg(method),
			StrokeThickness = 0,
			Content = new Label
			{
				Text = initials,
				FontSize = 14,
				FontAttributes = FontAttributes.Bold,
				TextColor = InitialDiskFg(method),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			},
			StrokeShape = new Ellipse()
		};

		var textCol = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = method, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 },
				new Label { Text = date, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		row.Children.Add(icon);
		Grid.SetColumn(icon, 0);
		row.Children.Add(textCol);
		Grid.SetColumn(textCol, 1);
		var valLbl = new Label { Text = value, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center };
		row.Children.Add(valLbl);
		Grid.SetColumn(valLbl, 2);
		var chev = new Label { Text = "\u203A", FontSize = 20, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center };
		row.Children.Add(chev);
		Grid.SetColumn(chev, 3);

		return new Border
		{
			StrokeThickness = 0,
			BackgroundColor = ThemeColors.Surface,
			Content = row,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 10, Opacity = 1 }
		};
	}

	HorizontalStackLayout SectionHeaderWithGlyph(PhotometerActionIconKind kind, string text) => new()
	{
		Spacing = 10,
		VerticalOptions = LayoutOptions.Center,
		Children =
		{
			new Border
			{
				WidthRequest = 36,
				HeightRequest = 36,
				Padding = 4,
				BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.1f),
				StrokeThickness = 0,
				Content = PhotometerActionIcons.Create(kind, () => AppConstants.Primary, 24),
				StrokeShape = new RoundRectangle { CornerRadius = 10 },
				VerticalOptions = LayoutOptions.Center
			},
			new Label
			{
				Text = text,
				FontAttributes = FontAttributes.Bold,
				FontSize = 16,
				TextColor = ThemeColors.OnSurface,
				VerticalOptions = LayoutOptions.Center
			}
		}
	};

	HorizontalStackLayout SectionHeaderWithAsset(string asset, string text) => new()
	{
		Spacing = 10,
		VerticalOptions = LayoutOptions.Center,
		Children =
		{
			new Border
			{
				WidthRequest = 36,
				HeightRequest = 36,
				BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.1f),
				StrokeThickness = 0,
				Content = AssetIcon(asset, 20),
				StrokeShape = new RoundRectangle { CornerRadius = 10 },
				VerticalOptions = LayoutOptions.Center
			},
			new Label
			{
				Text = text,
				FontAttributes = FontAttributes.Bold,
				FontSize = 16,
				TextColor = ThemeColors.OnSurface,
				VerticalOptions = LayoutOptions.Center
			}
		}
	};

	void BuildSetup()
	{
		var top = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			Padding = new Thickness(16, 12),
			BackgroundColor = ThemeColors.Surface,
			ColumnSpacing = 12
		};

		var close = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = ThemeColors.CloseButtonBg,
			StrokeThickness = 0,
			Content = new Label { Text = "\u2715", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 20 }
		};
		var closeTap = new TapGestureRecognizer();
		closeTap.Tapped += (_, _) => SetState(PhotometerState.NewAnalysis);
		close.GestureRecognizers.Add(closeTap);

		top.Children.Add(close);
		Grid.SetColumn(close, 0);
		var setupTitleCol = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
		setupTitleCol.Children.Add(new Label { Text = "Review methods", FontAttributes = FontAttributes.Bold, FontSize = 20 });
		setupTitleCol.Children.Add(new Label
		{
			Text = $"{_viewModel.SelectedTankDisplay} · {_selectedMethods.Count} parameters · Change tank from New measurement on the overview.",
			FontSize = 12,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});
		top.Children.Add(setupTitleCol);
		Grid.SetColumn(setupTitleCol, 1);

		BodyStack.Children.Add(top);

		BodyStack.Children.Add(FlowStepHeader(
			"Step 2 of 4 · Method queue",
			$"Parameters run sequentially on the instrument for {_viewModel.SelectedTankDisplay}."));

		var list = new VerticalStackLayout { Spacing = 12, Margin = new Thickness(0, 4, 0, 0) };
		for (var i = 0; i < _selectedMethods.Count; i++)
		{
			var m = _selectedMethods[i];
			list.Children.Add(MethodNumberTile(i + 1, m.Title, m.Unit));
		}
		BodyStack.Children.Add(list);

		FooterHost.IsVisible = true;
		var startBtn = new Button
		{
			Text = "Continue to confirm",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold
		};
		startBtn.Clicked += (_, _) => SetState(PhotometerState.StartMeasurement);
		FooterHost.Children.Add(startBtn);
	}

	void BuildStartMeasurement()
	{
		var top = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			Padding = new Thickness(4, 12, 4, 8),
			ColumnSpacing = 12
		};

		var back = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = ThemeColors.CloseButtonBg,
			StrokeThickness = 0,
			Content = new Label { Text = "\u2039", FontSize = 22, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 20 },
			VerticalOptions = LayoutOptions.Center
		};
		var backTap = new TapGestureRecognizer();
		backTap.Tapped += (_, _) => SetState(PhotometerState.Setup);
		back.GestureRecognizers.Add(backTap);

		var head = new VerticalStackLayout { Spacing = 4 };
		head.Children.Add(new Label
		{
			Text = "Ready to start",
			FontSize = 22,
			FontAttributes = FontAttributes.Bold
		});
		head.Children.Add(new Label
		{
			Text = "Verify tank and method order. The HI97115 will execute one parameter at a time.",
			FontSize = 13,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});

		top.Children.Add(back);
		top.Children.Add(head);
		Grid.SetColumn(head, 1);

		BodyStack.Children.Add(top);
		BodyStack.Children.Add(new BoxView { HeightRequest = 4 });
		BodyStack.Children.Add(FlowStepHeader(
			"Step 3 of 4 · Pre-flight",
			"Starting sends the first method to the photometer; the rest follow in order."));

		BodyStack.Children.Add(new BoxView { HeightRequest = 8 });

		var tankSummary = new Border
		{
			Padding = new Thickness(20, 18),
			BackgroundColor = ThemeColors.Surface,
			StrokeThickness = 1,
			Stroke = AppConstants.Primary.MultiplyAlpha(0.2f),
			StrokeShape = new RoundRectangle { CornerRadius = 20 }
		};
		tankSummary.Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 4), Radius = 16, Opacity = 1 };

		var tankTxt = new VerticalStackLayout { Spacing = 6 };
		tankTxt.Children.Add(new Label { Text = "Measuring for", FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant });
		tankTxt.Children.Add(new Label
		{
			Text = _viewModel.SelectedTankDisplay,
			FontSize = 22,
			FontAttributes = FontAttributes.Bold
		});
		tankTxt.Children.Add(new Label
		{
			Text = $"{_selectedMethods.Count} parameters queued. Go back to edit, or return home and use New measurement to change tank.",
			FontSize = 13,
			TextColor = ThemeColors.OnSurfaceMuted,
			LineBreakMode = LineBreakMode.WordWrap
		});
		tankSummary.Content = tankTxt;
		BodyStack.Children.Add(tankSummary);

		BodyStack.Children.Add(new BoxView { HeightRequest = 16 });
		BodyStack.Children.Add(new Label
		{
			Text = "Parameter queue",
			FontAttributes = FontAttributes.Bold,
			FontSize = 15,
			Margin = new Thickness(0, 0, 0, 8)
		});

		var methodStack = new VerticalStackLayout { Spacing = 10 };
		foreach (var m in _selectedMethods)
		{
			methodStack.Children.Add(StartMeasurementMethodRow(m.Title, m.Unit));
		}
		BodyStack.Children.Add(methodStack);

		FooterHost.IsVisible = true;
		var startMeasurementBtn = new Button
		{
			Text = "Start on photometer",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold
		};
		startMeasurementBtn.Clicked += (_, _) => SetState(PhotometerState.Running);
		FooterHost.Children.Add(startMeasurementBtn);

		var edit = new Button
		{
			Text = "Edit queue",
			HeightRequest = 48,
			BackgroundColor = Colors.Transparent,
			TextColor = AppConstants.Primary,
			BorderColor = AppConstants.Primary.MultiplyAlpha(0.35f),
			BorderWidth = 1.5,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold,
			Margin = new Thickness(0, 10, 0, 0)
		};
		edit.Clicked += (_, _) => SetState(PhotometerState.Setup);
		FooterHost.Children.Add(edit);
	}

	Border StartMeasurementMethodRow(string title, string unit)
	{
		var initials = MethodInitials(title);
		var avatar = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = InitialDiskBg(title),
			StrokeThickness = 0,
			Content = new Label
			{
				Text = initials,
				FontSize = 13,
				FontAttributes = FontAttributes.Bold,
				TextColor = InitialDiskFg(title),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			},
			StrokeShape = new Ellipse(),
			VerticalOptions = LayoutOptions.Center
		};

		var col = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Spacing = 2,
			Children =
			{
				new Label { Text = title, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 },
				new Label { Text = unit, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		var row = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12,
			Padding = 14,
			BackgroundColor = ThemeColors.Surface
		};
		row.Children.Add(avatar);
		row.Children.Add(col);
		Grid.SetColumn(col, 1);

		return new Border
		{
			StrokeThickness = 0,
			Content = row,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 8, Opacity = 1 }
		};
	}

	Border MethodNumberTile(int index, string title, string unit)
	{
		var row = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 14,
			Padding = 16,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.08f)
		};

		var num = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = AppConstants.Primary,
			StrokeThickness = 0,
			Content = new Label { Text = index.ToString(), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 10 }
		};

		var col = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = title, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 },
				new Label { Text = unit, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		var check = new Label { Text = "\u2713", FontSize = 22, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center };

		row.Children.Add(num);
		Grid.SetColumn(num, 0);
		row.Children.Add(col);
		Grid.SetColumn(col, 1);
		row.Children.Add(check);
		Grid.SetColumn(check, 2);

		return new Border
		{
			Stroke = AppConstants.Primary,
			StrokeThickness = 1.5,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.08f),
			Content = row,
			StrokeShape = new RoundRectangle { CornerRadius = AppConstants.RadiusCardSmall },
			Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 8, Opacity = 1 }
		};
	}

	int GetRunningStepOneBased()
	{
		for (var i = 0; i < _selectedMethods.Count; i++)
			if (_selectedMethods[i].Status == MethodStatus.Active)
				return i + 1;
		for (var i = 0; i < _selectedMethods.Count; i++)
			if (_selectedMethods[i].Status == MethodStatus.Pending)
				return i + 1;
		var done = _selectedMethods.Count(m => m.Status == MethodStatus.Done);
		return Math.Clamp(Math.Max(1, done), 1, Math.Max(1, _selectedMethods.Count));
	}

	string GetRunningMethodName()
	{
		var active = _selectedMethods.FirstOrDefault(m => m.Status == MethodStatus.Active);
		if (active is not null)
			return active.Title;
		var pending = _selectedMethods.FirstOrDefault(m => m.Status == MethodStatus.Pending);
		return pending?.Title ?? string.Empty;
	}

	void BuildRunning()
	{
		var step = GetRunningStepOneBased();
		var total = Math.Max(1, _selectedMethods.Count);
		var currentName = GetRunningMethodName();
		var allDone = AllRunningMethodsDone();

		var head = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(0, 8, 0, 12) };
		head.Children.Add(new Label
		{
			Text = "Step 4 of 4 · In progress",
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			CharacterSpacing = 0.3,
			TextColor = AppConstants.Primary
		});
		head.Children.Add(new Label
		{
			Text = allDone
				? $"{_viewModel.SelectedTankDisplay} · All parameters recorded"
				: $"{_viewModel.SelectedTankDisplay} · Parameter {step} of {total}",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		});
		if (!allDone && !string.IsNullOrEmpty(currentName))
			head.Children.Add(new Label { Text = currentName, FontSize = 16, TextColor = ThemeColors.OnSurface, FontAttributes = FontAttributes.Bold });
		head.Children.Add(new Label
		{
			Text = allDone
				? "Review recorded results below. Tap Finish sequence when you are ready to save this run."
				: "The HI97115 measures each parameter in order and sends results back automatically. You can finish the sequence at any time.",
			FontSize = 13,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});
		BodyStack.Children.Add(head);

		for (var i = 0; i < _selectedMethods.Count; i++)
			BodyStack.Children.Add(BuildRunningResultTile(i, _selectedMethods[i]));

		BodyStack.Children.Add(new BoxView { HeightRequest = 20 });

		if (!allDone)
		{
			var measuring = string.IsNullOrEmpty(currentName)
				? "Communicating with photometer…"
				: $"Measuring {currentName} on photometer…";
			var progRow = new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Spacing = 12 };
			progRow.Children.Add(new ActivityIndicator { IsRunning = true, Color = AppConstants.Primary, WidthRequest = 24, HeightRequest = 24 });
			progRow.Children.Add(new Label
			{
				Text = measuring,
				TextColor = ThemeColors.OnSurfaceVariant,
				VerticalOptions = LayoutOptions.Center
			});
			BodyStack.Children.Add(progRow);
			BodyStack.Children.Add(new BoxView { HeightRequest = 16 });
		}

		var finish = new Button
		{
			Text = "Finish sequence",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold,
			IsEnabled = true
		};
		finish.Clicked += async (_, _) => await OnFinishSequenceAsync();
		BodyStack.Children.Add(finish);
	}

	Border BuildRunningResultTile(int index, MethodItem method) =>
		ResultTile(
			method.Title,
			method.Unit,
			method.Value,
			method.Status == MethodStatus.Done,
			method.Status == MethodStatus.Active);

	void FinalizeRunForCompletion()
	{
		foreach (var m in _selectedMethods)
			m.Status = MethodStatus.Done;
	}

	static string FormatCompletionValue(MethodItem method)
	{
		var v = method.Value;
		if (v is "\u2014" or "—" || string.IsNullOrWhiteSpace(v))
			return "—";
		if (!string.IsNullOrWhiteSpace(method.Unit)
		    && !v.Contains(method.Unit, StringComparison.OrdinalIgnoreCase))
			return $"{v} {method.Unit}";
		return v;
	}

	Border BuildCompletionSummaryCard(int parameterCount)
	{
		var badge = new Border
		{
			WidthRequest = 56,
			HeightRequest = 56,
			BackgroundColor = AppConstants.Success.MultiplyAlpha(0.18f),
			StrokeThickness = 0,
			StrokeShape = new Ellipse(),
			VerticalOptions = LayoutOptions.Start,
			Content = new Label
			{
				Text = "\u2713",
				FontSize = 28,
				TextColor = AppConstants.Success,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		var textCol = new VerticalStackLayout { Spacing = 4 };
		textCol.Children.Add(new Label
		{
			Text = "Measurement complete",
			FontSize = 20,
			FontAttributes = FontAttributes.Bold,
			TextColor = ThemeColors.OnSurface
		});
		textCol.Children.Add(new Label
		{
			Text = $"{_viewModel.SelectedTankDisplay} · {parameterCount} parameters",
			FontSize = 15,
			FontAttributes = FontAttributes.Bold,
			TextColor = AppConstants.Primary
		});
		textCol.Children.Add(new Label
		{
			Text = "Review the readings below, then save to your log or discard this run.",
			FontSize = 13,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});

		var inner = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 16,
			Padding = new Thickness(18, 16)
		};
		inner.Children.Add(badge);
		inner.Children.Add(textCol);
		Microsoft.Maui.Controls.Grid.SetColumn(textCol, 1);

		return new Border
		{
			BackgroundColor = AppConstants.Success.MultiplyAlpha(0.06f),
			Stroke = AppConstants.Success.MultiplyAlpha(0.28f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = inner,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 2),
				Radius = 10,
				Opacity = 1
			}
		};
	}

	Border BuildCompletionResultRow(MethodItem method, bool showDividerBelow)
	{
		var initials = MethodInitials(method.Title);
		var avatar = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			BackgroundColor = InitialDiskBg(method.Title),
			StrokeThickness = 0,
			Content = new Label
			{
				Text = initials,
				FontSize = 13,
				FontAttributes = FontAttributes.Bold,
				TextColor = InitialDiskFg(method.Title),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			},
			StrokeShape = new Ellipse(),
			VerticalOptions = LayoutOptions.Center
		};

		var valueText = FormatCompletionValue(method);
		var hasValue = valueText != "—";

		var textCol = new VerticalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = method.Title,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
					MaxLines = 1
				},
				new Label
				{
					Text = string.IsNullOrWhiteSpace(method.Unit) ? "Recorded" : $"Recorded · {method.Unit}",
					FontSize = 12,
					TextColor = ThemeColors.OnSurfaceVariant
				}
			}
		};

		var resultRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 12,
			Padding = new Thickness(14, 12)
		};
		var valueLbl = new Label
		{
			Text = valueText,
			FontAttributes = FontAttributes.Bold,
			FontSize = 15,
			TextColor = hasValue ? ThemeColors.OnSurface : ThemeColors.OnSurfaceMuted,
			VerticalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		resultRow.Children.Add(avatar);
		Microsoft.Maui.Controls.Grid.SetColumn(avatar, 0);
		resultRow.Children.Add(textCol);
		Microsoft.Maui.Controls.Grid.SetColumn(textCol, 1);
		resultRow.Children.Add(valueLbl);
		Microsoft.Maui.Controls.Grid.SetColumn(valueLbl, 2);

		var stack = new VerticalStackLayout { Spacing = 0 };
		stack.Children.Add(resultRow);
		if (showDividerBelow)
		{
			stack.Children.Add(new BoxView
			{
				HeightRequest = 1,
				Color = ThemeColors.Divider,
				Margin = new Thickness(14, 0, 0, 0)
			});
		}

		return new Border
		{
			StrokeThickness = 0,
			BackgroundColor = Colors.Transparent,
			Content = stack
		};
	}

	void BuildCompleted()
	{
		FinalizeRunForCompletion();
		var count = _selectedMethods.Count;

		BodyStack.Children.Add(FlowStepHeader(
			"Step 4 of 4 · Complete",
			$"All parameters for {_viewModel.SelectedTankDisplay} have been recorded on the photometer."));

		BodyStack.Children.Add(BuildCompletionSummaryCard(count));
		BodyStack.Children.Add(new BoxView { HeightRequest = 16 });

		BodyStack.Children.Add(new Label
		{
			Text = "Recorded results",
			FontAttributes = FontAttributes.Bold,
			FontSize = 15,
			Margin = new Thickness(0, 0, 0, 8)
		});

		var resultsHost = new VerticalStackLayout { Spacing = 0 };
		for (var i = 0; i < _selectedMethods.Count; i++)
			resultsHost.Children.Add(BuildCompletionResultRow(_selectedMethods[i], i < _selectedMethods.Count - 1));

		var resultsCard = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = resultsHost,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 2),
				Radius = 10,
				Opacity = 1
			}
		};
		BodyStack.Children.Add(resultsCard);
		BodyStack.Children.Add(new BoxView { HeightRequest = 20 });

		FooterHost.IsVisible = true;

		var save = new Button
		{
			Text = "Save to log",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Success,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold
		};
		save.Clicked += (_, _) => SetState(PhotometerState.NewAnalysis);
		FooterHost.Children.Add(save);

		var discard = new Button
		{
			Text = "Discard run",
			HeightRequest = 48,
			BackgroundColor = Colors.Transparent,
			TextColor = ThemeColors.OnSurface,
			BorderColor = ThemeColors.Divider,
			BorderWidth = 1,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold,
			Margin = new Thickness(0, 10, 0, 0)
		};
		discard.Clicked += (_, _) => SetState(PhotometerState.NewAnalysis);
		FooterHost.Children.Add(discard);
	}

	Border ResultTile(string title, string unit, string value, bool done, bool active)
	{
		Color border = Colors.Transparent;
		Color bg = ThemeColors.Surface;
		Color iconBg = ThemeColors.ResultIconBgNeutral;
		Color iconFg = ThemeColors.OnSurfaceVariant;
		string glyph = "\u25CB";

		if (done)
		{
			border = AppConstants.Success;
			bg = AppConstants.Success.MultiplyAlpha(0.06f);
			iconBg = AppConstants.Success.MultiplyAlpha(0.15f);
			iconFg = AppConstants.Success;
			glyph = "\u2713";
		}
		else if (active)
		{
			border = AppConstants.Primary;
			bg = AppConstants.Primary.MultiplyAlpha(0.06f);
			iconBg = AppConstants.Primary.MultiplyAlpha(0.15f);
			iconFg = AppConstants.Primary;
			glyph = "\u21BB";
		}

		var inner = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 14,
			Padding = 16,
			BackgroundColor = bg
		};

		var circle = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			BackgroundColor = iconBg,
			StrokeThickness = 0,
			Content = new Label { Text = glyph, FontSize = 20, TextColor = iconFg, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new Ellipse()
		};

		var col = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = title, FontAttributes = FontAttributes.Bold },
				new Label { Text = unit, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		var dim = value == "\u2014" || value == "—";

		inner.Children.Add(circle);
		Grid.SetColumn(circle, 0);
		inner.Children.Add(col);
		Grid.SetColumn(col, 1);
		var valLbl = new Label
		{
			Text = value,
			FontAttributes = FontAttributes.Bold,
			FontSize = 16,
			TextColor = dim ? ThemeColors.OnSurfaceVariant.MultiplyAlpha(0.55f) : ThemeColors.OnSurface,
			VerticalOptions = LayoutOptions.Center
		};
		inner.Children.Add(valLbl);
		Grid.SetColumn(valLbl, 2);

		var wrap = new Border
		{
			Stroke = border,
			StrokeThickness = border == Colors.Transparent ? 0 : 1.5,
			BackgroundColor = bg,
			Content = inner,
			StrokeShape = new RoundRectangle { CornerRadius = AppConstants.RadiusCardSmall }
		};
		if (border != Colors.Transparent)
			wrap.Shadow = new Shadow { Brush = new SolidColorBrush(border.MultiplyAlpha(0.15f)), Offset = new Point(0, 2), Radius = 8, Opacity = 1 };
		return wrap;
	}

	void BeginMeasurement() => SelectPreset("All Methods");

	async Task OpenTankPickerAsync()
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is null)
			return;
		var picker = new TankPickerPage(_viewModel.SelectedTankNumber, n => _viewModel.SelectedTankNumber = n);
		await page.Navigation.PushModalAsync(new NavigationPage(picker));
	}

	async Task OpenSettings()
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is not null && Application.Current is App app)
			await page.Navigation.PushAsync(app.Services.GetRequiredService<PhotometerDeviceSettingsPage>());
	}

	async void OnBluetooth(object? sender, EventArgs e)
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is not null && Application.Current is App app)
			await page.Navigation.PushAsync(app.Services.GetRequiredService<DevicePage>());
	}

	public void ApplyTheme() => Rebuild();
}
