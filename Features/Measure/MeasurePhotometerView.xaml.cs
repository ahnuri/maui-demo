using HannaUIDemo.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Helpers;
using HannaUIDemo;
using Microsoft.Extensions.DependencyInjection;
using PhotometerState = HannaUIDemo.Features.Measure.PhotometerMeasureViewModel.MeasureState;

namespace HannaUIDemo.Features.Measure;

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
		Rebuild();
	}

	void SetState(PhotometerState s) => _viewModel.SetState(s);

	void Rebuild()
	{
		BodyStack.Children.Clear();
		FooterHost.Children.Clear();
		FooterHost.IsVisible = false;

		switch (_viewModel.State)
		{
			case PhotometerState.NewAnalysis:
				BuildNewAnalysis();
				break;
			case PhotometerState.Setup:
				BuildSetup();
				break;
			case PhotometerState.Running:
				BuildRunning();
				break;
			case PhotometerState.Completed:
				BuildCompleted();
				break;
		}
	}

	void BuildNewAnalysis()
	{
		var header = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			Margin = new Thickness(0, 0, 0, 6)
		};

		var iconBox = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
			StrokeThickness = 0,
			Content = new Label { Text = "\u2692", FontSize = 18, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 12 }
		};

		var title = new Label
		{
			Text = "New Analysis",
			FontSize = 22,
			FontAttributes = FontAttributes.Bold,
			VerticalOptions = LayoutOptions.Center,
			Margin = new Thickness(12, 0, 0, 0)
		};

		var settingsBtn = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
			StrokeThickness = 0,
			Content = new Label { Text = "\u2699", FontSize = 20, TextColor = AppConstants.Primary, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 12 }
		};
		var settingsTap = new TapGestureRecognizer();
		settingsTap.Tapped += async (_, _) => await OpenSettings();
		settingsBtn.GestureRecognizers.Add(settingsTap);

		header.Children.Add(iconBox);
		Grid.SetColumn(iconBox, 0);
		header.Children.Add(title);
		Grid.SetColumn(title, 1);
		header.Children.Add(settingsBtn);
		Grid.SetColumn(settingsBtn, 2);

		BodyStack.Children.Add(header);
		BodyStack.Children.Add(new Label
		{
			Text = "Select a sequence or start fresh measurement with Photometer.",
			FontSize = 14,
			TextColor = ThemeColors.OnSurfaceMuted,
			LineBreakMode = LineBreakMode.WordWrap,
			Margin = new Thickness(0, 0, 0, 24)
		});

		BodyStack.Children.Add(SectionHeader("\u26A1", "Quick Action"));
		BodyStack.Children.Add(new BoxView { HeightRequest = 12 });
		BodyStack.Children.Add(BuildPresetGrid());
		BodyStack.Children.Add(new BoxView { HeightRequest = 24 });
		BodyStack.Children.Add(SectionHeader("\u231A", "Recently Used"));
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

		var c1 = MakePresetCard("Daily Check", "3 methods", "\u2637", () => SelectPreset("Daily Check"));
		grid.Children.Add(c1);
		Grid.SetRow(c1, 0);
		Grid.SetColumn(c1, 0);

		var c2 = MakePresetCard("Weekly Check", "6 methods", "\u2637", () => SelectPreset("Weekly Check"));
		grid.Children.Add(c2);
		Grid.SetRow(c2, 0);
		Grid.SetColumn(c2, 1);

		var c3 = MakePresetCard("All Methods", "9 methods", "\u2234", () => SelectPreset("All Methods"));
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

	Border MakePresetCard(string title, string subtitle, string icon, Action onTap)
	{
		var inner = new VerticalStackLayout
		{
			Padding = 18,
			Spacing = 14,
			Children =
			{
				new Border
				{
					Padding = 10,
					BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
					StrokeThickness = 0,
					HorizontalOptions = LayoutOptions.Start,
					Content = new Label { Text = icon, FontSize = 22, TextColor = AppConstants.Primary },
					StrokeShape = new RoundRectangle { CornerRadius = 12 }
				},
				new Label { Text = title, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 },
				new Label { Text = subtitle, FontSize = 12, TextColor = ThemeColors.OnSurfaceVariant }
			}
		};

		var b = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			StrokeThickness = 0,
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
				new Label { Text = "+", FontSize = 32, TextColor = AppConstants.Primary.MultiplyAlpha(0.85f), HorizontalOptions = LayoutOptions.Center },
				new Label
				{
					Text = "Create Custom Sequence",
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

		var icon = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
			StrokeThickness = 0,
			Content = new Label { Text = "\u2697", FontSize = 20, TextColor = AppConstants.Primary, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
			StrokeShape = new RoundRectangle { CornerRadius = 12 }
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
		var chev = new Label { Text = ">", FontSize = 18, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center };
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

	HorizontalStackLayout SectionHeader(string icon, string text) => new()
	{
		Spacing = 8,
		Children =
		{
			new Label { Text = icon, FontSize = 16, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center },
			new Label { Text = text, FontAttributes = FontAttributes.Bold, FontSize = 16, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center }
		}
	};

	void BuildSetup()
	{
		var top = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
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

		var chip = new Border
		{
			Padding = new Thickness(12, 6),
			BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.12f),
			StrokeThickness = 0,
			Content = new Label { Text = "TANK 1", FontAttributes = FontAttributes.Bold, TextColor = AppConstants.Primary, FontSize = 12 },
			StrokeShape = new RoundRectangle { CornerRadius = 20 },
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};

		top.Children.Add(close);
		Grid.SetColumn(close, 0);
		var setupTitle = new Label { Text = "Setup Analysis", FontAttributes = FontAttributes.Bold, FontSize = 18, VerticalOptions = LayoutOptions.Center };
		top.Children.Add(setupTitle);
		Grid.SetColumn(setupTitle, 1);
		top.Children.Add(chip);
		Grid.SetColumn(chip, 2);

		BodyStack.Children.Add(top);

		var list = new VerticalStackLayout { Spacing = 12, Margin = new Thickness(0, 16, 0, 0) };
		for (var i = 0; i < _selectedMethods.Count; i++)
		{
			var m = _selectedMethods[i];
			list.Children.Add(MethodNumberTile(i + 1, m.Title, m.Unit));
		}
		BodyStack.Children.Add(list);

		FooterHost.IsVisible = true;
		var startBtn = new Button
		{
			Text = "\u25B6  Start Analysis",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold
		};
		startBtn.Clicked += (_, _) => SetState(PhotometerState.Running);
		FooterHost.Children.Add(startBtn);
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

	void BuildRunning()
	{
		BodyStack.Children.Add(CenteredTitle("Running Analysis", "Tank #1"));

		foreach (var m in _selectedMethods)
			BodyStack.Children.Add(ResultTile(m.Title, m.Unit, m.Value, m.Status == MethodStatus.Done, m.Status == MethodStatus.Active));

		BodyStack.Children.Add(new BoxView { HeightRequest = 24 });

		var progRow = new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Spacing = 12 };
		progRow.Children.Add(new ActivityIndicator { IsRunning = true, Color = AppConstants.Primary, WidthRequest = 24, HeightRequest = 24 });
		progRow.Children.Add(new Label { Text = "Processing sequence...", TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center });
		BodyStack.Children.Add(progRow);

		BodyStack.Children.Add(new BoxView { HeightRequest = 24 });

		var complete = new Button
		{
			Text = "\u2713  Complete Measurement",
			HeightRequest = 54,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton
		};
		complete.Clicked += (_, _) => SetState(PhotometerState.Completed);
		BodyStack.Children.Add(complete);
	}

	void BuildCompleted()
	{
		var head = new VerticalStackLayout { Spacing = 12, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 20, 0, 12) };
		head.Children.Add(new Label { Text = "\u2713", FontSize = 48, TextColor = AppConstants.Success, HorizontalTextAlignment = TextAlignment.Center });
		head.Children.Add(new Label { Text = "Analysis Complete", FontAttributes = FontAttributes.Bold, FontSize = 18, HorizontalTextAlignment = TextAlignment.Center });
		head.Children.Add(new Label { Text = "Tank #1", HorizontalTextAlignment = TextAlignment.Center, TextColor = ThemeColors.OnSurfaceMuted });
		BodyStack.Children.Add(head);

		foreach (var m in _selectedMethods.Where(x => x.Status == MethodStatus.Done))
			BodyStack.Children.Add(ResultTile(m.Title, m.Unit, m.Value, true, false));

		BodyStack.Children.Add(new BoxView { HeightRequest = 32 });

		var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)), ColumnSpacing = 12 };
		var discard = new Button { Text = "\u2715  Discard", HeightRequest = 52, BackgroundColor = Colors.Transparent, TextColor = ThemeColors.OnSurface, BorderColor = ThemeColors.Divider, BorderWidth = 1, CornerRadius = (int)AppConstants.RadiusButton };
		discard.Clicked += (_, _) => SetState(PhotometerState.NewAnalysis);
		var save = new Button { Text = "\u2713  Save Results", HeightRequest = 52, BackgroundColor = AppConstants.Success, TextColor = Colors.White, CornerRadius = (int)AppConstants.RadiusButton };
		save.Clicked += (_, _) => SetState(PhotometerState.NewAnalysis);
		row.Children.Add(discard);
		Grid.SetColumn(discard, 0);
		row.Children.Add(save);
		Grid.SetColumn(save, 1);
		BodyStack.Children.Add(row);
	}

	VerticalStackLayout CenteredTitle(string title, string sub) => new()
	{
		HorizontalOptions = LayoutOptions.Center,
		Spacing = 4,
		Margin = new Thickness(0, 20, 0, 12),
		Children =
		{
			new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 18, HorizontalTextAlignment = TextAlignment.Center },
			new Label { Text = sub, HorizontalTextAlignment = TextAlignment.Center, TextColor = ThemeColors.OnSurfaceMuted }
		}
	};

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

	async Task OpenSettings()
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is not null && Application.Current is App app)
			await page.Navigation.PushAsync(app.Services.GetRequiredService<SettingsPage>());
	}

	async void OnBluetooth(object? sender, EventArgs e)
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page?.Navigation is not null && Application.Current is App app)
			await page.Navigation.PushAsync(app.Services.GetRequiredService<DevicePage>());
	}

	public void ApplyTheme() => Rebuild();
}
