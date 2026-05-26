using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Photometer logs grouped by fixed tank id; display name can be renamed.</summary>
public partial class LogTankGroupViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required string DeviceModelId { get; init; }
	public required int TankId { get; init; }
	public required int LogFileCount { get; init; }
	public required int RecordCount { get; init; }
	public required string DateRangeSummary { get; init; }
	public required bool IsUploadedToCloud { get; init; }

	[ObservableProperty]
	private string _tankName = string.Empty;

	internal LogHistoryDeviceLogsViewModel? Owner { get; set; }

	public string TankIdLabel => Loc.T("LogHistory_TankHashFormat", TankId);
	public string FileCountLabel => LogFileCount == 1
		? Loc.T("LogHistory_FileCount_One")
		: Loc.T("LogHistory_FileCount_Many", LogFileCount);
	public string RecordCountLabel => Loc.T("LogHistory_RecordCountFormat", RecordCount);

	public Color AccentBackground => LogDeviceVisuals.AccentBackground;

	public bool ShowDetailChevron => Owner is { IsEditMode: false };

	public Color CloudUploadIconColor =>
		IsUploadedToCloud ? LogDeviceVisuals.CloudUploadedIcon : LogDeviceVisuals.CloudPendingIcon;

	/// <summary>PNG asset used for the per-row cloud-sync indicator (green when uploaded, grey otherwise).</summary>
	public string CloudSyncIconSource => LogDeviceVisuals.CloudSyncIcon(IsUploadedToCloud);

	public string CloudUploadAccessibilityHint =>
		IsUploadedToCloud ? Loc.T("Cloud_UploadedToCloud") : Loc.T("Cloud_NotUploadedToCloud");

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
			Loc.T("LogHistory_TankRename_Title"),
			Loc.T("LogHistory_TankRename_Label"),
			initialValue: TankName,
			maxLength: 32);

		if (string.IsNullOrWhiteSpace(name))
			return;

		TankName = name.Trim();
		LogHistoryCatalog.SetTankName(DeviceModelId, TankId, TankName);
		Owner?.RefreshAfterTankRename();
	}
}
