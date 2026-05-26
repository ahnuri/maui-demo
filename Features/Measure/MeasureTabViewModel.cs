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

	public string NavigationTitle =>
		ActiveDevice is { } kind
			? InstrumentRegistry.GetMeasureNavigationTitle(kind, Loc)
			: Loc.T("Shell_Measure");

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

	void NotifyDerived()
	{
		OnPropertyChanged(nameof(HasActiveDevice));
		OnPropertyChanged(nameof(ShowEmptyState));
		OnPropertyChanged(nameof(NavigationTitle));
		OnPropertyChanged(nameof(UsesHaloNavigationTitle));
	}
}
