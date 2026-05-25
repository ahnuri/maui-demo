using HannaUIDemo.Features.Instruments.Halo2.Logs;
using HannaUIDemo.Features.Instruments.Multimeter.Logs;
using HannaUIDemo.Features.Instruments.Photometer.Logs;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Backward-compatible facade over per-instrument parameter catalogs.</summary>
public static class LogParameterCatalog
{
	public static readonly IReadOnlyList<string> HaloFixed = Halo2LogParameters.Fixed;

	public static class PhotometerUnits
	{
		public const string AlkalinityMarine = PhotometerLogParameters.AlkalinityMarine;
		public const string CalciumMarine = PhotometerLogParameters.CalciumMarine;
		public const string MagnesiumMarine = PhotometerLogParameters.MagnesiumMarine;
		public const string NitrateMarineLR = PhotometerLogParameters.NitrateMarineLR;
		public const string NitrateMarineHR = PhotometerLogParameters.NitrateMarineHR;
		public const string NitriteMarineULR = PhotometerLogParameters.NitriteMarineULR;
		public const string PHMarine = PhotometerLogParameters.PHMarine;
		public const string PhosphateMarineULR = PhotometerLogParameters.PhosphateMarineULR;
		public const string AmmoniaMarine = PhotometerLogParameters.AmmoniaMarine;
		public static IReadOnlyList<string> All => PhotometerLogParameters.All;
	}

	public static class MultimeterParameters
	{
		public static IReadOnlyList<string> Default => MultimeterLogParameters.Default;
		public static IReadOnlyList<string> Optional => MultimeterLogParameters.Optional;
		public static IReadOnlyList<string> All => MultimeterLogParameters.All;
	}
}
