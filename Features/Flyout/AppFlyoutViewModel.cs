using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace HannaUIDemo.Features.Flyout;

/// <summary>
/// Flyout menu ViewModel: user profile, cloud sync status, navigation items, and sign-in/out commands.
/// </summary>
public partial class AppFlyoutViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;
	string _selectedRouteId = "home";

	[ObservableProperty] private bool _isLoggedIn;
	[ObservableProperty] private string _displayName = string.Empty;
	[ObservableProperty] private string _email = string.Empty;
	[ObservableProperty] private string _initials = "?";
	[ObservableProperty] private bool _showVerifiedBadge;
	[ObservableProperty] private bool _showCloudSyncStatus;
	[ObservableProperty] private Color _cloudSyncStatusColor = ThemeColors.MutedSignalDot;
	[ObservableProperty] private string _versionText = string.Empty;
	[ObservableProperty] private string _copyrightText = string.Empty;
	[ObservableProperty] private string _privacyPolicyText = string.Empty;
	[ObservableProperty] private string _languageLineText = string.Empty;

	public ObservableCollection<FlyoutNavItem> MainNavItems { get; } = new();
	public ObservableCollection<FlyoutNavItem> FooterNavItems { get; } = new();

	public string WelcomeTitle => Loc.T("Flyout_Welcome");
	public string WelcomeMessage => Loc.T("Flyout_WelcomeMessage");
	public string SignInLabel => Loc.T("Cloud_SignIn");
	public string VerifiedText => Loc.T("Flyout_Verified");
	public string CloudSyncActiveHint => Loc.T("Flyout_CloudSyncActive");
	public string CloudSyncPausedHint => Loc.T("Flyout_CloudSyncPaused");
	public string CloudSyncHint => _session.IsCloudSyncEnabled ? CloudSyncActiveHint : CloudSyncPausedHint;

	public AppFlyoutViewModel(UserSessionService session)
	{
		_session = session;
		_session.Load();
		IsLoggedIn = _session.IsLoggedIn;
		UserSessionService.SessionChanged += OnSessionChanged;
		RefreshUserInfo();
		RebuildMenus();
		RefreshFooterLabels();
	}

	void OnSessionChanged(object? sender, EventArgs e)
	{
		_session.Load();
		IsLoggedIn = _session.IsLoggedIn;
		RefreshUserInfo();
		RebuildMenus();
	}

	public void SetSelectedRoute(string? shellLocation)
	{
		var id = ResolveRouteId(shellLocation);
		if (_selectedRouteId == id)
			return;

		_selectedRouteId = id;
		UpdateSelectionStates();
	}

	void RefreshUserInfo()
	{
		DisplayName = _session.GetDisplayName();
		Email = _session.Email;
		Initials = _session.GetInitials();
		ShowVerifiedBadge = _session.IsLoggedIn && _session.IsVerified;
		ShowCloudSyncStatus = _session.IsLoggedIn;
		CloudSyncStatusColor = _session.IsCloudSyncEnabled ? AppConstants.Success : ThemeColors.MutedSignalDot;
		OnPropertyChanged(nameof(CloudSyncHint));
	}

	void RefreshFooterLabels()
	{
		VersionText = Loc.T("Flyout_VersionFormat", AppInfo.Current.VersionString);
		CopyrightText = Loc.T("Flyout_Copyright", DateTime.Now.Year.ToString());
		PrivacyPolicyText = Loc.T("Flyout_PrivacyPolicy");
		LanguageLineText = Loc.T("Flyout_LanguageLine", Loc.GetAutonym(Loc.CurrentLanguageCode));
	}

	protected override void ApplyLocalization()
	{
		RefreshFooterLabels();
		RefreshUserInfo();
		OnPropertyChanged(nameof(WelcomeTitle));
		OnPropertyChanged(nameof(WelcomeMessage));
		OnPropertyChanged(nameof(SignInLabel));
		OnPropertyChanged(nameof(VerifiedText));
		OnPropertyChanged(nameof(CloudSyncActiveHint));
		OnPropertyChanged(nameof(CloudSyncPausedHint));
		RebuildMenus();
	}

	public override void RefreshForTheme()
	{
		base.RefreshForTheme();
		RebuildMenus();
	}

	public void RebuildMenus()
	{
		MainNavItems.Clear();
		FooterNavItems.Clear();

		MainNavItems.Add(CreateIconItem("home", Loc.T("Flyout_Home"), "home_icon", "//home"));
		MainNavItems.Add(CreateIconItem("devices", Loc.T("Flyout_DeviceList"), "device_list_icon", pageType: typeof(DevicePage)));
		MainNavItems.Add(CreateIconItem("measure", Loc.T("Shell_Measure"), "measure_icon", "//measure"));
		MainNavItems.Add(CreateIconItem("logs", Loc.T("Shell_LogHistory"), "log_history", "//logs"));

		FooterNavItems.Add(CreateIconItem("help", Loc.T("Shell_Help"), "help_icon", "//help"));
		FooterNavItems.Add(CreateIconItem("settings", Loc.T("Flyout_Settings"), "app_settings_icon", pageType: typeof(SettingsPage)));

		if (IsLoggedIn)
			FooterNavItems.Add(CreateLogoutItem());

		UpdateSelectionStates();
	}

	static FlyoutNavItem CreateIconItem(
		string id,
		string title,
		string asset,
		string? shellRoute = null,
		Type? pageType = null,
		bool fullColorIcon = false) =>
		new()
		{
			Id = id,
			Title = title,
			IconSource = FileIcon(asset),
			IconBadgeBackground = fullColorIcon ? Colors.Transparent : ThemeColors.FlyoutIconBadge,
			IconSize = fullColorIcon ? 28 : 22,
			ShellRoute = shellRoute,
			PageType = pageType,
			Action = pageType is null ? FlyoutNavAction.ShellRoute : FlyoutNavAction.PushPage,
		};

	static ImageSource FileIcon(string asset) => ImageSource.FromFile(asset);

	static ImageSource GlyphIcon(string glyph, Color color) =>
		new FontImageSource
		{
			Glyph = glyph,
			Color = color,
			Size = 20,
		};

	FlyoutNavItem CreateLogoutItem() =>
		new()
		{
			Id = "logout",
			Title = Loc.T("Flyout_Logout"),
			IconSource = GlyphIcon("\u23CE", AppConstants.Error),
			IconBadgeBackground = ThemeColors.FlyoutIconBadge,
			IconSize = 20,
			Action = FlyoutNavAction.SignOut,
			IsDestructive = true,
			ShowChevron = false,
		};

	void UpdateSelectionStates()
	{
		foreach (var item in MainNavItems.Concat(FooterNavItems))
		{
			var selected = item.Id == _selectedRouteId;
			item.IsSelected = selected;
			item.ShowActiveBar = selected && !item.IsDestructive;
			item.RowBackground = selected && !item.IsDestructive
				? ThemeColors.FlyoutActiveRow
				: Colors.Transparent;
			item.TitleColor = item.IsDestructive
				? AppConstants.Error
				: ThemeColors.OnSurface;
		}
	}

	static string ResolveRouteId(string? location)
	{
		if (string.IsNullOrEmpty(location))
			return "home";

		if (location.Contains("measure", StringComparison.OrdinalIgnoreCase))
			return "measure";
		if (location.Contains("logs", StringComparison.OrdinalIgnoreCase))
			return "logs";
		if (location.Contains("info", StringComparison.OrdinalIgnoreCase))
			return "info";
		if (location.Contains("help", StringComparison.OrdinalIgnoreCase))
			return "help";
		if (location.Contains("device", StringComparison.OrdinalIgnoreCase))
			return "devices";
		if (location.Contains("settings", StringComparison.OrdinalIgnoreCase))
			return "settings";

		return "home";
	}

	[RelayCommand]
	async Task NavigateAsync(FlyoutNavItem? item)
	{
		if (item is null || Shell.Current is not AppShell shell)
			return;

		shell.FlyoutIsPresented = false;

		switch (item.Action)
		{
			case FlyoutNavAction.SignOut:
				_session.SignOut();
				await shell.GoToAsync("//home");
				return;
			case FlyoutNavAction.ShellRoute when item.Id == "measure":
				_selectedRouteId = item.Id;
				UpdateSelectionStates();
				await shell.PresentMeasureDevicePickerAsync();
				return;
			case FlyoutNavAction.ShellRoute when !string.IsNullOrEmpty(item.ShellRoute):
				_selectedRouteId = item.Id;
				UpdateSelectionStates();
				await shell.GoToAsync(item.ShellRoute);
				return;
			case FlyoutNavAction.PushPage when item.PageType is not null:
				_selectedRouteId = item.Id;
				UpdateSelectionStates();
				Page page = item.Id switch
				{
					"cloud" when !_session.IsLoggedIn => AppServices.Get<SignInPage>(),
					"settings" => AppServices.Get<SettingsPage>(),
					"devices" => AppServices.Get<DevicePage>(),
					_ => (Page)((App)Application.Current!).Services.GetRequiredService(item.PageType),
				};

				await shell.CurrentPage.Navigation.PushAsync(page);
				return;
		}
	}

	[RelayCommand]
	async Task LoginAsync()
	{
		if (Shell.Current is not AppShell shell)
			return;

		shell.FlyoutIsPresented = false;
		await shell.CurrentPage.Navigation.PushAsync(AppServices.Get<SignInPage>());
	}

	[RelayCommand]
	async Task ViewProfileAsync()
	{
		if (Shell.Current is not AppShell shell)
			return;

		shell.FlyoutIsPresented = false;
		if (!_session.IsLoggedIn)
		{
			await shell.CurrentPage.Navigation.PushAsync(AppServices.Get<SignInPage>());
			return;
		}

		await shell.CurrentPage.Navigation.PushAsync(AppServices.Get<ProfileInformationPage>());
	}

	[RelayCommand]
	async Task OpenPrivacyPolicyAsync() =>
		await AppLinks.OpenAsync(
			AppLinks.HannaInstruments,
			async () =>
			{
				if (Shell.Current?.CurrentPage is ContentPage page)
					await page.DisplayAlertAsync(
						Loc.T("Alert_Privacy_Title"),
						Loc.T("Alert_Privacy_Message"),
						Loc.T("Alert_OK"));
			});

	[RelayCommand]
	async Task OpenLanguageAsync()
	{
		if (Shell.Current is not AppShell shell)
			return;

		shell.FlyoutIsPresented = false;
		await shell.CurrentPage.Navigation.PushAsync(AppServices.Get<Features.Localization.LanguageSelectionPage>());
	}
}
