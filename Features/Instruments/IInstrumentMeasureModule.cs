using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments;

/// <summary>Contract for one instrument family's Measure tab content (view lifecycle + shell chrome).</summary>
public interface IInstrumentMeasureModule
{
	InstrumentKind Kind { get; }

	/// <summary>Lazy-created measure UI hosted inside the Measure tab page.</summary>
	View Content { get; }

	bool UsesLabChrome { get; }

	string GetNavigationTitle(LocalizationService loc);

	void ApplyTheme();

	void OnAppearing();

	/// <summary>Updates shell title, toolbar, and back button when this module is active.</summary>
	bool TryRefreshNavigation(IMeasureTabNavigationHost host, MeasureTabViewModel viewModel);
}
