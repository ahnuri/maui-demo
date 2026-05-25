using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class SignInViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;

	[ObservableProperty] private string _email = string.Empty;
	[ObservableProperty] private string _password = string.Empty;
	[ObservableProperty] private bool _isPasswordVisible;

	public SignInViewModel(UserSessionService session)
	{
		_session = session;
		// Email is the UI demo's signature — always show the fixed identity regardless of
		// what the previous session persisted. The Sign-In page binds this read-only.
		_email = UserSessionService.DemoEmail;
		_password = "demo";
		ApplyLocalization();
	}

	public string IntroText => Loc.T("Cloud_SignInIntro");
	public string EmailPlaceholder => Loc.T("Cloud_EmailPlaceholder");
	public string PasswordPlaceholder => Loc.T("Cloud_PasswordPlaceholder");
	public string ForgotPasswordText => Loc.T("Cloud_ForgotPassword");
	public string SignInButtonText => Loc.T("Cloud_SignIn");
	public string CreateAccountPrompt => Loc.T("Cloud_CreateAccountPrompt");
	public string CreateAccountLink => Loc.T("Cloud_CreateAccount");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(IntroText));
		OnPropertyChanged(nameof(EmailPlaceholder));
		OnPropertyChanged(nameof(PasswordPlaceholder));
		OnPropertyChanged(nameof(ForgotPasswordText));
		OnPropertyChanged(nameof(SignInButtonText));
		OnPropertyChanged(nameof(CreateAccountPrompt));
		OnPropertyChanged(nameof(CreateAccountLink));
	}

	[RelayCommand]
	void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

	[RelayCommand]
	async Task SignInAsync()
	{
		if (!_session.SignIn(Email, Password))
			return;

		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;

		await nav.PopAsync();
		if (nav.NavigationStack.LastOrDefault() is not SettingsPage)
			await nav.PushAsync(AppServices.Get<SettingsPage>());
	}

	[RelayCommand]
	void ForgotPassword() { }

	[RelayCommand]
	async Task CreateAccountAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<RegisterPage>());
	}
}
