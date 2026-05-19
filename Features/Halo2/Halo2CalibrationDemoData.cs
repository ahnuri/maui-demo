namespace HannaUIDemo.Features.Halo2;

/// <summary>Demo five-point pH calibration dataset shared by measure and calibration screens.</summary>
public static class Halo2CalibrationDemoData
{
	public const string LastCalibrationDisplay = "19/05/26, 12:19:50 PM";
	public const string OffsetDisplay = "0.1 mV";
	public const string AverageSlopeDisplay = "100.0%";
	public const string PointDateDisplay = "19/05/26";
	public const string PointTimeDisplay = "12:19:50 PM";

	public static readonly IReadOnlyList<Halo2CalibrationPoint> Points =
	[
		new("1.68", "314.7 mV", "25.0 °C"),
		new("4.01", "176.9 mV", "24.8 °C"),
		new("7.01", "-0.6 mV", "24.7 °C"),
		new("10.01", "-178.1 mV", "25.0 °C"),
		new("12.45", "-322.4 mV", "25.1 °C")
	];

	public static readonly IReadOnlyList<string> SegmentSlopes = ["99%", "99%", "100%", "100%"];
}

public readonly record struct Halo2CalibrationPoint(string Ph, string Millivolts, string Temperature);
