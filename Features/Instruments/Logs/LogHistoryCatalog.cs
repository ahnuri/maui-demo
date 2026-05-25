using HannaUIDemo.Features.Instruments.Abstractions;
using HannaUIDemo.Features.Instruments.Halo2.Logs;
using HannaUIDemo.Features.Instruments.Multimeter.Logs;
using HannaUIDemo.Features.Instruments.Photometer.Logs;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>
/// Aggregates per-instrument log contributors into one in-memory store for the Log History tab.
/// </summary>
public static class LogHistoryCatalog
{
	static readonly IInstrumentLogContributor[] Contributors =
	[
		new Halo2LogContributor(),
		new PhotometerLogContributor(),
		new MultimeterLogContributor()
	];

	public static IReadOnlyList<LogDeviceModelInfo> DeviceModels { get; private set; } = [];
	public static IReadOnlyList<LogEntryViewModel> Entries { get; private set; } = [];
	public static IReadOnlyList<PhotometerLogReadingInfo> PhotometerReadings { get; private set; } = [];

	static readonly Dictionary<(string ModelId, int TankId), string> TankNames = new();

	static LogHistoryCatalog() => Rebuild();

	/// <summary>Rebuilds demo data from all registered <see cref="IInstrumentLogContributor"/> implementations.</summary>
	public static void Rebuild()
	{
		TankNames.Clear();
		var aggregate = new LogCatalogAggregate();

		foreach (var contributor in Contributors)
			contributor.Contribute(aggregate);

		foreach (var pair in aggregate.TankNames)
			TankNames[pair.Key] = pair.Value;

		DeviceModels = aggregate.Models;
		Entries = aggregate.Sessions;
		PhotometerReadings = aggregate.PhotometerReadings;
	}

	public static string GetTankName(string modelId, int tankId) =>
		TankNames.TryGetValue((modelId, tankId), out var name) ? name : $"Tank {tankId}";

	public static void SetTankName(string modelId, int tankId, string name) =>
		TankNames[(modelId, tankId)] = name.Trim();

	public static string GetLastRecordedLabel(IEnumerable<LogEntryViewModel> entries, InstrumentKind kind)
	{
		var latest = entries.Where(e => e.InstrumentKind == kind).OrderByDescending(e => e.Start).FirstOrDefault();
		return latest is null ? "No recordings yet" : latest.Start;
	}

	public static IReadOnlyList<LogDeviceModelInfo> ModelsFor(InstrumentKind kind) =>
		DeviceModels.Where(m => m.Kind == kind).ToList();

	public static IReadOnlyList<LogEntryViewModel> EntriesForModel(string modelId) =>
		Entries.Where(e => e.DeviceModelId == modelId).ToList();

	public static IReadOnlyList<PhotometerLogReadingInfo> ReadingsForTank(string modelId, int tankId) =>
		PhotometerReadings.Where(r => r.ModelId == modelId && r.TankId == tankId)
			.OrderByDescending(r => r.SortKey)
			.ToList();
}

public sealed record LogDeviceModelInfo(
	string Id,
	InstrumentKind Kind,
	string ProductName,
	string SerialNumber,
	string DeviceName,
	string FirmwareVersion,
	string BleVersion)
{
	public string DeviceLabel => $"{SerialNumber} - {DeviceName}";
}

public sealed record PhotometerLogReadingInfo(
	string ModelId,
	int TankId,
	string ParameterName,
	string ValueDisplay,
	string Note,
	string Timestamp,
	bool IsUploadedToCloud,
	int SortKey);
