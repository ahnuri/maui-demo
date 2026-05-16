namespace HannaUIDemo.Features.Settings;

/// <summary>Settings page bound to <see cref="SettingsViewModel"/>.</summary>
public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	public void ApplyTheme()
	{
		if (BindingContext is SettingsViewModel vm)
			vm.RefreshForTheme();
	}
}
