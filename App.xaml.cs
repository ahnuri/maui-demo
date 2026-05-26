using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Halo2.Logs;
using HannaUIDemo.Features.Instruments.Logs;
using HannaUIDemo.Features.Instruments.Photometer;
using HannaUIDemo.Features.Instruments.Photometer.Logs;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo;

/// <summary>
/// Application host: applies stored theme/language, wires semantic resources, and refreshes open pages on theme change.
/// </summary>
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

	/// <summary>Re-applies theme styling to pages currently on the navigation stack.</summary>
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
