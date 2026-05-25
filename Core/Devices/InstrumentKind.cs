namespace HannaUIDemo.Core.Devices;

/// <summary>
/// Hanna instrument family used across Devices, Measure, and Log History.
/// </summary>
public enum InstrumentKind
{
	/// <summary>HI97115 wireless photometer.</summary>
	Photometer,

	/// <summary>HI98x94 multiparameter meter.</summary>
	Multimeter,

	/// <summary>Halo 2 pH / mV / temperature probe.</summary>
	Halo2
}
