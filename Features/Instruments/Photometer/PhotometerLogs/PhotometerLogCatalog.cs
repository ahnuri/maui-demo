using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Demo photometer log sessions, tank names, and per-reading recall data.</summary>
public static class PhotometerLogCatalog
{
	public static IEnumerable<LogDeviceModelInfo> DeviceModels =>
	[
		new("photo-1", InstrumentKind.Photometer, "HI97115", "HI97115001", "ReefLab", "1.8.2", "1.2.0"),
		new("photo-2", InstrumentKind.Photometer, "HI97115", "HI97115002", "FieldUnit", "1.8.1", "1.1.9"),
		new("photo-3", InstrumentKind.Photometer, "HI97115", "HI97115003", "Portable", "1.8.0", "1.1.8")
	];

	public static PhotometerLogBuildResult Build(IDictionary<(string ModelId, int TankId), string> tankNames)
	{
		RegisterTank(tankNames, "photo-1", 5, "CORAL1");
		RegisterTank(tankNames, "photo-1", 1, "TANK1");
		RegisterTank(tankNames, "photo-1", 2, "TANK2");
		RegisterTank(tankNames, "photo-2", 3, "REEF-A");
		RegisterTank(tankNames, "photo-3", 7, "POOL-90");

		var sessions = new List<LogEntryViewModel>();
		var readings = new List<PhotometerLogReadingInfo>();

		for (var i = 0; i < 10; i++)
		{
			sessions.Add(Session("photo-1", 5, $"CORAL1-Session-{i + 1}",
				"18/08/25 • 12:30 PM", "18/08/25 • 12:55 PM",
				["Phosphate Marine ULR", "Nitrite Marine ULR", "pH Marine"], $"{18 + i}", isUploadedToCloud: i % 4 != 3));
		}

		sessions.Add(Session("photo-1", 1, "TANK1-Weekly",
			"20/01/26 • 4:25 PM", "20/01/26 • 4:55 PM",
			["dKH", "ppm", "ppm", "pH"], "12", isUploadedToCloud: true));
		sessions.Add(Session("photo-1", 2, "TANK2-Production",
			"19/01/26 • 2:35 PM", "19/01/26 • 3:05 PM",
			PhotometerLogParameters.All.Take(8).ToArray(), "214", isUploadedToCloud: false));
		sessions.Add(Session("photo-2", 3, "REEF-A-Aug",
			"15/01/26 • 9:10 AM", "15/01/26 • 9:40 AM",
			PhotometerLogParameters.All.Take(6).ToArray(), "18", isUploadedToCloud: true));
		sessions.Add(Session("photo-3", 7, "POOL-90",
			"14/01/26 • 3:10 PM", "14/01/26 • 3:40 PM", ["dKH", "ppm", "pH"], "27", isUploadedToCloud: true));

		readings.AddRange(BuildPhotoReadings("photo-1", 5, "18/08/25", synced: true));
		readings.AddRange(BuildPhotoReadings("photo-1", 2, "19/01/26", synced: false, count: 6));
		readings.AddRange(BuildPhotoReadings("photo-2", 3, "15/01/26", synced: true, count: 4));

		return new PhotometerLogBuildResult(sessions, readings);
	}

	static void RegisterTank(IDictionary<(string, int), string> tankNames, string modelId, int tankId, string name) =>
		tankNames[(modelId, tankId)] = name;

	static IEnumerable<PhotometerLogReadingInfo> BuildPhotoReadings(
		string modelId,
		int tankId,
		string datePrefix,
		bool synced,
		int count = 8)
	{
		var parameters = new[]
		{
			("Phosphate Marine ULR", "0.22 ppm PO₄³⁻"),
			("Nitrite Marine ULR", "0.00 ppm NO₂⁻"),
			("pH Marine", "6.6 pH"),
			("Alkalinity Marine", "9.65 dKH"),
			("Calcium Marine", "446 ppm"),
			("Magnesium Marine", "1389 ppm"),
			("Nitrate Marine", "3.84 ppm"),
			("Phosphate Marine", "0.08 ppm")
		};

		for (var i = 0; i < count && i < parameters.Length; i++)
		{
			var (name, value) = parameters[i];
			yield return new PhotometerLogReadingInfo(
				modelId,
				tankId,
				name,
				value,
				Note: string.Empty,
				Timestamp: $"{datePrefix}, {12 + i:00}:{40 + i * 2:00}:00 PM",
				IsUploadedToCloud: synced || i % 3 != 2,
				SortKey: count - i);
		}
	}

	static LogEntryViewModel Session(
		string modelId,
		int? tankId,
		string title,
		string start,
		string stop,
		IReadOnlyList<string> parameters,
		string recordCount,
		bool isUploadedToCloud) =>
		new()
		{
			DeviceModelId = modelId,
			TankId = tankId,
			InstrumentKind = InstrumentKind.Photometer,
			Title = title,
			Start = start,
			Stop = stop,
			Parameters = parameters,
			RecordCount = recordCount,
			IsUploadedToCloud = isUploadedToCloud
		};
}

public sealed record PhotometerLogBuildResult(
	IReadOnlyList<LogEntryViewModel> Sessions,
	IReadOnlyList<PhotometerLogReadingInfo> Readings);
