using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Help;

/// <summary>Help content sections and navigation to Devices.</summary>
public partial class HelpViewModel : PageViewModelBase
{
	public ObservableCollection<HelpItem> Items { get; } = new();

	public HelpViewModel() => LoadItems();

	public override void RefreshForTheme() => LoadItems();

	void LoadItems()
	{
		Items.Clear();
		AddSection("\u25A1", "Getting Started");
		AddCard("\u224B", "Connect a Device", "Enable Bluetooth on your phone and photometer. Open Devices and tap Scan for Devices to connect Hanna instruments.");
		AddCard("\u2697", "Run a Measurement", "After connecting, open the Measure tab, select an instrument, and follow the on-screen steps.");
		AddSection("\u26A0", "Troubleshooting");
		AddCard("\u26A0", "Device Not Found", "Ensure Bluetooth is on, the instrument is powered, and within range. Then scan again from Devices.");
		AddCard("\u21BB", "Connection Lost", "If a device disconnects, return to Devices and tap Connect on the instrument.");
		AddSection("\u2709", "Support");
		AddCard("\u2709", "Contact Support", "For assistance, contact Hanna Instruments support at tech@hannainst.com");
	}

	void AddSection(string icon, string title) =>
		Items.Add(new HelpItem { IsSection = true, Icon = icon, Title = title });

	void AddCard(string icon, string title, string body) =>
		Items.Add(new HelpItem { Icon = icon, Title = title, Body = body });

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
