using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Measure;

/// <summary>Demo state for HI97115 on-device settings (backlight, contrast, etc.). Persisted locally.</summary>
public partial class PhotometerDeviceSettingsViewModel : PageViewModelBase
{
	const string Pfx = "photometer_device_";

	[ObservableProperty] private string _startupViewLabel = "Method Selection";

	[RelayCommand]
	async Task OpenStartupViewInfoAsync()
	{
		if (Shell.Current?.CurrentPage is null)
			return;
		await Shell.Current.CurrentPage.DisplayAlertAsync(
			"Start-up view",
			"Method selection lets you pick individual methods. Method group uses a predefined set.",
			"OK");
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
		StartupViewLabel = Preferences.Get($"{Pfx}startup", "Method Selection");
		BacklightPercent = Preferences.Get($"{Pfx}backlight", 72);
		ContrastPercent = Preferences.Get($"{Pfx}contrast", 55);
		SeparatorUsesComma = Preferences.Get($"{Pfx}sep_comma", true);
		DeviceLanguage = Preferences.Get($"{Pfx}lang", "English");
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
