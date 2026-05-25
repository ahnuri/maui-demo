using HannaUIDemo;
using HannaUIDemo.Core.Constants;
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

		ApplyShellChrome(host, photometer);

		if (photometer.IsNewAnalysis)
		{
			host.ClearTitleView();
			host.SetTitle(tabViewModel.NavigationTitle);
		}
		else
		{
			host.SetTitle(string.Empty);
			host.SetTitleView(BuildFlowTitleView(tabViewModel.NavigationTitle, photometer.SelectedTankDisplay));
		}

		RefreshToolbar(host, tabViewModel, photometer, view);
		return true;
	}

	static void ApplyShellChrome(IMeasureTabNavigationHost host, PhotometerMeasureViewModel photometer)
	{
		if (photometer.IsInMeasurementFlow)
		{
			host.DisableFlyout();
			host.SetBackCommand(new Command(photometer.NavigateBack));
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
		    || tabViewModel.ActiveDevice != InstrumentKind.Photometer
		    || photometer.IsInMeasurementFlow)
			return;

		if (Application.Current is App app)
			host.AddToolbarItem(NavToolbar.CreateProfileItem(host.Page, app));
	}

	static View BuildFlowTitleView(string meterName, string tankName)
	{
		var meterLabel = new Label
		{
			Text = meterName,
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		meterLabel.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		var tankLabel = new Label
		{
			Text = tankName,
			FontSize = 13,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		tankLabel.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

		return new VerticalStackLayout
		{
			Spacing = 1,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { meterLabel, tankLabel }
		};
	}
}
