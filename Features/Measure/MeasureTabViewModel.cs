using CommunityToolkit.Mvvm.ComponentModel;

using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;



namespace HannaUIDemo.Features.Measure;



/// <summary>Measure tab state: which instrument module is active.</summary>

public partial class MeasureTabViewModel : LocalizedViewModelBase

{

	public MeasureTabViewModel() => ApplyLocalization();



	[ObservableProperty] private InstrumentKind? _activeDevice;

	[ObservableProperty] private string _emptyStateMessage = string.Empty;



	public bool HasActiveDevice => ActiveDevice is not null;

	public bool ShowEmptyState => ActiveDevice is null;



	public string NavigationTitle => ActiveDevice switch

	{

		InstrumentKind.Photometer => ResolveTitle(InstrumentKind.Photometer, Loc),

		InstrumentKind.Multimeter => ResolveTitle(InstrumentKind.Multimeter, Loc),

		InstrumentKind.Halo2 => Loc.T("Shell_Home"),

		_ => Loc.T("Shell_Measure")

	};



	public bool UsesHaloNavigationTitle => ActiveDevice == InstrumentKind.Halo2;



	public void Select(InstrumentKind kind)

	{

		ActiveDevice = kind;

		NotifyDerived();

	}



	public void Disconnect()

	{

		ActiveDevice = null;

		NotifyDerived();

	}



	protected override void ApplyLocalization()

	{

		EmptyStateMessage = Loc.T("Measure_EmptyState");

		NotifyDerived();

	}



	partial void OnActiveDeviceChanged(InstrumentKind? value) => NotifyDerived();



	static string ResolveTitle(InstrumentKind kind, LocalizationService loc)

	{

		var key = InstrumentRegistry.Get(kind).MeasureNavigationTitleKey;

		return key.StartsWith("Shell_", StringComparison.Ordinal) ? loc.T(key) : key;

	}



	void NotifyDerived()

	{

		OnPropertyChanged(nameof(HasActiveDevice));

		OnPropertyChanged(nameof(ShowEmptyState));

		OnPropertyChanged(nameof(NavigationTitle));

		OnPropertyChanged(nameof(UsesHaloNavigationTitle));

	}

}


