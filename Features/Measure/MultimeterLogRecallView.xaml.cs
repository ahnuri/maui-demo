using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Measure;

/// <summary>Multimeter log recall view bound to <see cref="MultimeterLogRecallViewModel"/>.</summary>
public partial class MultimeterLogRecallView : ContentView
{
	public MultimeterLogRecallView()
	{
		InitializeComponent();
		BindingContext = AppServices.Get<MultimeterLogRecallViewModel>();
	}

	public void ApplyTheme()
	{
		if (BindingContext is MultimeterLogRecallViewModel vm)
			vm.RefreshForTheme();
	}
}
