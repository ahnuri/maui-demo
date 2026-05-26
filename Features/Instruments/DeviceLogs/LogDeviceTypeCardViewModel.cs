using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Top-level device family card on Log History (Halo 2, HI97115, Multimeter).</summary>
public partial class LogDeviceTypeCardViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required InstrumentKind Kind { get; init; }
	public required string Title { get; init; }
	public required string Subtitle { get; init; }
	public required int FileCount { get; init; }
	public required int RecordCount { get; init; }
	public required int ConnectedDeviceCount { get; init; }
	public required string LastRecordedLabel { get; init; }
	public required string CloudSyncStatus { get; init; }
	public required Color CloudSyncColor { get; init; }

	public ImageSource DeviceIcon => LogDeviceVisuals.IconSource(Kind);
	public Color Accent => LogDeviceVisuals.Accent;
	public Color AccentBackground => LogDeviceVisuals.AccentBackground;
	public Color CloudSyncBackground => CloudSyncColor.MultiplyAlpha(0.1f);

	public string FileCountLabel => FileCount == 1
		? Loc.T("LogHistory_FileCount_One")
		: Loc.T("LogHistory_FileCount_Many", FileCount);
	public string RecordCountLabel => Loc.T("LogHistory_RecordCountFormat", RecordCount);
	public string ConnectedLabel => ConnectedDeviceCount == 1
		? Loc.T("LogHistory_DeviceCount_One")
		: Loc.T("LogHistory_DeviceCount_Many", ConnectedDeviceCount);
	public string CloudSyncLabel => CloudSyncStatus;

	/// <summary>"Last recorded: 09:12 AM" formatted via the localized template.</summary>
	public string LastRecordedDisplay => Loc.T("LogHistory_LastRecordedFormat", LastRecordedLabel);
}
