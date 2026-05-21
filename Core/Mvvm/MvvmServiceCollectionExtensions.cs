using HannaUIDemo.Features.Halo2;
using HannaUIDemo.Features.Help;
using HannaUIDemo.Features.Home;
using HannaUIDemo.Features.Info;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Features.Logs;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Features.Flyout;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>Registers all MVVM ViewModels for dependency injection.</summary>
public static class MvvmServiceCollectionExtensions
{
	public static IServiceCollection AddHannaViewModels(this IServiceCollection services)
	{
		services.AddSingleton<AppFlyoutViewModel>();
		services.AddSingleton<HomeViewModel>();
		services.AddSingleton<MeasureTabViewModel>();
		services.AddSingleton<Halo2MeasureViewModel>();

		services.AddTransient<Features.Device.DeviceViewModel>();
		services.AddTransient<HelpViewModel>();
		services.AddTransient<LogsViewModel>();
		services.AddTransient<Halo2LogDetailViewModel>();
		services.AddTransient<DeviceInfoViewModel>();
		services.AddTransient<SettingsViewModel>();
		services.AddTransient<SignInViewModel>();
		services.AddTransient<RegisterViewModel>();
		services.AddTransient<HannaCloudHubViewModel>();
		services.AddTransient<HannaCloudSettingsViewModel>();
		services.AddTransient<ProfileInformationViewModel>();
		services.AddTransient<ResetPasswordViewModel>();
		services.AddTransient<LanguageSelectionViewModel>();
		services.AddTransient<MultimeterLogRecallViewModel>();
		services.AddSingleton<PhotometerMeasureViewModel>();
		services.AddTransient<Halo2SettingsViewModel>();
		services.AddTransient<Halo2CalibrationViewModel>();
		services.AddTransient<PhotometerDeviceSettingsViewModel>();

		return services;
	}
}
