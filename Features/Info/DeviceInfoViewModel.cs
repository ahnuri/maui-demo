using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Info;

/// <summary>Connected device information and rename.</summary>
public partial class DeviceInfoViewModel : LocalizedViewModelBase
{
	[ObservableProperty] private string _deviceName = "HI97115-Photometer";
	[ObservableProperty] private bool _showFirmwareBanner = true;

	public ObservableCollection<InfoSectionViewModel> Sections { get; } = new();

	public string ConnectedLabel => Loc.T("Info_Status_Connected");
	public string FirmwareUpdateBanner => Loc.T("Info_Banner_FirmwareUpdate");
	public string OpenDevicesLabel => Loc.T("Info_OpenDevicesButton");

	public DeviceInfoViewModel() => LoadSections();

	public override void RefreshForTheme() => LoadSections();

	protected override void ApplyLocalization()
	{
		LoadSections();
		OnPropertyChanged(nameof(ConnectedLabel));
		OnPropertyChanged(nameof(FirmwareUpdateBanner));
		OnPropertyChanged(nameof(OpenDevicesLabel));
	}

	void LoadSections()
	{
		Sections.Clear();
		Sections.Add(CreateSection("\u2139", Loc.T("Info_Section_DeviceDetails"),
			(Loc.T("Info_Field_DeviceName"), DeviceName),
			(Loc.T("Info_Field_Model"), "HI9810392"),
			(Loc.T("Info_Field_SerialNumber"), "SN-H2-039284")));
		Sections.Add(CreateSection("\u2699", Loc.T("Info_Section_Software"), ShowFirmwareBanner,
			(Loc.T("Info_Field_FirmwareVersion"), "v1.3.2"),
			(Loc.T("Info_Field_BluetoothVersion"), "v5.0"),
			(Loc.T("Info_Field_Language"), Loc.T("Photometer_Settings_DefaultLang")),
			(Loc.T("Info_Field_LanguagePackVersion"), "v1.0.4")));
		Sections.Add(CreateSection("\u2192", Loc.T("Info_Section_Connection"),
			(Loc.T("Info_Field_LastConnected"), "3rd Feb 2026, 15:55"),
			(Loc.T("Info_Field_ConnectionDuration"), Loc.T("Info_Value_47Minutes"))));
	}

	static InfoSectionViewModel CreateSection(string icon, string title, params (string Label, string Value)[] rows) =>
		CreateSection(icon, title, false, rows);

	static InfoSectionViewModel CreateSection(string icon, string title, bool showFirmwareBanner, params (string Label, string Value)[] rows)
	{
		var section = new InfoSectionViewModel { Icon = icon, Title = title, ShowFirmwareBanner = showFirmwareBanner };
		foreach (var (label, value) in rows)
			section.Rows.Add(new InfoRowViewModel { Label = label, Value = value });
		return section;
	}

	[RelayCommand]
	async Task RenameDeviceAsync()
	{
		if (Shell.Current?.CurrentPage is not Page page)
			return;

		var result = await page.DisplayPromptAsync(
			Loc.T("Info_RenameDialog_Title"),
			Loc.T("Info_Field_DeviceName"),
			initialValue: DeviceName,
			maxLength: 30,
			placeholder: Loc.T("Info_RenameDialog_Placeholder"));

		if (string.IsNullOrWhiteSpace(result) || result.Trim() == DeviceName)
			return;

		DeviceName = result.Trim();
		OnPropertyChanged(nameof(DeviceName));
		LoadSections();
		await page.DisplayAlertAsync(
			Loc.T("Info_RenameDialog_ToastTitle"),
			Loc.T("Info_RenameDialog_ToastMessage", DeviceName),
			Loc.T("Common_OK"));
	}

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
