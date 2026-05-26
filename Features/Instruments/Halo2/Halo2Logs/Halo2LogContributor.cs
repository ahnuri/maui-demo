using HannaUIDemo.Features.Instruments.Abstractions;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Supplies Halo 2 models and sessions to the shared log history catalog.</summary>
public sealed class Halo2LogContributor : IInstrumentLogContributor
{
	public InstrumentKind Kind => InstrumentKind.Halo2;

	public void Contribute(LogCatalogAggregate aggregate)
	{
		aggregate.Models.AddRange(Halo2LogCatalog.DeviceModels);
		aggregate.Sessions.AddRange(Halo2LogCatalog.Sessions());
	}
}
