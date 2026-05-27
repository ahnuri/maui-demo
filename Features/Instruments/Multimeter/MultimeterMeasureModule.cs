using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>Measure tab module for HI98x94 multiparameter log recall.</summary>
public sealed class MultimeterMeasureModule : IInstrumentMeasureModule
{
	/// <summary>Canonical device icon (resolved through <see cref="DeviceIconResolver"/>).</summary>
	static readonly string NavigationIcon = DeviceIconResolver.MultimeterIcon;

	MultimeterLogRecallView? _view;

	public InstrumentKind Kind => InstrumentKind.Multimeter;

	public View Content => _view ??= new MultimeterLogRecallView();

	public MultimeterLogRecallView View => (MultimeterLogRecallView)Content;

	public bool UsesLabChrome => false;

	/// <summary>Plain-text fallback (also used for accessibility / Shell.Title).</summary>
	public string GetNavigationTitle(LocalizationService loc) => loc.T("Instrument_Multimeter_NavTitle");

	public void ApplyTheme() => View.ApplyTheme();

	public void OnAppearing() { }

	/// <summary>
	/// Sets a custom Shell title view: "[multimeter icon]   HI98x94 - Multimeter".
	/// Returning true short-circuits the host's default plain-text title path.
	/// </summary>
	public bool TryRefreshNavigation(IMeasureTabNavigationHost host, MeasureTabViewModel viewModel)
	{
		if (viewModel.ActiveDevice != Kind)
			return false;

		// Clear the string title so the TitleView is the only text rendered (otherwise some
		// platforms render both, producing duplicate text in the nav bar).
		host.SetTitle(string.Empty);
		host.SetDeviceSwitcherTitleView(
			viewModel.Loc.T("Instrument_Multimeter_NavTitle"),
			iconSource: ImageSource.FromFile(NavigationIcon));
		return true;
	}
}
