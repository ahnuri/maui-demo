using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Instruments.Abstractions;
using HannaUIDemo.Features.Instruments.Logs;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Navigates from Log History to the Halo 2 session detail page.</summary>
public sealed class Halo2LogNavigator : IInstrumentLogNavigator
{
	public InstrumentKind Kind => InstrumentKind.Halo2;

	// public async Task NavigateToSessionAsync(Page hostPage, LogEntryViewModel entry)
	// {
	// 	var detailPage = AppServices.Get<Halo2LogDetailPage>();
	// 	detailPage.ViewModel.LoadFrom(entry);
	// 	await hostPage.Navigation.PushAsync(detailPage);
	// }

	public Task NavigateToSessionAsync(Page hostPage, LogEntryViewModel entry) =>
		hostPage.DisplayAlertAsync(
			"Log detail",
			"Detailed views for Halo 2 logs is in progress.",
			"OK");

	public Task NavigateToTankAsync(Page hostPage, LogTankGroupViewModel tank) =>
		Task.CompletedTask;
}

