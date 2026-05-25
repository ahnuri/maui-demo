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

		ApplyShellChrome(host, photometer);

		if (photometer.IsNewAnalysis)
		{
			// Landing state: "[photometer icon]  HI97115 - Meter".
			// Clear page.Title so platforms don't render the string title alongside the
			// custom TitleView (Android duplicates otherwise).
			host.SetTitle(string.Empty);
			host.SetTitleView(BuildLandingTitleView(tabViewModel.NavigationTitle));
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

	/// <summary>
	/// Photometer landing title: device icon followed by the meter name. Mirrors the
	/// multimeter nav title style so the three instrument families share a consistent header.
	/// </summary>
	static View BuildLandingTitleView(string meterName)
	{
		var icon = new Image
		{
			Source = DeviceIconResolver.PhotometerIcon,
			Aspect = Aspect.AspectFit,
			WidthRequest = 22,
			HeightRequest = 22,
			VerticalOptions = LayoutOptions.Center
		};

		var label = new Label
		{
			Text = meterName,
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		label.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		return new HorizontalStackLayout
		{
			Spacing = 8,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { icon, label }
		};
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
