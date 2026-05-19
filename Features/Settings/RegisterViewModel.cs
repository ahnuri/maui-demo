using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class RegisterViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;

	[ObservableProperty] private string _email = string.Empty;
	[ObservableProperty] private string _password = string.Empty;
	[ObservableProperty] private string _confirmPassword = string.Empty;
	[ObservableProperty] private bool _showPassword;
	[ObservableProperty] private bool _showConfirmPassword;
	[ObservableProperty] private bool _showOptionalFields;
	[ObservableProperty] private string _firstName = string.Empty;
	[ObservableProperty] private string _lastName = string.Empty;
	[ObservableProperty] private string _organization = string.Empty;
	[ObservableProperty] private string _mobile = string.Empty;
	[ObservableProperty] private string _errorMessage = string.Empty;
	[ObservableProperty] private bool _hasError;

	public RegisterViewModel(UserSessionService session)
	{
		_session = session;
		ApplyLocalization();
	}

	public string PageTitle => Loc.T("Register_Title");
	public string Subtitle => Loc.T("Register_Subtitle");
	public string RequiredSectionTitle => Loc.T("Register_RequiredSection");
	public string OptionalSectionTitle => Loc.T("Register_OptionalSection");
	public string EmailPlaceholder => Loc.T("Cloud_EmailPlaceholder");
	public string PasswordPlaceholder => Loc.T("Cloud_PasswordPlaceholder");
	public string ConfirmPasswordPlaceholder => Loc.T("Register_ConfirmPassword");
	public string PasswordRules => Loc.T("Cloud_PasswordRules");
	public string RegisterButtonText => Loc.T("Register_Button");
	public string SignInPrompt => Loc.T("Register_SignInPrompt");
	public string SignInLink => Loc.T("Cloud_SignIn");
	public string FirstNameLabel => Loc.T("Profile_FirstName");
	public string LastNameLabel => Loc.T("Profile_LastName");
	public string OrganizationLabel => Loc.T("Profile_Organization");
	public string MobileLabel => Loc.T("Profile_Mobile");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(Subtitle));
		OnPropertyChanged(nameof(RequiredSectionTitle));
		OnPropertyChanged(nameof(OptionalSectionTitle));
		OnPropertyChanged(nameof(EmailPlaceholder));
		OnPropertyChanged(nameof(PasswordPlaceholder));
		OnPropertyChanged(nameof(ConfirmPasswordPlaceholder));
		OnPropertyChanged(nameof(PasswordRules));
		OnPropertyChanged(nameof(RegisterButtonText));
		OnPropertyChanged(nameof(SignInPrompt));
		OnPropertyChanged(nameof(SignInLink));
		OnPropertyChanged(nameof(FirstNameLabel));
		OnPropertyChanged(nameof(LastNameLabel));
		OnPropertyChanged(nameof(OrganizationLabel));
		OnPropertyChanged(nameof(MobileLabel));
	}

	[RelayCommand]
	void TogglePassword() => ShowPassword = !ShowPassword;

	[RelayCommand]
	void ToggleConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;

	[RelayCommand]
	void ToggleOptionalFields() => ShowOptionalFields = !ShowOptionalFields;

	[RelayCommand]
	async Task RegisterAsync()
	{
		HasError = false;
		ErrorMessage = string.Empty;

		if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
		{
			ErrorMessage = Loc.T("Register_ErrorRequired");
			HasError = true;
			return;
		}

		if (Password != ConfirmPassword)
		{
			ErrorMessage = Loc.T("Register_ErrorPasswordMatch");
			HasError = true;
			return;
		}

		if (Password.Length < 8)
		{
			ErrorMessage = Loc.T("Register_ErrorPasswordLength");
			HasError = true;
			return;
		}

		var profile = new UserProfile
		{
			FirstName = FirstName.Trim(),
			LastName = LastName.Trim(),
			Organization = Organization.Trim(),
			Mobile = Mobile.Trim()
		};

		if (!_session.Register(Email, Password, profile))
			return;

		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;

		await nav.PopAsync();
		if (nav.NavigationStack.LastOrDefault() is not SettingsPage)
			await nav.PushAsync(AppServices.Get<SettingsPage>());
	}

	[RelayCommand]
	async Task GoToSignInAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PopAsync();
	}
}
