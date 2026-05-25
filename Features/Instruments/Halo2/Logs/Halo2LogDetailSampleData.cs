namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Demo readings for Halo 2 log history detail (H2STagging-style session).</summary>
public static class Halo2LogDetailSampleData
{
	public static IReadOnlyList<Halo2LogTableRow> Rows { get; } =
	[
		new("7.74", "-43.9", "25.0", "11/05/26, 7:01:19 AM"),
		new("8.21", "-71.6", "25.0", "11/05/26, 7:01:20 AM"),
		new("8.66", "-97.9", "25.0", "11/05/26, 7:01:21 AM"),
		new("9.12", "-124.2", "25.0", "11/05/26, 7:01:22 AM"),
		new("9.58", "-150.5", "25.0", "11/05/26, 7:01:23 AM"),
		new("10.04", "-176.8", "25.0", "11/05/26, 7:01:24 AM"),
		new("10.50", "-203.1", "25.0", "11/05/26, 7:01:25 AM"),
		new("10.96", "-229.4", "25.0", "11/05/26, 7:01:26 AM"),
		new("11.42", "-255.7", "25.0", "11/05/26, 7:01:27 AM"),
		new("11.88", "-282.0", "25.0", "11/05/26, 7:01:28 AM"),
		new("12.34", "-308.3", "25.0", "11/05/26, 7:01:29 AM"),
		new("12.80", "-334.6", "25.0", "11/05/26, 7:01:30 AM"),
		new("13.26", "-360.9", "25.0", "11/05/26, 7:01:31 AM"),
		new("13.72", "-387.2", "25.0", "11/05/26, 7:01:32 AM"),
		new("14.18", "-413.5", "25.0", "11/05/26, 7:01:33 AM"),
		new("14.64", "-439.8", "25.0", "11/05/26, 7:01:34 AM"),
		new("15.10", "-466.1", "25.0", "11/05/26, 7:01:35 AM"),
		new("15.56", "-492.4", "25.0", "11/05/26, 7:01:36 AM"),
		new("16.00", "-800.0", "25.0", "11/05/26, 7:01:37 AM", isAlert: true),
		new("16.00", "-800.0", "25.0", "11/05/26, 7:01:38 AM", isAlert: true),
		new("16.00", "-800.0", "25.0", "11/05/26, 7:01:39 AM", isAlert: true),
		new("7.94", "-55.7", "25.0", "11/05/26, 7:01:40 AM", isTagged: true),
		new("8.01", "-59.8", "25.0", "11/05/26, 7:01:41 AM"),
		new("8.08", "-63.9", "25.0", "11/05/26, 7:01:42 AM"),
		new("8.15", "-68.0", "25.0", "11/05/26, 7:01:43 AM"),
		new("8.22", "-72.1", "25.0", "11/05/26, 7:01:44 AM"),
		new("8.29", "-76.2", "25.0", "11/05/26, 7:01:45 AM"),
		new("8.36", "-80.3", "25.0", "11/05/26, 7:01:46 AM"),
		new("8.43", "-84.4", "25.0", "11/05/26, 7:01:47 AM"),
		new("8.50", "-88.5", "25.0", "11/05/26, 7:01:48 AM")
	];

	public static IReadOnlyList<Halo2LogChartPoint> ChartPoints { get; } =
	[
		new(6.99, 25.0, "6:59:48AM"),
		new(12.5, 25.0, "7:04:00AM"),
		new(14.2, 25.0, "7:09:19AM"),
		new(7.9, 25.0, "7:14:30AM"),
		new(8.4, 25.0, "7:18:50AM")
	];
}

public sealed record Halo2LogTableRow(
	string Ph,
	string Mv,
	string Temp,
	string Date,
	bool isTagged = false,
	bool isAlert = false);

public sealed record Halo2LogChartPoint(double Ph, double Temp, string TimeLabel);
