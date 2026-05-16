using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Measure;

/// <summary>Photometer measure flow state (drives <see cref="MeasurePhotometerView"/> UI).</summary>
public partial class PhotometerMeasureViewModel : PageViewModelBase
{
	public enum MeasureState
	{
		NewAnalysis,
		Setup,
		Running,
		Completed
	}

	[ObservableProperty] private MeasureState _state = MeasureState.NewAnalysis;

	public bool IsNewAnalysis => State == MeasureState.NewAnalysis;
	public bool IsSetup => State == MeasureState.Setup;
	public bool IsRunning => State == MeasureState.Running;
	public bool IsCompleted => State == MeasureState.Completed;

	partial void OnStateChanged(MeasureState value)
	{
		OnPropertyChanged(nameof(IsNewAnalysis));
		OnPropertyChanged(nameof(IsSetup));
		OnPropertyChanged(nameof(IsRunning));
		OnPropertyChanged(nameof(IsCompleted));
		StateChanged?.Invoke(this, value);
	}

	public event EventHandler<MeasureState>? StateChanged;

	public void SetState(MeasureState state) => State = state;

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
