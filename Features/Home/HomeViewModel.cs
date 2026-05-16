using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Home;

/// <summary>Home dashboard: log previews and navigation.</summary>
public partial class HomeViewModel : PageViewModelBase
{
	[ObservableProperty] private int _todayLogsCount = 24;
	[ObservableProperty] private int _connectedDevicesCount;
	[ObservableProperty] private int _selectedLogTabIndex;

	[ObservableProperty] private Color _tabHaloBackground = AppConstants.Primary;
	[ObservableProperty] private Color _tabPhotoBackground = Colors.Transparent;
	[ObservableProperty] private Color _tabMultiBackground = Colors.Transparent;
	[ObservableProperty] private Color _tabHaloLabelColor = Colors.White;
	[ObservableProperty] private Color _tabPhotoLabelColor = ThemeColors.OnSurfaceVariant;
	[ObservableProperty] private Color _tabMultiLabelColor = ThemeColors.OnSurfaceVariant;
	[ObservableProperty] private FontAttributes _tabHaloFont = FontAttributes.Bold;
	[ObservableProperty] private FontAttributes _tabPhotoFont = FontAttributes.None;
	[ObservableProperty] private FontAttributes _tabMultiFont = FontAttributes.None;

	public ObservableCollection<HomeLogRow> HaloLogs { get; } = new();
	public ObservableCollection<HomeLogRow> PhotoLogs { get; } = new();
	public ObservableCollection<HomeLogRow> MultiLogs { get; } = new();

	public bool IsHaloLogListVisible => SelectedLogTabIndex == 0;
	public bool IsPhotoLogListVisible => SelectedLogTabIndex == 1;
	public bool IsMultiLogListVisible => SelectedLogTabIndex == 2;

	public HomeViewModel()
	{
		LoadLogRows();
		SyncLogTabChrome();
	}

	public override void RefreshForTheme()
	{
		LoadLogRows();
		SyncLogTabChrome();
		OnPropertyChanged(nameof(IsHaloLogListVisible));
		OnPropertyChanged(nameof(IsPhotoLogListVisible));
		OnPropertyChanged(nameof(IsMultiLogListVisible));
	}

	void LoadLogRows()
	{
		ReplaceRows(HaloLogs,
		[
			("Halo-2-pH", "Ends: 10 mins ago", "78"),
			("HI9810392-Halo2 (auto)", "Ends: 2 days ago", "3.6K"),
			("HI9810392-Halo2", "Ends: 3 days ago", "1.3K"),
		]);
		ReplaceRows(PhotoLogs,
		[
			("Tank Cl2", "Ends: 1 hour ago", "27"),
			("NH3 Tank2", "Ends: 2 hours ago", "55"),
			("Fish Tank2", "Ends: 11 hours ago", "73"),
		]);
		ReplaceRows(MultiLogs,
		[
			("PH-FULLGLP", "Ends: 3 hours ago", "49.9K"),
			("LOD-CL2 (LOD)", "Ends: 8 hours ago", "27"),
			("TDS-ALL", "Ends: 11 hours ago", "750"),
		]);
	}

	static void ReplaceRows(ObservableCollection<HomeLogRow> target, (string title, string sub, string val)[] rows)
	{
		target.Clear();
		foreach (var r in rows)
		{
			var initials = r.title.Length >= 2 ? r.title[..2].ToUpperInvariant() : r.title.ToUpperInvariant();
			target.Add(new HomeLogRow(initials, r.title, r.sub, r.val));
		}
	}

	partial void OnSelectedLogTabIndexChanged(int value)
	{
		SyncLogTabChrome();
		OnPropertyChanged(nameof(IsHaloLogListVisible));
		OnPropertyChanged(nameof(IsPhotoLogListVisible));
		OnPropertyChanged(nameof(IsMultiLogListVisible));
	}

	void SyncLogTabChrome()
	{
		TabHaloBackground = SelectedLogTabIndex == 0 ? AppConstants.Primary : Colors.Transparent;
		TabPhotoBackground = SelectedLogTabIndex == 1 ? AppConstants.Primary : Colors.Transparent;
		TabMultiBackground = SelectedLogTabIndex == 2 ? AppConstants.Primary : Colors.Transparent;
		TabHaloLabelColor = SelectedLogTabIndex == 0 ? Colors.White : ThemeColors.OnSurfaceVariant;
		TabPhotoLabelColor = SelectedLogTabIndex == 1 ? Colors.White : ThemeColors.OnSurfaceVariant;
		TabMultiLabelColor = SelectedLogTabIndex == 2 ? Colors.White : ThemeColors.OnSurfaceVariant;
		TabHaloFont = SelectedLogTabIndex == 0 ? FontAttributes.Bold : FontAttributes.None;
		TabPhotoFont = SelectedLogTabIndex == 1 ? FontAttributes.Bold : FontAttributes.None;
		TabMultiFont = SelectedLogTabIndex == 2 ? FontAttributes.Bold : FontAttributes.None;
	}

	[RelayCommand]
	void SelectLogTab(object? parameter)
	{
		var index = ParseTabIndex(parameter);
		if (index is >= 0 and <= 2)
			SelectedLogTabIndex = index;
	}

	static int ParseTabIndex(object? parameter) => parameter switch
	{
		int i => i,
		string s when int.TryParse(s, out var n) => n,
		_ => -1
	};

	[RelayCommand]
	async Task ConnectDeviceAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}

	[RelayCommand]
	async Task ViewAllLogsAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//logs");
	}
}
