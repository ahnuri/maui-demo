using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Multimeter.Logs;

/// <summary>Demo multiparameter log sessions and device models for Log History.</summary>
public static class MultimeterLogCatalog
{
	public static IEnumerable<LogDeviceModelInfo> DeviceModels =>
	[
		new("multi-10", InstrumentKind.Multimeter, "HI98494 / HI98594", "HI9849410", "BenchA", "3.1.0", "2.1.4"),
		new("multi-11", InstrumentKind.Multimeter, "HI98494 / HI98594", "HI9859411", "BenchB", "3.0.8", "2.1.2"),
		new("multi-12", InstrumentKind.Multimeter, "HI98494 / HI98594", "HI9859412", "FieldM", "3.0.5", "2.1.0")
	];

	public static IEnumerable<LogEntryViewModel> Sessions()
	{
		yield return Session("multi-12", null, "LOT_All_Params",
			"27/01/26 • 11:25 AM", "27/01/26 • 11:55 AM", MultimeterLogParameters.All, "45,321", isUploadedToCloud: true);
		yield return Session("multi-11", null, "LOD5 (LOD)",
			"23/01/26 • 1:25 PM", "23/01/26 • 1:55 PM",
			["pH", "mVpH", "Abs. EC", "Salinity", "Temperature"], "33", isUploadedToCloud: false);
		yield return Session("multi-11", null, "Field-EC-Log",
			"22/01/26 • 9:15 AM", "22/01/26 • 9:45 AM",
			["pH", "EC", "Temperature"], "412", isUploadedToCloud: true);
		yield return Session("multi-12", null, "Harbor-LOT",
			"21/01/26 • 2:00 PM", "21/01/26 • 2:30 PM",
			MultimeterLogParameters.Default, "2,104", isUploadedToCloud: false);
		yield return Session("multi-10", null, "LOD-PhECO-PDO",
			"19/01/26 • 2:35 PM", "19/01/26 • 3:05 PM",
			["pH", "EC", "%DO", "ppmDO", "Temperature"], "1,247", isUploadedToCloud: true);
		yield return Session("multi-10", null, "LOT-Weekly",
			"17/01/26 • 11:25 AM", "17/01/26 • 11:55 AM",
			MultimeterLogParameters.Default, "1,780", isUploadedToCloud: false);
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
			InstrumentKind = InstrumentKind.Multimeter,
			Title = title,
			Start = start,
			Stop = stop,
			Parameters = parameters,
			RecordCount = recordCount,
			IsUploadedToCloud = isUploadedToCloud
		};
}
