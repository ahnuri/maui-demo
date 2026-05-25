namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Persisted Halo 2 UI preferences. Thin wrapper over MAUI's <see cref="Preferences.Default"/>,
/// which survives app restarts. Used by both <see cref="Halo2SettingsViewModel"/> (writes) and
/// <see cref="Halo2MeasureViewModel.SyncFromPreferences"/> (reads).
///
/// Why a static helper instead of an injected service: values are simple primitives and there is
/// no testing seam requirement today. Promote to <c>IPreferences</c> + DI if behavior diverges
/// per platform or unit-tests need to swap the backing store.
/// </summary>
public static class Halo2Preferences
{
	// Storage keys (kept const so a future migration can search-and-rename safely).
	public const string KeyPrimaryDisplay = "Halo2PrimaryDisplay";
	public const string KeyTemperatureUnit = "Halo2TemperatureUnit";

	/// <summary>Primary channel(s) shown on the measure screen. One of: <c>ph</c>, <c>mv</c>, <c>both</c>.</summary>
	public static string GetPrimaryDisplay() => Preferences.Default.Get(KeyPrimaryDisplay, "ph");

	public static void SetPrimaryDisplay(string value) => Preferences.Default.Set(KeyPrimaryDisplay, value);

	/// <summary>True = Fahrenheit, False = Celsius. Stored as the single character "F"/"C" for human-readable debugging.</summary>
	public static bool UseFahrenheit() =>
		Preferences.Default.Get(KeyTemperatureUnit, "C").Equals("F", StringComparison.OrdinalIgnoreCase);

	public static void SetTemperatureUnit(bool fahrenheit) =>
		Preferences.Default.Set(KeyTemperatureUnit, fahrenheit ? "F" : "C");
}
