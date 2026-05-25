namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Shell route registration for Halo 2 sub-pages (settings, calibration).
///
/// <see cref="Register"/> is called once from <see cref="AppShell"/> constructor so the routes
/// are available globally. Use <c>Shell.Current.GoToAsync(Halo2Routes.Settings)</c> to push a
/// sub-page on the current Shell tab — Shell creates the destination page via DI.
///
/// Sub-pages should call <see cref="ConfigureSubPageChrome"/> from their constructor (or XAML
/// equivalent) so flyout is disabled and a back button is shown.
/// </summary>
public static class Halo2Routes
{
	public const string Settings = "Halo2Settings";
	public const string Calibration = "Halo2Calibration";

	public static void Register()
	{
		Routing.RegisterRoute(Settings, typeof(Halo2SettingsPage));
		Routing.RegisterRoute(Calibration, typeof(Halo2CalibrationPage));
	}

	/// <summary>Standard chrome for Halo 2 sub-pages — hide flyout, show back button.</summary>
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
