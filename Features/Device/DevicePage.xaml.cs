namespace HannaUIDemo.Features.Device;

/// <summary>Modal devices list — hosts <see cref="DeviceView"/> with <see cref="DeviceViewModel"/>.</summary>
public partial class DevicePage : ContentPage
{
	readonly DeviceViewModel _viewModel;
	ToolbarItem? _scanToolbarItem;

	public DevicePage(DeviceViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		BindingContext = viewModel;
		RootView.BindingContext = viewModel;
		ConfigureChrome(this);
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	static void ConfigureChrome(DevicePage page)
	{
		Shell.SetFlyoutBehavior(page, FlyoutBehavior.Disabled);
		Shell.SetNavBarIsVisible(page, true);
		Shell.SetNavBarHasShadow(page, false);
		Shell.SetBackButtonBehavior(page, new BackButtonBehavior
		{
			IsVisible = true,
			IsEnabled = true,
			TextOverride = "Home"
		});
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		UpdateScanToolbar();
	}

	void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(DeviceViewModel.IsScanning)
		    or nameof(DeviceViewModel.ScanToolbarText))
			UpdateScanToolbar();
	}

	void UpdateScanToolbar()
	{
		if (_scanToolbarItem is null)
		{
			_scanToolbarItem = new ToolbarItem
			{
				Order = ToolbarItemOrder.Primary,
				Priority = 0,
				Command = _viewModel.ScanOrStopCommand
			};
			ToolbarItems.Add(_scanToolbarItem);
		}

		_scanToolbarItem.Text = _viewModel.ScanToolbarText;
	}

	public void ApplyTheme() => RootView.RefreshForTheme();

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}
}
