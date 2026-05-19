namespace HannaUIDemo.Features.Settings;

public partial class HannaCloudSettingsPage : ContentPage
{
	public HannaCloudSettingsPage(HannaCloudSettingsViewModel viewModel)
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
