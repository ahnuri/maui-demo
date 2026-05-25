using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Demo;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Measure;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Device;

/// <summary>
/// Devices tab ViewModel: BLE scan simulation, connect/disconnect, and navigation to Measure.
/// Uses CommunityToolkit.Mvvm for bindable state and commands.
/// </summary>
public partial class DeviceViewModel : LocalizedViewModelBase
{
	[ObservableProperty] private string _pageSubtitle = string.Empty;
	[ObservableProperty] private string _headerTitle = string.Empty;
	[ObservableProperty] private string _connectedTitle = string.Empty;
	[ObservableProperty] private string _connectedSubtitle = string.Empty;
	[ObservableProperty] private string _associatedTitle = string.Empty;
	[ObservableProperty] private string _associatedSubtitle = string.Empty;
	[ObservableProperty] private string _nearbyTitle = string.Empty;
	[ObservableProperty] private string _nearbySubtitle = string.Empty;
	[ObservableProperty] private string _noConnectedHint = string.Empty;
	[ObservableProperty] private string _hintText = string.Empty;
	[ObservableProperty] private string _measureButtonText = string.Empty;
	[ObservableProperty] private bool _isScanning;
	[ObservableProperty] private int _connectedCount;

	/// <summary>True when no instruments are connected — drives empty-state hint visibility.</summary>
	public bool ShowNoConnectedHint => ConnectedCount == 0;

	/// <summary>True while a discovery scan is running — drives scan banner visibility.</summary>
	public bool ShowScanBanner => IsScanning;

	/// <summary>Toolbar label toggles between Start Scan and Stop based on <see cref="IsScanning"/>.</summary>
	public string ScanToolbarText => IsScanning ? Loc.T("Toolbar_Stop") : Loc.T("Toolbar_Scan");

	/// <summary>Status line shown under the header during an active scan.</summary>
	public string ScanStatusText => IsScanning ? Loc.T("Device_ScanStatus") : string.Empty;

	readonly HashSet<string> _connectedIds = [.. DemoDeviceCatalog.DefaultConnectedIds];
	CancellationTokenSource? _scanCts;

	public ObservableCollection<DeviceListItem> ConnectedDevices { get; } = new();
	public ObservableCollection<DeviceListItem> AssociatedDevices { get; } = new();
	public ObservableCollection<DeviceListItem> AvailableDevices { get; } = new();

	public DeviceViewModel()
	{
		RebuildLists();
		ApplyLocalization();
	}

	/// <summary>Called from the view when culture changes outside the base subscription.</summary>
	public void RefreshLocalization() => ApplyLocalization();

	protected override void ApplyLocalization()
	{
		PageSubtitle = Loc.T("Device_PageSubtitle");
		HeaderTitle = Loc.T("Device_Header");
		ConnectedTitle = Loc.T("Device_Connected");
		ConnectedSubtitle = Loc.T("Device_ConnectedSub");
		AssociatedTitle = Loc.T("Device_Associated");
		AssociatedSubtitle = Loc.T("Device_AssociatedSub");
		NearbyTitle = Loc.T("Device_Nearby");
		NearbySubtitle = Loc.T("Device_NearbySub");
		NoConnectedHint = Loc.T("Device_NoConnected");
		HintText = Loc.T("Device_Hint");
		MeasureButtonText = Loc.T("Device_Measure");
		OnPropertyChanged(nameof(ScanToolbarText));
		OnPropertyChanged(nameof(ScanStatusText));
	}

	public override void RefreshForTheme()
	{
		base.RefreshForTheme();
		RebuildLists();
	}

	/// <summary>Rebuilds the three device lists from the demo catalog and current connection set.</summary>
	void RebuildLists()
	{
		Replace(ConnectedDevices, DemoDeviceCatalog.ConnectedDevices(_connectedIds));
		foreach (var item in ConnectedDevices)
			item.IsConnected = true;

		Replace(AssociatedDevices, DemoDeviceCatalog.AssociatedDevices(_connectedIds));
		Replace(AvailableDevices, DemoDeviceCatalog.AvailableDevices());
		ConnectedCount = ConnectedDevices.Count;
		OnPropertyChanged(nameof(ShowNoConnectedHint));
	}

