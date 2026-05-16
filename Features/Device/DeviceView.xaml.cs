namespace HannaUIDemo.Features.Device;

/// <summary>Devices list UI bound to <see cref="DeviceViewModel"/>.</summary>
public partial class DeviceView : ContentView
{
	public DeviceView() => InitializeComponent();

	public void RefreshForTheme()
	{
		if (BindingContext is DeviceViewModel vm)
			vm.RefreshForTheme();
	}
}
