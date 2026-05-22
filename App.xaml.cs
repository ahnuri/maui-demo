using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Theme;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Features.Logs;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		Services = services;
		services.GetRequiredService<LocalizationService>().ApplyStoredLanguage();
		services.GetRequiredService<ThemeService>().ApplyStoredTheme();
		InitializeComponent();
		SemanticResources.Update(this);
		RequestedThemeChanged += OnRequestedThemeChanged;
	}

	public IServiceProvider Services { get; }

	void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
	{
		SemanticResources.Update(this);
		foreach (var window in Windows)
		{
			if (window.Page is AppShell appShell)
				appShell.ApplyTheme();

			RefreshNavigationStack(window.Page);
		}
	}

	static void RefreshNavigationStack(Page? page)
	{
		if (page is not Shell { CurrentPage: { } current })
			return;

		if (current.Navigation?.NavigationStack is not { } stack)
			return;

		foreach (var p in stack)
		{
			switch (p)
			{
				case DevicePage device:
					device.ApplyTheme();
					break;
				case SettingsPage settings:
					settings.ApplyTheme();
					break;
				case Halo2SettingsPage halo2Settings:
					halo2Settings.ApplyTheme();
					break;
				case PhotometerDeviceSettingsPage photometerDevice:
					photometerDevice.ApplyTheme();
					break;
				case Halo2CalibrationPage halo2Calibration:
					halo2Calibration.ApplyTheme();
					break;
				case Halo2LogDetailPage halo2LogDetail:
					halo2LogDetail.ApplyTheme();
					break;
				case LogHistoryDeviceLogsPage deviceLogs:
					deviceLogs.ApplyTheme();
					break;
				case LogHistoryTankReadingsPage tankReadings:
					tankReadings.ApplyTheme();
					break;
			}
		}
	}

	protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}
