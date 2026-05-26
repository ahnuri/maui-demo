using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo;

namespace HannaUIDemo.Features.Home;

/// <summary>Landing page: product overview, features, and primary navigation.</summary>
public partial class HomeViewModel : LocalizedViewModelBase
{
	public HomeViewModel() => ApplyLocalization();

	[ObservableProperty] private int _todayLogsCount = 24;
	[ObservableProperty] private int _connectedDevicesCount;
	[ObservableProperty] private string _connectedSummary = string.Empty;

	public string HeroBadge { get; private set; } = string.Empty;
	public string HeroTitle { get; private set; } = string.Empty;
	public string HeroSubtitle { get; private set; } = string.Empty;
	public string ConnectedLabel { get; private set; } = string.Empty;
	public string RecordsTodayLabel { get; private set; } = string.Empty;
	public string RecordsTodaySub { get; private set; } = string.Empty;
	public string ScanInstrumentsLabel { get; private set; } = string.Empty;
	public string SupportedFamiliesLabel { get; private set; } = string.Empty;
	public string ChipHalo { get; private set; } = string.Empty;
	public string ChipMarineMaster { get; private set; } = string.Empty;
	public string ChipPhotometer { get; private set; } = string.Empty;
	public string ChipMultiparameter { get; private set; } = string.Empty;
	public string ChipMeter { get; private set; } = string.Empty;
	public string ChipBluetooth { get; private set; } = string.Empty;
	public string MeasureTitle { get; private set; } = string.Empty;
	public string MeasureSub { get; private set; } = string.Empty;
	public string LogsTitle { get; private set; } = string.Empty;
	public string LogsSub { get; private set; } = string.Empty;
	public string WorkflowSection { get; private set; } = string.Empty;
	public string WorkflowConnect { get; private set; } = string.Empty;
	public string WorkflowConnectSub { get; private set; } = string.Empty;
	public string WorkflowMeasure { get; private set; } = string.Empty;
	public string WorkflowMeasureSub { get; private set; } = string.Empty;
	public string WorkflowStore { get; private set; } = string.Empty;
	public string WorkflowStoreSub { get; private set; } = string.Empty;
	public string WorkflowReview { get; private set; } = string.Empty;
	public string WorkflowReviewSub { get; private set; } = string.Empty;
	public string CapabilitiesSection { get; private set; } = string.Empty;
	public string CapabilityLiveStream { get; private set; } = string.Empty;
	public string CapabilityLiveStreamSub { get; private set; } = string.Empty;
	public string CapabilityTagReadings { get; private set; } = string.Empty;
	public string CapabilityTagReadingsSub { get; private set; } = string.Empty;
	public string CapabilityAutoManualLogs { get; private set; } = string.Empty;
	public string CapabilityAutoManualLogsSub { get; private set; } = string.Empty;
	public string CapabilityPdfCsvExport { get; private set; } = string.Empty;
	public string CapabilityPdfCsvExportSub { get; private set; } = string.Empty;
	public string CapabilityCloudSync { get; private set; } = string.Empty;
	public string CapabilityCloudSyncSub { get; private set; } = string.Empty;
	public string CapabilityMethodUnit { get; private set; } = string.Empty;
	public string CapabilityMethodUnitSub { get; private set; } = string.Empty;
	public string CapabilityTableGraph { get; private set; } = string.Empty;
	public string CapabilityTableGraphSub { get; private set; } = string.Empty;

	[RelayCommand]
	Task ScanDevicesAsync() => OpenDevicesAsync();

