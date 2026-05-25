using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Theme;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace HannaUIDemo.Features.Measure;

/// <summary>
/// Bottom-sheet device picker presented on the current page (no tab navigation until a device is chosen).
/// Built once at startup for fast open animations.
/// </summary>
public sealed class MeasureDevicePickerPresenter
{
	const double SheetSlideDistance = 48;
	const uint ScrimFadeMs = 120;
	const uint SheetSlideMs = 160;
	const uint DismissScrimMs = 100;
	const uint DismissSheetMs = 130;

	readonly Grid _overlay;
	readonly BoxView _scrim;
	readonly Border _sheet;
	readonly Grid _navBusyOverlay;
	readonly ActivityIndicator _navBusyIndicator;
	readonly Label _navBusyLabel;
	readonly Func<InstrumentKind, Task> _onDevicePickedAsync;

	ContentPage? _hostPage;
	View? _savedContent;
	Grid? _hostGrid;
	bool _isOpen;
	bool _isAnimating;
	int _navBusyDepth;

	public MeasureDevicePickerPresenter(Func<InstrumentKind, Task> onDevicePickedAsync)
	{
		_onDevicePickedAsync = onDevicePickedAsync;
		(_overlay, _scrim, _sheet) = BuildOverlay();
		(_navBusyOverlay, _navBusyIndicator, _navBusyLabel) = BuildNavBusyOverlay();
	}

	public async Task PresentAsync()
	{
		if (_isOpen || _isAnimating)
			return;

		if (Shell.Current?.CurrentPage is not ContentPage page)
			return;

		EnsureAttachedTo(page);
		_isAnimating = true;
		_isOpen = true;
		_overlay.IsVisible = true;
		_overlay.InputTransparent = false;
		_scrim.Opacity = 0;
		_sheet.TranslationY = SheetSlideDistance;

		try
		{
			await Task.WhenAll(
				_scrim.FadeToAsync(1, ScrimFadeMs, Easing.CubicOut),
				_sheet.TranslateToAsync(0, 0, SheetSlideMs, Easing.CubicOut));
		}
		finally
		{
			_isAnimating = false;
		}
	}

	public async Task DismissAsync()
	{
		if (!_isOpen || _isAnimating)
			return;

		_isAnimating = true;
		try
		{
			await Task.WhenAll(
				_scrim.FadeToAsync(0, DismissScrimMs, Easing.CubicIn),
				_sheet.TranslateToAsync(0, SheetSlideDistance, DismissSheetMs, Easing.CubicIn));
		}
		finally
		{
			_overlay.IsVisible = false;
			_overlay.InputTransparent = true;
			_isOpen = false;
			_isAnimating = false;
			DetachFromHost();
		}
	}

	void EnsureAttachedTo(ContentPage page)
	{
		if (_hostPage == page && _hostGrid is not null)
			return;

		DetachFromHost();
		_hostPage = page;
		_savedContent = page.Content;
		_hostGrid = new Grid();
		if (_savedContent is not null)
			_hostGrid.Children.Add(_savedContent);

		_overlay.IsVisible = false;
		_overlay.InputTransparent = true;
		_navBusyOverlay.IsVisible = false;
		_navBusyOverlay.InputTransparent = true;

		_hostGrid.Children.Add(_overlay);
		_hostGrid.Children.Add(_navBusyOverlay);
		page.Content = _hostGrid;
	}

	public void ShowNavigating(string? message = null)
	{
		if (_hostGrid is null)
			return;

		_navBusyDepth++;
		_navBusyLabel.Text = message ?? "Loading…";
		_navBusyLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
		_navBusyOverlay.IsVisible = true;
		_navBusyOverlay.InputTransparent = false;
		_navBusyIndicator.IsRunning = true;
	}

	public void HideNavigating()
	{
		if (_navBusyDepth > 0)
			_navBusyDepth--;

		if (_navBusyDepth != 0)
			return;

		_navBusyIndicator.IsRunning = false;
		_navBusyOverlay.IsVisible = false;
		_navBusyOverlay.InputTransparent = true;
	}

	void HideImmediately()
	{
		if (!_isOpen)
			return;

		_overlay.IsVisible = false;
		_overlay.InputTransparent = true;
		_isOpen = false;
		_isAnimating = false;
		_scrim.Opacity = 0;
		_sheet.TranslationY = SheetSlideDistance;
	}

	void DetachFromHost()
	{
		if (_hostPage is null)
			return;

		_overlay.IsVisible = false;
		_overlay.InputTransparent = true;
		if (_hostGrid is not null)
		{
			_hostGrid.Children.Remove(_overlay);
			_hostGrid.Children.Remove(_navBusyOverlay);
		}

		_navBusyDepth = 0;
		_navBusyOverlay.IsVisible = false;
		_navBusyOverlay.InputTransparent = true;

		if (_savedContent is not null)
			_hostPage.Content = _savedContent;

		_hostPage = null;
		_hostGrid = null;
		_savedContent = null;
		_isOpen = false;
	}

