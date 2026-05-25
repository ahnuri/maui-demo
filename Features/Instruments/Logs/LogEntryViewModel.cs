using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>One saved log session in Hanna Lab history.</summary>
public partial class LogEntryViewModel : ObservableObject
{
	const int CollapsedVisibleCount = 3;

	public string Id { get; init; } = Guid.NewGuid().ToString("N");
	public required string DeviceModelId { get; init; }
	public int? TankId { get; init; }
	public required InstrumentKind InstrumentKind { get; init; }

	[ObservableProperty]
	private string _title = string.Empty;

	public required string Start { get; init; }
	public required string Stop { get; init; }
	public string? RecordCount { get; init; }
	public IReadOnlyList<string> Parameters { get; init; } = [];

	[ObservableProperty] private bool _isUploadedToCloud;
	[ObservableProperty] private bool _isSelected;
	[ObservableProperty] private bool _isParametersExpanded;
	[ObservableProperty] private bool _isEditModeActive;

	internal LogHistoryDeviceLogsViewModel? Owner { get; set; }

	public bool ShowRecordCount => !string.IsNullOrWhiteSpace(RecordCount);

	public bool ShowDetailChevron =>
		InstrumentKind == InstrumentKind.Halo2 && TankId is null && !IsEditModeActive;

	public Color CloudUploadIconColor =>
		IsUploadedToCloud ? LogDeviceVisuals.CloudUploadedIcon : LogDeviceVisuals.CloudPendingIcon;

	/// <summary>PNG asset used for the per-row cloud-sync indicator (green when uploaded, grey otherwise).</summary>
	public string CloudSyncIconSource => LogDeviceVisuals.CloudSyncIcon(IsUploadedToCloud);

	public string CloudUploadAccessibilityHint =>
		IsUploadedToCloud ? "Uploaded to cloud" : "Not uploaded to cloud";

	partial void OnIsUploadedToCloudChanged(bool value)
	{
		OnPropertyChanged(nameof(CloudUploadIconColor));
		OnPropertyChanged(nameof(CloudSyncIconSource));
		OnPropertyChanged(nameof(CloudUploadAccessibilityHint));
	}

	public bool ShowTankBadge => InstrumentKind == InstrumentKind.Photometer && TankId is int id;

	public string TankBadge =>
		TankId is int id && Owner?.TryGetTankName(DeviceModelId, id) is { } name
			? name
			: TankId is int tid
				? $"Tank {tid}"
				: string.Empty;

	public Color RecordCountBackground => LogDeviceVisuals.AccentBackground;

	public Color RecordCountColor => LogDeviceVisuals.Accent;

	public string DateRangeLabel => $"{Start}  →  {Stop}";

	public string DeviceBadge => InstrumentKind switch
	{
		InstrumentKind.Halo2 => "Halo 2",
		InstrumentKind.Photometer => "HI97115",
		InstrumentKind.Multimeter => "HI98x94",
		_ => "Log"
	};

	public Color CardStroke => IsSelected ? LogDeviceVisuals.Accent : ThemeColors.Divider;

	public string ParametersFullSummary =>
		Parameters.Count == 0 ? "—" : string.Join(" · ", Parameters);

	public string ParametersCollapsedSummary
	{
		get
		{
			if (Parameters.Count == 0)
				return "—";
			if (InstrumentKind == InstrumentKind.Halo2)
				return ParametersFullSummary;
			if (Parameters.Count <= CollapsedVisibleCount)
				return ParametersFullSummary;
			return string.Join(" · ", Parameters.Take(CollapsedVisibleCount))
			       + $" · +{Parameters.Count - CollapsedVisibleCount} more";
		}
	}

	public string ParametersDisplay =>
		IsParametersExpanded || !ShowParametersExpand
			? ParametersFullSummary
			: ParametersCollapsedSummary;

	public bool ShowParametersExpand =>
		InstrumentKind != InstrumentKind.Halo2 && Parameters.Count > CollapsedVisibleCount;

	public string ParametersExpandLabel =>
		IsParametersExpanded ? "Show less" : "Show all parameters";

	/// <summary>0 = unlimited lines; 1 = single-line summary.</summary>
	public int ParametersMaxLines =>
		IsParametersExpanded || !ShowParametersExpand ? 0 : 1;

	partial void OnIsSelectedChanged(bool value) =>
		OnPropertyChanged(nameof(CardStroke));

	partial void OnIsParametersExpandedChanged(bool value)
	{
		OnPropertyChanged(nameof(ParametersDisplay));
		OnPropertyChanged(nameof(ParametersExpandLabel));
		OnPropertyChanged(nameof(ParametersMaxLines));
	}

	[RelayCommand]
	async Task OpenLogAsync()
	{
		if (Owner is null)
			return;

		await Owner.HandleLogTapAsync(this);
	}

	internal void SetEditModeActive(bool active)
	{
		IsEditModeActive = active;
		OnPropertyChanged(nameof(ShowDetailChevron));
	}

	[RelayCommand]
	void ToggleParametersExpanded()
	{
		if (!ShowParametersExpand)
			return;

		IsParametersExpanded = !IsParametersExpanded;
		OnPropertyChanged(nameof(ParametersDisplay));
		OnPropertyChanged(nameof(ParametersMaxLines));
	}
}
