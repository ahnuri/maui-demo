namespace HannaUIDemo.Features.Measure;

/// <summary>Instrument type shown on the Measure tab (selected from Devices).</summary>
public enum MeasureDeviceKind
{
	/// <summary>HI97115 wireless photometer measure UI.</summary>
	Photometer,

	/// <summary>HI98x94 multiparameter log-recall UI.</summary>
	Multimeter,

	/// <summary>Halo 2 live pH / mV / temperature UI.</summary>
	Halo2
}
