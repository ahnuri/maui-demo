using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Instruments.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Single photometer measurement row inside a tank log history.</summary>
public partial class PhotometerLogReadingViewModel : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

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
		IsUploadedToCloud ? Loc.T("Cloud_UploadedToCloud") : Loc.T("Cloud_NotUploadedToCloud");

	partial void OnIsUploadedToCloudChanged(bool value)
	{
		OnPropertyChanged(nameof(CloudUploadIconColor));
		OnPropertyChanged(nameof(CloudSyncIconSource));
		OnPropertyChanged(nameof(CloudUploadAccessibilityHint));
	}
}
