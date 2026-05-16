namespace HannaUIDemo.Features.Help;

/// <summary>One row in the Help list (section header or content card).</summary>
public sealed class HelpItem
{
	public bool IsSection { get; init; }
	public string Icon { get; init; } = string.Empty;
	public string Title { get; init; } = string.Empty;
	public string Body { get; init; } = string.Empty;
}
