using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Halo2;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Live Halo 2 readings + UI display preferences.
///
/// Lifecycle:
///   - Registered as a singleton in <see cref="HannaUIDemo.Core.Mvvm.MvvmServiceCollectionExtensions"/>.
///   - <see cref="Halo2MeasureView"/> owns the simulated sampling timer and pushes the
///     latest pH / mV / temperature / battery / probe-condition values here via
///     <c>PushReadingsToViewModel()</c> on every tick.
///   - Display preferences (pH vs mV primary, °C vs °F) are mirrored from
///     <see cref="Halo2Preferences"/> via <see cref="SyncFromPreferences"/> whenever the
///     settings page or theme changes.
///
/// Why this VM is intentionally thin: chart drawing, history buffering, and live timer
/// management live in the View because they require MAUI dispatcher + canvas APIs.
/// This VM only exposes the latest snapshot for any external consumer (toolbar,
/// summary cards, accessibility labels, future BLE bridge, etc.).
/// </summary>
public partial class Halo2MeasureViewModel : PageViewModelBase
{
	/// <summary>Table = historical rows list. Graph = live trend chart.</summary>
	public enum DisplayMode
	{
		Table,
		Graph
	}

	// Latest sample (pushed from view's simulation timer ~1Hz; replace with BLE feed for prod).
	[ObservableProperty] private double _ph = 7.02;
	[ObservableProperty] private double _millivolts = -12.4;
	[ObservableProperty] private double _temperatureC = 25.3;

	/// <summary>"Stable" or "Drifting" — driven by view's stability heuristic.</summary>
	[ObservableProperty] private string _stabilityLabel = "Stable";

	[ObservableProperty] private int _batteryPercent = 44;

	/// <summary>0–100. Maps to <see cref="Halo2ProbeConditionIcons"/> in 10% buckets.</summary>
	[ObservableProperty] private int _probeConditionPercent = 94;

	/// <summary>True when the user has tagged the current reading row (demo toggle in the header).</summary>
	[ObservableProperty] private bool _isTagged;

	[ObservableProperty] private DisplayMode _mode = DisplayMode.Table;

	// Display toggles mirror Halo2Preferences (kept here so XAML bindings can react instantly).
	[ObservableProperty] private bool _showPh = true;
	[ObservableProperty] private bool _showMillivolts;
	[ObservableProperty] private bool _useFahrenheit;

	/// <summary>Hard-coded demo serial; swap for the connected device name when wiring BLE.</summary>
	public string DeviceLabel => "HI12322 • Probe 2";

	/// <summary>Formatted temperature respecting the current unit preference.</summary>
	public string TemperatureDisplay => UseFahrenheit
		? $"{CelsiusToFahrenheit(TemperatureC):F1} °F"
		: $"{TemperatureC:F1} °C";

	// MVVM Toolkit source-generated partial hooks: re-fire TemperatureDisplay when its inputs change.
	partial void OnTemperatureCChanged(double value) => OnPropertyChanged(nameof(TemperatureDisplay));
	partial void OnUseFahrenheitChanged(bool value) => OnPropertyChanged(nameof(TemperatureDisplay));

	/// <summary>
	/// Pull persisted display preferences into the VM (call after returning from settings
	/// or when the theme reapplies). Keeps preference storage as the single source of truth.
	/// </summary>
	public void SyncFromPreferences()
	{
		var primary = Halo2Preferences.GetPrimaryDisplay().ToLowerInvariant();
		ShowPh = primary is "ph" or "both";
		ShowMillivolts = primary is "mv" or "both";
		UseFahrenheit = Halo2Preferences.UseFahrenheit();
	}

	/// <summary>Pushes Shell to the Halo 2 device-settings sub-page (registered in <see cref="Halo2Routes"/>).</summary>
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
