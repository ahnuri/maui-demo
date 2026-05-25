using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>
/// Log History detail ViewModel for one device family (Halo, photometer, or multimeter).
/// Manages model filters, edit mode, selection, sync, and navigation to tank readings or Halo detail.
/// </summary>
public partial class LogHistoryDeviceLogsViewModel : PageViewModelBase
{
	[ObservableProperty] private bool _isSyncing;
	[ObservableProperty] private bool _isEditMode;
	[ObservableProperty] private string _pageTitle = string.Empty;
	[ObservableProperty] private string _pageSubtitle = string.Empty;

	public ObservableCollection<LogModelFilterChipViewModel> ModelFilters { get; } = new();
	public ObservableCollection<LogModelSectionViewModel> ModelSections { get; } = new();

	InstrumentKind _kind;
	string? _selectedModelFilterId;
	Page? _hostPage;
	readonly InstrumentLogNavigatorHost _logNavigators = AppServices.Get<InstrumentLogNavigatorHost>();

	/// <summary>Instrument family this list is filtered to. Set by <see cref="Load"/>.</summary>
	public InstrumentKind Kind => _kind;

	public bool IsPhotometerList => _kind == InstrumentKind.Photometer;
	public bool ShowLogEditButton => !IsPhotometerList;
	public bool ShowLogActionRow => ShowLogEditButton;
	public bool ShowEditModeActions => IsEditMode && ShowLogEditButton;
	public bool ShowClearSelection => IsEditMode && SelectedCount > 0;
	public string EditModeButtonText => IsEditMode ? "Done" : "Edit";
	public bool ShowSelectionBar => IsEditMode && SelectedCount > 0 && ShowLogEditButton;
	public string SelectionSummary => SelectedCount == 1 ? "1 log selected" : $"{SelectedCount} logs selected";
	public string ModelFilterHint => IsPhotometerList ? "Filter by device · tanks" : "Filter by device · log files";

	public int SelectedCount => VisibleLogEntries().Count(e => e.IsSelected);

	public void AttachHost(Page hostPage) => _hostPage = hostPage;

	public void Load(InstrumentKind kind)
	{
		_kind = kind;
		_selectedModelFilterId = null;
		IsEditMode = false;
		LogHistoryData.ReloadFromCatalog();
		WireEntries();
		PageTitle = TypeTitle(kind);
		PageSubtitle = kind == InstrumentKind.Photometer
			? "Tanks grouped by device"
			: "Log files grouped by device";
		LoadModelSections();
		OnPropertyChanged(nameof(IsPhotometerList));
		OnPropertyChanged(nameof(ShowLogEditButton));
		OnPropertyChanged(nameof(ModelFilterHint));
	}

	public override void RefreshForTheme() => LoadModelSections();

	public string TryGetTankName(string modelId, int tankId) =>
		LogHistoryCatalog.GetTankName(modelId, tankId);

	partial void OnIsEditModeChanged(bool value)
	{
		OnPropertyChanged(nameof(EditModeButtonText));
		OnPropertyChanged(nameof(ShowEditModeActions));
		OnPropertyChanged(nameof(ShowLogActionRow));
		OnPropertyChanged(nameof(ShowSelectionBar));
		OnPropertyChanged(nameof(ShowClearSelection));

		foreach (var entry in VisibleLogEntries())
			entry.SetEditModeActive(value);

		if (!value)
			ClearSelection();
	}

	void WireEntries()
	{
		foreach (var entry in LogHistoryData.Entries.Where(e => e.InstrumentKind == _kind))
		{
			entry.Owner = this;
			entry.PropertyChanged -= OnEntryPropertyChanged;
			entry.PropertyChanged += OnEntryPropertyChanged;
		}
	}

