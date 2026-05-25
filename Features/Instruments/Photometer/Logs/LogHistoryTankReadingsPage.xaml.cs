using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Features.Instruments.Logs;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Photometer.Logs;

public partial class LogHistoryTankReadingsPage : ContentPage
{
	readonly LogHistoryTankReadingsViewModel _viewModel;

	public LogHistoryTankReadingsPage(LogHistoryTankReadingsViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		RootView.BindingContext = viewModel;
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
	}

	public void Initialize(LogTankGroupViewModel tank)
	{
		_viewModel.Load(tank);
		Title = _viewModel.TankTitle;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		NavToolbar.ConfigureDetail(this, _viewModel.TankTitle);
		ShellChrome.ApplyStandard(this);
		Title = _viewModel.TankTitle;
	}

	internal void ApplyTheme()
	{
		ShellChrome.ApplyStandard(this);
		NavToolbar.ConfigureDetail(this, _viewModel.TankTitle);
	}
}
