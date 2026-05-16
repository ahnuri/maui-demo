using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Halo2;

/// <summary>Halo 2 device settings: measure mode, temperature unit, and navigation to calibration.</summary>
public partial class Halo2SettingsViewModel : PageViewModelBase
{
	public const string DeviceName = "HI12322 • Probe 2";
	public const string DeviceIcon = "halo2_device_icon.png";

	[ObservableProperty] private string _deviceSubtitle = string.Empty;
	[ObservableProperty] private string _primaryDisplay = "ph";
	[ObservableProperty] private bool _useFahrenheit;

	public Halo2SettingsViewModel() => RefreshFromPreferences();

	public void RefreshFromPreferences()
	{
		PrimaryDisplay = Halo2Preferences.GetPrimaryDisplay();
		UseFahrenheit = Halo2Preferences.UseFahrenheit();
		DeviceSubtitle = BuildSubtitle();
	}

	[RelayCommand]
	async Task OpenCalibrationAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync(Halo2Routes.Calibration);
	}

	[RelayCommand]
	void SetPrimaryDisplay(string mode)
	{
		Halo2Preferences.SetPrimaryDisplay(mode);
		PrimaryDisplay = mode;
		DeviceSubtitle = BuildSubtitle();
	}

	[RelayCommand]
	void SetTemperatureUnit(bool fahrenheit)
	{
		Halo2Preferences.SetTemperatureUnit(fahrenheit);
		UseFahrenheit = fahrenheit;
		DeviceSubtitle = BuildSubtitle();
	}

	string BuildSubtitle()
	{
		var mode = PrimaryDisplay.ToLowerInvariant() switch
		{
			"mv" => "mV mode",
			"both" => "pH & mV mode",
			_ => "pH mode"
		};
		var temp = UseFahrenheit ? "°F" : "°C";
		return $"{mode} • 0.01 resolution • {temp} • ATC";
	}
}
