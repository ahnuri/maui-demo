using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Measure;

/// <summary>Multimeter log recall list and sync action.</summary>
public partial class MultimeterLogRecallViewModel : PageViewModelBase
{
	[ObservableProperty] private int _totalLogs = 6;

	public ObservableCollection<MultimeterLogItemViewModel> Logs { get; } = new();

	public MultimeterLogRecallViewModel() => LoadLogs();

	public override void RefreshForTheme() => LoadLogs();

	void LoadLogs()
	{
		Logs.Clear();
		foreach (var item in SampleLogs)
			Logs.Add(item);
		TotalLogs = Logs.Count;
	}

	static IEnumerable<MultimeterLogItemViewModel> SampleLogs =>
	[
		new() { Title = "LOD_ALL (LOD)", Recorded = "1/23/26 • 6:04:18 PM", Samples = "25", IsSyncing = false },
		new() { Title = "LOD5 (LOD)", Recorded = "1/12/26 • 11:57:48 AM", Samples = "12", IsSyncing = true },
		new() { Title = "LOT2-All", Recorded = "6/13/25 • 9:18:13 AM", Samples = "72", IsSyncing = false },
		new() { Title = "pHE-GLP", Recorded = "6/9/25 • 1:10:59 AM", Samples = "167", IsSyncing = true },
		new() { Title = "PhECO-PDO", Recorded = "4/12/25 • 10:13:19 AM", Samples = "1247", IsSyncing = false },
		new() { Title = "LOD-TEST (LOD)", Recorded = "3/11/25 • 10:10:59 AM", Samples = "17", IsSyncing = true },
	];

	[RelayCommand]
	void SyncLogs() { }
}
