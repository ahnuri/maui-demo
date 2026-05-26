using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Logs;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>View model for a saved Halo 2 log session detail screen.</summary>
public partial class Halo2LogDetailViewModel : LocalizedViewModelBase
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
			Loc.T("Halo_Log_RenameTitle"),
			Loc.T("Halo_Log_RenameLabel"),
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

		var cancel = Loc.T("Common_Cancel");
		var format = await page.DisplayActionSheetAsync(
			Loc.T("Halo_Log_ExportTitle"),
			cancel,
			null,
			Loc.T("Common_Pdf"),
			Loc.T("Common_Csv"));

		if (string.IsNullOrEmpty(format) || format == cancel)
			return;

		var body = format == Loc.T("Common_Csv") ? BuildCsvExport() : BuildPdfExport();
		await Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = Loc.T("Halo_Log_ShareSubjectFormat", LogFileName, format),
			Text = body
		});
	}

	string BuildPdfExport()
	{
		var rows = Halo2LogDetailSampleData.Rows;
		var lines = new List<string>
		{
			Loc.T("Halo_Log_ExportPdf_Header"),
			Loc.T("Halo_Log_ExportPdf_LogLineFormat", LogFileName),
			Loc.T("Halo_Log_ExportPdf_RecordedFormat", RecordedDate),
			Loc.T("Halo_Log_ExportPdf_LastCalibrationFormat", Halo2CalibrationDemoData.LastCalibrationDisplay),
			Loc.T("Halo_Log_ExportPdf_OffsetFormat", Halo2CalibrationDemoData.OffsetDisplay),
			Loc.T("Halo_Log_ExportPdf_AvgSlopeFormat", Halo2CalibrationDemoData.AverageSlopeDisplay),
			"",
			Loc.T("Halo_Log_ExportPdf_ReadingsHeader"),
		};

		var rec = 1;
		foreach (var row in rows)
		{
			var flags = row.isAlert
				? Loc.T("Halo_Log_ExportPdf_FlagAlert")
				: row.isTagged
					? Loc.T("Halo_Log_ExportPdf_FlagTagged")
					: string.Empty;
			lines.Add(Loc.T("Halo_Log_ExportPdf_RowFormat", rec, row.Ph, row.Mv, row.Temp, row.Date, flags));
			rec++;
		}

		return string.Join(Environment.NewLine, lines);
	}

	string BuildCsvExport()
	{
		var header = new[]
		{
			Loc.T("Halo_Log_ExportCsv_HeaderLogName"), LogFileName,
			Loc.T("Halo_Log_ExportCsv_HeaderRecorded"), RecordedDate,
			Loc.T("Halo_Log_ExportCsv_HeaderLastCalibration"), Halo2CalibrationDemoData.LastCalibrationDisplay,
			Loc.T("Halo_Log_ExportCsv_HeaderOffset"), Halo2CalibrationDemoData.OffsetDisplay,
			Loc.T("Halo_Log_ExportCsv_HeaderAvgSlope"), Halo2CalibrationDemoData.AverageSlopeDisplay
		};

		var meta = string.Join(Environment.NewLine,
			Enumerable.Range(0, header.Length / 2)
				.Select(i => $"{Csv(header[i * 2])},{Csv(header[i * 2 + 1])}"));

		var tableHeader = Loc.T("Halo_Log_ExportCsv_TableHeader");
		var tableRows = Halo2LogDetailSampleData.Rows.Select((r, i) =>
			$"{i + 1},{Csv(r.Ph)},{Csv(r.Mv)},{Csv(r.Temp)},{Csv(r.Date)},{r.isTagged},{r.isAlert}");

		return meta + Environment.NewLine + tableHeader + Environment.NewLine + string.Join(Environment.NewLine, tableRows);
	}

	static string Csv(string value) =>
		"\"" + value.Replace("\"", "\"\"") + "\"";
}
