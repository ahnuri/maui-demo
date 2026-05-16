using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Features.Help;
using HannaUIDemo.Features.Home;
using HannaUIDemo.Features.Info;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Features.Logs;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Helpers;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace HannaUIDemo;

public partial class AppShell : Shell
{
	string? _lastNavLocation;
	LocalizationService? _localization;
	MeasureDeviceKind? _pendingMeasureDevice;
	bool _suppressMeasurePicker;

	public AppShell()
	{
		InitializeComponent();
		Halo2Routes.Register();
		if (Application.Current is App app)
		{
			_localization = app.Services.GetRequiredService<LocalizationService>();
			LocalizationService.CultureChanged += OnLocalizationCultureChanged;
			RefreshShellLocalization();
		}

		Navigated += OnShellNavigated;
		ApplyShellChrome();
	}

	void OnLocalizationCultureChanged(object? sender, EventArgs e) =>
		MainThread.BeginInvokeOnMainThread(RefreshShellLocalization);

	void RefreshShellLocalization()
	{
		if (_localization is null)
			return;

		HomeShellContent.Title = _localization.T("Shell_Home");
		MeasureShellContent.Title = _localization.T("Shell_Measure");
		LogsShellContent.Title = _localization.T("Shell_LogHistory");
		InfoShellContent.Title = _localization.T("Shell_Information");
		HelpShellContent.Title = _localization.T("Shell_Help");
		HannaCloudMenuItem.Text = _localization.T("Shell_HannaCloud");

		FlyoutVersionLabel.Text = _localization.T("Flyout_VersionFormat", AppInfo.Current.VersionString);
		FlyoutCopyrightLabel.Text = _localization.T("Flyout_Copyright", DateTime.Now.Year.ToString());
		FlyoutPrivacyPolicyLabel.Text = _localization.T("Flyout_PrivacyPolicy");
		FlyoutLanguageLabel.Text = _localization.T("Flyout_LanguageLine", _localization.GetAutonym(_localization.CurrentLanguageCode));

		if (HomeShellContent.Content is HomeTabPage home)
			NavToolbar.Configure(home, "Shell_Home");
		if (MeasureShellContent.Content is MeasureTabPage measure)
			NavToolbar.Configure(measure, "Shell_Measure");
		if (LogsShellContent.Content is LogsTabPage logs)
			NavToolbar.Configure(logs, "Shell_LogHistory");
		if (InfoShellContent.Content is InfoTabPage info)
			NavToolbar.Configure(info, "PageToolbar_Info");
		if (HelpShellContent.Content is HelpTabPage help)
			NavToolbar.Configure(help, "PageToolbar_Help");
	}

	void OnHannaCloudMenuClicked(object? sender, EventArgs e) => _ = OpenHannaCloudAsync();

	static async Task OpenHannaCloudAsync()
	{
		try
		{
			await Launcher.Default.OpenAsync(new Uri("https://www.hannainst.com/", UriKind.Absolute));
		}
		catch
		{
			// Demo: ignore launcher failures
		}
	}

	async void OnFlyoutPrivacyPolicyTapped(object? sender, TappedEventArgs e)
	{
		try
		{
			await Launcher.Default.OpenAsync(new Uri("https://www.hannainst.com/", UriKind.Absolute));
		}
		catch
		{
			await DisplayAlertAsync("Privacy", "Unable to open the privacy page.", "OK");
		}
	}

	async void OnFlyoutLanguageTapped(object? sender, TappedEventArgs e)
	{
		if (Application.Current is not App app)
			return;
		FlyoutIsPresented = false;
		var page = app.Services.GetRequiredService<LanguageSelectionPage>();
		await Navigation.PushAsync(page);
	}

	void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
	{
		ApplyShellChrome();
		var loc = e.Current?.Location?.OriginalString ?? string.Empty;
		var wasMeasure = IsMeasureRoute(_lastNavLocation);
		var isMeasure = IsMeasureRoute(loc);
		if (isMeasure && !wasMeasure && MeasureShellContent.Content is MeasureTabPage mp)
		{
			if (_pendingMeasureDevice is { } device)
			{
				mp.SelectDevice(device);
				_pendingMeasureDevice = null;
				_suppressMeasurePicker = false;
			}
			else if (!_suppressMeasurePicker)
			{
				mp.ShowDevicePicker();
			}
		}

		_lastNavLocation = loc;
	}

	/// <summary>Switch to Measure tab and open the given device view.</summary>
	public async Task NavigateToMeasureDeviceAsync(MeasureDeviceKind device)
	{
		_pendingMeasureDevice = device;
		_suppressMeasurePicker = true;
		await GoToAsync("//measure");
		if (MeasureShellContent.Content is MeasureTabPage mp)
		{
			mp.SelectDevice(device);
			_pendingMeasureDevice = null;
		}
	}

	static bool IsMeasureRoute(string? path) =>
		path?.Contains("measure", StringComparison.OrdinalIgnoreCase) == true;

	internal void ApplyShellChrome()
	{
		Shell.SetBackgroundColor(this, ThemeColors.Surface);
		Shell.SetForegroundColor(this, ThemeColors.OnSurface);
		Shell.SetTitleColor(this, ThemeColors.OnSurface);
		Shell.SetUnselectedColor(this, ThemeColors.OnSurfaceVariant);
	}

	/// <summary>Refresh shell chrome and all tab content when system theme changes.</summary>
	public void ApplyTheme()
	{
		ApplyShellChrome();
		RefreshShellLocalization();
		(HomeShellContent.Content as HomeTabPage)?.ApplyTheme();
		(MeasureShellContent.Content as MeasureTabPage)?.ApplyTheme();
		(LogsShellContent.Content as LogsTabPage)?.ApplyTheme();
		(InfoShellContent.Content as InfoTabPage)?.ApplyTheme();
		(HelpShellContent.Content as HelpTabPage)?.ApplyTheme();
	}
}
