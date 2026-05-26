using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Abstractions;
using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

/// <summary>Navigates to photometer tank reading lists; session rows open via tank groups.</summary>
public sealed class PhotometerLogNavigator : IInstrumentLogNavigator
{
	public InstrumentKind Kind => InstrumentKind.Photometer;

	public Task NavigateToSessionAsync(Page hostPage, LogEntryViewModel entry) =>
		hostPage.DisplayAlertAsync("Log detail", "Open a tank to view readings.", "OK");

	public async Task NavigateToTankAsync(Page hostPage, LogTankGroupViewModel tank)
	{
		var page = AppServices.Get<LogHistoryTankReadingsPage>();
		page.Initialize(tank);
		await hostPage.Navigation.PushAsync(page);
	}
}
