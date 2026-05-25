using HannaUIDemo;

namespace HannaUIDemo.Core.Theme;

/// <summary>Persists and applies light / dark / system appearance.</summary>
public sealed class ThemeService
{
	public const string PreferenceKey = "app_theme_option";

	public AppThemeOption CurrentOption { get; private set; } = AppThemeOption.System;

	public static event EventHandler? ThemeChanged;

	public void ApplyStoredTheme() => SetTheme(ReadStoredOption(), persist: false);

	public void SetTheme(AppThemeOption option, bool persist = true)
	{
		CurrentOption = option;
		if (persist)
			Preferences.Set(PreferenceKey, (int)option);

		if (Application.Current is not Application app)
			return;

		app.UserAppTheme = option switch
		{
			AppThemeOption.Light => AppTheme.Light,
			AppThemeOption.Dark => AppTheme.Dark,
			_ => AppTheme.Unspecified
		};

		SemanticResources.Update(app);
		PropagateThemeToUi(app);
		ThemeChanged?.Invoke(this, EventArgs.Empty);
	}

	static AppThemeOption ReadStoredOption()
	{
		var raw = Preferences.Get(PreferenceKey, (int)AppThemeOption.System);
		return Enum.IsDefined(typeof(AppThemeOption), raw) ? (AppThemeOption)raw : AppThemeOption.System;
	}

	static void PropagateThemeToUi(Application app)
	{
		foreach (var window in app.Windows)
		{
			if (window.Page is AppShell shell)
				shell.ApplyTheme();

			if (window.Page is not Shell { CurrentPage: { } current })
				continue;

			if (current.Navigation?.NavigationStack is not { } stack)
				continue;

			foreach (var page in stack)
			{
				switch (page)
				{
					case Features.Device.DevicePage device:
						device.ApplyTheme();
						break;
					case Features.Settings.SettingsPage settings:
						settings.ApplyTheme();
						break;
					case Features.Instruments.Halo2.Halo2SettingsPage halo2Settings:
						halo2Settings.ApplyTheme();
						break;
					case Features.Instruments.Photometer.PhotometerDeviceSettingsPage photometerDeviceSettings:
						photometerDeviceSettings.ApplyTheme();
						break;
				}
			}
		}
	}
}
