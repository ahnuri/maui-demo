using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Photometer logs grouped by fixed tank id; display name can be renamed.</summary>
public partial class LogTankGroupViewModel : ObservableObject
{
	public required string DeviceModelId { get; init; }
	public required int TankId { get; init; }
	public required int LogFileCount { get; init; }
	public required int RecordCount { get; init; }
	public required string DateRangeSummary { get; init; }
	public required bool IsUploadedToCloud { get; init; }

	[ObservableProperty]
	private string _tankName = string.Empty;

	internal LogHistoryDeviceLogsViewModel? Owner { get; set; }

	public string TankIdLabel => $"Tank #{TankId}";
	public string FileCountLabel => LogFileCount == 1 ? "1 log file" : $"{LogFileCount} log files";
	public string RecordCountLabel => $"{RecordCount:N0} records";

	public Color AccentBackground => LogDeviceVisuals.AccentBackground;

	public bool ShowDetailChevron => Owner is { IsEditMode: false };

	public Color CloudUploadIconColor =>
		IsUploadedToCloud ? LogDeviceVisuals.CloudUploadedIcon : LogDeviceVisuals.CloudPendingIcon;

	/// <summary>PNG asset used for the per-row cloud-sync indicator (green when uploaded, grey otherwise).</summary>
	public string CloudSyncIconSource => LogDeviceVisuals.CloudSyncIcon(IsUploadedToCloud);

	public string CloudUploadAccessibilityHint =>
		IsUploadedToCloud ? "Uploaded to cloud" : "Not uploaded to cloud";

	[RelayCommand]
	async Task OpenTankAsync()
	{
		if (Owner is null)
			return;

		await Owner.OpenTankGroupAsync(this);
	}

	[RelayCommand]
	async Task RenameTankAsync()
	{
		if (Owner is null || Shell.Current?.CurrentPage is not Page page)
			return;

		var name = await page.DisplayPromptAsync(
			"Rename tank",
			"Tank name (tank id stays the same)",
			initialValue: TankName,
			maxLength: 32);

		if (string.IsNullOrWhiteSpace(name))
			return;

		TankName = name.Trim();
		LogHistoryCatalog.SetTankName(DeviceModelId, TankId, TankName);
		Owner?.RefreshAfterTankRename();
	}
}
