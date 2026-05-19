using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class HannaCloudHubViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;

	public HannaCloudHubViewModel(UserSessionService session)
	{
		_session = session;
		ApplyLocalization();
	}

	public string PageTitle => Loc.T("Shell_HannaCloud");
	public string HubIntro => Loc.T("Cloud_HubIntro");

	public string CloudSettingsText => Loc.T("Cloud_Settings");
	public string ProfileInformationText => Loc.T("Cloud_ProfileInformation");
	public string ResetPasswordText => Loc.T("Cloud_ResetPassword");
	public string LogOutText => Loc.T("Cloud_LogOut");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(HubIntro));
		OnPropertyChanged(nameof(CloudSettingsText));
		OnPropertyChanged(nameof(ProfileInformationText));
		OnPropertyChanged(nameof(ResetPasswordText));
		OnPropertyChanged(nameof(LogOutText));
	}

	[RelayCommand]
	async Task OpenCloudSettingsAsync() => await PushAsync<HannaCloudSettingsPage>();

	[RelayCommand]
	async Task OpenProfileAsync() => await PushAsync<ProfileInformationPage>();

	[RelayCommand]
	async Task OpenResetPasswordAsync() => await PushAsync<ResetPasswordPage>();

	[RelayCommand]
	async Task LogOutAsync()
	{
		_session.SignOut();
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;

		while (nav.NavigationStack.Count > 1)
			await nav.PopAsync();

		await nav.PushAsync(AppServices.Get<SignInPage>());
	}

	static async Task PushAsync<TPage>() where TPage : Page
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<TPage>());
	}
}
