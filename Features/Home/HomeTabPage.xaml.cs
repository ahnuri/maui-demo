using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Helpers;

namespace HannaUIDemo.Features.Home;

public partial class HomeTabPage : ContentPage
{
	public HomeTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<HomeViewModel>();
		NavToolbar.Configure(this, "Shell_Home");
	}

	internal void ApplyTheme()
	{
		if (RootView.BindingContext is HomeViewModel vm)
			vm.RefreshForTheme();
	}
}