	(Grid overlay, BoxView scrim, Border sheet) BuildOverlay()
	{
		var scrim = new BoxView();
		scrim.SetDynamicResource(BoxView.ColorProperty, "OverlayScrim");

		var sheet = new Border
		{
			VerticalOptions = LayoutOptions.End,
			StrokeThickness = 0,
			Padding = new Thickness(20, 16, 20, 24),
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20, 20, 0, 0) }
		};
		sheet.SetDynamicResource(Border.BackgroundColorProperty, "Surface");
		sheet.Shadow = new Shadow
		{
			Brush = new SolidColorBrush(ThemeColors.SoftShadow),
			Offset = new Point(0, -4),
			Radius = 16,
			Opacity = 1
		};

		var stack = new VerticalStackLayout { Spacing = 16 };
		var handle = new BoxView
		{
			HeightRequest = 4,
			WidthRequest = 40,
			CornerRadius = 2,
			HorizontalOptions = LayoutOptions.Center
		};
		handle.SetDynamicResource(BoxView.ColorProperty, "HandleBar");
		stack.Children.Add(handle);

		var titleLabel = new Label
		{
			Text = "Select Device to Measure/Download",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};
		titleLabel.SetDynamicResource(Label.TextColorProperty, "OnSurface");
		stack.Children.Add(titleLabel);

		var families = InstrumentRegistry.All;
		for (var i = 0; i < families.Count; i++)
		{
			if (i > 0)
			{
				var divider = new BoxView { HeightRequest = 1 };
				divider.SetDynamicResource(BoxView.ColorProperty, "Divider");
				stack.Children.Add(divider);
			}

			var family = families[i];
			var thumb = family.PickerIcon ?? family.PickerThumbText ?? "?";
			stack.Children.Add(BuildDeviceRow(
				thumb,
				family.PickerTitle,
				family.PickerSubtitle,
				family.PickerUsesTealAccent,
				family.Kind));
		}

		sheet.Content = stack;

		var scrimTap = new TapGestureRecognizer();
		scrimTap.Tapped += async (_, _) => await DismissAsync();
		scrim.GestureRecognizers.Add(scrimTap);

		var overlay = new Grid
		{
			IsVisible = false,
			InputTransparent = true,
			CascadeInputTransparent = true,
			Children = { scrim, sheet }
		};
		return (overlay, scrim, sheet);
	}

	Border BuildDeviceRow(string thumb, string title, string subtitle, bool isTeal, InstrumentKind kind)
	{
		var tileBg = isTeal ? "SubtleTeal" : "SubtleGreen";
		var row = new Border
		{
			StrokeThickness = 0,
			Padding = new Thickness(12)
		};
		row.SetDynamicResource(Border.BackgroundColorProperty, "SurfaceSecondary");

		var icon = new Border
		{
			WidthRequest = 36,
			HeightRequest = 36,
			StrokeThickness = 1,
			Stroke = AppConstants.Primary,
			StrokeShape = new RoundRectangle { CornerRadius = 8 }
		};
		icon.SetDynamicResource(Border.BackgroundColorProperty, tileBg);
		icon.Content = thumb.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
			? new Image
			{
				Source = thumb,
				WidthRequest = 26,
				HeightRequest = 26,
				Aspect = Aspect.AspectFit,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
			: new Label
			{
				Text = thumb,
				FontSize = 11,
				FontAttributes = FontAttributes.Bold,
				TextColor = AppConstants.Primary,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

		var titleLbl = new Label
		{
			Text = title,
			FontAttributes = FontAttributes.Bold,
			FontSize = 15
		};
		titleLbl.SetDynamicResource(Label.TextColorProperty, "OnSurface");
		var subLbl = new Label
		{
			Text = subtitle,
			FontSize = 12,
			LineBreakMode = LineBreakMode.WordWrap
		};
		subLbl.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

		var inner = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			RowDefinitions = new RowDefinitionCollection(
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)),
			ColumnSpacing = 12
		};
		inner.Children.Add(icon);
		Grid.SetRowSpan(icon, 2);
		inner.Children.Add(titleLbl);
		Grid.SetColumn(titleLbl, 1);
		inner.Children.Add(subLbl);
		Grid.SetColumn(subLbl, 1);
		Grid.SetRow(subLbl, 1);
		var chevron = new Label
		{
			Text = "\u203A",
			FontSize = 28,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center
		};
		inner.Children.Add(chevron);
		Grid.SetColumn(chevron, 2);
		Grid.SetRowSpan(chevron, 2);

		row.Content = inner;

		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await OnRowTappedAsync(kind);
		row.GestureRecognizers.Add(tap);
		return row;
	}

	async Task OnRowTappedAsync(InstrumentKind kind)
	{
		if (_isAnimating)
			return;

		HideImmediately();
		try
		{
			await _onDevicePickedAsync(kind);
		}
		finally
		{
			DetachFromHost();
		}
	}

	static (Grid overlay, ActivityIndicator indicator, Label label) BuildNavBusyOverlay()
	{
		var indicator = new ActivityIndicator
		{
			Color = AppConstants.Primary,
			WidthRequest = 36,
			HeightRequest = 36,
			HorizontalOptions = LayoutOptions.Center
		};
		var label = new Label
		{
			FontSize = 14,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(24, 12, 24, 0)
		};
		label.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		var stack = new VerticalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Spacing = 0,
			Children = { indicator, label }
		};

		var scrim = new BoxView();
		scrim.SetDynamicResource(BoxView.ColorProperty, "OverlayScrim");

		var overlay = new Grid
		{
			IsVisible = false,
			InputTransparent = true,
			CascadeInputTransparent = true,
			Children = { scrim, stack }
		};
		return (overlay, indicator, label);
	}
}
