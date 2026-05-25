using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Single photometer measurement row inside a tank log history.</summary>
public partial class PhotometerLogReadingViewModel : ObservableObject
{
	public required string ParameterName { get; init; }
	public required string ValueDisplay { get; init; }
	public required string Note { get; init; }
	public required string Timestamp { get; init; }

	[ObservableProperty]
	private bool _isUploadedToCloud;

	public Color CloudUploadIconColor =>
		IsUploadedToCloud ? LogDeviceVisuals.CloudUploadedIcon : LogDeviceVisuals.CloudPendingIcon;

	public string CloudUploadGlyph => "\u2601";

	/// <summary>PNG asset used for the per-reading cloud-sync indicator (green when uploaded, grey otherwise).</summary>
	public string CloudSyncIconSource => LogDeviceVisuals.CloudSyncIcon(IsUploadedToCloud);

	public string CloudUploadAccessibilityHint =>
		IsUploadedToCloud ? "Uploaded to cloud" : "Not uploaded to cloud";

	partial void OnIsUploadedToCloudChanged(bool value)
	{
		OnPropertyChanged(nameof(CloudUploadIconColor));
		OnPropertyChanged(nameof(CloudSyncIconSource));
		OnPropertyChanged(nameof(CloudUploadAccessibilityHint));
	}
}
