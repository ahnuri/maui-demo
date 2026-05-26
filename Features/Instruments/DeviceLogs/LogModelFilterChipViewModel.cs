using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Model filter chip on device-type log list (e.g. Model 1 · 5 logs).</summary>
public partial class LogModelFilterChipViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	/// <summary>null = show all models.</summary>
	public string? ModelId { get; init; }
	public required string Label { get; init; }
	public required int Count { get; init; }
	public required string UnitLabel { get; init; }

	[ObservableProperty] private bool _isSelected;
	[ObservableProperty] private Color _backgroundColor = ThemeColors.ChipUnselected;
	[ObservableProperty] private Color _textColor = ThemeColors.ChipUnselectedText;
	[ObservableProperty] private Color _strokeColor = ThemeColors.Divider;
	[ObservableProperty] private FontAttributes _fontAttributes = FontAttributes.None;

	public string CountLine => Loc.T("LogHistory_ChipCountFormat", Count, UnitLabel);

	public void ApplySelection(bool selected, Color accent)
	{
		IsSelected = selected;
		BackgroundColor = selected ? accent.MultiplyAlpha(0.12f) : ThemeColors.ChipUnselected;
		TextColor = selected ? accent : ThemeColors.ChipUnselectedText;
		StrokeColor = selected ? accent : ThemeColors.Divider;
		FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
	}
}
