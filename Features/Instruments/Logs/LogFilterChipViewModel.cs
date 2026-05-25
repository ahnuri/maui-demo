using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Filter chip on the Logs screen.</summary>
public partial class LogFilterChipViewModel : ObservableObject
{
	public LogFilterKind Filter { get; init; }
	public string Label { get; init; } = string.Empty;
	public int Count { get; init; }

	[ObservableProperty] private bool _isSelected;

	[ObservableProperty] private Color _backgroundColor = ThemeColors.ChipUnselected;
	[ObservableProperty] private Color _textColor = ThemeColors.ChipUnselectedText;
	[ObservableProperty] private Color _strokeColor = ThemeColors.Divider;
	[ObservableProperty] private FontAttributes _fontAttributes = FontAttributes.None;

	public string CountLabel => $"{Count}";

	public void ApplySelection(bool selected, Color accent)
	{
		IsSelected = selected;
		BackgroundColor = selected ? accent.MultiplyAlpha(0.12f) : ThemeColors.ChipUnselected;
		TextColor = selected ? accent : ThemeColors.ChipUnselectedText;
		StrokeColor = selected ? accent : ThemeColors.Divider;
		FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
	}
}

/// <summary>Log history filter.</summary>
public enum LogFilterKind
{
	All,
	Halo,
	Hi97115,
	Multimeter
}
