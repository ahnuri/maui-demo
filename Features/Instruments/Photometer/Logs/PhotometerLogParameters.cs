namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Photometer method/unit labels for log history and tank readings.</summary>
public static class PhotometerLogParameters
{
	public const string AlkalinityMarine = "dKH";
	public const string CalciumMarine = "ppm";
	public const string MagnesiumMarine = "ppm";
	public const string NitrateMarineLR = "ppm";
	public const string NitrateMarineHR = "ppm";
	public const string NitriteMarineULR = "ppb";
	public const string PHMarine = "pH";
	public const string PhosphateMarineULR = "ppm";
	public const string AmmoniaMarine = "ppm";

	public static IReadOnlyList<string> All { get; } =
	[
		AlkalinityMarine,
		CalciumMarine,
		MagnesiumMarine,
		NitrateMarineLR,
		NitrateMarineHR,
		NitriteMarineULR,
		PHMarine,
		PhosphateMarineULR,
		AmmoniaMarine
	];
}
