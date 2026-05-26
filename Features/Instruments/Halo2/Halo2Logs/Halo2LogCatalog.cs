using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Demo Halo 2 log sessions and device models for Log History.</summary>
public static class Halo2LogCatalog
{
	public static IEnumerable<LogDeviceModelInfo> DeviceModels =>
	[
		new("halo-1", InstrumentKind.Halo2, "Halo 2", "HI9810392", "HaloT3", "2.4.1", "1.0.8"),
		new("halo-2", InstrumentKind.Halo2, "Halo 2", "HI9811041", "HaloR3", "2.4.0", "1.0.7"),
		new("halo-3", InstrumentKind.Halo2, "Halo 2", "HI9811108", "LabBench", "2.3.9", "1.0.6")
	];

	public static IEnumerable<LogEntryViewModel> Sessions()
	{
		yield return Session("halo-1", null, "H2STagging",
			"11/05/26 • 7:01 AM", "11/05/26 • 7:18 AM", Halo2LogParameters.Fixed, "318", isUploadedToCloud: true);
		yield return Session("halo-1", null, "HI9810392-Halo2",
			"30/01/26 • 3:45 PM", "30/01/26 • 3:55 PM", Halo2LogParameters.Fixed, "142", isUploadedToCloud: false);
		yield return Session("halo-2", null, "HI9811041-Session",
			"29/01/26 • 1:35 PM", "29/01/26 • 2:05 PM", Halo2LogParameters.Fixed, "89", isUploadedToCloud: true);
		yield return Session("halo-2", null, "Field-pH-Log",
			"27/01/26 • 10:25 AM", "27/01/26 • 10:55 AM", Halo2LogParameters.Fixed, "56", isUploadedToCloud: false);
		yield return Session("halo-3", null, "Lab-Cal-Check",
			"25/01/26 • 8:10 AM", "25/01/26 • 8:30 AM", Halo2LogParameters.Fixed, "31", isUploadedToCloud: true);
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
			InstrumentKind = InstrumentKind.Halo2,
			Title = title,
			Start = start,
			Stop = stop,
			Parameters = parameters,
			RecordCount = recordCount,
			IsUploadedToCloud = isUploadedToCloud
		};
}
