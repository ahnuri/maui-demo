using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>Measure tab module for HI97115 photometer UI.</summary>
public sealed class PhotometerMeasureModule : IInstrumentMeasureModule
{
	MeasurePhotometerView? _view;

	public InstrumentKind Kind => InstrumentKind.Photometer;

	public View Content => _view ??= new MeasurePhotometerView();

	public MeasurePhotometerView View => (MeasurePhotometerView)Content;

	public bool UsesLabChrome => false;

	public string GetNavigationTitle(LocalizationService loc) =>
		InstrumentRegistry.Get(Kind).MeasureNavigationTitleKey is { } key && key.StartsWith("Shell_", StringComparison.Ordinal)
			? loc.T(key)
			: InstrumentRegistry.Get(Kind).MeasureNavigationTitleKey;

	public void ApplyTheme() => View.ApplyTheme();

	public void OnAppearing() { }

	public bool TryRefreshNavigation(IMeasureTabNavigationHost host, MeasureTabViewModel viewModel) =>
		PhotometerShellNavigation.TryRefresh(host, viewModel, View, View.PhotometerViewModel);
}
