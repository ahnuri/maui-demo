namespace HannaUIDemo.Features.Settings;

public partial class ResetPasswordPage : ContentPage
{
	public ResetPasswordPage(ResetPasswordViewModel viewModel)
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
