namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Canned five-point pH calibration dataset shared between the calibration summary on the
/// Measure tab and the Halo 2 calibration sub-page. Pre-formatted strings (not numbers)
/// because the device returns localized/rounded values and the demo just displays them.
///
/// Replace with values pulled from the connected probe (e.g. via BLE GATT characteristic)
/// when wiring real hardware. The shape of the data (<see cref="Halo2CalibrationPoint"/>
/// and <see cref="SegmentSlopes"/>) is what the UI binds to — keep it stable.
/// </summary>
public static class Halo2CalibrationDemoData
{
	public const string LastCalibrationDisplay = "19/05/26, 12:19:50 PM";

	/// <summary>Probe offset at pH 7.00 — closer to 0 mV is healthier.</summary>
	public const string OffsetDisplay = "0.1 mV";

	/// <summary>Mean slope across the four calibration segments (target: 95–105%).</summary>
	public const string AverageSlopeDisplay = "100.0%";

	public const string PointDateDisplay = "19/05/26";
	public const string PointTimeDisplay = "12:19:50 PM";

	/// <summary>Five buffer points in ascending pH order — order matters for slope rendering.</summary>
	public static readonly IReadOnlyList<Halo2CalibrationPoint> Points =
	[
		new("1.68", "314.7 mV", "25.0 °C"),
		new("4.01", "176.9 mV", "24.8 °C"),
		new("7.01", "-0.6 mV", "24.7 °C"),
		new("10.01", "-178.1 mV", "25.0 °C"),
		new("12.45", "-322.4 mV", "25.1 °C")
	];

	/// <summary>Slope between each consecutive pair in <see cref="Points"/> (length = Points.Count - 1).</summary>
	public static readonly IReadOnlyList<string> SegmentSlopes = ["99%", "99%", "100%", "100%"];
}

/// <summary>A single buffer point on the calibration curve (pre-formatted for direct display).</summary>
public readonly record struct Halo2CalibrationPoint(string Ph, string Millivolts, string Temperature);
