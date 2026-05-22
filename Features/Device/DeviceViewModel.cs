using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Measure;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Device;

/// <summary>Devices screen state: connection toggles, discovery lists, and measure navigation.</summary>
public partial class DeviceViewModel : PageViewModelBase
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

	public bool ShowNoConnectedHint => ConnectedCount == 0;
	public bool ShowScanBanner => IsScanning;

	public string ScanToolbarText => IsScanning ? Loc.T("Toolbar_Stop") : Loc.T("Toolbar_Scan");
	public string ScanStatusText => IsScanning ? Loc.T("Device_ScanStatus") : string.Empty;

	LocalizationService Loc => AppServices.Get<LocalizationService>();

	readonly HashSet<string> _connectedIds = ["hi97115", "hi98494", "halo2"];
	CancellationTokenSource? _scanCts;

	public ObservableCollection<DeviceListItem> ConnectedDevices { get; } = new();
	public ObservableCollection<DeviceListItem> AssociatedDevices { get; } = new();
	public ObservableCollection<DeviceListItem> AvailableDevices { get; } = new();

	public DeviceViewModel()
	{
		ApplyLocalization();
		RebuildLists();
	}

	public void RefreshLocalization() => ApplyLocalization();

	void ApplyLocalization()
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
		RebuildLists();
		ApplyLocalization();
	}

	void RebuildLists()
	{
		Replace(ConnectedDevices, GetConnectedCatalog().Where(d => d.IsConnected));
		Replace(AssociatedDevices, GetAssociatedCatalog().Where(d => !d.IsConnected));
		Replace(AvailableDevices, GetAvailableCatalog());
		ConnectedCount = ConnectedDevices.Count;
		OnPropertyChanged(nameof(ShowNoConnectedHint));
	}

	IEnumerable<DeviceListItem> GetConnectedCatalog() =>
	[
		BuildItem("hi97115", "Marine Master Multiparameter Photometer", "HI97105-001", "1.4.2", 92, "2 min ago",
			MeasureDeviceKind.Photometer),
		BuildItem("hi98494", "HI98x94 - Multiparameter", "HI98494-MM", "2.1.0", 78, "8 min ago",
			MeasureDeviceKind.Multimeter),
		BuildItem("halo2", "Halo 2", "HI12322", "3.0.1", 44, "Just now",
			MeasureDeviceKind.Halo2),
	];

	IEnumerable<DeviceListItem> GetAssociatedCatalog() =>
	[
		BuildItem("hi97115-pt1", "HI97115-PT1", "HI97115-PT1", null, null, null, null),
		BuildItem("hi9810391", "HI9810391-Halo", "HI9810391", null, null, null, null),
	];

	IEnumerable<DeviceListItem> GetAvailableCatalog() =>
	[
		BuildItem("hi98494-ak1", "HI98494-AK1", "HI98494-AK1", null, null, null, null, strong: true),
		BuildItem("hi9810392", "HI9810392-Halo2", "HI9810392", null, null, null, null, strong: false, signal: "Low"),
	];

	DeviceListItem BuildItem(
		string id,
		string name,
		string serial,
		string? firmware,
		int? battery,
		string? lastSeen,
		MeasureDeviceKind? kind,
		bool strong = true,
		string signal = "Strong")
	{
		var connected = _connectedIds.Contains(id);
		var item = new DeviceListItem
		{
			Id = id,
			Name = name,
			Serial = serial,
			Firmware = firmware,
			BatteryPercent = battery,
			LastSeen = lastSeen,
			DeviceIcon = ResolveIcon(kind, name),
			ThumbText = ResolveThumb(kind, name),
			SignalText = signal,
			IsStrongSignal = strong,
			IsConnected = connected,
			MeasureKind = kind
		};
		item.RefreshChrome();
		return item;
	}

	static string? ResolveIcon(MeasureDeviceKind? kind, string name) => kind switch
	{
		MeasureDeviceKind.Photometer => "tab_photometer.png",
		MeasureDeviceKind.Multimeter => "tab_multimeter.png",
		MeasureDeviceKind.Halo2 => "halo2_device_icon.png",
		_ => InferIcon(name)
	};

	static string? ResolveThumb(MeasureDeviceKind? kind, string name)
	{
		if (kind is not null || InferIcon(name) is not null)
			return null;

		return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
	}

	static string? InferIcon(string name)
	{
		var n = name.ToUpperInvariant();
		if (n.Contains("HALO"))
			return "halo2_device_icon.png";
		if (n.Contains("97115") || n.Contains("PHOTO") || n.Contains("PT1"))
			return "tab_photometer.png";
		if (n.Contains("98494") || n.Contains("98X") || n.Contains("MULTI"))
			return "tab_multimeter.png";
		return null;
	}

	static void Replace(ObservableCollection<DeviceListItem> target, IEnumerable<DeviceListItem> items)
	{
		target.Clear();
		foreach (var item in items)
			target.Add(item);
	}

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

		if (item.MeasureKind is not null)
			ClearMeasureSelectionIfNeeded(item.MeasureKind.Value);

		ConnectedDevices.Remove(item);

		if (GetAssociatedCatalog().Any(d => d.Id == item.Id) || item.MeasureKind is not null)
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

	void ClearMeasureSelectionIfNeeded(MeasureDeviceKind kind)
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

	[RelayCommand]
	async Task OpenMeasureAsync(DeviceListItem? item)
	{
		if (item is null || !item.CanOpenMeasure || item.MeasureKind is not { } kind
		    || Shell.Current is not AppShell shell)
			return;

		if (shell.Navigation.NavigationStack.Count > 1)
			await shell.Navigation.PopAsync();

		await shell.NavigateToMeasureDeviceAsync(kind);
	}

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
