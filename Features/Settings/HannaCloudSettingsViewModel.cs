using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class HannaCloudSettingsViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;

	[ObservableProperty] private bool _autoUpload = true;

	public ObservableCollection<CloudSyncDeviceItem> Devices { get; } = new();

	public HannaCloudSettingsViewModel(UserSessionService session)
	{
		_session = session;
		_autoUpload = session.IsCloudSyncEnabled;
		LoadDevices();
		ApplyLocalization();
	}

	partial void OnAutoUploadChanged(bool value) => _session.SetCloudSyncEnabled(value);

	public string PageTitle => Loc.T("Cloud_Settings");
	public string AutoUploadTitle => Loc.T("Cloud_AutoUpload");
	public string AutoUploadDescription => Loc.T("Cloud_AutoUploadDescription");
	public string AvailableDevicesTitle => Loc.T("Cloud_AvailableDevices");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(AutoUploadTitle));
		OnPropertyChanged(nameof(AutoUploadDescription));
		OnPropertyChanged(nameof(AvailableDevicesTitle));

		var halo = Devices.FirstOrDefault(d => d.HasChildOptions);
		if (halo is not null)
		{
			halo.LogFilesLabel = Loc.T("Cloud_LogFiles");
			halo.TaggedDataLabel = Loc.T("Cloud_TaggedData");
		}
	}

	void LoadDevices()
	{
		Devices.Clear();

		var halo2 = new CloudSyncDeviceItem
		{
			Name = Loc.T("Cloud_Halo2Device"),
			DeviceIcon = "tab_halo.png",
			HasChildOptions = true,
			IsEnabled = true,
			LogFilesEnabled = true,
			TaggedDataEnabled = true,
			LogFilesLabel = Loc.T("Cloud_LogFiles"),
			TaggedDataLabel = Loc.T("Cloud_TaggedData")
		};
		halo2.RefreshChildVisibility();
		Devices.Add(halo2);

		Devices.Add(new CloudSyncDeviceItem { Name = "HI97115 Marine Photometer", DeviceIcon = "tab_photometer.png", IsEnabled = true });
		Devices.Add(new CloudSyncDeviceItem { Name = "HI98494 Multiparameter Meter", DeviceIcon = "tab_multimeter.png", IsEnabled = true });
		Devices.Add(new CloudSyncDeviceItem { Name = "HI98594 Multiparameter Meter", DeviceIcon = "tab_multimeter.png", IsEnabled = true });
	}
}
