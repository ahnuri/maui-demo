using HannaUIDemo.Core.Auth;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Helpers;

public static class ProfileNavigation
{
	public static async Task OpenProfileAsync(ContentPage hostPage)
	{
		if (Application.Current is not App app)
			return;

		var session = app.Services.GetRequiredService<UserSessionService>();
		session.Load();

		var page = session.IsLoggedIn
			? (Page)app.Services.GetRequiredService<SettingsPage>()
			: app.Services.GetRequiredService<SignInPage>();

		await hostPage.Navigation.PushAsync(page);
	}
}
