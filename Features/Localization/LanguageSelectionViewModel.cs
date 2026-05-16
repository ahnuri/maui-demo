using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Localization;

/// <summary>Language picker list and selection.</summary>
public partial class LanguageSelectionViewModel : PageViewModelBase
{
	readonly LocalizationService _localization;

	[ObservableProperty] private string _subtitle = string.Empty;
	[ObservableProperty] private string _doneLabel = string.Empty;

	public ObservableCollection<LanguageOptionViewModel> Languages { get; } = new();

	public LanguageSelectionViewModel(LocalizationService localization)
	{
		_localization = localization;
		RefreshLocalizedStrings();
		LoadLanguages();
	}

	public void RefreshLocalizedStrings()
	{
		Subtitle = _localization.T("Page_Language_Subtitle");
		DoneLabel = _localization.T("Page_Language_Done");
	}

	public void LoadLanguages()
	{
		Languages.Clear();
		var current = _localization.CurrentLanguageCode;
		foreach (var (code, autonym) in _localization.GetLanguageOptions())
		{
			Languages.Add(new LanguageOptionViewModel
			{
				Code = code,
				DisplayName = autonym,
				IsSelected = string.Equals(code, current, StringComparison.OrdinalIgnoreCase)
			});
		}
	}

	[RelayCommand]
	async Task SelectLanguageAsync(LanguageOptionViewModel? option)
	{
		if (option is null || Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;

		_localization.SetLanguage(option.Code);
		await nav.PopAsync();
	}

	[RelayCommand]
	async Task DoneAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is { } nav)
			await nav.PopAsync();
	}
}
