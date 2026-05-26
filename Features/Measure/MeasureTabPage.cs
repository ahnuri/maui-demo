using System.Windows.Input;
using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Features.Instruments;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Measure;

/// <summary>
/// Measure tab host: device-agnostic shell that delegates UI and navigation to instrument modules.
/// </summary>
public sealed class MeasureTabPage : ContentPage, IMeasureTabNavigationHost
{
	readonly Label _emptyStateLabel;
	readonly Grid _deviceHost;
	readonly Grid _busyOverlay;
	readonly ActivityIndicator _busyIndicator;
	readonly Label _busyLabel;

	readonly MeasureTabViewModel _viewModel;
	readonly InstrumentMeasureHost _measureHost;
	readonly Dictionary<InstrumentKind, View> _moduleViews = new();
	IInstrumentMeasureModule? _activeModule;
	int _busyDepth;
	BackButtonBehavior? _backBehavior;

	public MeasureTabPage()
	{
		_viewModel = AppServices.Get<MeasureTabViewModel>();
		_measureHost = AppServices.Get<InstrumentMeasureHost>();
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

		foreach (var module in _measureHost.All)
		{
			var view = module.Content;
			view.IsVisible = false;
			_moduleViews[module.Kind] = view;
			_deviceHost.Children.Add(view);
		}

		(_busyOverlay, _busyIndicator, _busyLabel) = BuildBusyOverlay();

		var root = new Grid();
		root.Children.Add(_deviceHost);
		root.Children.Add(_busyOverlay);
		Content = root;

		RefreshShellNavigation();
	}

	public ContentPage Page => this;

	public LocalizationService Localization =>
		((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public void SetTitle(string title) => Title = title;

	public void ClearTitleView() => Shell.SetTitleView(this, null);

	public void SetTitleView(View titleView) => Shell.SetTitleView(this, titleView);

	public void EnableFlyout()
	{
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
		Shell.SetBackButtonBehavior(this, null);
		_backBehavior = null;
	}

	public void DisableFlyout() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

	public void SetBackCommand(ICommand command)
	{
		_backBehavior ??= new BackButtonBehavior();
		_backBehavior.IsVisible = true;
		_backBehavior.IsEnabled = true;
		_backBehavior.TextOverride = string.Empty;
		_backBehavior.Command = command;
		Shell.SetBackButtonBehavior(this, _backBehavior);
	}

	public void ClearBackBehavior() => EnableFlyout();

	public void ClearToolbar() => ToolbarItems.Clear();

	public void AddToolbarItem(ToolbarItem item) => ToolbarItems.Add(item);

	/// <summary>Selects and shows the measure UI for an instrument family.</summary>
	public void SelectDevice(InstrumentKind kind)
	{
		_viewModel.Select(kind);
		HideMeasureContent();
		_emptyStateLabel.IsVisible = false;

		_activeModule = _measureHost.Get(kind);
		_moduleViews[kind].IsVisible = true;

		if (_viewModel.UsesHaloNavigationTitle)
			ClearTitleView();

		ApplyNavigationChrome();
		RefreshShellNavigation();
	}

	/// <summary>Clears the active device and returns to the empty measure state.</summary>
	public void DisconnectDevice()
	{
		_viewModel.Disconnect();
		_activeModule = null;
		HideMeasureContent();
		ClearTitleView();
		_emptyStateLabel.Text = _viewModel.EmptyStateMessage;
		ApplyNavigationChrome();
		RefreshShellNavigation();
	}

	void ApplyNavigationChrome()
	{
		if (_activeModule?.UsesLabChrome == true)
			ShellChrome.ApplyLab(this);
		else
			ShellChrome.ApplyStandard(this);
	}

	/// <summary>Delegates shell chrome to the active instrument module.</summary>
	public void RefreshShellNavigation()
	{
		if (_activeModule?.TryRefreshNavigation(this, _viewModel) == true)
			return;

		EnableFlyout();

		if (_viewModel.HasActiveDevice && _activeModule is not null)
		{
			ClearTitleView();
			Title = _activeModule.GetNavigationTitle(Localization);
			return;
		}

		ClearTitleView();
		Title = Localization.T("Shell_Measure");
	}

	public async Task DisconnectAndOpenDevicesAsync()
	{
		DisconnectDevice();
		await Navigation.PushAsync(AppServices.Get<DevicePage>());
	}

	public void ShowBusy(string? message = null)
	{
		_busyDepth++;
		_busyLabel.Text = message ?? Localization.T("Measure_LoadingFallback");
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
		foreach (var view in _moduleViews.Values)
			view.IsVisible = false;
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
		_activeModule?.OnAppearing();
	}

	public void ApplyTheme()
	{
		foreach (var module in _measureHost.All)
			module.ApplyTheme();

		if (_viewModel.ActiveDevice is null)
			_viewModel.RefreshForTheme();

		ApplyNavigationChrome();
		RefreshShellNavigation();
	}
}
