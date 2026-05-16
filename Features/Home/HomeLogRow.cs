namespace HannaUIDemo.Features.Home;

/// <summary>One row in the home “recent measurements” lists.</summary>
public sealed class HomeLogRow(string initials, string title, string subtitle, string value)
{
	public string Initials { get; } = initials;
	public string Title { get; } = title;
	public string Subtitle { get; } = subtitle;
	public string Value { get; } = value;
}
