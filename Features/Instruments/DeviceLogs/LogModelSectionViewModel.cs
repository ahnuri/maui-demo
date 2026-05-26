using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>One connected instrument (same product, unique serial number) with its logs or tanks.</summary>
public partial class LogModelSectionViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required string ModelId { get; init; }
	public required string SerialNumber { get; init; }
	public required string DeviceName { get; init; }
	public required string FirmwareVersion { get; init; }
	public required string BleVersion { get; init; }
	public required InstrumentKind Kind { get; init; }

	public ObservableCollection<LogEntryViewModel> LogEntries { get; } = new();
	public ObservableCollection<LogTankGroupViewModel> TankGroups { get; } = new();

	public bool IsPhotometer => Kind == InstrumentKind.Photometer;
	public string DeviceLabel => Loc.T("LogHistory_DeviceLabelFormat", SerialNumber, DeviceName);
	public string FirmwareLabel => Loc.T("LogHistory_FirmwareLabel", FirmwareVersion);
	public string BleLabel => Loc.T("LogHistory_BleLabel", BleVersion);
	public ImageSource DeviceIcon => LogDeviceVisuals.IconSource(Kind);
	public Color Accent => LogDeviceVisuals.Accent;

	public string SummaryLabel => IsPhotometer
		? (TankGroups.Count == 1 ? Loc.T("LogHistory_TankCount_One") : Loc.T("LogHistory_TankCount_Many", TankGroups.Count))
		: (LogEntries.Count == 1 ? Loc.T("LogHistory_FileCount_One") : Loc.T("LogHistory_FileCount_Many", LogEntries.Count));
}
