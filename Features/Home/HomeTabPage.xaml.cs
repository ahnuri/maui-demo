using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Home;

public partial class HomeTabPage : ContentPage
{
	public HomeTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<HomeViewModel>();
		NavToolbar.ConfigureLanding(this);
		ShellChrome.ApplyGrouped(this);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ShellChrome.ApplyGrouped(this);
	}

	internal void ApplyTheme()
	{
		ShellChrome.ApplyGrouped(this);
		if (RootView.BindingContext is HomeViewModel vm)
			vm.RefreshForTheme();
	}
}
