using HannaUIDemo.Features.Instruments;
using HannaUIDemo.Features.Instruments.Abstractions;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Halo2.Logs;
using HannaUIDemo.Features.Instruments.Multimeter;
using HannaUIDemo.Features.Instruments.Multimeter.Logs;
using HannaUIDemo.Features.Instruments.Photometer;
using HannaUIDemo.Features.Instruments.Photometer.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>Registers per-instrument measure modules, log navigators, and host registries.</summary>
public static class InstrumentServiceCollectionExtensions
{
	public static IServiceCollection AddInstrumentModules(this IServiceCollection services)
	{
		// Measure tab plugins (one Content view per InstrumentKind)
		services.AddSingleton<PhotometerMeasureModule>();
		services.AddSingleton<MultimeterMeasureModule>();
		services.AddSingleton<Halo2MeasureModule>();
		services.AddSingleton<IInstrumentMeasureModule>(sp => sp.GetRequiredService<PhotometerMeasureModule>());
		services.AddSingleton<IInstrumentMeasureModule>(sp => sp.GetRequiredService<MultimeterMeasureModule>());
		services.AddSingleton<IInstrumentMeasureModule>(sp => sp.GetRequiredService<Halo2MeasureModule>());
		services.AddSingleton<InstrumentMeasureHost>();

		// Log History detail navigation (per family)
		services.AddSingleton<Halo2LogNavigator>();
		services.AddSingleton<PhotometerLogNavigator>();
		services.AddSingleton<MultimeterLogNavigator>();
		services.AddSingleton<IInstrumentLogNavigator>(sp => sp.GetRequiredService<Halo2LogNavigator>());
		services.AddSingleton<IInstrumentLogNavigator>(sp => sp.GetRequiredService<PhotometerLogNavigator>());
		services.AddSingleton<IInstrumentLogNavigator>(sp => sp.GetRequiredService<MultimeterLogNavigator>());
		services.AddSingleton<InstrumentLogNavigatorHost>();

		// Log catalog contributors (also used by static LogHistoryCatalog.Rebuild for demo data)
		services.AddSingleton<Halo2LogContributor>();
		services.AddSingleton<PhotometerLogContributor>();
		services.AddSingleton<MultimeterLogContributor>();
		services.AddSingleton<IInstrumentLogContributor>(sp => sp.GetRequiredService<Halo2LogContributor>());
		services.AddSingleton<IInstrumentLogContributor>(sp => sp.GetRequiredService<PhotometerLogContributor>());
		services.AddSingleton<IInstrumentLogContributor>(sp => sp.GetRequiredService<MultimeterLogContributor>());

		return services;
	}
}
