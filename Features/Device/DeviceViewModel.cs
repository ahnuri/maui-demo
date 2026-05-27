using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Demo;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Measure;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Device;

/// <summary>
/// Devices tab ViewModel: BLE scan simulation, connect/disconnect, and navigation to Measure.
/// Uses CommunityToolkit.Mvvm for bindable state and commands.
/// </summary>
public partial class DeviceViewModel : LocalizedViewModelBase
{
	const int ConnectedTab = 0;
	const int AssociatedTab = 1;
	const int NearbyTab = 2;

	[ObservableProperty] private string _pageSubtitle = string.Empty;
	[ObservableProperty] private string _headerTitle = string.Empty;
	[ObservableProperty] private string _connectedTitle = string.Empty;
	[ObservableProperty] private string _connectedSubtitle = string.Empty;
	[ObservableProperty] private string _associatedTitle = string.Empty;
	[ObservableProperty] private string _associatedSubtitle = string.Empty;
	[ObservableProperty] private string _nearbyTitle = string.Empty;
	[ObservableProperty] private string _nearbySubtitle = string.Empty;
	[ObservableProperty] private string _noConnectedHint = string.Empty;
	[ObservableProperty] private string _noAssociatedHint = string.Empty;
	[ObservableProperty] private string _noNearbyHint = string.Empty;
	[ObservableProperty] private string _hintText = string.Empty;
	[ObservableProperty] private string _measureButtonText = string.Empty;
	[ObservableProperty] private bool _isScanning;
	[ObservableProperty] private int _connectedCount;
	[ObservableProperty] private int _associatedCount;
	[ObservableProperty] private int _nearbyCount;
	[ObservableProperty] private int _selectedTabIndex;

	/// <summary>True when no instruments are connected — drives empty-state hint visibility.</summary>
	public bool ShowNoConnectedHint => ConnectedCount == 0;

	/// <summary>True when the Associated list is empty — drives empty-state hint visibility.</summary>
	public bool ShowNoAssociatedHint => AssociatedCount == 0;

	/// <summary>True when the Nearby list is empty — drives empty-state hint visibility.</summary>
	public bool ShowNoNearbyHint => NearbyCount == 0;

	/// <summary>True while a discovery scan is running — drives scan banner visibility.</summary>
	public bool ShowScanBanner => IsScanning;

	/// <summary>Toolbar label toggles between Start Scan and Stop based on <see cref="IsScanning"/>.</summary>
	public string ScanToolbarText => IsScanning ? Loc.T("Toolbar_Stop") : Loc.T("Toolbar_Scan");

	/// <summary>Status line shown under the header during an active scan.</summary>
	public string ScanStatusText => IsScanning ? Loc.T("Device_ScanStatus") : string.Empty;

	// ── Tab selection state (iOS-style segmented control) ──────────────
	public bool IsConnectedTabSelected => SelectedTabIndex == ConnectedTab;
	public bool IsAssociatedTabSelected => SelectedTabIndex == AssociatedTab;
	public bool IsNearbyTabSelected => SelectedTabIndex == NearbyTab;

	/// <summary>Combined "Label (N)" string for each segment.</summary>
	public string ConnectedTabLabel => $"{ConnectedTitle} ({ConnectedCount})";
	public string AssociatedTabLabel => $"{AssociatedTitle} ({AssociatedCount})";
	public string NearbyTabLabel => $"{NearbyTitle} ({NearbyCount})";

	public Color ConnectedTabBackground => SegmentBackground(IsConnectedTabSelected);
	public Color AssociatedTabBackground => SegmentBackground(IsAssociatedTabSelected);
	public Color NearbyTabBackground => SegmentBackground(IsNearbyTabSelected);

	public Color ConnectedTabTextColor => SegmentTextColor(IsConnectedTabSelected);
	public Color AssociatedTabTextColor => SegmentTextColor(IsAssociatedTabSelected);
	public Color NearbyTabTextColor => SegmentTextColor(IsNearbyTabSelected);

	public FontAttributes ConnectedTabFontAttributes => SegmentFontAttributes(IsConnectedTabSelected);
	public FontAttributes AssociatedTabFontAttributes => SegmentFontAttributes(IsAssociatedTabSelected);
	public FontAttributes NearbyTabFontAttributes => SegmentFontAttributes(IsNearbyTabSelected);

	public double ConnectedTabShadowOpacity => SegmentShadowOpacity(IsConnectedTabSelected);
	public double AssociatedTabShadowOpacity => SegmentShadowOpacity(IsAssociatedTabSelected);
	public double NearbyTabShadowOpacity => SegmentShadowOpacity(IsNearbyTabSelected);

