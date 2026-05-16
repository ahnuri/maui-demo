using HannaUIDemo.Constants;

namespace HannaUIDemo.Features.Measure;

/// <summary>One multimeter log recall card.</summary>
public sealed class MultimeterLogItemViewModel
{
	public required string Title { get; init; }
	public required string Recorded { get; init; }
	public required string Samples { get; init; }
	public bool IsSyncing { get; init; }

	public string StatusGlyph => IsSyncing ? "\u21BB" : "\u2713";
	public Color StatusColor => IsSyncing ? Colors.Orange : AppConstants.Success;
}
