namespace HannaUIDemo.Features.Instruments.Logs;

public partial class LogHistoryDeviceLogsView : ContentView
{
	public LogHistoryDeviceLogsView() => InitializeComponent();

	public void RefreshForTheme()
	{
		if (BindingContext is LogHistoryDeviceLogsViewModel vm)
			vm.RefreshForTheme();
	}
}
