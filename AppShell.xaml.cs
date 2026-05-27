using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Help;
using HannaUIDemo.Features.Home;
using HannaUIDemo.Features.Info;
using HannaUIDemo.Features.Instruments.Logs;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Features.Flyout;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace HannaUIDemo;

/// <summary>
/// Root Shell: flyout menu, tab routes (Home, Measure, Logs, Help), and cross-tab navigation helpers.
/// </summary>
public partial class AppShell : Shell
{
	string? _lastNavLocation;
	readonly LocalizationService _localization;
	InstrumentKind? _pendingMeasureDevice;
	readonly AppFlyoutViewModel _flyoutViewModel;
	readonly AppFlyoutView _flyoutView;
	readonly MeasureDevicePickerPresenter _measureDevicePicker;

	public AppShell()
	{
		InitializeComponent();
		Halo2Routes.Register();

		var appHost = (App)Application.Current!;
		_flyoutViewModel = appHost.Services.GetRequiredService<AppFlyoutViewModel>();
		_flyoutView = appHost.Services.GetRequiredService<AppFlyoutView>();
		_localization = appHost.Services.GetRequiredService<LocalizationService>();
		_measureDevicePicker = new MeasureDevicePickerPresenter(OnMeasureDevicePickedAsync, _localization);
		FlyoutContent = _flyoutView;

		PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
				_flyoutView.ResetCollapse();
		};

		LocalizationService.CultureChanged += OnLocalizationCultureChanged;
		RefreshShellLocalization();

