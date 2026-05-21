using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Helpers;

public static class NavToolbar
{
	/// <summary>Landing tab: Bluetooth scan and settings in the nav bar.</summary>
	public static void ConfigureLanding(ContentPage page)
	{
		if (Application.Current is not App app)
			return;

		var loc = app.Services.GetRequiredService<LocalizationService>();
		page.Title = loc.T("Shell_Home");
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateBluetoothScanItem(page, app, loc));
		page.ToolbarItems.Add(CreateSettingsItem(page, app, loc));
	}

	public static void Configure(ContentPage page, string titleLocalizationKey, bool includeDisconnect = false, Func<Task>? onDisconnectAsync = null)
	{
		if (Application.Current is not App app)
			return;

		var loc = app.Services.GetRequiredService<LocalizationService>();
		page.Title = loc.T(titleLocalizationKey);
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateSettingsItem(page, app, loc));

		if (includeDisconnect && onDisconnectAsync is not null)
			page.ToolbarItems.Add(CreateDisconnectItem(loc, onDisconnectAsync));
	}

	/// <summary>Pushed detail pages with a fixed title (not Shell tab labels).</summary>
	public static void ConfigureDetail(ContentPage page, string title)
	{
		if (Application.Current is not App app)
			return;

		page.Title = title;
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateSettingsItem(page, app, app.Services.GetRequiredService<LocalizationService>()));
	}

	static ToolbarItem CreateBluetoothScanItem(ContentPage page, App app, LocalizationService loc) => new()
	{
		Text = loc.T("Toolbar_ScanDevices"),
		Order = ToolbarItemOrder.Primary,
		Priority = 0,
		IconImageSource = "bluetooth_icon",
		Command = new Command(async () =>
		{
			await page.Navigation.PushAsync(app.Services.GetRequiredService<DevicePage>());
		})
	};

	static ToolbarItem CreateSettingsItem(ContentPage page, App app, LocalizationService loc) => new()
	{
		Text = loc.T("Toolbar_Profile"),
		Order = ToolbarItemOrder.Primary,
		Priority = 1,
		IconImageSource = "tab_profile",
		// IconImageSource = new FontImageSource
		// {
		// 	Glyph = "\u2699",
		// 	Size = 20,
		// 	Color = AppConstants.Primary
		// },
		Command = new Command(() => _ = ProfileNavigation.OpenProfileAsync(page))
	};

	static ToolbarItem CreateDisconnectItem(LocalizationService loc, Func<Task> onDisconnectAsync) => new()
	{
		Text = loc.T("Toolbar_Disconnect"),
		Order = ToolbarItemOrder.Primary,
		Priority = 2,
		Command = new Command(() => _ = onDisconnectAsync())
	};

	internal static ToolbarItem CreateProfileItem(ContentPage page, App app) =>
		CreateSettingsItem(page, app, app.Services.GetRequiredService<LocalizationService>());
}
