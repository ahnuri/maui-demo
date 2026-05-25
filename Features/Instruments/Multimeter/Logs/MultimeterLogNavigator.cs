using HannaUIDemo.Features.Instruments.Abstractions;
using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Multimeter.Logs;

/// <summary>Placeholder navigator until multiparameter log detail screens are implemented.</summary>
public sealed class MultimeterLogNavigator : IInstrumentLogNavigator
{
	public InstrumentKind Kind => InstrumentKind.Multimeter;

	public Task NavigateToSessionAsync(Page hostPage, LogEntryViewModel entry) =>
		hostPage.DisplayAlertAsync(
			"Log detail",
			"Detailed views for multimeter logs are coming soon.",
			"OK");

	public Task NavigateToTankAsync(Page hostPage, LogTankGroupViewModel tank) =>
		Task.CompletedTask;
}
