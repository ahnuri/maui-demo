using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Photometer readings for one tank.</summary>
public partial class LogHistoryTankReadingsViewModel : PageViewModelBase
{
	[ObservableProperty] private string _tankTitle = string.Empty;
	[ObservableProperty] private string _tankSubtitle = string.Empty;
	[ObservableProperty] private string _dateFrom = "18/08/25";
	[ObservableProperty] private string _dateTo = "18/08/25";

	public ObservableCollection<PhotometerLogReadingViewModel> Readings { get; } = new();

	LogTankGroupViewModel? _tank;

	public void Load(LogTankGroupViewModel tank)
	{
		_tank = tank;
		TankTitle = tank.TankName;
		TankSubtitle = $"{tank.TankIdLabel} · {tank.RecordCountLabel}";
		Readings.Clear();

		foreach (var info in LogHistoryCatalog.ReadingsForTank(tank.DeviceModelId, tank.TankId))
		{
			Readings.Add(new PhotometerLogReadingViewModel
			{
				ParameterName = info.ParameterName,
				ValueDisplay = info.ValueDisplay,
				Note = string.IsNullOrEmpty(info.Note) ? "Note:" : info.Note,
				Timestamp = info.Timestamp,
				IsUploadedToCloud = info.IsUploadedToCloud
			});
		}

		if (Readings.Count > 0)
		{
			var day = Readings[^1].Timestamp.Split(',')[0];
			DateFrom = day;
			DateTo = day;
		}
	}

	[RelayCommand]
	async Task RenameTankAsync()
	{
		if (_tank is null || Shell.Current?.CurrentPage is not Page page)
			return;

		var name = await page.DisplayPromptAsync(
			"Rename tank",
			"Tank name (tank id stays the same)",
			initialValue: _tank.TankName,
			maxLength: 32);

		if (string.IsNullOrWhiteSpace(name))
			return;

		_tank.TankName = name.Trim();
		LogHistoryCatalog.SetTankName(_tank.DeviceModelId, _tank.TankId, _tank.TankName);
		TankTitle = _tank.TankName;
		_tank.Owner?.RefreshAfterTankRename();
	}
}
