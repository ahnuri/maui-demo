using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>Demo state for HI97115 on-device settings (backlight, contrast, etc.). Persisted locally.</summary>
public partial class PhotometerDeviceSettingsViewModel : LocalizedViewModelBase
{
	const string Pfx = "photometer_device_";

	[ObservableProperty] private string _startupViewLabel = string.Empty;

	[RelayCommand]
	async Task OpenStartupViewInfoAsync()
	{
		if (Shell.Current?.CurrentPage is null)
			return;
		await Shell.Current.CurrentPage.DisplayAlertAsync(
			Loc.T("Photometer_Settings_StartupViewTitle"),
			Loc.T("Photometer_Settings_StartupViewInfo"),
			Loc.T("Common_OK"));
	}

	[ObservableProperty] private double _backlightPercent = 72;

	[ObservableProperty] private double _contrastPercent = 55;

	[ObservableProperty] private bool _separatorUsesComma = true;

	[ObservableProperty] private string _deviceLanguage = "English";

	[ObservableProperty] private bool _beepEnabled = true;

	[ObservableProperty] private bool _tutorialEnabled;

	[RelayCommand]
	Task SyncLogRecallAsync() => Task.CompletedTask;

	public PhotometerDeviceSettingsViewModel()
	{
		var startupDefault = Loc.T("Photometer_Settings_StartupMethodSelection");
		var langDefault = Loc.T("Photometer_Settings_DefaultLang");
		StartupViewLabel = Preferences.Get($"{Pfx}startup", startupDefault);
		BacklightPercent = Preferences.Get($"{Pfx}backlight", 72);
		ContrastPercent = Preferences.Get($"{Pfx}contrast", 55);
		SeparatorUsesComma = Preferences.Get($"{Pfx}sep_comma", true);
		DeviceLanguage = Preferences.Get($"{Pfx}lang", langDefault);
		BeepEnabled = Preferences.Get($"{Pfx}beep", true);
		TutorialEnabled = Preferences.Get($"{Pfx}tutorial", false);
	}

	partial void OnBacklightPercentChanged(double value) =>
		Preferences.Set($"{Pfx}backlight", (int)Math.Clamp(Math.Round(value), 0, 100));

	partial void OnContrastPercentChanged(double value) =>
		Preferences.Set($"{Pfx}contrast", (int)Math.Clamp(Math.Round(value), 0, 100));

	partial void OnSeparatorUsesCommaChanged(bool value) =>
		Preferences.Set($"{Pfx}sep_comma", value);

	partial void OnBeepEnabledChanged(bool value) =>
		Preferences.Set($"{Pfx}beep", value);

	partial void OnTutorialEnabledChanged(bool value) =>
		Preferences.Set($"{Pfx}tutorial", value);
}
