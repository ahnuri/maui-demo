using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Measure;

/// <summary>Measure tab selection state (which instrument view is active).</summary>
public partial class MeasureTabViewModel : LocalizedViewModelBase
{
	public MeasureTabViewModel() => ApplyLocalization();

	[ObservableProperty] private MeasureDeviceKind? _activeDevice;
	[ObservableProperty] private string _emptyStateMessage = string.Empty;

	public bool HasActiveDevice => ActiveDevice is not null;
	public bool IsPhotometerActive => ActiveDevice == MeasureDeviceKind.Photometer;
	public bool IsMultimeterActive => ActiveDevice == MeasureDeviceKind.Multimeter;
	public bool IsHalo2Active => ActiveDevice == MeasureDeviceKind.Halo2;
	public bool ShowEmptyState => ActiveDevice is null;

	public string NavigationTitle => ActiveDevice switch
	{
		MeasureDeviceKind.Photometer => "HI97115",
		MeasureDeviceKind.Multimeter => "HI98x94",
		MeasureDeviceKind.Halo2 => Loc.T("Shell_Home"),
		_ => Loc.T("Shell_Measure")
	};

	public bool UsesHaloNavigationTitle => ActiveDevice == MeasureDeviceKind.Halo2;

	public void Select(MeasureDeviceKind kind)
	{
		ActiveDevice = kind;
		NotifyVisibility();
	}

	public void Disconnect()
	{
		ActiveDevice = null;
		NotifyVisibility();
	}

	protected override void ApplyLocalization()
	{
		if (ActiveDevice is null)
			EmptyStateMessage = Loc.T("Measure_EmptyState");
	}

	partial void OnActiveDeviceChanged(MeasureDeviceKind? value)
	{
		NotifyVisibility();
		if (value is null)
			EmptyStateMessage = Loc.T("Measure_EmptyState");
	}

	void NotifyVisibility()
	{
		OnPropertyChanged(nameof(HasActiveDevice));
		OnPropertyChanged(nameof(IsPhotometerActive));
		OnPropertyChanged(nameof(IsMultimeterActive));
		OnPropertyChanged(nameof(IsHalo2Active));
		OnPropertyChanged(nameof(ShowEmptyState));
		OnPropertyChanged(nameof(NavigationTitle));
		OnPropertyChanged(nameof(UsesHaloNavigationTitle));
	}
}
