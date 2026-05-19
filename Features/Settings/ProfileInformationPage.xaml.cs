namespace HannaUIDemo.Features.Settings;

public partial class ProfileInformationPage : ContentPage
{
	public ProfileInformationPage(ProfileInformationViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
		if (BindingContext is ProfileInformationViewModel vm)
			vm.LoadProfile();
	}
}
