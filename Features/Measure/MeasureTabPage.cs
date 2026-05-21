using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Measure;

/// <summary>Measure tab: photometer, multimeter, or Halo 2 view (opened from Devices page).</summary>
public sealed class MeasureTabPage : ContentPage
{
	const double SheetSlideDistance = 100;

	readonly MeasurePhotometerView _photometer;
	readonly MultimeterLogRecallView _multimeter;
	readonly Halo2MeasureView _halo2;
	readonly Label _emptyStateLabel;
	readonly Grid _deviceHost;
	readonly Grid _overlay;
	readonly BoxView _pickerScrim;
	readonly Border _pickerSheet;

	readonly MeasureTabViewModel _viewModel;
	BackButtonBehavior? _photometerBackBehavior;

	public MeasurePhotometerView Photometer => _photometer;
	public MultimeterLogRecallView Multimeter => _multimeter;
	public Halo2MeasureView Halo2 => _halo2;

	/// <summary>Shell / XAML default constructor.</summary>
	public MeasureTabPage() : this(new MeasurePhotometerView(), new MultimeterLogRecallView(), new Halo2MeasureView())
	{
	}

	public MeasureTabPage(MeasurePhotometerView photometer, MultimeterLogRecallView multimeter, Halo2MeasureView halo2)
	{
		_viewModel = AppServices.Get<MeasureTabViewModel>();
		BindingContext = _viewModel;

		_photometer = photometer;
		_multimeter = multimeter;
		_halo2 = halo2;
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
		Shell.SetNavBarIsVisible(this, true);
		Shell.SetNavBarHasShadow(this, false);
		Shell.SetBackButtonBehavior(this, null);
		ApplyNavigationChrome();

		_emptyStateLabel = new Label
		{
			Text = _viewModel.EmptyStateMessage,
			FontSize = 15,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(32, 0),
			LineBreakMode = LineBreakMode.WordWrap
		};
		_emptyStateLabel.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

		_deviceHost = new Grid();
		_deviceHost.Children.Add(_emptyStateLabel);
		_deviceHost.Children.Add(_photometer);
		_deviceHost.Children.Add(_multimeter);
		_deviceHost.Children.Add(_halo2);
		HideMeasureContent();
		_emptyStateLabel.IsVisible = true;

		(_overlay, _pickerScrim, _pickerSheet) = BuildPickerOverlay();

		var root = new Grid();
		root.Children.Add(_deviceHost);
		root.Children.Add(_overlay);
		Content = root;

		RefreshShellNavigation();
	}

	/// <summary>Selects and shows a measure device (called from Devices page or shell).</summary>
	public void SelectDevice(MeasureDeviceKind kind)
	{
		_viewModel.Select(kind);
		HideMeasureContent();
		_emptyStateLabel.IsVisible = false;

		switch (kind)
		{
			case MeasureDeviceKind.Photometer:
				_photometer.IsVisible = true;
				break;
			case MeasureDeviceKind.Multimeter:
				ClearCustomNavigationTitle();
				_multimeter.IsVisible = true;
				break;
			case MeasureDeviceKind.Halo2:
				_halo2.IsVisible = true;
				break;
		}

		if (_viewModel.UsesHaloNavigationTitle)
			ClearCustomNavigationTitle();

		ApplyNavigationChrome();
		RefreshShellNavigation();
	}

	/// <summary>Clears the active device and returns to the empty measure state.</summary>
	public void DisconnectDevice()
	{
		_viewModel.Disconnect();
		HideMeasureContent();
		ClearCustomNavigationTitle();
		_emptyStateLabel.Text = _viewModel.EmptyStateMessage;
		ApplyNavigationChrome();
		RefreshShellNavigation();
	}

	void ApplyNavigationChrome()
	{
		if (_viewModel.ActiveDevice == MeasureDeviceKind.Halo2)
			ShellChrome.ApplyLab(this);
		else
			ShellChrome.ApplyStandard(this);
	}

	/// <summary>Shell title + toolbar (respects photometer measurement flow — no profile while in flow).</summary>
	public void RefreshShellNavigation()
	{
		if (_photometer.IsVisible && _viewModel.ActiveDevice == MeasureDeviceKind.Photometer)
		{
			_photometer.SyncNavigationChrome();
			return;
		}

		EnsureFlyoutEnabled();

		if (_viewModel.HasActiveDevice)
		{
			ClearCustomNavigationTitle();
			Title = _viewModel.NavigationTitle;
			RefreshMeasureToolbar();
			return;
		}

		if (Application.Current is App app)
		{
			var loc = app.Services.GetRequiredService<LocalizationService>();
			ClearCustomNavigationTitle();
			Title = loc.T("Shell_Measure");
		}

		RefreshMeasureToolbar();
	}

	void RefreshMeasureToolbar(PhotometerMeasureViewModel? photometer = null)
	{
		ToolbarItems.Clear();

		photometer ??= _photometer.PhotometerViewModel;

		if (_photometer.IsVisible
		    && _viewModel.ActiveDevice == MeasureDeviceKind.Photometer
		    && photometer.IsInMeasurementFlow)
			return;

		if (Application.Current is not App app)
			return;

		ToolbarItems.Add(NavToolbar.CreateProfileItem(this, app));
	}

