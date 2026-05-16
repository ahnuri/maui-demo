using Foundation;
using UIKit;

namespace HannaUIDemo;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		var ok = base.FinishedLaunching(application, launchOptions!);
		// Tab bar material closer to App Store (scroll edge + standard match).
		var appearance = new UITabBarAppearance();
		appearance.ConfigureWithDefaultBackground();
		UITabBar.Appearance.StandardAppearance = appearance;
		UITabBar.Appearance.ScrollEdgeAppearance = appearance;
		return ok;
	}
}
