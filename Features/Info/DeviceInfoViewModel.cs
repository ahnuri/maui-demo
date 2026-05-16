using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Info;

/// <summary>Connected device information and rename.</summary>
public partial class DeviceInfoViewModel : PageViewModelBase
{
	[ObservableProperty] private string _deviceName = "HI97115-Photometer";
	[ObservableProperty] private bool _showFirmwareBanner = true;

	public ObservableCollection<InfoSectionViewModel> Sections { get; } = new();

	public DeviceInfoViewModel() => LoadSections();

	public override void RefreshForTheme() => LoadSections();

	void LoadSections()
	{
		Sections.Clear();
		Sections.Add(CreateSection("\u2139", "Device Details",
			("Device Name", DeviceName),
			("Model", "HI9810392"),
			("Serial Number", "SN-H2-039284")));
		Sections.Add(CreateSection("\u2699", "Software", ShowFirmwareBanner,
			("Firmware Version", "v1.3.2"),
			("Bluetooth Version", "v5.0"),
			("Language", "English"),
			("Language Pack Version", "v1.0.4")));
		Sections.Add(CreateSection("\u2192", "Connection",
			("Last Connected", "3rd Feb 2026, 15:55"),
			("Connection Duration", "47 Minutes")));
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
			"Rename Device",
			"Device Name",
			initialValue: DeviceName,
			maxLength: 30,
			placeholder: "e.g. Lab Photometer 1");

		if (string.IsNullOrWhiteSpace(result) || result.Trim() == DeviceName)
			return;

		DeviceName = result.Trim();
		OnPropertyChanged(nameof(DeviceName));
		LoadSections();
		await page.DisplayAlertAsync("Device", $"Device renamed to \"{DeviceName}\"", "OK");
	}

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
