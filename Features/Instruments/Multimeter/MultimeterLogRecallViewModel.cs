using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>HI98494 / HI98594 log recall list, sync, download, and share.</summary>
public partial class MultimeterLogRecallViewModel : PageViewModelBase
{
	[ObservableProperty] private MultimeterLogFilter _activeFilter = MultimeterLogFilter.All;
	[ObservableProperty] private bool _isSyncing;

	public ObservableCollection<MultimeterLogItemViewModel> Logs { get; } = new();

	public int TotalLogs => Logs.Count;
	public int LotLogCount => Logs.Count(l => l.FileType == MultimeterLogFileType.Lot);
	public int LodLogCount => Logs.Count(l => l.FileType == MultimeterLogFileType.Lod);

	public IEnumerable<MultimeterLogItemViewModel> VisibleLogs => ActiveFilter switch
	{
		MultimeterLogFilter.Lot => Logs.Where(l => l.FileType == MultimeterLogFileType.Lot),
		MultimeterLogFilter.Lod => Logs.Where(l => l.FileType == MultimeterLogFileType.Lod),
		_ => Logs
	};

	public event EventHandler? LogsChanged;

	public MultimeterLogRecallViewModel() => LoadLogs();

	public override void RefreshForTheme()
	{
		LoadLogs();
		NotifyCounts();
	}

	void LoadLogs()
	{
		Logs.Clear();
		foreach (var item in SampleLogs)
			Logs.Add(item);
		NotifyCounts();
		LogsChanged?.Invoke(this, EventArgs.Empty);
	}

	void NotifyCounts()
	{
		OnPropertyChanged(nameof(TotalLogs));
		OnPropertyChanged(nameof(LotLogCount));
		OnPropertyChanged(nameof(LodLogCount));
		OnPropertyChanged(nameof(VisibleLogs));
	}

	partial void OnActiveFilterChanged(MultimeterLogFilter value)
	{
		OnPropertyChanged(nameof(VisibleLogs));
		LogsChanged?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand]
	void SetFilter(string filter) =>
		ActiveFilter = filter switch
		{
			"LOT" => MultimeterLogFilter.Lot,
			"LOD" => MultimeterLogFilter.Lod,
			_ => MultimeterLogFilter.All
		};

	[RelayCommand]
	async Task SyncLogsAsync()
	{
		if (IsSyncing)
			return;
		IsSyncing = true;
		try
		{
			await Task.Delay(1400);
			LoadLogs();
		}
		finally
		{
			IsSyncing = false;
		}
	}

	[RelayCommand]
	async Task DownloadLogAsync(MultimeterLogItemViewModel? log)
	{
		if (log is null || log.IsDownloading)
			return;

		var replacing = false;
		if (log.IsDownloaded)
		{
			if (Shell.Current?.CurrentPage is not Page page)
				return;

			var confirmReplace = await page.DisplayAlertAsync(
				"Already downloaded",
				$"\"{log.Title}\" is already saved in Hanna Lab. Download again and replace the existing file?",
				"Download and replace",
				"Cancel");

			if (!confirmReplace)
				return;

			replacing = true;
		}

		await PerformDownloadAsync(log, replacing);
	}

	async Task PerformDownloadAsync(MultimeterLogItemViewModel log, bool replacing)
	{
		log.IsDownloading = true;
		LogsChanged?.Invoke(this, EventArgs.Empty);
		try
		{
			await Task.Delay(1500);
			log.IsDownloaded = true;
			if (Shell.Current?.CurrentPage is Page page)
			{
				var message = replacing
					? $"{log.RecordCount} records from \"{log.Title}\" replaced the previous download in Hanna Lab."
					: $"{log.RecordCount} records from \"{log.Title}\" were saved to Hanna Lab.";

				await page.DisplayAlertAsync("Download complete", message, "OK");
			}
		}
		finally
		{
			log.IsDownloading = false;
			LogsChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	[RelayCommand]
	async Task ShareLogAsync(MultimeterLogItemViewModel? log)
	{
		if (log is null)
			return;

		var text =
			$"HI98x94 log file: {log.Title}\n" +
			$"Type: {log.FileTypeLabel}\n" +
			$"Records: {log.RecordCount}\n" +
			$"Start: {log.StartRecorded}\n" +
			$"Stop: {log.StopRecorded}\n" +
			$"Parameters: {log.ParametersSummary}";

		await Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = log.Title,
			Text = text
		});
	}

