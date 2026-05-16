using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Features.Settings;
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
		builder.Services.AddHannaViewModels();

		builder.Services.AddTransient<DevicePage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<Halo2CalibrationPage>();
		builder.Services.AddTransient<Halo2SettingsPage>();
		builder.Services.AddTransient<LanguageSelectionPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
