using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Settings;

/// <summary>Global app settings: appearance, language, and Hanna Cloud account.</summary>
public partial class SettingsViewModel : LocalizedViewModelBase
{
	readonly ThemeService _themeService;
	readonly UserSessionService _session;

	[ObservableProperty] private AppThemeOption _themeOption = AppThemeOption.System;
	[ObservableProperty] private bool _isLoggedIn = true;
	[ObservableProperty] private Color _systemThemeBackground = Colors.Transparent;
	[ObservableProperty] private Color _lightThemeBackground = Colors.Transparent;
	[ObservableProperty] private Color _darkThemeBackground = Colors.Transparent;
	[ObservableProperty] private Color _systemThemeTextColor = ThemeColors.OnSurface;
	[ObservableProperty] private Color _lightThemeTextColor = ThemeColors.OnSurface;
	[ObservableProperty] private Color _darkThemeTextColor = ThemeColors.OnSurface;

	public SettingsViewModel(ThemeService themeService, UserSessionService session)
	{
		_themeService = themeService;
		_session = session;
		_themeOption = themeService.CurrentOption;
		_isLoggedIn = session.IsLoggedIn;
		SyncThemeChrome();
		ApplyLocalization();
		UserSessionService.SessionChanged += OnSessionChanged;
	}

	public string PageTitle => Loc.T("Settings_PageTitle");
	public string PageSubtitle => Loc.T("Settings_Subtitle");
	public string AppearanceSectionTitle => Loc.T("Settings_AppSection");
	public string SignedInStatus => Loc.T("Settings_SignedIn");
	public string CloudAccountTitle => Loc.T("Settings_CloudAccount");
	public string UserEmail => _session.Email;
	public string AppearanceTitle => Loc.T("Settings_Appearance");
	public string AppearanceSubtitle => Loc.T("Settings_AppearanceSub");
	public string LanguageRowTitle => Loc.T("Settings_Language");
	public string CloudSectionTitle => Loc.T("Settings_CloudSection");
	public string CloudSignedInSummary => Loc.T("Settings_CloudSignedInSummary", _session.Email);
	public string CloudManageButton => Loc.T("Settings_CloudManage");
	public string CloudSignInButton => Loc.T("Cloud_SignIn");
	public string ThemeSystemLabel => Loc.T("Theme_System");
	public string ThemeLightLabel => Loc.T("Theme_Light");
	public string ThemeDarkLabel => Loc.T("Theme_Dark");

	public string LanguageDisplay => Loc.GetAutonym(Loc.CurrentLanguageCode);

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(PageSubtitle));
		OnPropertyChanged(nameof(AppearanceSectionTitle));
		OnPropertyChanged(nameof(SignedInStatus));
		OnPropertyChanged(nameof(CloudAccountTitle));
		OnPropertyChanged(nameof(UserEmail));
		OnPropertyChanged(nameof(AppearanceTitle));
		OnPropertyChanged(nameof(AppearanceSubtitle));
		OnPropertyChanged(nameof(LanguageRowTitle));
		OnPropertyChanged(nameof(CloudSectionTitle));
		OnPropertyChanged(nameof(CloudSignedInSummary));
		OnPropertyChanged(nameof(CloudManageButton));
		OnPropertyChanged(nameof(CloudSignInButton));
		OnPropertyChanged(nameof(ThemeSystemLabel));
		OnPropertyChanged(nameof(ThemeLightLabel));
		OnPropertyChanged(nameof(ThemeDarkLabel));
		OnPropertyChanged(nameof(LanguageDisplay));
	}

	public override void RefreshForTheme()
	{
		base.RefreshForTheme();
		SyncThemeChrome();
	}

	void OnSessionChanged(object? sender, EventArgs e)
	{
		IsLoggedIn = _session.IsLoggedIn;
		OnPropertyChanged(nameof(CloudSignedInSummary));
		OnPropertyChanged(nameof(UserEmail));
	}

	void SyncThemeChrome()
	{
		SystemThemeBackground = ThemeOption == AppThemeOption.System ? AppConstants.Primary : Colors.Transparent;
		LightThemeBackground = ThemeOption == AppThemeOption.Light ? AppConstants.Primary : Colors.Transparent;
		DarkThemeBackground = ThemeOption == AppThemeOption.Dark ? AppConstants.Primary : Colors.Transparent;

		SystemThemeTextColor = ThemeOption == AppThemeOption.System ? Colors.White : ThemeColors.OnSurface;
		LightThemeTextColor = ThemeOption == AppThemeOption.Light ? Colors.White : ThemeColors.OnSurface;
		DarkThemeTextColor = ThemeOption == AppThemeOption.Dark ? Colors.White : ThemeColors.OnSurface;
	}

	void ApplyThemeOption(AppThemeOption option)
	{
		if (ThemeOption == option)
			return;

		ThemeOption = option;
		_themeService.SetTheme(option);
		SyncThemeChrome();
	}

	[RelayCommand]
	void SelectSystemTheme() => ApplyThemeOption(AppThemeOption.System);

	[RelayCommand]
	void SelectLightTheme() => ApplyThemeOption(AppThemeOption.Light);

	[RelayCommand]
	void SelectDarkTheme() => ApplyThemeOption(AppThemeOption.Dark);

	[RelayCommand]
	async Task OpenLanguageAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<LanguageSelectionPage>());
	}

	[RelayCommand]
	async Task OpenCloudHubAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<HannaCloudHubPage>());
	}

	[RelayCommand]
	async Task OpenSignInAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<SignInPage>());
	}

	public void RefreshSession()
	{
		_session.Load();
		IsLoggedIn = _session.IsLoggedIn;
		OnPropertyChanged(nameof(CloudSignedInSummary));
		OnPropertyChanged(nameof(UserEmail));
	}
}
