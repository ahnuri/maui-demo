using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Logs;

public partial class LogHistoryDeviceLogsPage : ContentPage
{
	readonly LogHistoryDeviceLogsViewModel _viewModel;

	public LogHistoryDeviceLogsViewModel ViewModel => _viewModel;

	public LogHistoryDeviceLogsPage(LogHistoryDeviceLogsViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		RootView.BindingContext = viewModel;
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
	}

	public void Initialize(InstrumentKind kind)
	{
		_viewModel.AttachHost(this);
		_viewModel.Load(kind);
		ApplyNavigationChrome();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.AttachHost(this);
		ApplyNavigationChrome();
		ShellChrome.ApplyStandard(this);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (_viewModel.ShowEditModeActions)
			_viewModel.ToggleEditModeCommand.Execute(null);
	}

	internal void ApplyTheme()
	{
		ShellChrome.ApplyStandard(this);
		ApplyNavigationChrome();
		RootView.RefreshForTheme();
	}

	void ApplyNavigationChrome() =>
		NavToolbar.ConfigureDetail(
			this,
			_viewModel.PageTitle,
			_viewModel.PageSubtitle,
			iconSource: LogDeviceVisuals.IconAsset(_viewModel.Kind));
}