	void ClearCustomNavigationTitle() => Shell.SetTitleView(this, null);

	void EnsureFlyoutEnabled()
	{
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
		Shell.SetBackButtonBehavior(this, null);
		_photometerBackBehavior = null;
	}

	void ApplyPhotometerShellChrome(PhotometerMeasureViewModel photometer)
	{
		if (photometer.IsInMeasurementFlow)
		{
			Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
			_photometerBackBehavior ??= new BackButtonBehavior();
			_photometerBackBehavior.IsVisible = true;
			_photometerBackBehavior.IsEnabled = true;
			_photometerBackBehavior.TextOverride = string.Empty;
			_photometerBackBehavior.Command = new Command(photometer.NavigateBack);
			Shell.SetBackButtonBehavior(this, _photometerBackBehavior);
		}
		else
		{
			EnsureFlyoutEnabled();
		}
	}

	/// <summary>Overview: standard title + profile + flyout. Flow: meter/tank nav, back button, no profile.</summary>
	public void SyncPhotometerNavigation(PhotometerMeasureViewModel photometer)
	{
		if (!_photometer.IsVisible || _viewModel.ActiveDevice != MeasureDeviceKind.Photometer)
			return;

		ApplyPhotometerShellChrome(photometer);

		if (photometer.IsNewAnalysis)
		{
			ClearCustomNavigationTitle();
			Title = _viewModel.NavigationTitle;
		}
		else
		{
			Title = string.Empty;
			ApplyPhotometerFlowTitleView(photometer);
		}

		RefreshMeasureToolbar(photometer);
	}

	void ApplyPhotometerFlowTitleView(PhotometerMeasureViewModel photometer)
	{
		var meterName = new Label
		{
			Text = _viewModel.NavigationTitle,
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		meterName.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		var tankName = new Label
		{
			Text = photometer.SelectedTankDisplay,
			FontSize = 13,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		tankName.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

		var titleStack = new VerticalStackLayout
		{
			Spacing = 1,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { meterName, tankName }
		};

		Shell.SetTitleView(this, titleStack);
	}

	public async Task DisconnectAndOpenDevicesAsync()
	{
		DisconnectDevice();
		await Navigation.PushAsync(AppServices.Get<DevicePage>());
	}

	/// <summary>Optional bottom-sheet picker (e.g. legacy flows).</summary>
	public void ShowDevicePicker() => _ = PresentPickerAsync();

	async Task PresentPickerAsync()
	{
		if (_overlay.IsVisible && _pickerScrim.Opacity >= 0.99)
			return;

		_overlay.IsVisible = true;
		_overlay.InputTransparent = false;
		_pickerSheet.TranslationY = SheetSlideDistance;
		_pickerScrim.Opacity = 0;
		await Task.WhenAll(
			_pickerScrim.FadeToAsync(1, 220, Easing.CubicOut),
			_pickerSheet.TranslateToAsync(0, 0, 280, Easing.CubicOut));
	}

	void HideMeasureContent()
	{
		_photometer.IsVisible = false;
		_multimeter.IsVisible = false;
		_halo2.IsVisible = false;
		_emptyStateLabel.IsVisible = _viewModel.ShowEmptyState;
	}

	async Task DismissPickerAsync()
	{
		if (!_overlay.IsVisible)
			return;

		await Task.WhenAll(
			_pickerScrim.FadeToAsync(0, 200, Easing.CubicIn),
			_pickerSheet.TranslateToAsync(0, SheetSlideDistance, 220, Easing.CubicIn));
		_overlay.IsVisible = false;
		_overlay.InputTransparent = true;
	}

	async void OnDismissMeasurePicker(object? sender, TappedEventArgs e) => await DismissPickerAsync();

	async void OnPickPhotometer(object? sender, TappedEventArgs e)
	{
		await DismissPickerAsync();
		SelectDevice(MeasureDeviceKind.Photometer);
	}

	async void OnPickMultimeter(object? sender, TappedEventArgs e)
	{
		await DismissPickerAsync();
		SelectDevice(MeasureDeviceKind.Multimeter);
	}

	async void OnPickHalo2(object? sender, TappedEventArgs e)
	{
		await DismissPickerAsync();
		SelectDevice(MeasureDeviceKind.Halo2);
	}

	(Grid overlay, BoxView scrim, Border sheet) BuildPickerOverlay()
	{
		var scrim = new BoxView();
		scrim.SetDynamicResource(BoxView.ColorProperty, "OverlayScrim");
		var scrimTap = new TapGestureRecognizer();
		scrimTap.Tapped += OnDismissMeasurePicker;
		scrim.GestureRecognizers.Add(scrimTap);

		var sheet = new Border
		{
			VerticalOptions = LayoutOptions.End,
			StrokeThickness = 0,
			Padding = new Thickness(20, 16, 20, 24),
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20, 20, 0, 0) }
		};
		sheet.SetDynamicResource(Border.BackgroundColorProperty, "Surface");
		sheet.Shadow = new Shadow
		{
			Brush = new SolidColorBrush(ThemeColors.SoftShadow),
			Offset = new Point(0, -4),
			Radius = 16,
			Opacity = 1
		};

		var photoRow = BuildDeviceRow(
			"HI",
			"HI97105 Photometer",
			"Measure and download with Marine Master Multiparameter Photometer",
			isTeal: true,
			OnPickPhotometer);

		var divider = new BoxView { HeightRequest = 1 };
		divider.SetDynamicResource(BoxView.ColorProperty, "Divider");

		var multiRow = BuildDeviceRow(
			"94",
			"HI98x94 - Multiparameter",
			"Download logs with HI98x94 - Multiparameter",
			isTeal: false,
			OnPickMultimeter);

		var divider2 = new BoxView { HeightRequest = 1 };
		divider2.SetDynamicResource(BoxView.ColorProperty, "Divider");

		var haloRow = BuildDeviceRow(
			"tab_halo",
			"Halo 2",
			"Live pH, mV and temperature tracking with tags",
			isTeal: true,
			OnPickHalo2);

		var stack = new VerticalStackLayout { Spacing = 16 };
		var handle = new BoxView { HeightRequest = 4, WidthRequest = 40, CornerRadius = 2, HorizontalOptions = LayoutOptions.Center };
		handle.SetDynamicResource(BoxView.ColorProperty, "HandleBar");
		stack.Children.Add(handle);

		var titleLabel = new Label
		{
			Text = "Select Device to Measure/Download",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};
		titleLabel.SetDynamicResource(Label.TextColorProperty, "OnSurface");
		stack.Children.Add(titleLabel);

		stack.Children.Add(photoRow);
		stack.Children.Add(divider);
		stack.Children.Add(multiRow);
		stack.Children.Add(divider2);
		stack.Children.Add(haloRow);
		sheet.Content = stack;

		var grid = new Grid { IsVisible = false, InputTransparent = true };
		grid.Children.Add(scrim);
		grid.Children.Add(sheet);
		return (grid, scrim, sheet);
	}

