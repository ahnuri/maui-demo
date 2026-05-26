namespace HannaUIDemo.Features.Instruments.Logs;

public partial class LogHistoryHomeView : ContentView
{
	public LogHistoryHomeView() => InitializeComponent();

	public void RefreshForTheme()
	{
		if (BindingContext is LogHistoryHomeViewModel vm)
			vm.RefreshForTheme();
	}
}
