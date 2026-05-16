using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Measure;

/// <summary>Measure tab selection state (which instrument view is active).</summary>
public partial class MeasureTabViewModel : PageViewModelBase
{
	[ObservableProperty] private MeasureDeviceKind? _activeDevice;

	[ObservableProperty] private string _emptyStateMessage =
		"Open a connected device from the Devices page to start measuring.";

	public bool HasActiveDevice => ActiveDevice is not null;
	public bool IsPhotometerActive => ActiveDevice == MeasureDeviceKind.Photometer;
	public bool IsMultimeterActive => ActiveDevice == MeasureDeviceKind.Multimeter;
	public bool IsHalo2Active => ActiveDevice == MeasureDeviceKind.Halo2;
	public bool ShowEmptyState => ActiveDevice is null;

	public string NavigationTitle => ActiveDevice switch
	{
		MeasureDeviceKind.Photometer => "HI97115",
		MeasureDeviceKind.Multimeter => "HI98x94",
		MeasureDeviceKind.Halo2 => "Hanna Lab",
		_ => ResolveDefaultMeasureTitle()
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

	public void RefreshLocalizedEmptyMessage()
	{
		if (ActiveDevice is null)
			EmptyStateMessage = "Open a connected device from the Devices page to start measuring.";
	}

	partial void OnActiveDeviceChanged(MeasureDeviceKind? value) => NotifyVisibility();

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

	static string ResolveDefaultMeasureTitle()
	{
		if (Application.Current is App app)
			return app.Services.GetRequiredService<LocalizationService>().T("Shell_Measure");
		return AppConstants.MeasureTabTitle;
	}
}
