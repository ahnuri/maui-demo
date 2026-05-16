namespace HannaUIDemo.Features.Info;

/// <summary>Label/value pair in a device info section.</summary>
public sealed class InfoRowViewModel
{
	public required string Label { get; init; }
	public required string Value { get; init; }
}
