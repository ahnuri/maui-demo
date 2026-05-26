namespace HannaUIDemo.Features.Instruments.Multimeter.Logs;

/// <summary>Multiparameter channel names used in multimeter log history.</summary>
public static class MultimeterLogParameters
{
	public static IReadOnlyList<string> Default { get; } =
	[
		"pH",
		"mVpH",
		"mVORP",
		"Temperature",
		"Abs. EC",
		"%DO",
		"Pressure"
	];

	public static IReadOnlyList<string> Optional { get; } =
	[
		"EC",
		"ppmDO",
		"Resistivity",
		"TDS",
		"Salinity",
		"Seawater",
		"Turbidity"
	];

	public static IReadOnlyList<string> All { get; } = Default.Concat(Optional).ToArray();
}
