using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Core.Helpers;

namespace HannaUIDemo.Features.Home;

public partial class HomeTabPage : ContentPage
{
	public HomeTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<HomeViewModel>();
		NavToolbar.ConfigureLanding(this);
	}

	internal void ApplyTheme()
	{
		if (RootView.BindingContext is HomeViewModel vm)
			vm.RefreshForTheme();
	}
}
