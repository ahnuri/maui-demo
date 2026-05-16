using HannaUIDemo.Core.Localization;

namespace HannaUIDemo.Features.Localization;

/// <summary>Language picker page bound to <see cref="LanguageSelectionViewModel"/>.</summary>
public partial class LanguageSelectionPage : ContentPage
{
	readonly LanguageSelectionViewModel _viewModel;

	public LanguageSelectionPage(LanguageSelectionViewModel viewModel, LocalizationService localization)
	{
		_viewModel = viewModel;
		InitializeComponent();
		BindingContext = viewModel;
		_localization = localization;
	}

	readonly LocalizationService _localization;

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Title = _localization.T("Page_Language_Title");
		ToolbarItems.Clear();
		ToolbarItems.Add(new ToolbarItem
		{
			Text = _viewModel.DoneLabel,
			Order = ToolbarItemOrder.Primary,
			Priority = 0,
			Command = _viewModel.DoneCommand
		});
		_viewModel.RefreshLocalizedStrings();
		_viewModel.LoadLanguages();
	}
}
