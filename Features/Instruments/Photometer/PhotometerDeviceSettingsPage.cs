using System.Globalization;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>HI97115 on-instrument style settings (not app / cloud account settings).</summary>
public sealed class PhotometerDeviceSettingsPage : ContentPage
{
	readonly PhotometerDeviceSettingsViewModel _vm;
	readonly LocalizationService _loc;

	public PhotometerDeviceSettingsPage()
		: this(AppServices.Get<PhotometerDeviceSettingsViewModel>()) { }

	public PhotometerDeviceSettingsPage(PhotometerDeviceSettingsViewModel viewModel)
	{
		_vm = viewModel;
		_loc = viewModel.Loc;
		BindingContext = _vm;
		Title = _loc.T("Photometer_Settings_PageTitle");
		ApplyChrome();

		var foot1 = Footnote(_loc.T("Photometer_Settings_FootnoteMethodGroup"));
		var foot2 = Footnote(_loc.T("Photometer_Settings_FootnoteSync"));

		var separatorGrid = BuildSeparatorSegment();

		var backlightSlider = new Slider(0, 100, _vm.BacklightPercent)
		{
			MinimumTrackColor = AppConstants.Primary,
			MaximumTrackColor = ThemeColors.SliderTrackBg,
			ThumbColor = AppConstants.Primary,
			HorizontalOptions = LayoutOptions.Fill
		};
		backlightSlider.SetBinding(Slider.ValueProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.BacklightPercent), source: _vm, mode: BindingMode.TwoWay));
		var backlightPct = new Label
		{
			FontSize = 15,
			MinimumWidthRequest = 44,
			HorizontalTextAlignment = TextAlignment.End,
			VerticalOptions = LayoutOptions.Center
		};
		backlightPct.SetBinding(Label.TextProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.BacklightPercent), source: _vm, mode: BindingMode.OneWay,
				converter: new PercentLabelConverter()));

		var contrastSlider = new Slider(0, 100, _vm.ContrastPercent)
		{
			MinimumTrackColor = AppConstants.Primary,
			MaximumTrackColor = ThemeColors.SliderTrackBg,
			ThumbColor = AppConstants.Primary,
			HorizontalOptions = LayoutOptions.Fill
		};
		contrastSlider.SetBinding(Slider.ValueProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.ContrastPercent), source: _vm, mode: BindingMode.TwoWay));
		var contrastPct = new Label
		{
			FontSize = 15,
			MinimumWidthRequest = 44,
			HorizontalTextAlignment = TextAlignment.End,
			VerticalOptions = LayoutOptions.Center
		};
		contrastPct.SetBinding(Label.TextProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.ContrastPercent), source: _vm, mode: BindingMode.OneWay,
				converter: new PercentLabelConverter()));

		var beepSwitch = new Switch { OnColor = AppConstants.Primary, ThumbColor = Colors.White };
		beepSwitch.SetBinding(Switch.IsToggledProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.BeepEnabled), source: _vm, mode: BindingMode.TwoWay));

		var tutorialSwitch = new Switch { OnColor = AppConstants.Primary, ThumbColor = Colors.White };
		tutorialSwitch.SetBinding(Switch.IsToggledProperty,
			new Binding(nameof(PhotometerDeviceSettingsViewModel.TutorialEnabled), source: _vm, mode: BindingMode.TwoWay));

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16, 8, 16, 32),
				Spacing = 18,
				Children =
				{
					new Label
					{
						Text = _loc.T("Photometer_Settings_DeviceHeader"),
						FontSize = 13,
						FontAttributes = FontAttributes.Bold,
						TextColor = ThemeColors.OnSurfaceVariant
					},
					GroupedStack(
						ChevronRow(_loc.T("Photometer_Settings_StartupView"), nameof(PhotometerDeviceSettingsViewModel.StartupViewLabel), _vm,
							async () => await _vm.OpenStartupViewInfoCommand.ExecuteAsync(null)),
						foot1,
						ChevronRowPlaceholder(_loc.T("Photometer_Settings_ChemicalForm"), _loc.T("Photometer_Settings_ChemicalFormValue")),
						SyncRow(_vm),
						foot2),
					SectionHeader(_loc.T("Photometer_Settings_DeviceSection")),
					GroupedStack(
						SliderRow(_loc.T("Photometer_Settings_Backlight"), backlightSlider, backlightPct),
						Divider(),
						SliderRow(_loc.T("Photometer_Settings_Contrast"), contrastSlider, contrastPct),
						Divider(),
						SegmentRow(_loc.T("Photometer_Settings_Separator"), separatorGrid),
						Divider(),
						ChevronRowValue(_loc.T("Photometer_Settings_Language"), nameof(PhotometerDeviceSettingsViewModel.DeviceLanguage), _vm),
						Divider(),
						SwitchRow(_loc.T("Photometer_Settings_Beep"), beepSwitch),
						Divider(),
						SwitchRow(_loc.T("Photometer_Settings_Tutorial"), tutorialSwitch))
				}
			}
		};
	}

	public void ApplyTheme() => ApplyChrome();

	void ApplyChrome()
	{
		BackgroundColor = ThemeColors.StoreGroupedBackground;
		ShellChrome.ApplyStandard(this);
	}

	static Label SectionHeader(string text) => new()
	{
		Text = text,
		FontSize = 13,
		FontAttributes = FontAttributes.Bold,
		TextColor = ThemeColors.OnSurfaceVariant,
		CharacterSpacing = 0.5,
		Margin = new Thickness(4, 4, 0, 0)
	};

	static Label Footnote(string text) => new()
	{
		Text = text,
		FontSize = 13,
		TextColor = ThemeColors.OnSurfaceVariant,
		LineBreakMode = LineBreakMode.WordWrap,
		Margin = new Thickness(16, 0, 16, 8)
	};

	Border GroupedStack(params View[] children)
	{
		var stack = new VerticalStackLayout { Spacing = 0 };
		foreach (var c in children)
			stack.Children.Add(c);

		var card = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Padding = new Thickness(0, 4, 0, 4),
			Content = stack
		};
		card.Shadow = new Shadow
		{
			Brush = new SolidColorBrush(ThemeColors.SoftShadow),
			Offset = new Point(0, 1),
			Radius = 8,
			Opacity = 1
		};
		return card;
	}

	static BoxView Divider() => new()
	{
		HeightRequest = 1,
		Margin = new Thickness(16, 0, 0, 0),
		Color = ThemeColors.Divider
	};

	Border ChevronRow(string title, string bindingPath, PhotometerDeviceSettingsViewModel vm, Func<Task>? infoTap = null)
	{
		var grid = RowGrid();
		var titleLbl = new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center };
		grid.Children.Add(titleLbl);
		var trail = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End };
		var val = new Label { FontSize = 17, TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center };
		val.SetBinding(Label.TextProperty, new Binding(bindingPath, source: vm));
		trail.Children.Add(val);
		trail.Children.Add(new Label
		{
			Text = "\u203A",
			FontSize = 22,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center
		});
		grid.Children.Add(trail);
		Grid.SetColumn(trail, 1);

		var border = PlainRow(grid);
		if (infoTap is not null)
		{
			var tap = new TapGestureRecognizer();
			tap.Tapped += async (_, _) => await infoTap();
			border.GestureRecognizers.Add(tap);
		}
		return border;
	}

	Border ChevronRowPlaceholder(string title, string value)
	{
		var grid = RowGrid();
		grid.Children.Add(new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center });
		var trail = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End };
		trail.Children.Add(new Label { Text = value, FontSize = 17, TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center });
		trail.Children.Add(new Label { Text = "\u203A", FontSize = 22, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center });
		grid.Children.Add(trail);
		Grid.SetColumn(trail, 1);
		return PlainRow(grid);
	}

	Border ChevronRowValue(string title, string bindingPath, PhotometerDeviceSettingsViewModel vm)
	{
		var grid = RowGrid();
		grid.Children.Add(new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center });
		var trail = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.End };
		var val = new Label { FontSize = 17, TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center };
		val.SetBinding(Label.TextProperty, new Binding(bindingPath, source: vm));
		trail.Children.Add(val);
		trail.Children.Add(new Label { Text = "\u203A", FontSize = 22, TextColor = AppConstants.Primary, VerticalOptions = LayoutOptions.Center });
		grid.Children.Add(trail);
		Grid.SetColumn(trail, 1);
		return PlainRow(grid);
	}

	Border SyncRow(PhotometerDeviceSettingsViewModel vm)
	{
		var grid = RowGrid();
		var col = new VerticalStackLayout { Spacing = 2 };
		col.Children.Add(new Label
		{
			Text = _loc.T("Photometer_Settings_SyncRow"),
			FontSize = 17,
			TextColor = ThemeColors.OnSurface,
			LineBreakMode = LineBreakMode.WordWrap
		});
		grid.Children.Add(col);
		var icon = new Label
		{
			Text = "\u21BB",
			FontSize = 22,
			TextColor = AppConstants.Primary,
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};
		grid.Children.Add(icon);
		Grid.SetColumn(icon, 1);
		var border = PlainRow(grid);
		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await vm.SyncLogRecallCommand.ExecuteAsync(null);
		border.GestureRecognizers.Add(tap);
		return border;
	}

	Border SliderRow(string title, Slider slider, Label pctLabel)
	{
		var outer = new VerticalStackLayout { Spacing = 10, Margin = new Thickness(16, 12, 16, 12) };
		var top = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto))
		};
		top.Children.Add(new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center });
		top.Children.Add(pctLabel);
		Grid.SetColumn(pctLabel, 1);
		outer.Children.Add(top);
		outer.Children.Add(slider);
		return PlainRow(outer);
	}

	Border SegmentRow(string title, View segment)
	{
		var grid = RowGrid();
		grid.Children.Add(new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center });
		segment.VerticalOptions = LayoutOptions.Center;
		grid.Children.Add(segment);
		Grid.SetColumn(segment, 1);
		return PlainRow(grid);
	}

	Border SwitchRow(string title, Switch sw)
	{
		var grid = RowGrid();
		grid.Children.Add(new Label { Text = title, FontSize = 17, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center });
		sw.HorizontalOptions = LayoutOptions.End;
		grid.Children.Add(sw);
		Grid.SetColumn(sw, 1);
		return PlainRow(grid);
	}

	static Grid RowGrid() => new()
	{
		ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)),
		ColumnSpacing = 12,
		Padding = new Thickness(16, 10, 16, 10),
		MinimumHeightRequest = 48
	};

	static Border PlainRow(View inner) => new() { BackgroundColor = Colors.Transparent, StrokeThickness = 0, Content = inner };

	Grid BuildSeparatorSegment()
	{
		var dotBtn = new Border { Padding = new Thickness(18, 8), StrokeShape = new RoundRectangle { CornerRadius = 8 } };
		var commaBtn = new Border { Padding = new Thickness(18, 8), StrokeShape = new RoundRectangle { CornerRadius = 8 } };
		dotBtn.Content = new Label { Text = _loc.T("Photometer_Settings_SeparatorDot"), FontSize = 15, HorizontalOptions = LayoutOptions.Center };
		commaBtn.Content = new Label { Text = _loc.T("Photometer_Settings_SeparatorComma"), FontSize = 15, HorizontalOptions = LayoutOptions.Center };

		void StyleButtons(bool commaOn)
		{
			dotBtn.BackgroundColor = commaOn ? ThemeColors.ChipUnselected : ThemeColors.Surface;
			dotBtn.StrokeThickness = commaOn ? 0 : 1;
			dotBtn.Stroke = AppConstants.Primary;
			((Label)dotBtn.Content).TextColor = commaOn ? ThemeColors.OnSurfaceVariant : AppConstants.Primary;

			commaBtn.BackgroundColor = commaOn ? ThemeColors.Surface : ThemeColors.ChipUnselected;
			commaBtn.StrokeThickness = commaOn ? 1 : 0;
			commaBtn.Stroke = AppConstants.Primary;
			((Label)commaBtn.Content).TextColor = commaOn ? AppConstants.Primary : ThemeColors.OnSurfaceVariant;
		}

		StyleButtons(_vm.SeparatorUsesComma);
		_vm.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(PhotometerDeviceSettingsViewModel.SeparatorUsesComma))
				MainThread.BeginInvokeOnMainThread(() => StyleButtons(_vm.SeparatorUsesComma));
		};

		var dotTap = new TapGestureRecognizer();
		dotTap.Tapped += (_, _) => _vm.SeparatorUsesComma = false;
		dotBtn.GestureRecognizers.Add(dotTap);
		var commaTap = new TapGestureRecognizer();
		commaTap.Tapped += (_, _) => _vm.SeparatorUsesComma = true;
		commaBtn.GestureRecognizers.Add(commaTap);

		var wrap = new Border
		{
			HorizontalOptions = LayoutOptions.End,
			Padding = 3,
			BackgroundColor = ThemeColors.SliderTrackBg,
			StrokeShape = new RoundRectangle { CornerRadius = 10 },
			Content = new HorizontalStackLayout { Spacing = 4, Children = { dotBtn, commaBtn } }
		};
		wrap.StrokeThickness = 0;

		var grid = new Grid();
		grid.Children.Add(wrap);
		return grid;
	}

	sealed class PercentLabelConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			var loc = ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();
			if (value is double d)
				return loc.T("Common_PercentFormat", (int)Math.Round(Math.Clamp(d, 0, 100)));
			return loc.T("Common_PercentFormat", 0);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