	void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(LogEntryViewModel.IsSelected))
			NotifySelection();
	}

	void NotifySelection()
	{
		OnPropertyChanged(nameof(SelectedCount));
		OnPropertyChanged(nameof(ShowSelectionBar));
		OnPropertyChanged(nameof(ShowClearSelection));
		OnPropertyChanged(nameof(SelectionSummary));
	}

	[RelayCommand]
	void ToggleEditMode() => IsEditMode = !IsEditMode;

	[RelayCommand]
	void SelectModelFilter(LogModelFilterChipViewModel? chip)
	{
		if (chip is null)
			return;

		_selectedModelFilterId = chip.ModelId;
		ApplyModelFilterSelection();
		LoadModelSections();
	}

	/// <summary>Opens tank readings via the photometer log navigator module.</summary>
	internal async Task OpenTankGroupAsync(LogTankGroupViewModel tank)
	{
		if (IsEditMode || _hostPage is null)
			return;

		await _logNavigators.Get(InstrumentKind.Photometer).NavigateToTankAsync(_hostPage, tank);
	}

	public void RefreshAfterTankRename() => LoadModelSections();

	[RelayCommand]
	void SelectAllVisible()
	{
		foreach (var entry in VisibleLogEntries())
			entry.IsSelected = true;
	}

	[RelayCommand]
	void ClearSelection()
	{
		foreach (var entry in LogHistoryData.Entries)
			entry.IsSelected = false;
	}

	[RelayCommand]
	async Task ShareSelectedAsync()
	{
		var selected = VisibleLogEntries().Where(e => e.IsSelected).ToList();
		if (selected.Count == 0 || Shell.Current?.CurrentPage is not Page page)
			return;

		var format = await page.DisplayActionSheetAsync("Export selected logs", "Cancel", null, "PDF", "CSV");
		if (string.IsNullOrEmpty(format) || format == "Cancel")
			return;

		await Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = $"Hanna Lab logs ({format})",
			Text = BuildExportBody(selected, format)
		});
	}

	[RelayCommand]
	async Task DeleteSelectedAsync()
	{
		var selected = VisibleLogEntries().Where(e => e.IsSelected).ToList();
		if (selected.Count == 0 || Shell.Current?.CurrentPage is not Page page)
			return;

		var message = selected.Count == 1
			? $"Delete \"{selected[0].Title}\" from Hanna Lab? This cannot be undone."
			: $"Delete {selected.Count} logs from Hanna Lab? This cannot be undone.";

		if (!await page.DisplayAlertAsync("Delete logs", message, "Delete", "Cancel"))
			return;

		LogHistoryData.RemoveEntries(selected);
		WireEntries();
		LoadModelSections();
		NotifySelection();
	}

	[RelayCommand]
	async Task SyncSelectedToCloudAsync()
	{
		var selected = VisibleLogEntries().Where(e => e.IsSelected).ToList();
		if (selected.Count == 0 || Shell.Current?.CurrentPage is not Page page || IsSyncing)
			return;

		IsSyncing = true;
		try
		{
			await Task.Delay(1400);
			foreach (var entry in selected)
				entry.IsUploadedToCloud = true;
			await page.DisplayAlertAsync("Cloud upload", $"{selected.Count} log file(s) uploaded to cloud.", "OK");
		}
		finally
		{
			IsSyncing = false;
		}
	}

	/// <summary>Routes log row taps to the instrument-specific log navigator.</summary>
	internal async Task HandleLogTapAsync(LogEntryViewModel entry)
	{
		if (IsEditMode)
		{
			entry.IsSelected = !entry.IsSelected;
			return;
		}

		if (_hostPage is null)
			return;

		await _logNavigators.Get(entry.InstrumentKind).NavigateToSessionAsync(_hostPage, entry);
	}

	void LoadModelSections()
	{
		LoadModelFilters();
		ModelSections.Clear();

		var models = LogHistoryCatalog.ModelsFor(_kind)
			.Where(m => _selectedModelFilterId is null || m.Id == _selectedModelFilterId);

		foreach (var model in models)
		{
			var modelEntries = LogHistoryData.Entries.Where(e => e.DeviceModelId == model.Id).ToList();
			if (modelEntries.Count == 0)
				continue;

			var section = new LogModelSectionViewModel
			{
				ModelId = model.Id,
				SerialNumber = model.SerialNumber,
				DeviceName = model.DeviceName,
				FirmwareVersion = model.FirmwareVersion,
				BleVersion = model.BleVersion,
				Kind = _kind
			};

			if (_kind == InstrumentKind.Photometer)
			{
				foreach (var tank in BuildTankGroupsForModel(model.Id, modelEntries))
					section.TankGroups.Add(tank);
				if (section.TankGroups.Count == 0)
					continue;
			}
			else
			{
				foreach (var entry in modelEntries.OrderByDescending(e => e.Start))
					section.LogEntries.Add(entry);
			}

			ModelSections.Add(section);
		}
	}

	void LoadModelFilters()
	{
		ModelFilters.Clear();
		var unit = _kind == InstrumentKind.Photometer ? "tanks" : "logs";
		var entries = LogHistoryData.Entries.Where(e => e.InstrumentKind == _kind).ToList();

		var totalCount = _kind == InstrumentKind.Photometer ? CountTanks(entries) : entries.Count;

		ModelFilters.Add(new LogModelFilterChipViewModel
		{
			ModelId = null,
			Label = "All",
			Count = totalCount,
			UnitLabel = unit
		});

		foreach (var model in LogHistoryCatalog.ModelsFor(_kind))
		{
			var modelEntries = entries.Where(e => e.DeviceModelId == model.Id).ToList();
			if (modelEntries.Count == 0)
				continue;

			var count = _kind == InstrumentKind.Photometer ? CountTanks(modelEntries) : modelEntries.Count;
			ModelFilters.Add(new LogModelFilterChipViewModel
			{
				ModelId = model.Id,
				Label = model.DeviceLabel,
				Count = count,
				UnitLabel = unit
			});
		}

		ApplyModelFilterSelection();
	}

	void ApplyModelFilterSelection()
	{
		foreach (var chip in ModelFilters)
			chip.ApplySelection(chip.ModelId == _selectedModelFilterId, LogDeviceVisuals.Accent);
	}

	List<LogTankGroupViewModel> BuildTankGroupsForModel(string modelId, List<LogEntryViewModel> modelEntries)
	{
		var result = new List<LogTankGroupViewModel>();
		foreach (var group in modelEntries.Where(e => e.TankId is int).GroupBy(e => e.TankId!.Value).OrderBy(g => g.Key))
		{
			var entries = group.ToList();
			var tankId = group.Key;
			var catalogRecords = LogHistoryCatalog.ReadingsForTank(modelId, tankId).Count;
			var sessionRecords = entries.Sum(e => ParseRecordCount(e.RecordCount));
			var recordCount = modelId == "photo-1" && tankId == 5
				? 247
				: Math.Max(catalogRecords, sessionRecords);

			var readings = LogHistoryCatalog.ReadingsForTank(modelId, tankId);
			var vm = new LogTankGroupViewModel
			{
				DeviceModelId = modelId,
				TankId = tankId,
				TankName = LogHistoryCatalog.GetTankName(modelId, tankId),
				LogFileCount = entries.Count,
				RecordCount = recordCount,
				DateRangeSummary = BuildDateRangeSummary(entries),
				IsUploadedToCloud = entries.All(e => e.IsUploadedToCloud)
					&& (readings.Count == 0 || readings.All(r => r.IsUploadedToCloud))
			};
			vm.Owner = this;
			result.Add(vm);
		}

		return result;
	}

	IEnumerable<LogEntryViewModel> VisibleLogEntries()
	{
		if (_kind == InstrumentKind.Photometer)
			return [];

		return ModelSections.SelectMany(s => s.LogEntries);
	}

	static string TypeTitle(InstrumentKind kind) => kind switch
	{
		InstrumentKind.Halo2 => "Halo 2",
		InstrumentKind.Photometer => "HI97115 Photometer",
		InstrumentKind.Multimeter => "Multimeter",
		_ => "Logs"
	};

	static int CountTanks(IEnumerable<LogEntryViewModel> entries) =>
		entries.Where(e => e.TankId is int).Select(e => (e.DeviceModelId, e.TankId)).Distinct().Count();

	static int ParseRecordCount(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return 0;
		var digits = new string(text.Where(char.IsDigit).ToArray());
		return int.TryParse(digits, out var n) ? n : 0;
	}

	static string BuildDateRangeSummary(IReadOnlyList<LogEntryViewModel> entries)
	{
		if (entries.Count == 0)
			return "—";
		if (entries.Count == 1)
			return entries[0].DateRangeLabel;
		return $"{entries.Min(e => e.Start)}  →  {entries.Max(e => e.Stop)}";
	}

	static string BuildExportBody(IReadOnlyList<LogEntryViewModel> logs, string format)
	{
		if (format == "CSV")
		{
			var lines = new List<string> { "Device,Model,Title,TankId,Start,Stop,Records,Parameters" };
			foreach (var log in logs)
			{
				lines.Add(string.Join(",",
					Csv(log.DeviceBadge), Csv(log.DeviceModelId), Csv(log.Title),
					Csv(log.TankId?.ToString() ?? ""), Csv(log.Start), Csv(log.Stop),
					Csv(log.RecordCount ?? ""), Csv(log.ParametersFullSummary)));
			}
			return string.Join(Environment.NewLine, lines);
		}

		var pdfLines = logs.Select(log =>
			$"• {log.Title} ({log.DeviceBadge})\n  {log.Start} → {log.Stop}\n  Records: {log.RecordCount}");
		return "Hanna Lab — Log history export\n\n" + string.Join("\n\n", pdfLines);
	}

	static string Csv(string value) =>
		"\"" + value.Replace("\"", "\"\"") + "\"";
}
