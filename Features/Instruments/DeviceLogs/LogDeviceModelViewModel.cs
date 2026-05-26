using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>One physical instrument model under a device family.</summary>
public partial class LogDeviceModelViewModel : ObservableObject
{
	public required string Id { get; init; }
	public required InstrumentKind Kind { get; init; }
	public required string DisplayName { get; init; }
	public required string SerialNumber { get; init; }
	public required int FileCount { get; init; }
	public required int RecordCount { get; init; }

	public ImageSource DeviceIcon => LogDeviceVisuals.IconSource(Kind);
	public Color Accent => LogDeviceVisuals.Accent;
	public string SerialLabel => $"S/N {SerialNumber}";
	public string StatsLabel => $"{FileCount} files · {RecordCount:N0} records";
}
