using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Localization;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Settings;

/// <summary>Photometer / app settings screen state.</summary>
public partial class SettingsViewModel : PageViewModelBase
{
	[ObservableProperty] private double _backlight = 50;
	[ObservableProperty] private double _contrast = 30;
	[ObservableProperty] private bool _beep;
	[ObservableProperty] private bool _tutorial;
	[ObservableProperty] private bool _useComma = true;
	[ObservableProperty] private Color _dotSeparatorBackground = Colors.Transparent;
	[ObservableProperty] private Color _commaSeparatorBackground = AppConstants.Primary;
	[ObservableProperty] private Color _dotSeparatorTextColor = ThemeColors.OnSurface;
	[ObservableProperty] private Color _commaSeparatorTextColor = Colors.White;

	public string BacklightPercent => $"{(int)Backlight}%";
	public string ContrastPercent => $"{(int)Contrast}%";
	public string LanguageDisplay => "English";

	public SettingsViewModel() => SyncSeparatorChrome();

	partial void OnBacklightChanged(double value) => OnPropertyChanged(nameof(BacklightPercent));
	partial void OnContrastChanged(double value) => OnPropertyChanged(nameof(ContrastPercent));
	partial void OnUseCommaChanged(bool value) => SyncSeparatorChrome();

	void SyncSeparatorChrome()
	{
		DotSeparatorBackground = UseComma ? Colors.Transparent : AppConstants.Primary;
		CommaSeparatorBackground = UseComma ? AppConstants.Primary : Colors.Transparent;
		DotSeparatorTextColor = UseComma ? ThemeColors.OnSurface : Colors.White;
		CommaSeparatorTextColor = UseComma ? Colors.White : ThemeColors.OnSurface;
	}

	[RelayCommand]
	async Task OpenLanguageAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<LanguageSelectionPage>());
	}

	[RelayCommand]
	void SelectDotSeparator() => UseComma = false;

	[RelayCommand]
	void SelectCommaSeparator() => UseComma = true;

	[RelayCommand]
	void OpenChemicalForm() { }

	[RelayCommand]
	void SyncReadings() { }
}
