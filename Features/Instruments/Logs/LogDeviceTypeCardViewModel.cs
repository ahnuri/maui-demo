using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Top-level device family card on Log History (Halo 2, HI97115, Multimeter).</summary>
public partial class LogDeviceTypeCardViewModel : ObservableObject
{
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

	public string FileCountLabel => FileCount == 1 ? "1 log file" : $"{FileCount} log files";
	public string RecordCountLabel => $"{RecordCount:N0} records";
	public string ConnectedLabel => ConnectedDeviceCount == 1
		? "1 device connected"
		: $"{ConnectedDeviceCount} devices connected";
	public string CloudSyncLabel => CloudSyncStatus;
}
