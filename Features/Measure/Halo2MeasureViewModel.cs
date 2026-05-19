using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Halo2;

namespace HannaUIDemo.Features.Measure;

/// <summary>Live Halo 2 readings and display preferences (updated by <see cref="Halo2MeasureView"/>).</summary>
public partial class Halo2MeasureViewModel : PageViewModelBase
{
	public enum DisplayMode
	{
		Table,
		Graph
	}

	[ObservableProperty] private double _ph = 7.02;
	[ObservableProperty] private double _millivolts = -12.4;
	[ObservableProperty] private double _temperatureC = 25.3;
	[ObservableProperty] private string _stabilityLabel = "Stable";
	[ObservableProperty] private int _batteryPercent = 44;
	[ObservableProperty] private int _probeConditionPercent = 94;
	[ObservableProperty] private bool _isTagged;
	[ObservableProperty] private DisplayMode _mode = DisplayMode.Table;
	[ObservableProperty] private bool _showPh = true;
	[ObservableProperty] private bool _showMillivolts;
	[ObservableProperty] private bool _useFahrenheit;

	public string DeviceLabel => "HI12322 • Probe 2";
	public string TemperatureDisplay => UseFahrenheit
		? $"{CelsiusToFahrenheit(TemperatureC):F1} °F"
		: $"{TemperatureC:F1} °C";

	partial void OnTemperatureCChanged(double value) => OnPropertyChanged(nameof(TemperatureDisplay));
	partial void OnUseFahrenheitChanged(bool value) => OnPropertyChanged(nameof(TemperatureDisplay));

	public void SyncFromPreferences()
	{
		var primary = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		ShowPh = primary is "ph" or "both";
		ShowMillivolts = primary is "mv" or "both";
		UseFahrenheit = Halo2Preferences.UseFahrenheit();
	}

	[RelayCommand]
	async Task OpenSettingsAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync(Halo2Routes.Settings);
	}

	[RelayCommand]
	void ToggleDisplayMode() =>
		Mode = Mode == DisplayMode.Table ? DisplayMode.Graph : DisplayMode.Table;

	static double CelsiusToFahrenheit(double c) => c * 9 / 5 + 32;
}
