namespace HannaUIDemo.Features.Flyout;

public partial class AppFlyoutView : ContentView
{
	const double ShowStickyThreshold = 108;
	const double HideStickyThreshold = 72;

	bool _stickyVisible;
	bool _scrollUpdateQueued;
	double _pendingOffset;

	public AppFlyoutView(AppFlyoutViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		viewModel.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName is nameof(AppFlyoutViewModel.IsLoggedIn))
				ResetCollapse();
		};
	}

	public void ResetCollapse()
	{
		_stickyVisible = false;
		FlyoutList.ScrollTo(0, -1, ScrollToPosition.Start, animate: false);
		UpdateStickyBar(false);
	}

	void OnFlyoutScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		_pendingOffset = Math.Max(0, e.VerticalOffset);
		if (_scrollUpdateQueued)
			return;

		_scrollUpdateQueued = true;
		Dispatcher.Dispatch(ProcessStickyScroll);
	}

	void ProcessStickyScroll()
	{
		_scrollUpdateQueued = false;
		var offset = _pendingOffset;

		var show = _stickyVisible
			? offset > HideStickyThreshold
			: offset > ShowStickyThreshold;

		if (show != _stickyVisible)
		{
			_stickyVisible = show;
			UpdateStickyBar(show);
		}
	}

	void UpdateStickyBar(bool show)
	{
		if (SignedInProfileRoot.IsVisible)
		{
			CollapsedSignedInBar.IsVisible = show;
			CollapsedSignedInBar.Opacity = show ? 1 : 0;
			CollapsedSignedInBar.InputTransparent = !show;
			CollapsedGuestBar.IsVisible = false;
			CollapsedGuestBar.InputTransparent = true;
			return;
		}

		if (GuestProfileRoot.IsVisible)
		{
			CollapsedGuestBar.IsVisible = show;
			CollapsedGuestBar.Opacity = show ? 1 : 0;
			CollapsedGuestBar.InputTransparent = !show;
			CollapsedSignedInBar.IsVisible = false;
			CollapsedSignedInBar.InputTransparent = true;
		}
	}

	public AppFlyoutViewModel ViewModel => (AppFlyoutViewModel)BindingContext;
}
