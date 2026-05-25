using HannaUIDemo.Features.Instruments.Abstractions;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Supplies photometer models, tank sessions, and per-reading rows to Log History.</summary>
public sealed class PhotometerLogContributor : IInstrumentLogContributor
{
	public InstrumentKind Kind => InstrumentKind.Photometer;

	public void Contribute(LogCatalogAggregate aggregate)
	{
		aggregate.Models.AddRange(PhotometerLogCatalog.DeviceModels);
		var built = PhotometerLogCatalog.Build(aggregate.TankNames);
		aggregate.Sessions.AddRange(built.Sessions);
		aggregate.PhotometerReadings.AddRange(built.Readings);
	}
}
