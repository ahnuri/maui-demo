using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Abstractions;

/// <summary>
/// Per-family navigation from Log History lists to detail screens (Halo session, photometer tank, etc.).
/// </summary>
public interface IInstrumentLogNavigator
{
	InstrumentKind Kind { get; }

	/// <summary>Opens the detail UI for a log session row tap.</summary>
	Task NavigateToSessionAsync(Page hostPage, LogEntryViewModel entry);

	/// <summary>Opens tank readings when the family groups logs by tank (photometer only).</summary>
	Task NavigateToTankAsync(Page hostPage, LogTankGroupViewModel tank);
}