	Border BuildDeviceRow(string thumb, string title, string subtitle, bool isTeal, EventHandler<TappedEventArgs> onTap)
	{
		var tileBg = isTeal ? "SubtleTeal" : "SubtleGreen";
		var row = new Border
		{
			StrokeThickness = 0,
			Padding = new Thickness(12)
		};
		row.SetDynamicResource(Border.BackgroundColorProperty, "SurfaceSecondary");
		var tap = new TapGestureRecognizer();
		tap.Tapped += onTap;
		row.GestureRecognizers.Add(tap);

		var icon = new Border
		{
			WidthRequest = 36,
			HeightRequest = 36,
			StrokeThickness = 1,
			Stroke = AppConstants.Primary,
			StrokeShape = new RoundRectangle { CornerRadius = 8 }
		};
		icon.SetDynamicResource(Border.BackgroundColorProperty, tileBg);
		icon.Content = thumb == "tab_halo"
			? new Image
			{
				Source = "tab_halo.png",
				WidthRequest = 26,
				HeightRequest = 26,
				Aspect = Aspect.AspectFit,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
			: new Label
			{
				Text = thumb,
				FontSize = 11,
				FontAttributes = FontAttributes.Bold,
				TextColor = AppConstants.Primary,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

		var titleLbl = new Label
		{
			Text = title,
			FontAttributes = FontAttributes.Bold,
			FontSize = 15
		};
		titleLbl.SetDynamicResource(Label.TextColorProperty, "OnSurface");
		var subLbl = new Label
		{
			Text = subtitle,
			FontSize = 12,
			LineBreakMode = LineBreakMode.WordWrap
		};
		subLbl.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

		var inner = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)),
			ColumnSpacing = 12
		};
		inner.Children.Add(icon);
		Grid.SetRowSpan(icon, 2);
		inner.Children.Add(titleLbl);
		Grid.SetColumn(titleLbl, 1);
		inner.Children.Add(subLbl);
		Grid.SetColumn(subLbl, 1);
		Grid.SetRow(subLbl, 1);
		var chevron = new Label
		{
			Text = "\u203A",
			FontSize = 28,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center
		};
		inner.Children.Add(chevron);
		Grid.SetColumn(chevron, 2);
		Grid.SetRowSpan(chevron, 2);

		row.Content = inner;
		return row;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ApplyNavigationChrome();
		RefreshShellNavigation();
		if (_halo2.IsVisible)
			_halo2.SyncSettingsFromPreferences();
	}

	public void ApplyTheme()
	{
		_photometer.ApplyTheme();
		_multimeter.ApplyTheme();
		_halo2.ApplyTheme();
		if (_viewModel.ActiveDevice is null)
			_viewModel.RefreshForTheme();
		ApplyNavigationChrome();
		RefreshShellNavigation();
	}
}