	[RelayCommand]
	Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return Task.CompletedTask;
		return nav.PushAsync(AppServices.Get<DevicePage>());
	}

	[RelayCommand]
	async Task DemoModeAsync()
	{
		if (Shell.Current is AppShell shell)
			await shell.PresentMeasureDevicePickerAsync();
	}

	[RelayCommand]
	async Task ViewLogsAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//logs");
	}

	protected override void ApplyLocalization()
	{
		HeroBadge = Loc.T("Home_HeroBadge");
		HeroTitle = Loc.T("Home_HeroTitle");
		HeroSubtitle = Loc.T("Home_HeroSubtitle");
		ConnectedLabel = Loc.T("Home_ConnectedLabel");
		RecordsTodayLabel = Loc.T("Home_RecordsToday");
		RecordsTodaySub = Loc.T("Home_RecordsTodaySub");
		ScanInstrumentsLabel = Loc.T("Home_ScanInstruments");
		SupportedFamiliesLabel = Loc.T("Home_SupportedLabel");
		ChipHalo = Loc.T("Home_ChipHalo");
		ChipMarineMaster = Loc.T("Home_ChipMarineMaster");
		ChipPhotometer = Loc.T("Home_ChipPhotometer");
		ChipMultiparameter = Loc.T("Home_ChipMultiparameter");
		ChipMeter = Loc.T("Home_ChipMeter");
		MeasureTitle = Loc.T("Home_MeasureTitle");
		MeasureSub = Loc.T("Home_MeasureSub");
		LogsTitle = Loc.T("Home_LogsTitle");
		LogsSub = Loc.T("Home_LogsSub");
		WorkflowSection = Loc.T("Home_WorkflowSection");
		WorkflowConnect = Loc.T("Home_WorkflowConnect");
		WorkflowConnectSub = Loc.T("Home_WorkflowConnectSub");
		WorkflowMeasure = Loc.T("Home_WorkflowMeasure");
		WorkflowMeasureSub = Loc.T("Home_WorkflowMeasureSub");
		WorkflowStore = Loc.T("Home_WorkflowStore");
		WorkflowStoreSub = Loc.T("Home_WorkflowStoreSub");
		WorkflowReview = Loc.T("Home_WorkflowReview");
		WorkflowReviewSub = Loc.T("Home_WorkflowReviewSub");
		CapabilitiesSection = Loc.T("Home_CapabilitiesSection");
		CapabilityLiveStream = Loc.T("Home_Capability_LiveStream");
		CapabilityLiveStreamSub = Loc.T("Home_Capability_LiveStreamSub");
		CapabilityTagReadings = Loc.T("Home_Capability_TagReadings");
		CapabilityTagReadingsSub = Loc.T("Home_Capability_TagReadingsSub");
		CapabilityAutoManualLogs = Loc.T("Home_Capability_AutoManualLogs");
		CapabilityAutoManualLogsSub = Loc.T("Home_Capability_AutoManualLogsSub");
		CapabilityPdfCsvExport = Loc.T("Home_Capability_PdfCsvExport");
		CapabilityPdfCsvExportSub = Loc.T("Home_Capability_PdfCsvExportSub");
		CapabilityCloudSync = Loc.T("Home_Capability_CloudSync");
		CapabilityCloudSyncSub = Loc.T("Home_Capability_CloudSyncSub");
		CapabilityMethodUnit = Loc.T("Home_Capability_MethodUnit");
		CapabilityMethodUnitSub = Loc.T("Home_Capability_MethodUnitSub");
		CapabilityTableGraph = Loc.T("Home_Capability_TableGraph");
		CapabilityTableGraphSub = Loc.T("Home_Capability_TableGraphSub");

		NotifyHeroStringsChanged();
		UpdateConnectedSummary();
	}

	void NotifyHeroStringsChanged()
	{
		OnPropertyChanged(nameof(HeroBadge));
		OnPropertyChanged(nameof(HeroTitle));
		OnPropertyChanged(nameof(HeroSubtitle));
		OnPropertyChanged(nameof(ConnectedLabel));
		OnPropertyChanged(nameof(RecordsTodayLabel));
		OnPropertyChanged(nameof(RecordsTodaySub));
		OnPropertyChanged(nameof(ScanInstrumentsLabel));
		OnPropertyChanged(nameof(SupportedFamiliesLabel));
		OnPropertyChanged(nameof(ChipHalo));
		OnPropertyChanged(nameof(ChipMarineMaster));
		OnPropertyChanged(nameof(ChipPhotometer));
		OnPropertyChanged(nameof(ChipMultiparameter));
		OnPropertyChanged(nameof(ChipMeter));
		//OnPropertyChanged(nameof(ChipBluetooth));
		OnPropertyChanged(nameof(MeasureTitle));
		OnPropertyChanged(nameof(MeasureSub));
		OnPropertyChanged(nameof(LogsTitle));
		OnPropertyChanged(nameof(LogsSub));
		OnPropertyChanged(nameof(WorkflowSection));
		OnPropertyChanged(nameof(WorkflowConnect));
		OnPropertyChanged(nameof(WorkflowConnectSub));
		OnPropertyChanged(nameof(WorkflowMeasure));
		OnPropertyChanged(nameof(WorkflowMeasureSub));
		OnPropertyChanged(nameof(WorkflowStore));
		OnPropertyChanged(nameof(WorkflowStoreSub));
		OnPropertyChanged(nameof(WorkflowReview));
		OnPropertyChanged(nameof(WorkflowReviewSub));
		OnPropertyChanged(nameof(CapabilitiesSection));
		OnPropertyChanged(nameof(CapabilityLiveStream));
		OnPropertyChanged(nameof(CapabilityLiveStreamSub));
		OnPropertyChanged(nameof(CapabilityTagReadings));
		OnPropertyChanged(nameof(CapabilityTagReadingsSub));
		OnPropertyChanged(nameof(CapabilityAutoManualLogs));
		OnPropertyChanged(nameof(CapabilityAutoManualLogsSub));
		OnPropertyChanged(nameof(CapabilityPdfCsvExport));
		OnPropertyChanged(nameof(CapabilityPdfCsvExportSub));
		OnPropertyChanged(nameof(CapabilityCloudSync));
		OnPropertyChanged(nameof(CapabilityCloudSyncSub));
		OnPropertyChanged(nameof(CapabilityMethodUnit));
		OnPropertyChanged(nameof(CapabilityMethodUnitSub));
		OnPropertyChanged(nameof(CapabilityTableGraph));
		OnPropertyChanged(nameof(CapabilityTableGraphSub));
	}

	partial void OnConnectedDevicesCountChanged(int value) => UpdateConnectedSummary();

	void UpdateConnectedSummary()
	{
		ConnectedSummary = ConnectedDevicesCount switch
		{
			0 => Loc.T("Home_ConnectedNone"),
			1 => Loc.T("Home_ConnectedOne"),
			_ => Loc.T("Home_ConnectedMany", ConnectedDevicesCount)
		};
	}
}
