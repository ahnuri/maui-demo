namespace HannaUIDemo.Features.Settings;

public partial class HannaCloudHubPage : ContentPage
{
	public HannaCloudHubPage(HannaCloudHubViewModel viewModel)
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
