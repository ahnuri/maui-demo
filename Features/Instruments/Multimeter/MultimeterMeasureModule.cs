using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>Measure tab module for HI98x94 multiparameter log recall.</summary>
public sealed class MultimeterMeasureModule : IInstrumentMeasureModule
{
	MultimeterLogRecallView? _view;

	public InstrumentKind Kind => InstrumentKind.Multimeter;

	public View Content => _view ??= new MultimeterLogRecallView();

	public MultimeterLogRecallView View => (MultimeterLogRecallView)Content;

	public bool UsesLabChrome => false;

	public string GetNavigationTitle(LocalizationService loc) =>
		InstrumentRegistry.Get(Kind).MeasureNavigationTitleKey;

	public void ApplyTheme() => View.ApplyTheme();

	public void OnAppearing() { }

	public bool TryRefreshNavigation(IMeasureTabNavigationHost host, MeasureTabViewModel viewModel) => false;
}
