using HannaUIDemo;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Helpers;

public static class NavToolbar
{
	public static void Configure(ContentPage page, string titleLocalizationKey, bool includeDisconnect = false, Func<Task>? onDisconnectAsync = null)
	{
		if (Application.Current is not App app)
			return;

		var loc = app.Services.GetRequiredService<LocalizationService>();
		page.Title = loc.T(titleLocalizationKey);
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateProfileItem(page, app));

		if (includeDisconnect && onDisconnectAsync is not null)
			page.ToolbarItems.Add(CreateDisconnectItem(onDisconnectAsync));
	}

	internal static ToolbarItem CreateDisconnectItem(Func<Task> onDisconnectAsync) => new()
	{
		Text = "Disconnect",
		Order = ToolbarItemOrder.Primary,
		Priority = 1,
		Command = new Command(() => _ = onDisconnectAsync())
	};

	internal static ToolbarItem CreateProfileItem(ContentPage page, App app) => new()
	{
		Text = "Profile",
		Order = ToolbarItemOrder.Primary,
		Priority = 0,
		IconImageSource = "tab_profile",
		Command = new Command(async () =>
		{
			await page.Navigation.PushAsync(app.Services.GetRequiredService<SettingsPage>());
		})
	};
}