	static IEnumerable<MultimeterLogItemViewModel> SampleLogs =>
	[
		new()
		{
			Id = "lod-all",
			Title = "LOD_ALL",
			FileType = MultimeterLogFileType.Lod,
			StartRecorded = "1/23/26 • 4:02:11 PM",
			StopRecorded = "1/23/26 • 6:04:18 PM",
			RecordCount = 25,
			Parameters =
			[
				new("pH", "pH"),
				new("EC", "µS/cm"),
				new("DO", "%"),
				new("Temperature", "°C")
			],
			IsDownloaded = true
		},
		new()
		{
			Id = "lod5",
			Title = "LOD5",
			FileType = MultimeterLogFileType.Lod,
			StartRecorded = "1/12/26 • 10:15:02 AM",
			StopRecorded = "1/12/26 • 11:57:48 AM",
			RecordCount = 12,
			Parameters =
			[
				new("pH", "pH"),
				new("EC", "mS/cm"),
				new("Salinity", "ppt")
			]
		},
		new()
		{
			Id = "lot2-all",
			Title = "LOT2-All",
			FileType = MultimeterLogFileType.Lot,
			StartRecorded = "6/13/25 • 7:40:05 AM",
			StopRecorded = "6/13/25 • 9:18:13 AM",
			RecordCount = 72,
			Parameters =
			[
				new("pH", "pH"),
				new("ORP", "mV"),
				new("DO", "%"),
				new("Pressure", "psi")
			],
			IsDownloaded = true
		},
		new()
		{
			Id = "lot-phe-glp",
			Title = "LOT-pHE-GLP",
			FileType = MultimeterLogFileType.Lot,
			StartRecorded = "6/9/25 • 12:05:44 AM",
			StopRecorded = "6/9/25 • 1:10:59 AM",
			RecordCount = 167,
			Parameters =
			[
				new("pH", "pH"),
				new("Temperature", "°C")
			]
		},
		new()
		{
			Id = "lod-pheco-pdo",
			Title = "LOD-PhECO-PDO",
			FileType = MultimeterLogFileType.Lod,
			StartRecorded = "4/12/25 • 8:22:31 AM",
			StopRecorded = "4/12/25 • 10:13:19 AM",
			RecordCount = 1247,
			Parameters =
			[
				new("pH", "pH"),
				new("EC", "µS/cm"),
				new("DO", "%"),
				new("Temperature", "°C"),
				new("Salinity", "ppt")
			],
			IsDownloaded = true
		},
		new()
		{
			Id = "lod-test",
			Title = "LOD-TEST",
			FileType = MultimeterLogFileType.Lod,
			StartRecorded = "3/11/25 • 9:48:02 AM",
			StopRecorded = "3/11/25 • 10:10:59 AM",
			RecordCount = 17,
			Parameters =
			[
				new("pH", "pH"),
				new("EC", "µS/cm")
			]
		},
		new()
		{
			Id = "lot-weekly",
			Title = "LOT-Weekly",
			FileType = MultimeterLogFileType.Lot,
			StartRecorded = "2/2/26 • 6:10:00 AM",
			StopRecorded = "2/2/26 • 8:45:22 AM",
			RecordCount = 48,
			Parameters =
			[
				new("pH", "pH"),
				new("EC", "mS/cm"),
				new("TDS", "ppm"),
				new("Temperature", "°C")
			]
		}
	];
}
