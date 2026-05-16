namespace HannaUIDemo.Features.Info;

/// <summary>Device information view bound to <see cref="DeviceInfoViewModel"/>.</summary>
public partial class DeviceInfoView : ContentView
{
	public DeviceInfoView() => InitializeComponent();

	public void RefreshForTheme()
	{
		if (BindingContext is DeviceInfoViewModel vm)
			vm.RefreshForTheme();
	}
}
