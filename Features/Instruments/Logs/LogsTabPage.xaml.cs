using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>
/// Shell tab host for Log History. Binds <see cref="LogHistoryHomeView"/> to <see cref="LogHistoryHomeViewModel"/>.
/// </summary>
public partial class LogsTabPage : ContentPage
{
	readonly LogHistoryHomeViewModel _viewModel;

	public LogsTabPage()
	{
		InitializeComponent();
		_viewModel = AppServices.Get<LogHistoryHomeViewModel>();
		_viewModel.AttachHost(this);
		RootView.BindingContext = _viewModel;
		NavToolbar.Configure(this, "Shell_LogHistory");
		ShellChrome.ApplyStandard(this);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.AttachHost(this);
		_viewModel.OnAppearing();
		ShellChrome.ApplyStandard(this);
	}

	/// <summary>Propagates theme changes to the embedded log history view.</summary>
	internal void ApplyTheme()
	{
		ShellChrome.ApplyStandard(this);
		RootView.RefreshForTheme();
	}
}
