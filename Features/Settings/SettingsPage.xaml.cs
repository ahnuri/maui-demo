namespace HannaUIDemo.Features.Settings;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
		if (BindingContext is SettingsViewModel vm)
			vm.RefreshSession();
	}

	public void ApplyTheme()
	{
		if (BindingContext is SettingsViewModel vm)
			vm.RefreshForTheme();
	}
}
