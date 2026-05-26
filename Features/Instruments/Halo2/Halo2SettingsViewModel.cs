using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Halo 2 device settings: measure mode (pH / mV / both), temperature unit (°C / °F),
/// and navigation to the calibration sub-page.
///
/// All Set* commands write to <see cref="Halo2Preferences"/> first (the persisted source
/// of truth), then update local <see cref="ObservableProperty"/>s so XAML bindings refresh.
/// Returning to the Measure tab triggers <c>Halo2MeasureViewModel.SyncFromPreferences()</c>
/// via <c>Halo2MeasureModule.OnAppearing()</c> so live readings adopt the new preferences.
/// </summary>
public partial class Halo2SettingsViewModel : LocalizedViewModelBase
{
	public string DeviceName => Loc.T("Halo_Device_Name");
	public const string DeviceIcon = "halo2_device_icon.png";

	/// <summary>One-line subtitle like "pH mode • 0.01 resolution • °C • ATC" shown under the device name.</summary>
	[ObservableProperty] private string _deviceSubtitle = string.Empty;

	/// <summary>Persisted display mode: "ph" | "mv" | "both". Mirrors <see cref="Halo2Preferences.GetPrimaryDisplay"/>.</summary>
	[ObservableProperty] private string _primaryDisplay = "ph";

	[ObservableProperty] private bool _useFahrenheit;

	public Halo2SettingsViewModel() => RefreshFromPreferences();

	/// <summary>Re-reads preferences and updates the subtitle. Call after preferences change externally.</summary>
	public void RefreshFromPreferences()
	{
		PrimaryDisplay = Halo2Preferences.GetPrimaryDisplay();
		UseFahrenheit = Halo2Preferences.UseFahrenheit();
		DeviceSubtitle = BuildSubtitle();
	}

	/// <summary>Pushes Shell to the calibration sub-page.</summary>
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

	protected override void ApplyLocalization()
	{
		DeviceSubtitle = BuildSubtitle();
		OnPropertyChanged(nameof(DeviceName));
	}

	string BuildSubtitle()
	{
		var mode = PrimaryDisplay.ToLowerInvariant() switch
		{
			"mv" => Loc.T("Halo_Settings_ModeMv"),
			"both" => Loc.T("Halo_Settings_ModeBoth"),
			_ => Loc.T("Halo_Settings_ModePh")
		};
		var temp = UseFahrenheit
			? Loc.T("Halo_TemperatureUnit_Fahrenheit")
			: Loc.T("Halo_TemperatureUnit_Celsius");
		// ATC = Automatic Temperature Compensation (always on for Halo 2).
		return Loc.T("Halo_Settings_StatusFormat", mode, temp);
	}
}
