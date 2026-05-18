using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Home;

/// <summary>Landing page: product overview, features, and primary navigation.</summary>
public partial class HomeViewModel : LocalizedViewModelBase
{
	public HomeViewModel() => ApplyLocalization();

	[ObservableProperty] private int _todayLogsCount = 24;
	[ObservableProperty] private int _connectedDevicesCount;
	[ObservableProperty] private string _connectedSummary = string.Empty;

	[RelayCommand]
	Task ScanDevicesAsync() => OpenDevicesAsync();

	[RelayCommand]
	Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return Task.CompletedTask;
		return nav.PushAsync(AppServices.Get<DevicePage>());
	}

	[RelayCommand]
	async Task DemoModeAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//measure");
	}

	[RelayCommand]
	async Task ViewLogsAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//logs");
	}

	protected override void ApplyLocalization() => UpdateConnectedSummary();

	partial void OnConnectedDevicesCountChanged(int value) => UpdateConnectedSummary();

	void UpdateConnectedSummary()
	{
		ConnectedSummary = ConnectedDevicesCount switch
		{
			0 => Loc.T("Home_ConnectedNone"),
			1 => Loc.T("Home_ConnectedOne"),
			_ => Loc.T("Home_ConnectedMany", ConnectedDevicesCount)
		};
	}
}
