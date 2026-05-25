namespace HannaUIDemo.Features.Instruments.Abstractions;

/// <summary>
/// Mutable bag used when rebuilding the shared Log History store from per-family catalogs.
/// </summary>
public sealed class LogCatalogAggregate
{
	public List<Logs.LogDeviceModelInfo> Models { get; } = [];
	public List<Logs.LogEntryViewModel> Sessions { get; } = [];
	public List<Logs.PhotometerLogReadingInfo> PhotometerReadings { get; } = [];
	public Dictionary<(string ModelId, int TankId), string> TankNames { get; } = new();
}

/// <summary>
/// One instrument family's demo log data (sessions, models, and optional photometer readings).
/// </summary>
public interface IInstrumentLogContributor
{
	InstrumentKind Kind { get; }

	void Contribute(LogCatalogAggregate aggregate);
}
