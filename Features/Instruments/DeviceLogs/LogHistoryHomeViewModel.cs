using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Log History landing — device type cards only.</summary>
public partial class LogHistoryHomeViewModel : LocalizedViewModelBase
{
	public ObservableCollection<LogDeviceTypeCardViewModel> DeviceTypeCards { get; } = new();

	public string EmptyHint => Loc.T("LogHistory_Empty");

	Page? _hostPage;

	public void AttachHost(Page hostPage) => _hostPage = hostPage;

	public override void RefreshForTheme() => LoadDeviceTypeCards();

	public void OnAppearing() => LoadDeviceTypeCards();

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(EmptyHint));
		LoadDeviceTypeCards();
	}

	void LoadDeviceTypeCards()
	{
		LogHistoryData.EnsureInitialized();
		DeviceTypeCards.Clear();

		foreach (var kind in new[] { InstrumentKind.Halo2, InstrumentKind.Photometer, InstrumentKind.Multimeter })
		{
			var models = LogHistoryCatalog.ModelsFor(kind);
			var entries = LogHistoryData.Entries.Where(e => e.InstrumentKind == kind).ToList();
			var connected = models.Count(m => entries.Any(e => e.DeviceModelId == m.Id));

			var cloud = CloudStatusFor(kind);
			DeviceTypeCards.Add(new LogDeviceTypeCardViewModel
			{
				Kind = kind,
				Title = TypeTitle(kind),
				Subtitle = TypeSubtitle(kind),
				FileCount = entries.Count,
				RecordCount = SumRecords(entries, kind),
				ConnectedDeviceCount = connected,
				LastRecordedLabel = LogHistoryCatalog.GetLastRecordedLabel(LogHistoryData.Entries, kind),
				CloudSyncStatus = cloud.Status,
				CloudSyncColor = cloud.Color
			});
		}
	}

	[RelayCommand]
	async Task OpenDeviceTypeAsync(LogDeviceTypeCardViewModel? card)
	{
		if (card is null)
			return;

		var nav = _hostPage?.Navigation ?? Shell.Current?.CurrentPage?.Navigation;
		if (nav is null)
			return;

		var page = AppServices.Get<LogHistoryDeviceLogsPage>();
		page.Initialize(card.Kind);
		await nav.PushAsync(page);
	}

	string TypeTitle(InstrumentKind kind) => kind switch
	{
		InstrumentKind.Halo2 => Loc.T("LogHistory_TypeTitle_Halo2"),
		InstrumentKind.Photometer => Loc.T("LogHistory_TypeTitle_Photometer"),
		InstrumentKind.Multimeter => Loc.T("LogHistory_TypeTitle_Multimeter"),
		_ => Loc.T("LogHistory_TypeTitle_Generic")
	};

	(string Status, Color Color) CloudStatusFor(InstrumentKind kind) =>
		kind switch
		{
			InstrumentKind.Halo2 => (Loc.T("LogHistory_CloudUpToDate"), ThemeColors.LabSuccess),
			InstrumentKind.Photometer => (Loc.T("LogHistory_CloudPending", 2), AppConstants.Primary),
			InstrumentKind.Multimeter => (Loc.T("LogHistory_CloudUpToDate"), ThemeColors.LabSuccess),
			_ => (Loc.T("Common_Empty"), ThemeColors.OnSurfaceVariant)
		};

	string TypeSubtitle(InstrumentKind kind) => kind switch
	{
		InstrumentKind.Halo2 => Loc.T("LogHistory_TypeSubtitle_MultipleDevices"),
		InstrumentKind.Photometer => Loc.T("LogHistory_TypeSubtitle_TanksMultipleDevices"),
		InstrumentKind.Multimeter => Loc.T("LogHistory_TypeSubtitle_MultipleDevices"),
		_ => string.Empty
	};

	static int SumRecords(IReadOnlyList<LogEntryViewModel> entries, InstrumentKind kind)
	{
		var fromSessions = entries.Sum(e => ParseRecordCount(e.RecordCount));
		if (kind != InstrumentKind.Photometer)
			return fromSessions;

		var tankKeys = entries.Where(e => e.TankId is int)
			.Select(e => (DeviceModelId: e.DeviceModelId, TankId: e.TankId!.Value))
			.Distinct()
			.ToList();

		var fromReadings = tankKeys.Sum(k =>
			k.DeviceModelId == "photo-1" && k.TankId == 5
				? 247
				: LogHistoryCatalog.ReadingsForTank(k.DeviceModelId, k.TankId).Count);

		return Math.Max(fromSessions, fromReadings);
	}

	static int ParseRecordCount(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return 0;
		var digits = new string(text.Where(char.IsDigit).ToArray());
		return int.TryParse(digits, out var n) ? n : 0;
	}
}
