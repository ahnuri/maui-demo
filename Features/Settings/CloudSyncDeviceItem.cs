using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Features.Settings;

public partial class CloudSyncDeviceItem : ObservableObject
{
	public string Name { get; init; } = string.Empty;
	public string? DeviceIcon { get; init; }
	public bool HasChildOptions { get; init; }

	[ObservableProperty] private bool _isEnabled;
	[ObservableProperty] private bool _logFilesEnabled;
	[ObservableProperty] private bool _taggedDataEnabled;
	[ObservableProperty] private bool _showChildOptions;

	public string LogFilesLabel { get; set; } = "Log Files";
	public string TaggedDataLabel { get; set; } = "Tagged Data";

	public bool ShowDeviceIcon => !string.IsNullOrEmpty(DeviceIcon);

	partial void OnIsEnabledChanged(bool value) =>
		ShowChildOptions = value && HasChildOptions;

	public void RefreshChildVisibility() =>
		ShowChildOptions = IsEnabled && HasChildOptions;
}
