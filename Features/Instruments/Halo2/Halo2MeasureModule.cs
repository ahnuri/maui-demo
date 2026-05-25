using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Measure tab plugin for Halo 2 (live pH / mV / temperature readings).
///
/// Lifecycle:
///   - Registered as a singleton in <c>InstrumentServiceCollectionExtensions.AddInstrumentModules</c>.
///   - <see cref="MeasureTabPage"/> instantiates all modules at startup, adds each module's
///     <see cref="Content"/> to its <c>_deviceHost</c> Grid, and toggles <see cref="View.IsVisible"/>
///     when the user picks a device from the bottom-sheet picker.
///   - The Halo 2 view is created lazily on first <see cref="Content"/> access to keep cold-start fast.
///
/// <see cref="UsesLabChrome"/> returns true so the Measure tab applies the dark "lab" navigation
/// theme (custom title view, dark status bar) instead of the standard surface chrome used by
/// Multimeter / Photometer.
/// </summary>
public sealed class Halo2MeasureModule : IInstrumentMeasureModule
{
	Halo2MeasureView? _view;

	public InstrumentKind Kind => InstrumentKind.Halo2;

	/// <summary>Lazy view instance — keeps the live-sampling timer from spinning up until the user actually picks Halo 2.</summary>
	public View Content => _view ??= new Halo2MeasureView();

	public Halo2MeasureView View => (Halo2MeasureView)Content;

	public bool UsesLabChrome => true;

	public string GetNavigationTitle(LocalizationService loc) =>
		loc.T(InstrumentRegistry.Get(Kind).MeasureNavigationTitleKey);

	/// <summary>Rebuilds the view tree so theme-dependent colors and dynamic resources re-evaluate.</summary>
	public void ApplyTheme() => View.ApplyTheme();

	/// <summary>Re-syncs preferences each time the Measure tab is shown (after returning from settings/calibration).</summary>
	public void OnAppearing() => View.SyncSettingsFromPreferences();

	/// <summary>
	/// Halo 2 uses the default Shell chrome from <see cref="MeasureTabPage.RefreshShellNavigation"/>
	/// — there's no instrument-specific back button / title view needed, so we return false to let
	/// the fallback path run.
	/// </summary>
	public bool TryRefreshNavigation(IMeasureTabNavigationHost host, MeasureTabViewModel viewModel) => false;
}
