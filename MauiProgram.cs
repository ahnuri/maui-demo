using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Core.Theme;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Features.Flyout;
using Microsoft.Extensions.Logging;

namespace HannaUIDemo;

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
		builder.Services.AddTransient<AppFlyoutView>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
