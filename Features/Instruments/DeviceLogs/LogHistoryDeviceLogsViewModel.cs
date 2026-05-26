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
public partial class LogHistoryDeviceLogsViewModel : LocalizedViewModelBase
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
	public string EditModeButtonText => IsEditMode ? Loc.T("Common_Done") : Loc.T("Common_Edit");
	public bool ShowSelectionBar => IsEditMode && SelectedCount > 0 && ShowLogEditButton;
	public string SelectionSummary => SelectedCount == 1
		? Loc.T("LogHistory_SelectionFormat", 1)
		: Loc.T("LogHistory_SelectionFormatMany", SelectedCount);
	public string ModelFilterHint => IsPhotometerList
		? Loc.T("LogHistory_FilterByDeviceTanks")
		: Loc.T("LogHistory_FilterByDeviceFiles");

	public string TapToSelectHint => Loc.T("LogHistory_TapToSelect");
	public string SelectAllLabel => Loc.T("Common_SelectAll");
	public string ClearLabel => Loc.T("Common_Clear");
	public string ShareLabel => Loc.T("Common_Share");
	public string DeleteLabel => Loc.T("Common_Delete");
	public string SyncLabel => Loc.T("Common_Sync");

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
			? Loc.T("LogHistory_PageSubtitle_Tanks")
			: Loc.T("LogHistory_PageSubtitle_Files");
		LoadModelSections();
		OnPropertyChanged(nameof(IsPhotometerList));
		OnPropertyChanged(nameof(ShowLogEditButton));
		OnPropertyChanged(nameof(ModelFilterHint));
	}

	public override void RefreshForTheme() => LoadModelSections();

	protected override void ApplyLocalization()
	{
		PageTitle = TypeTitle(_kind);
		PageSubtitle = _kind == InstrumentKind.Photometer
			? Loc.T("LogHistory_PageSubtitle_Tanks")
			: Loc.T("LogHistory_PageSubtitle_Files");
		OnPropertyChanged(nameof(EditModeButtonText));
		OnPropertyChanged(nameof(SelectionSummary));
		OnPropertyChanged(nameof(ModelFilterHint));
		OnPropertyChanged(nameof(TapToSelectHint));
		OnPropertyChanged(nameof(SelectAllLabel));
		OnPropertyChanged(nameof(ClearLabel));
		OnPropertyChanged(nameof(ShareLabel));
		OnPropertyChanged(nameof(DeleteLabel));
		OnPropertyChanged(nameof(SyncLabel));
		LoadModelSections();
	}

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

		var cancel = Loc.T("Common_Cancel");
		var format = await page.DisplayActionSheetAsync(
			Loc.T("LogHistory_ExportTitle"),
			cancel,
			null,
			Loc.T("Common_Pdf"),
			Loc.T("Common_Csv"));
		if (string.IsNullOrEmpty(format) || format == cancel)
			return;

		await Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = Loc.T("LogHistory_ExportSubject", format),
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
			? Loc.T("LogHistory_DeleteSingleFormat", selected[0].Title)
			: Loc.T("LogHistory_DeleteMultiFormat", selected.Count);

		if (!await page.DisplayAlertAsync(
				Loc.T("LogHistory_DeleteDialogTitle"),
				message,
				Loc.T("Common_Delete"),
				Loc.T("Common_Cancel")))
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
			await page.DisplayAlertAsync(
				Loc.T("LogHistory_CloudUploadDialogTitle"),
				Loc.T("LogHistory_CloudUploadDialogFormat", selected.Count),
				Loc.T("Common_OK"));
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
		var unit = _kind == InstrumentKind.Photometer
			? Loc.T("LogHistory_TankCountUnit")
			: Loc.T("LogHistory_LogCountUnit");
		var entries = LogHistoryData.Entries.Where(e => e.InstrumentKind == _kind).ToList();

		var totalCount = _kind == InstrumentKind.Photometer ? CountTanks(entries) : entries.Count;

		ModelFilters.Add(new LogModelFilterChipViewModel
		{
			ModelId = null,
			Label = Loc.T("Multimeter_LogRecall_FilterAll"),
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

	string TypeTitle(InstrumentKind kind) => kind switch
	{
		InstrumentKind.Halo2 => Loc.T("LogHistory_TypeTitle_Halo2"),
		InstrumentKind.Photometer => Loc.T("LogHistory_TypeTitle_Photometer"),
		InstrumentKind.Multimeter => Loc.T("LogHistory_TypeTitle_Multimeter"),
		_ => Loc.T("LogHistory_TypeTitle_Generic")
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

	string BuildDateRangeSummary(IReadOnlyList<LogEntryViewModel> entries)
	{
		if (entries.Count == 0)
			return Loc.T("Common_Empty");
		if (entries.Count == 1)
			return entries[0].DateRangeLabel;
		return Loc.T("LogHistory_DateRangeArrowFormat", entries.Min(e => e.Start) ?? string.Empty, entries.Max(e => e.Stop) ?? string.Empty);
	}

	string BuildExportBody(IReadOnlyList<LogEntryViewModel> logs, string format)
	{
		if (format == Loc.T("Common_Csv"))
		{
			var lines = new List<string> { Loc.T("LogHistory_ExportCsvHeader") };
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
		return Loc.T("LogHistory_ExportSubject_Pdf_Header") + "\n\n" + string.Join("\n\n", pdfLines);
	}

	static string Csv(string value) =>
		"\"" + value.Replace("\"", "\"\"") + "\"";
}
