using HannaUIDemo.Features.Instruments.Abstractions;

namespace HannaUIDemo.Features.Instruments.Multimeter.Logs;

/// <summary>Supplies multiparameter models and sessions to the shared log history catalog.</summary>
public sealed class MultimeterLogContributor : IInstrumentLogContributor
{
	public InstrumentKind Kind => InstrumentKind.Multimeter;

	public void Contribute(LogCatalogAggregate aggregate)
	{
		aggregate.Models.AddRange(MultimeterLogCatalog.DeviceModels);
		aggregate.Sessions.AddRange(MultimeterLogCatalog.Sessions());
	}
}
