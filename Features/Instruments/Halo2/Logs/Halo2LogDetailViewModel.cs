using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Logs;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>View model for a saved Halo 2 log session detail screen.</summary>
public partial class Halo2LogDetailViewModel : PageViewModelBase
{
	LogEntryViewModel? _sourceEntry;

	[ObservableProperty] private string _logFileName = string.Empty;
	[ObservableProperty] private string _recordedDate = string.Empty;

	public void LoadFrom(LogEntryViewModel entry)
	{
		_sourceEntry = entry;
		LogFileName = entry.Title;
		RecordedDate = entry.DateRangeLabel;
	}

	[RelayCommand]
	async Task RenameLogAsync()
	{
		if (Shell.Current?.CurrentPage is not Page page)
			return;

		var name = await page.DisplayPromptAsync(
			"Rename log",
			"Log file name",
			initialValue: LogFileName,
			maxLength: 48);

		if (string.IsNullOrWhiteSpace(name))
			return;

		LogFileName = name.Trim();
		if (_sourceEntry is not null)
			_sourceEntry.Title = LogFileName;
	}

	[RelayCommand]
	async Task ExportLogAsync()
	{
		if (Shell.Current?.CurrentPage is not Page page)
			return;

		var format = await page.DisplayActionSheetAsync(
			"Export log",
			"Cancel",
			null,
			"PDF",
			"CSV");

		if (string.IsNullOrEmpty(format) || format == "Cancel")
			return;

		var body = format == "CSV" ? BuildCsvExport() : BuildPdfExport();
		await Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = $"Hanna Lab — {LogFileName} ({format})",
			Text = body
		});
	}

	string BuildPdfExport()
	{
		var rows = Halo2LogDetailSampleData.Rows;
		var lines = new List<string>
		{
			"Hanna Lab — Halo 2 log export",
			$"Log: {LogFileName}",
			$"Recorded: {RecordedDate}",
			$"Last calibration: {Halo2CalibrationDemoData.LastCalibrationDisplay}",
			$"Offset: {Halo2CalibrationDemoData.OffsetDisplay}",
			$"Average slope: {Halo2CalibrationDemoData.AverageSlopeDisplay}",
			"",
			"Readings:",
		};

		var rec = 1;
		foreach (var row in rows)
		{
			var flags = row.isAlert ? " [alert]" : row.isTagged ? " [tagged]" : string.Empty;
			lines.Add($"  #{rec}  pH {row.Ph}  mV {row.Mv}  {row.Temp} °C  {row.Date}{flags}");
			rec++;
		}

		return string.Join(Environment.NewLine, lines);
	}

	string BuildCsvExport()
	{
		var header = new[]
		{
			"LogFileName", LogFileName,
			"Recorded", RecordedDate,
			"LastCalibration", Halo2CalibrationDemoData.LastCalibrationDisplay,
			"Offset", Halo2CalibrationDemoData.OffsetDisplay,
			"AverageSlope", Halo2CalibrationDemoData.AverageSlopeDisplay
		};

		var meta = string.Join(Environment.NewLine,
			Enumerable.Range(0, header.Length / 2)
				.Select(i => $"{Csv(header[i * 2])},{Csv(header[i * 2 + 1])}"));

		var tableHeader = "#Rec,pH,mV,Temp_C,Date,Tagged,Alert";
		var tableRows = Halo2LogDetailSampleData.Rows.Select((r, i) =>
			$"{i + 1},{Csv(r.Ph)},{Csv(r.Mv)},{Csv(r.Temp)},{Csv(r.Date)},{r.isTagged},{r.isAlert}");

		return meta + Environment.NewLine + tableHeader + Environment.NewLine + string.Join(Environment.NewLine, tableRows);
	}

	static string Csv(string value) =>
		"\"" + value.Replace("\"", "\"\"") + "\"";
}
