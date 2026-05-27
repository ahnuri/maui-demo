using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo;

namespace HannaUIDemo.Features.Home;

/// <summary>
/// Landing page view model. Surfaces hero metrics, the "How It Works" workflow
/// steps, supported instrument chips, and the trust footer line.
/// All user-facing copy is resolved through <see cref="LocalizedViewModelBase.Loc"/>
/// so the view never hard-codes strings.
/// </summary>
public partial class HomeViewModel : LocalizedViewModelBase
{
	public HomeViewModel() => ApplyLocalization();

	[ObservableProperty] private int _todayLogsCount = 24;
	[ObservableProperty] private int _connectedDevicesCount;
	[ObservableProperty] private string _connectedSummary = string.Empty;

	// Hero card
	public string HeroBadge { get; private set; } = string.Empty;
	public string HeroTitle { get; private set; } = string.Empty;
	public string HeroSubtitle { get; private set; } = string.Empty;
	public string ConnectedLabel { get; private set; } = string.Empty;
	public string ConnectedSub { get; private set; } = string.Empty;
	public string RecordsTodayLabel { get; private set; } = string.Empty;
	public string RecordsTodaySub { get; private set; } = string.Empty;
	public string ScanInstrumentsLabel { get; private set; } = string.Empty;
	public string ScanInstrumentsSub { get; private set; } = string.Empty;

	// How It Works workflow steps
	public string WorkflowSection { get; private set; } = string.Empty;
	public string WorkflowConnect { get; private set; } = string.Empty;
	public string WorkflowConnectSub { get; private set; } = string.Empty;
	public string WorkflowMeasure { get; private set; } = string.Empty;
	public string WorkflowMeasureSub { get; private set; } = string.Empty;
	public string WorkflowStore { get; private set; } = string.Empty;
	public string WorkflowStoreSub { get; private set; } = string.Empty;
	public string WorkflowReview { get; private set; } = string.Empty;
	public string WorkflowReviewSub { get; private set; } = string.Empty;

	// Supported devices section
	public string SupportedFamiliesLabel { get; private set; } = string.Empty;
	public string ViewAllLabel { get; private set; } = string.Empty;
	public string MoreSupportedLabel { get; private set; } = string.Empty;
	public string ChipHalo { get; private set; } = string.Empty;
	public string ChipMarineMaster { get; private set; } = string.Empty;
	public string ChipPhotometer { get; private set; } = string.Empty;
	public string ChipMultiparameter { get; private set; } = string.Empty;
	public string ChipMeter { get; private set; } = string.Empty;

	// Trust footer
	public string TrustTitle { get; private set; } = string.Empty;
	public string TrustSub { get; private set; } = string.Empty;

	[RelayCommand]
	Task ScanDevicesAsync() => OpenDevicesAsync();

	[RelayCommand]
	Task OpenDevicesAsync() // Opens the device list page, but could also trigger a scan and show a popup with results, etc.
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return Task.CompletedTask;
		return nav.PushAsync(AppServices.Get<DevicePage>());
	}

	protected override void ApplyLocalization()
	{
		HeroBadge = Loc.T("Home_HeroBadge");
		HeroTitle = Loc.T("Home_HeroTitle");
		HeroSubtitle = Loc.T("Home_HeroSubtitle");
		ConnectedLabel = Loc.T("Home_ConnectedLabel");
		ConnectedSub = Loc.T("Home_ConnectedSub");
		RecordsTodayLabel = Loc.T("Home_RecordsToday");
		RecordsTodaySub = Loc.T("Home_RecordsTodaySub");
		ScanInstrumentsLabel = Loc.T("Home_ScanInstruments");
		ScanInstrumentsSub = Loc.T("Home_ScanInstrumentsSub");

		WorkflowSection = Loc.T("Home_WorkflowSection");
		WorkflowConnect = Loc.T("Home_WorkflowConnect");
		WorkflowConnectSub = Loc.T("Home_WorkflowConnectSub");
		WorkflowMeasure = Loc.T("Home_WorkflowMeasure");
		WorkflowMeasureSub = Loc.T("Home_WorkflowMeasureSub");
		WorkflowStore = Loc.T("Home_WorkflowStore");
		WorkflowStoreSub = Loc.T("Home_WorkflowStoreSub");
		WorkflowReview = Loc.T("Home_WorkflowReview");
		WorkflowReviewSub = Loc.T("Home_WorkflowReviewSub");

		SupportedFamiliesLabel = Loc.T("Home_SupportedLabel");
		ViewAllLabel = Loc.T("Home_ViewAll");
		MoreSupportedLabel = Loc.T("Home_MoreSupported");
		ChipHalo = Loc.T("Home_ChipHalo");
		ChipMarineMaster = Loc.T("Home_ChipMarineMaster");
		ChipPhotometer = Loc.T("Home_ChipPhotometer");
		ChipMultiparameter = Loc.T("Home_ChipMultiparameter");
		ChipMeter = Loc.T("Home_ChipMeter");

		TrustTitle = Loc.T("Home_TrustTitle");
		TrustSub = Loc.T("Home_TrustSub");

		NotifyHeroStringsChanged();
		UpdateConnectedSummary();
	}

	void NotifyHeroStringsChanged()
	{
		OnPropertyChanged(nameof(HeroBadge));
		OnPropertyChanged(nameof(HeroTitle));
		OnPropertyChanged(nameof(HeroSubtitle));
		OnPropertyChanged(nameof(ConnectedLabel));
		OnPropertyChanged(nameof(ConnectedSub));
		OnPropertyChanged(nameof(RecordsTodayLabel));
		OnPropertyChanged(nameof(RecordsTodaySub));
		OnPropertyChanged(nameof(ScanInstrumentsLabel));
		OnPropertyChanged(nameof(ScanInstrumentsSub));
		OnPropertyChanged(nameof(WorkflowSection));
		OnPropertyChanged(nameof(WorkflowConnect));
		OnPropertyChanged(nameof(WorkflowConnectSub));
		OnPropertyChanged(nameof(WorkflowMeasure));
		OnPropertyChanged(nameof(WorkflowMeasureSub));
		OnPropertyChanged(nameof(WorkflowStore));
		OnPropertyChanged(nameof(WorkflowStoreSub));
		OnPropertyChanged(nameof(WorkflowReview));
		OnPropertyChanged(nameof(WorkflowReviewSub));
		OnPropertyChanged(nameof(SupportedFamiliesLabel));
		OnPropertyChanged(nameof(ViewAllLabel));
		OnPropertyChanged(nameof(MoreSupportedLabel));
		OnPropertyChanged(nameof(ChipHalo));
		OnPropertyChanged(nameof(ChipMarineMaster));
		OnPropertyChanged(nameof(ChipPhotometer));
		OnPropertyChanged(nameof(ChipMultiparameter));
		OnPropertyChanged(nameof(ChipMeter));
		OnPropertyChanged(nameof(TrustTitle));
		OnPropertyChanged(nameof(TrustSub));
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
