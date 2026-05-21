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
		StartMeasurement,
		Running,
		Completed
	}

	[ObservableProperty] private MeasureState _state = MeasureState.NewAnalysis;

	/// <summary>Active tank index (1–100). Measurements and methods are scoped to this tank in the UI.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(SelectedTankDisplay))]
	private int _selectedTankNumber = 1;

	public string SelectedTankDisplay => $"Tank {SelectedTankNumber}";

	public bool IsNewAnalysis => State == MeasureState.NewAnalysis;
	public bool IsInMeasurementFlow => State != MeasureState.NewAnalysis;
	public bool IsSetup => State == MeasureState.Setup;
	public bool IsStartMeasurement => State == MeasureState.StartMeasurement;
	public bool IsRunning => State == MeasureState.Running;
	public bool IsCompleted => State == MeasureState.Completed;

	partial void OnStateChanged(MeasureState value)
	{
		OnPropertyChanged(nameof(IsNewAnalysis));
		OnPropertyChanged(nameof(IsInMeasurementFlow));
		OnPropertyChanged(nameof(IsSetup));
		OnPropertyChanged(nameof(IsStartMeasurement));
		OnPropertyChanged(nameof(IsRunning));
		OnPropertyChanged(nameof(IsCompleted));
		StateChanged?.Invoke(this, value);
	}

	public event EventHandler<MeasureState>? StateChanged;

	public void SetState(MeasureState state) => State = state;

	/// <summary>Shell back during measurement flow (Setup → overview, etc.).</summary>
	public void NavigateBack()
	{
		State = State switch
		{
			MeasureState.Setup => MeasureState.NewAnalysis,
			MeasureState.StartMeasurement => MeasureState.Setup,
			MeasureState.Running => MeasureState.StartMeasurement,
			MeasureState.Completed => MeasureState.NewAnalysis,
			_ => State
		};
	}

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
