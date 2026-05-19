namespace HannaUIDemo.Features.Settings;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
	}
}
