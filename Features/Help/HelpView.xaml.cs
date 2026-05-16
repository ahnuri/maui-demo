namespace HannaUIDemo.Features.Help;

/// <summary>Help screen view bound to <see cref="HelpViewModel"/>.</summary>
public partial class HelpView : ContentView
{
	public HelpView() => InitializeComponent();

	public void RefreshForTheme()
	{
		if (BindingContext is HelpViewModel vm)
			vm.RefreshForTheme();
	}
}
