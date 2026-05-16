namespace HannaUIDemo.Features.Halo2;

/// <summary>Shell navigation routes for Halo 2 sub-pages (enables back button instead of flyout).</summary>
public static class Halo2Routes
{
	public const string Settings = "Halo2Settings";
	public const string Calibration = "Halo2Calibration";

	public static void Register()
	{
		Routing.RegisterRoute(Settings, typeof(Halo2SettingsPage));
		Routing.RegisterRoute(Calibration, typeof(Halo2CalibrationPage));
	}

	public static void ConfigureSubPageChrome(ContentPage page)
	{
		Shell.SetFlyoutBehavior(page, FlyoutBehavior.Disabled);
		Shell.SetNavBarIsVisible(page, true);
		Shell.SetNavBarHasShadow(page, false);
		Shell.SetBackButtonBehavior(page, new BackButtonBehavior
		{
			IsVisible = true,
			IsEnabled = true
		});
	}
}