	/// <summary>
	/// Opacity applied to the Connected and Associated tab segments while a
	/// scan is running. Nearby stays at 1.0 since the user is locked on it.
	/// </summary>
	public double ScanLockedTabOpacity => IsScanning ? 0.4 : 1.0;

	/// <summary>Active section subtitle shown under the tab bar.</summary>
	public string ActiveSectionSubtitle => SelectedTabIndex switch
	{
		AssociatedTab => AssociatedSubtitle,
		NearbyTab => NearbySubtitle,
		_ => ConnectedSubtitle,
	};

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
		NoAssociatedHint = Loc.T("Device_NoAssociated");
		NoNearbyHint = Loc.T("Device_NoNearby");
		HintText = Loc.T("Device_Hint");
		MeasureButtonText = Loc.T("Device_Measure");
		OnPropertyChanged(nameof(ScanToolbarText));
		OnPropertyChanged(nameof(ScanStatusText));
		OnPropertyChanged(nameof(ActiveSectionSubtitle));
	}

	public override void RefreshForTheme()
	{
		base.RefreshForTheme();
		RebuildLists();
		NotifyTabVisualsChanged();
	}

	/// <summary>
	/// Rebuilds all three device lists from the demo catalog and the current
	/// connection set. Used on initial load and theme refresh — NOT during a
	/// scan (use <see cref="RefreshScanResults"/> for that, since rebuilding
	/// Connected/Associated while their sections are hidden can leave their
	/// BindableLayout in a stale state on iOS).
	/// </summary>
	void RebuildLists()
	{
		Replace(ConnectedDevices, DemoDeviceCatalog.ConnectedDevices(_connectedIds));
		foreach (var item in ConnectedDevices)
			item.IsConnected = true;

		Replace(AssociatedDevices, DemoDeviceCatalog.AssociatedDevices(_connectedIds));
		Replace(AvailableDevices, DemoDeviceCatalog.AvailableDevices());
		UpdateCounts();
	}

	/// <summary>
	/// Refreshes only the Nearby (Available) list — the result of a BLE scan
	/// pass. Connected and Associated lists are left intact so they remain
	/// populated when the user navigates back to those tabs after the scan.
	/// </summary>
	void RefreshScanResults()
	{
		Replace(AvailableDevices, DemoDeviceCatalog.AvailableDevices());
		UpdateCounts();
	}

	void UpdateCounts()
	{
		ConnectedCount = ConnectedDevices.Count;
		AssociatedCount = AssociatedDevices.Count;
		NearbyCount = AvailableDevices.Count;
		OnPropertyChanged(nameof(ShowNoConnectedHint));
		OnPropertyChanged(nameof(ShowNoAssociatedHint));
		OnPropertyChanged(nameof(ShowNoNearbyHint));
	}

	static void Replace(ObservableCollection<DeviceListItem> target, IEnumerable<DeviceListItem> items)
	{
		target.Clear();
		foreach (var item in items)
			target.Add(item);
	}

	/// <summary>
	/// Switches the active section tab (0=Connected, 1=Associated, 2=Nearby).
	/// The command is blocked while a scan is running so the user stays on
	/// the Nearby tab and can't navigate away mid-scan.
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanSelectTab))]
	void SelectTab(object? parameter)
	{
		if (parameter is null)
			return;

		int index = parameter switch
		{
			int i => i,
			string s when int.TryParse(s, out var parsed) => parsed,
			_ => SelectedTabIndex
		};

		if (index < ConnectedTab || index > NearbyTab || index == SelectedTabIndex)
			return;

		SelectedTabIndex = index;
	}

	/// <summary>Tab switching is disabled while a scan is in progress.</summary>
	bool CanSelectTab(object? parameter) => !IsScanning;

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

		UpdateCounts();
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

		UpdateCounts();
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

	/// <summary>
	/// Opens the Measure tab for a connected instrument that supports
	/// measurement. The modal Devices page is popped without animation so
	/// the transition feels instant — without this, the default ~300ms iOS
	/// pop animation serialises before the tab switch and the screen looks
	/// like it's "loading".
	/// </summary>
	[RelayCommand]
	async Task OpenMeasureAsync(DeviceListItem? item)
	{
		if (item is null || !item.CanOpenMeasure || item.InstrumentKind is not { } kind
		    || Shell.Current is not AppShell shell)
			return;

		if (shell.Navigation.NavigationStack.Count > 1)
			await shell.Navigation.PopAsync(animated: false);

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

	/// <summary>
	/// Simulates a BLE discovery pass. While scanning is active the user is
	/// locked onto the Nearby tab; after the pass completes only the Nearby
	/// (Available) list is refreshed — Connected and Associated are left
	/// alone so they stay populated when the user navigates back to them.
	/// </summary>
	[RelayCommand]
	async Task ScanAsync()
	{
		if (IsScanning)
			return;

		_scanCts?.Cancel();
		_scanCts = new CancellationTokenSource();
		var token = _scanCts.Token;

		// Force the Nearby tab BEFORE flipping IsScanning so the tab switch
		// goes through (CanSelectTab returns false once IsScanning is true,
		// but we're setting SelectedTabIndex directly so it isn't blocked).
		if (SelectedTabIndex != NearbyTab)
			SelectedTabIndex = NearbyTab;

		IsScanning = true;
		OnScanStateChanged();

		try
		{
			await Task.Delay(2400, token);
			if (!token.IsCancellationRequested)
				RefreshScanResults();
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
		OnPropertyChanged(nameof(ScanLockedTabOpacity));
		SelectTabCommand.NotifyCanExecuteChanged();
	}

	partial void OnIsScanningChanged(bool value) => OnScanStateChanged();

	partial void OnSelectedTabIndexChanged(int value)
	{
		OnPropertyChanged(nameof(IsConnectedTabSelected));
		OnPropertyChanged(nameof(IsAssociatedTabSelected));
		OnPropertyChanged(nameof(IsNearbyTabSelected));
		OnPropertyChanged(nameof(ActiveSectionSubtitle));
		NotifyTabVisualsChanged();
	}

	partial void OnConnectedSubtitleChanged(string value) => OnPropertyChanged(nameof(ActiveSectionSubtitle));
	partial void OnAssociatedSubtitleChanged(string value) => OnPropertyChanged(nameof(ActiveSectionSubtitle));
	partial void OnNearbySubtitleChanged(string value) => OnPropertyChanged(nameof(ActiveSectionSubtitle));

	void NotifyTabVisualsChanged()
	{
		OnPropertyChanged(nameof(ConnectedTabBackground));
		OnPropertyChanged(nameof(AssociatedTabBackground));
		OnPropertyChanged(nameof(NearbyTabBackground));
		OnPropertyChanged(nameof(ConnectedTabTextColor));
		OnPropertyChanged(nameof(AssociatedTabTextColor));
		OnPropertyChanged(nameof(NearbyTabTextColor));
		OnPropertyChanged(nameof(ConnectedTabFontAttributes));
		OnPropertyChanged(nameof(AssociatedTabFontAttributes));
		OnPropertyChanged(nameof(NearbyTabFontAttributes));
		OnPropertyChanged(nameof(ConnectedTabShadowOpacity));
		OnPropertyChanged(nameof(AssociatedTabShadowOpacity));
		OnPropertyChanged(nameof(NearbyTabShadowOpacity));
	}

	void NotifyTabLabelsChanged()
	{
		OnPropertyChanged(nameof(ConnectedTabLabel));
		OnPropertyChanged(nameof(AssociatedTabLabel));
		OnPropertyChanged(nameof(NearbyTabLabel));
	}

	partial void OnConnectedTitleChanged(string value) => NotifyTabLabelsChanged();
	partial void OnAssociatedTitleChanged(string value) => NotifyTabLabelsChanged();
	partial void OnNearbyTitleChanged(string value) => NotifyTabLabelsChanged();
	partial void OnConnectedCountChanged(int value) => OnPropertyChanged(nameof(ConnectedTabLabel));
	partial void OnAssociatedCountChanged(int value) => OnPropertyChanged(nameof(AssociatedTabLabel));
	partial void OnNearbyCountChanged(int value) => OnPropertyChanged(nameof(NearbyTabLabel));

	// iOS-style segmented control: active pill uses Surface fill, inactive
	// segments stay transparent over the SurfaceSecondary container.
	static Color SegmentBackground(bool isSelected) =>
		isSelected ? ThemeColors.Surface : Colors.Transparent;

	static Color SegmentTextColor(bool isSelected) =>
		isSelected ? ThemeColors.OnSurface : ThemeColors.OnSurfaceVariant;

	static FontAttributes SegmentFontAttributes(bool isSelected) =>
		isSelected ? FontAttributes.Bold : FontAttributes.None;

	// Soft drop shadow for the active segment (light-mode reads as ~12% black).
	// Dark mode uses a deeper opacity so the surface still separates from the
	// SurfaceSecondary track.
	static double SegmentShadowOpacity(bool isSelected)
	{
		if (!isSelected)
			return 0;
		return ThemeColors.IsDark ? 0.45 : 0.12;
	}
}
