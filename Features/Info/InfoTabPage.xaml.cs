using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Helpers;

namespace HannaUIDemo.Features.Info;

public partial class InfoTabPage : ContentPage
{
	public InfoTabPage()
	{
		InitializeComponent();
		RootView.BindingContext = AppServices.Get<DeviceInfoViewModel>();
		NavToolbar.Configure(this, "PageToolbar_Info");
	}

	internal void ApplyTheme()
	{
		if (RootView.BindingContext is DeviceInfoViewModel vm)
			vm.RefreshForTheme();
		RootView.RefreshForTheme();
	}
}
