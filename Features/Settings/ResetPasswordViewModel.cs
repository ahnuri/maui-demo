using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class ResetPasswordViewModel : LocalizedViewModelBase
{
	[ObservableProperty] private string _oldPassword = string.Empty;
	[ObservableProperty] private string _newPassword = string.Empty;
	[ObservableProperty] private string _confirmPassword = string.Empty;
	[ObservableProperty] private bool _showOldPassword;
	[ObservableProperty] private bool _showNewPassword;
	[ObservableProperty] private bool _showConfirmPassword;

	public ResetPasswordViewModel() => ApplyLocalization();

	public string PageTitle => Loc.T("Cloud_ResetPassword");
	public string OldPasswordPlaceholder => Loc.T("Cloud_OldPassword");
	public string NewPasswordPlaceholder => Loc.T("Cloud_NewPassword");
	public string ConfirmPasswordPlaceholder => Loc.T("Cloud_ConfirmPassword");
	public string PasswordRules => Loc.T("Cloud_PasswordRules");
	public string ResetButtonText => Loc.T("Cloud_ResetPassword");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(OldPasswordPlaceholder));
		OnPropertyChanged(nameof(NewPasswordPlaceholder));
		OnPropertyChanged(nameof(ConfirmPasswordPlaceholder));
		OnPropertyChanged(nameof(PasswordRules));
		OnPropertyChanged(nameof(ResetButtonText));
	}

	[RelayCommand]
	void ToggleOldPassword() => ShowOldPassword = !ShowOldPassword;

	[RelayCommand]
	void ToggleNewPassword() => ShowNewPassword = !ShowNewPassword;

	[RelayCommand]
	void ToggleConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;

	[RelayCommand]
	async Task ResetAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is { } nav)
			await nav.PopAsync();
	}
}
