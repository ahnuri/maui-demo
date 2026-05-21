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
	MeasurePhotometerView? _photometer;
	MultimeterLogRecallView? _multimeter;
	Halo2MeasureView? _halo2;
	readonly Label _emptyStateLabel;
	readonly Grid _deviceHost;
	readonly Grid _busyOverlay;
	readonly ActivityIndicator _busyIndicator;
	readonly Label _busyLabel;

	readonly MeasureTabViewModel _viewModel;
	int _busyDepth;
	BackButtonBehavior? _photometerBackBehavior;

	public MeasurePhotometerView Photometer => EnsurePhotometer();
	public MultimeterLogRecallView Multimeter => EnsureMultimeter();
	public Halo2MeasureView Halo2 => EnsureHalo2();

	public MeasureTabPage()
	{
		_viewModel = AppServices.Get<MeasureTabViewModel>();
		BindingContext = _viewModel;

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
		HideMeasureContent();
		_emptyStateLabel.IsVisible = true;

		(_busyOverlay, _busyIndicator, _busyLabel) = BuildBusyOverlay();

		var root = new Grid();
		root.Children.Add(_deviceHost);
		root.Children.Add(_busyOverlay);
		Content = root;

		RefreshShellNavigation();
	}

	MeasurePhotometerView EnsurePhotometer()
	{
		if (_photometer is not null)
			return _photometer;

		_photometer = new MeasurePhotometerView();
		_photometer.IsVisible = false;
		_deviceHost.Children.Add(_photometer);
		return _photometer;
	}

	MultimeterLogRecallView EnsureMultimeter()
	{
		if (_multimeter is not null)
			return _multimeter;

		_multimeter = new MultimeterLogRecallView();
		_multimeter.IsVisible = false;
		_deviceHost.Children.Add(_multimeter);
		return _multimeter;
	}

	Halo2MeasureView EnsureHalo2()
	{
		if (_halo2 is not null)
			return _halo2;

		_halo2 = new Halo2MeasureView();
		_halo2.IsVisible = false;
		_deviceHost.Children.Add(_halo2);
		return _halo2;
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
				EnsurePhotometer().IsVisible = true;
				break;
			case MeasureDeviceKind.Multimeter:
				ClearCustomNavigationTitle();
				EnsureMultimeter().IsVisible = true;
				break;
			case MeasureDeviceKind.Halo2:
				EnsureHalo2().IsVisible = true;
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
		if (_photometer is { IsVisible: true } && _viewModel.ActiveDevice == MeasureDeviceKind.Photometer)
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

		if (_photometer is null)
			return;

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
		if (_photometer is not { IsVisible: true } || _viewModel.ActiveDevice != MeasureDeviceKind.Photometer)
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

	public void ShowBusy(string? message = null)
	{
		_busyDepth++;
		_busyLabel.Text = message ?? "Loading…";
		_busyLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
		_busyOverlay.IsVisible = true;
		_busyOverlay.InputTransparent = false;
		_busyIndicator.IsRunning = true;
	}

	public void HideBusy()
	{
		if (_busyDepth > 0)
			_busyDepth--;

		if (_busyDepth != 0)
			return;

		_busyIndicator.IsRunning = false;
		_busyOverlay.IsVisible = false;
		_busyOverlay.InputTransparent = true;
	}

	void HideMeasureContent()
	{
		if (_photometer is not null)
			_photometer.IsVisible = false;
		if (_multimeter is not null)
			_multimeter.IsVisible = false;
		if (_halo2 is not null)
			_halo2.IsVisible = false;
		_emptyStateLabel.IsVisible = _viewModel.ShowEmptyState;
	}

	(Grid overlay, ActivityIndicator indicator, Label label) BuildBusyOverlay()
	{
		var indicator = new ActivityIndicator
		{
			Color = AppConstants.Primary,
			WidthRequest = 36,
			HeightRequest = 36,
			HorizontalOptions = LayoutOptions.Center
		};
		var label = new Label
		{
			FontSize = 14,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(24, 12, 24, 0)
		};
		label.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		var stack = new VerticalStackLayout
		{
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children = { indicator, label }
		};

		var scrim = new BoxView();
		scrim.SetDynamicResource(BoxView.ColorProperty, "OverlayScrim");

		var overlay = new Grid
		{
			IsVisible = false,
			InputTransparent = true,
			Children = { scrim, stack }
		};
		return (overlay, indicator, label);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ApplyNavigationChrome();
		RefreshShellNavigation();
		if (_halo2 is { IsVisible: true })
			_halo2.SyncSettingsFromPreferences();
	}

	public void ApplyTheme()
	{
		_photometer?.ApplyTheme();
		_multimeter?.ApplyTheme();
		_halo2?.ApplyTheme();
		if (_viewModel.ActiveDevice is null)
			_viewModel.RefreshForTheme();
		ApplyNavigationChrome();
		RefreshShellNavigation();
	}
}
