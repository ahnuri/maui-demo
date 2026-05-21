using HannaUIDemo.Core.Constants;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Measure;

/// <summary>Modal sheet to pick completed parameters to re-measure on the photometer.</summary>
public sealed class RerunMethodsPickerPage : ContentPage
{
	public sealed record RerunMethodOption(int Index, string Title, string DisplayValue);

	readonly IReadOnlyList<RerunMethodOption> _methods;
	readonly Action<IReadOnlyList<int>> _onRerun;
	readonly HashSet<int> _selectedIndices = new();
	readonly VerticalStackLayout _listHost = new() { Spacing = 10 };
	readonly Button _rerunButton;

	public RerunMethodsPickerPage(IReadOnlyList<RerunMethodOption> methods, Action<IReadOnlyList<int>> onRerun)
	{
		_methods = methods;
		_onRerun = onRerun;
		Title = "Re-run measurements";
		BackgroundColor = ThemeColors.StoreGroupedBackground;
		Shell.SetNavBarHasShadow(this, false);
		ToolbarItems.Add(new ToolbarItem("Cancel", null, OnCancel));

		_rerunButton = new Button
		{
			Text = "Re-run",
			HeightRequest = AppConstants.ButtonHeight,
			BackgroundColor = AppConstants.Primary,
			TextColor = Colors.White,
			CornerRadius = (int)AppConstants.RadiusButton,
			FontAttributes = FontAttributes.Bold,
			IsEnabled = false,
			Margin = new Thickness(16, 0, 16, 16)
		};
		_rerunButton.Clicked += async (_, _) => await OnRerunAsync();

		var header = new VerticalStackLayout
		{
			Padding = new Thickness(20, 12, 20, 8),
			Spacing = 6,
			Children =
			{
				new Label
				{
					Text = "Select parameters",
					FontSize = 22,
					FontAttributes = FontAttributes.Bold,
					TextColor = ThemeColors.OnSurface
				},
				new Label
				{
					Text = "Choose one or more completed measurements to run again on the HI97115.",
					FontSize = 14,
					TextColor = ThemeColors.OnSurfaceVariant,
					LineBreakMode = LineBreakMode.WordWrap
				}
			}
		};

		var scroll = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 0,
				Children = { header, _listHost }
			}
		};
		var root = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};
		root.Children.Add(scroll);
		Microsoft.Maui.Controls.Grid.SetRow(scroll, 0);
		root.Children.Add(_rerunButton);
		Microsoft.Maui.Controls.Grid.SetRow(_rerunButton, 1);
		Content = root;

		RebuildList();
	}

	void RebuildList()
	{
		_listHost.Children.Clear();
		foreach (var method in _methods)
			_listHost.Children.Add(BuildMethodRow(method));
		UpdateRerunButton();
	}

	void UpdateRerunButton()
	{
		var count = _selectedIndices.Count;
		_rerunButton.Text = count > 0 ? $"Re-run ({count})" : "Re-run";
		_rerunButton.IsEnabled = count > 0;
	}

	void ToggleSelection(int index)
	{
		if (!_selectedIndices.Add(index))
			_selectedIndices.Remove(index);
		RebuildList();
	}

	Border BuildMethodRow(RerunMethodOption method)
	{
		var selected = _selectedIndices.Contains(method.Index);
		var initials = MethodInitials(method.Title);

		var check = new Border
		{
			WidthRequest = 24,
			HeightRequest = 24,
			BackgroundColor = selected ? AppConstants.Primary : Colors.Transparent,
			Stroke = selected ? AppConstants.Primary : ThemeColors.Divider,
			StrokeThickness = selected ? 0 : 1.5,
			StrokeShape = new RoundRectangle { CornerRadius = 6 },
			VerticalOptions = LayoutOptions.Center,
			Content = selected
				? new Label
				{
					Text = "\u2713",
					FontSize = 14,
					TextColor = Colors.White,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
				: null
		};

		var avatar = new Border
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = InitialDiskBg(method.Title),
			StrokeThickness = 0,
			Content = new Label
			{
				Text = initials,
				FontSize = 13,
				FontAttributes = FontAttributes.Bold,
				TextColor = InitialDiskFg(method.Title),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			},
			StrokeShape = new Ellipse(),
			VerticalOptions = LayoutOptions.Center
		};

		var textCol = new VerticalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = method.Title,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
					MaxLines = 1,
					TextColor = ThemeColors.OnSurface
				},
				new Label
				{
					Text = method.DisplayValue,
					FontSize = 12,
					TextColor = ThemeColors.OnSurfaceVariant
				}
			}
		};

		var pickerRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12,
			Padding = new Thickness(14, 12)
		};
		pickerRow.Children.Add(check);
		Microsoft.Maui.Controls.Grid.SetColumn(check, 0);
		pickerRow.Children.Add(avatar);
		Microsoft.Maui.Controls.Grid.SetColumn(avatar, 1);
		pickerRow.Children.Add(textCol);
		Microsoft.Maui.Controls.Grid.SetColumn(textCol, 2);

		var wrap = new Border
		{
			Margin = new Thickness(16, 0, 16, 0),
			BackgroundColor = selected ? AppConstants.Primary.MultiplyAlpha(0.07f) : ThemeColors.Surface,
			Stroke = selected ? AppConstants.Primary : ThemeColors.Divider,
			StrokeThickness = selected ? 1.5 : 1,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = pickerRow
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => ToggleSelection(method.Index);
		wrap.GestureRecognizers.Add(tap);
		return wrap;
	}

	async Task OnRerunAsync()
	{
		if (_selectedIndices.Count == 0)
			return;

		var indices = _selectedIndices.OrderBy(i => i).ToList();
		if (Navigation.ModalStack.Count > 0)
			await Navigation.PopModalAsync();
		_onRerun(indices);
	}

	async void OnCancel()
	{
		if (Navigation.ModalStack.Count > 0)
			await Navigation.PopModalAsync();
	}

	static string MethodInitials(string title)
	{
		var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length >= 2)
			return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
		if (parts.Length == 1 && parts[0].Length >= 2)
			return parts[0][..2].ToUpperInvariant();
		return parts.Length == 1 ? parts[0].ToUpperInvariant() : "?";
	}

	static readonly Color[] InitialPalette =
	[
		Color.FromRgb(14, 165, 198),
		Color.FromRgb(99, 102, 241),
		Color.FromRgb(236, 72, 153),
		Color.FromRgb(34, 197, 94),
		Color.FromRgb(245, 158, 11),
		Color.FromRgb(168, 85, 247),
		Color.FromRgb(239, 68, 68),
		Color.FromRgb(20, 184, 166),
	];

	static Color InitialDiskBg(string methodTitle) =>
		InitialPalette[Math.Abs(methodTitle.GetHashCode(StringComparison.Ordinal)) % InitialPalette.Length];

	static Color InitialDiskFg(string methodTitle) =>
		InitialDiskBg(methodTitle).GetLuminosity() > 0.55 ? Color.FromRgb(30, 41, 59) : Colors.White;
}
