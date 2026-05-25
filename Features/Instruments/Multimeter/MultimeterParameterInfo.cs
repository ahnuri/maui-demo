namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>One measured parameter included in a multimeter log file.</summary>
public sealed record MultimeterParameterInfo(string Name, string Unit)
{
	public string Display => string.IsNullOrWhiteSpace(Unit) ? Name : $"{Name} ({Unit})";
}
