using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Help;

public partial class HelpTabPage : ContentPage
{
	public HelpTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<HelpViewModel>();
		NavToolbar.Configure(this, "PageToolbar_Help");
		ShellChrome.ApplyStandard(this);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ShellChrome.ApplyStandard(this);
	}

	internal void ApplyTheme()
	{
		ShellChrome.ApplyStandard(this);
		if (RootView.BindingContext is HelpViewModel vm)
			vm.RefreshForTheme();
		RootView.RefreshForTheme();
	}
}
