using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>One multimeter log recall file on the HI98494 / HI98594.</summary>
public partial class MultimeterLogItemViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required string Id { get; init; }
	public required string Title { get; init; }
	public required MultimeterLogFileType FileType { get; init; }
	public required string StartRecorded { get; init; }
	public required string StopRecorded { get; init; }
	public required int RecordCount { get; init; }
	public required IReadOnlyList<MultimeterParameterInfo> Parameters { get; init; }

	[ObservableProperty] private bool _isDownloading;
	[ObservableProperty] private bool _isDownloaded;

	public static Color LodAccent => Color.FromArgb("#6366F1");

	public string FileTypeLabel => FileType == MultimeterLogFileType.Lot ? "LOT" : "LOD";

	public Color FileTypeAccent => FileType == MultimeterLogFileType.Lot ? AppConstants.Primary : LodAccent;

	public Color FileTypeBackground => FileTypeAccent.MultiplyAlpha(0.14f);

	public Color FileTypeForeground => FileTypeAccent;

	public string ParametersSummary =>
		Parameters.Count == 0
			? Loc.T("Multimeter_LogRecall_NoParameters")
			: string.Join(" · ", Parameters.Select(p => p.Display));

	public string StatusGlyph => IsDownloading ? "\u21BB" : IsDownloaded ? "\u2713" : "\u2193";
	public Color StatusColor => IsDownloading ? Colors.Orange : IsDownloaded ? AppConstants.Success : ThemeColors.OnSurfaceVariant;
}
