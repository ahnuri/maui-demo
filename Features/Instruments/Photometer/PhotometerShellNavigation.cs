using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Features.Measure;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>
/// Photometer-specific Shell navigation: dual-line title during measurement and back-to-flow behavior.
/// </summary>
internal static class PhotometerShellNavigation
{
	public static bool TryRefresh(
		IMeasureTabNavigationHost host,
		MeasureTabViewModel tabViewModel,
		MeasurePhotometerView view,
		PhotometerMeasureViewModel photometer)
	{
		if (!view.IsVisible || tabViewModel.ActiveDevice != InstrumentKind.Photometer)
			return false;

		ApplyShellChrome(host, photometer, view);

		if (photometer.IsNewAnalysis)
		{
			// Landing state: "[photometer icon]  HI97115 - Meter".
			// Clear page.Title so platforms don't render the string title alongside the
			// custom TitleView (Android duplicates otherwise).
			host.SetTitle(string.Empty);
			host.SetDeviceSwitcherTitleView(
				tabViewModel.NavigationTitle,
				iconSource: ImageSource.FromFile(DeviceIconResolver.PhotometerIcon));
		}
		else
		{
			host.SetTitle(string.Empty);
			host.SetDeviceSwitcherTitleView(
				tabViewModel.NavigationTitle,
				photometer.SelectedTankDisplay,
				enabled: false);
		}

		RefreshToolbar(host, tabViewModel, photometer, view);
		return true;
	}

	static void ApplyShellChrome(
		IMeasureTabNavigationHost host,
		PhotometerMeasureViewModel photometer,
		MeasurePhotometerView view)
	{
		if (photometer.IsCompleted)
		{
			host.DisableFlyout();
			host.HideBackButton();
		}
		else if (photometer.IsInMeasurementFlow)
		{
			host.DisableFlyout();
			host.SetBackCommand(new Command(view.NavigateBackInFlow));
		}
		else
		{
			host.EnableFlyout();
		}
	}

	static void RefreshToolbar(
		IMeasureTabNavigationHost host,
		MeasureTabViewModel tabViewModel,
		PhotometerMeasureViewModel photometer,
		MeasurePhotometerView view)
	{
		host.ClearToolbar();

		if (!view.IsVisible
		    || tabViewModel.ActiveDevice != InstrumentKind.Photometer)
			return;

		if (photometer.IsInMeasurementFlow)
			return;

		if (Application.Current is App app)
			host.AddToolbarItem(NavToolbar.CreateProfileItem(host.Page, app));
	}

}
