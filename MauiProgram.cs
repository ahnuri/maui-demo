using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Core.Theme;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Halo2.Logs;
using HannaUIDemo.Features.Instruments.Logs;
using HannaUIDemo.Features.Instruments.Photometer;
using HannaUIDemo.Features.Instruments.Photometer.Logs;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Features.Flyout;
using Microsoft.Extensions.Logging;

namespace HannaUIDemo;

/// <summary>MAUI entry point: services, ViewModels, instrument modules, and navigation pages.</summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<LocalizationService>();
		builder.Services.AddSingleton<ThemeService>();
		builder.Services.AddSingleton<UserSessionService>();
		builder.Services.AddHannaViewModels();

		builder.Services.AddTransient<DevicePage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<SignInPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<HannaCloudHubPage>();
		builder.Services.AddTransient<HannaCloudSettingsPage>();
		builder.Services.AddTransient<ProfileInformationPage>();
		builder.Services.AddTransient<ResetPasswordPage>();
		builder.Services.AddTransient<Halo2CalibrationPage>();
		builder.Services.AddTransient<Halo2SettingsPage>();
		builder.Services.AddTransient<PhotometerDeviceSettingsPage>();
		builder.Services.AddTransient<LanguageSelectionPage>();
		builder.Services.AddTransient<Halo2LogDetailPage>();
		builder.Services.AddTransient<LogHistoryDeviceLogsPage>();
		builder.Services.AddTransient<LogHistoryTankReadingsPage>();
		builder.Services.AddTransient<AppFlyoutView>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
