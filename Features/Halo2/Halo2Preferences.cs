namespace HannaUIDemo.Features.Halo2;

/// <summary>Demo preferences shared between Halo 2 measure UI and device settings.</summary>
public static class Halo2Preferences
{
	public const string KeyPrimaryDisplay = "Halo2PrimaryDisplay";
	public const string KeyTemperatureUnit = "Halo2TemperatureUnit";

	/// <summary>One of: <c>ph</c>, <c>mv</c>, <c>both</c>.</summary>
	public static string GetPrimaryDisplay() => Preferences.Default.Get(KeyPrimaryDisplay, "ph");

	public static void SetPrimaryDisplay(string value) => Preferences.Default.Set(KeyPrimaryDisplay, value);

	public static bool UseFahrenheit() =>
		Preferences.Default.Get(KeyTemperatureUnit, "C").Equals("F", StringComparison.OrdinalIgnoreCase);

	public static void SetTemperatureUnit(bool fahrenheit) =>
		Preferences.Default.Set(KeyTemperatureUnit, fahrenheit ? "F" : "C");
}
