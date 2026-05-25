using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>One connected instrument (same product, unique serial number) with its logs or tanks.</summary>
public partial class LogModelSectionViewModel : ObservableObject
{
	public required string ModelId { get; init; }
	public required string SerialNumber { get; init; }
	public required string DeviceName { get; init; }
	public required string FirmwareVersion { get; init; }
	public required string BleVersion { get; init; }
	public required InstrumentKind Kind { get; init; }

	public ObservableCollection<LogEntryViewModel> LogEntries { get; } = new();
	public ObservableCollection<LogTankGroupViewModel> TankGroups { get; } = new();

	public bool IsPhotometer => Kind == InstrumentKind.Photometer;
	public string DeviceLabel => $"{SerialNumber} - {DeviceName}";
	public string FirmwareLabel => $"FW Version: {FirmwareVersion}";
	public string BleLabel => $"BLE Version: {BleVersion}";
	public ImageSource DeviceIcon => LogDeviceVisuals.IconSource(Kind);
	public Color Accent => LogDeviceVisuals.Accent;

	public string SummaryLabel => IsPhotometer
		? (TankGroups.Count == 1 ? "1 tank" : $"{TankGroups.Count} tanks")
		: (LogEntries.Count == 1 ? "1 log file" : $"{LogEntries.Count} log files");
}