		Navigated += OnShellNavigated;
		ApplyShellChrome();
		_flyoutViewModel.SetSelectedRoute(CurrentState?.Location?.OriginalString);
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(600), PreWarmMeasureTab);
	}

	public void PreWarmMeasureTab() => _ = MeasureShellContent.Content;

	void OnLocalizationCultureChanged(object? sender, EventArgs e) =>
		MainThread.BeginInvokeOnMainThread(RefreshShellLocalization);

	void RefreshShellLocalization()
	{
		HomeShellContent.Title = _localization.T("Shell_Home");
		MeasureShellContent.Title = _localization.T("Shell_Measure");
		LogsShellContent.Title = _localization.T("Shell_LogHistory");
		//InfoShellContent.Title = _localization.T("Shell_Information");
		HelpShellContent.Title = _localization.T("Shell_Help");

		_flyoutViewModel.RefreshForTheme();

		if (HomeShellContent.Content is HomeTabPage home)
			NavToolbar.ConfigureLanding(home);
		if (MeasureShellContent.Content is MeasureTabPage measure)
			measure.RefreshShellNavigation();
		if (LogsShellContent.Content is LogsTabPage logs)
			NavToolbar.Configure(logs, "Shell_LogHistory");
		// if (InfoShellContent.Content is InfoTabPage info)
		// 	NavToolbar.Configure(info, "PageToolbar_Info");
		if (HelpShellContent.Content is HelpTabPage help)
			NavToolbar.Configure(help, "PageToolbar_Help");

		RefreshPushedPagesLocalization();
	}

	void RefreshPushedPagesLocalization()
	{
		if (CurrentPage?.Navigation?.NavigationStack is not { } stack)
			return;

		foreach (var page in stack)
		{
			switch (page)
			{
				case Features.Device.DevicePage device:
					device.ApplyTheme();
					break;
				case Features.Settings.SettingsPage settings:
					settings.ApplyTheme();
					break;
			}
		}
	}

	void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
	{
		ApplyShellChrome();
		_flyoutViewModel.SetSelectedRoute(e.Current?.Location?.OriginalString);

		var loc = e.Current?.Location?.OriginalString ?? string.Empty;
		var wasMeasure = IsMeasureRoute(_lastNavLocation);
		var isMeasure = IsMeasureRoute(loc);
		if (isMeasure && !wasMeasure && _pendingMeasureDevice is { } pending
		    && MeasureShellContent.Content is MeasureTabPage mp)
		{
			mp.SelectDevice(pending);
			_pendingMeasureDevice = null;
		}

		_lastNavLocation = loc;
	}

	/// <summary>True when the shell is already on the Measure tab.</summary>
	public bool IsOnMeasureTab() => IsMeasureRoute(CurrentState?.Location?.OriginalString);

	MeasureTabPage? GetMeasureTabPage() => MeasureShellContent.Content as MeasureTabPage;

	/// <summary>Shows device picker on the current page without navigating away first.</summary>
	public Task PresentMeasureDevicePickerAsync() => _measureDevicePicker.PresentAsync();

	async Task OnMeasureDevicePickedAsync(InstrumentKind kind)
	{
		if (IsOnMeasureTab() && GetMeasureTabPage() is MeasureTabPage measureTab)
		{
			measureTab.SelectDevice(kind);
			return;
		}

		await NavigateToMeasureDeviceAsync(kind);
	}

	string GetOpeningMessage(InstrumentKind kind) =>
		InstrumentRegistry.GetOpeningMessage(kind, _localization);

	/// <summary>
	/// Switch to Measure tab and open the given device view. Tab navigation
	/// is non-animated so the device row's "Measure" button feels instant —
	/// Shell tab switches have nothing to animate visually anyway (the tab
	/// content swap is a hard cut) and skipping animation removes the
	/// default ~150-200ms wait.
	/// </summary>
	public Task NavigateToMeasureDeviceAsync(InstrumentKind device) =>
		NavigateToMeasureDeviceAsync(device, animate: false);

	/// <summary>
	/// Overload that lets callers opt back into the animated nav + spinner
	/// overlay (used by the bottom-sheet device picker, where the slower
	/// transition is part of the picker's choreography).
	/// </summary>
	public async Task NavigateToMeasureDeviceAsync(InstrumentKind device, bool animate)
	{
		var showNavigating = animate && !IsOnMeasureTab();
		if (showNavigating)
			_measureDevicePicker.ShowNavigating(GetOpeningMessage(device));

		try
		{
			_pendingMeasureDevice = device;
			await GoToAsync("//measure", animate);

			if (_pendingMeasureDevice is not null && GetMeasureTabPage() is MeasureTabPage mp)
			{
				mp.SelectDevice(device);
				_pendingMeasureDevice = null;
			}
		}
		finally
		{
			if (showNavigating)
				_measureDevicePicker.HideNavigating();
		}
	}

	static bool IsMeasureRoute(string? path) =>
		path?.Contains("measure", StringComparison.OrdinalIgnoreCase) == true;

	internal void ApplyShellChrome()
	{
		Shell.SetBackgroundColor(this, ThemeColors.PageBackground);
		Shell.SetForegroundColor(this, ThemeColors.OnSurface);
		Shell.SetTitleColor(this, ThemeColors.OnSurface);
		Shell.SetUnselectedColor(this, ThemeColors.OnSurfaceVariant);
		FlyoutBackgroundColor = ThemeColors.FlyoutBackground;

		if (CurrentPage is ContentPage current)
			RefreshCurrentPageChrome(current);
	}

	static void RefreshCurrentPageChrome(ContentPage page)
	{
		switch (page)
		{
			case HomeTabPage:
				ShellChrome.ApplyGrouped(page);
				break;
			case MeasureTabPage measure:
				measure.ApplyTheme();
				break;
			default:
				ShellChrome.ApplyStandard(page);
				break;
		}
	}

	/// <summary>Refresh shell chrome and all tab content when system theme changes.</summary>
	public void ApplyTheme()
	{
		ApplyShellChrome();
		RefreshShellLocalization();
		(HomeShellContent.Content as HomeTabPage)?.ApplyTheme();
		(MeasureShellContent.Content as MeasureTabPage)?.ApplyTheme();
		(LogsShellContent.Content as LogsTabPage)?.ApplyTheme();
		//(InfoShellContent.Content as InfoTabPage)?.ApplyTheme();
		(HelpShellContent.Content as HelpTabPage)?.ApplyTheme();
	}
}