	static void Replace(ObservableCollection<DeviceListItem> target, IEnumerable<DeviceListItem> items)
	{
		target.Clear();
		foreach (var item in items)
			target.Add(item);
	}

	/// <summary>Toggles connect/disconnect for the tapped instrument row.</summary>
	[RelayCommand]
	void ToggleConnection(DeviceListItem? item)
	{
		if (item is null)
			return;

		if (item.IsConnected)
			Disconnect(item);
		else
			Connect(item);
	}

	void Connect(DeviceListItem item)
	{
		_connectedIds.Add(item.Id);
		item.IsConnected = true;
		item.RefreshChrome();

		AssociatedDevices.Remove(item);
		AvailableDevices.Remove(item);

		if (ConnectedDevices.All(d => d.Id != item.Id))
			ConnectedDevices.Add(item);

		ConnectedCount = ConnectedDevices.Count;
		OnPropertyChanged(nameof(ShowNoConnectedHint));
	}

	void Disconnect(DeviceListItem item)
	{
		_connectedIds.Remove(item.Id);
		item.IsConnected = false;
		item.RefreshChrome();

		if (item.InstrumentKind is not null)
			ClearMeasureSelectionIfNeeded(item.InstrumentKind.Value);

		ConnectedDevices.Remove(item);

		if (DemoDeviceCatalog.AssociatedDevices(_connectedIds).Any(d => d.Id == item.Id) || item.InstrumentKind is not null)
		{
			if (AssociatedDevices.All(d => d.Id != item.Id))
				AssociatedDevices.Insert(0, item);
		}
		else if (AvailableDevices.All(d => d.Id != item.Id))
		{
			AvailableDevices.Insert(0, item);
		}

		ConnectedCount = ConnectedDevices.Count;
		OnPropertyChanged(nameof(ShowNoConnectedHint));
	}

	/// <summary>Clears the active measure device if the disconnected instrument was selected.</summary>
	void ClearMeasureSelectionIfNeeded(InstrumentKind kind)
	{
		if (Application.Current is not App app)
			return;

		var measureVm = app.Services.GetRequiredService<MeasureTabViewModel>();
		if (measureVm.ActiveDevice != kind)
			return;

		if (Shell.Current?.CurrentItem is not null
		    && Shell.Current.CurrentItem.CurrentItem?.Route == "measure"
		    && Shell.Current.CurrentPage is MeasureTabPage measurePage)
			measurePage.DisconnectDevice();
	}

	/// <summary>Opens the Measure tab for a connected instrument that supports measurement.</summary>
	[RelayCommand]
	async Task OpenMeasureAsync(DeviceListItem? item)
	{
		if (item is null || !item.CanOpenMeasure || item.InstrumentKind is not { } kind
		    || Shell.Current is not AppShell shell)
			return;

		if (shell.Navigation.NavigationStack.Count > 1)
			await shell.Navigation.PopAsync();

		await shell.NavigateToMeasureDeviceAsync(kind);
	}

	/// <summary>Starts a scan or cancels an in-progress scan.</summary>
	[RelayCommand]
	async Task ScanOrStopAsync()
	{
		if (IsScanning)
		{
			_scanCts?.Cancel();
			return;
		}

		await ScanAsync();
	}

	/// <summary>Simulates a BLE discovery pass, then refreshes nearby devices.</summary>
	[RelayCommand]
	async Task ScanAsync()
	{
		if (IsScanning)
			return;

		_scanCts?.Cancel();
		_scanCts = new CancellationTokenSource();
		var token = _scanCts.Token;

		IsScanning = true;
		OnScanStateChanged();

		try
		{
			await Task.Delay(2400, token);
			if (!token.IsCancellationRequested)
				RebuildLists();
		}
		catch (TaskCanceledException)
		{
			// User tapped Stop
		}
		finally
		{
			IsScanning = false;
			OnScanStateChanged();
			_scanCts?.Dispose();
			_scanCts = null;
		}
	}

	void OnScanStateChanged()
	{
		OnPropertyChanged(nameof(ScanToolbarText));
		OnPropertyChanged(nameof(ShowScanBanner));
		OnPropertyChanged(nameof(ScanStatusText));
	}

	partial void OnIsScanningChanged(bool value) => OnScanStateChanged();
}
