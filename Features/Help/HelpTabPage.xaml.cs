using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Helpers;

namespace HannaUIDemo.Features.Help;

public partial class HelpTabPage : ContentPage
{
	public HelpTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<HelpViewModel>();
		NavToolbar.Configure(this, "PageToolbar_Help");
	}

	internal void ApplyTheme()
	{
		if (RootView.BindingContext is HelpViewModel vm)
			vm.RefreshForTheme();
		RootView.RefreshForTheme();
	}
}
