namespace HannaUIDemo.Features.Settings;

public partial class SignInPage : ContentPage
{
	public SignInPage(SignInViewModel viewModel)
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
