using HannaUIDemo.Core.Constants;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Photometer;

/// <summary>Modal sheet to pick tank (1–100) with search and grouped list styling.</summary>
public sealed class TankPickerPage : ContentPage
{
	readonly Action<int> _onSelected;
	readonly int _initialTank;
	readonly VerticalStackLayout _listHost = new() { Spacing = 0 };
	readonly SearchBar _search;
	string _query = "";
	bool _dismissed;

	public TankPickerPage(int currentTank, Action<int> onSelected)
	{
		_onSelected = onSelected;
		_initialTank = Math.Clamp(currentTank, 1, 100);
		Title = "Select tank";
		BackgroundColor = ThemeColors.StoreGroupedBackground;
		Shell.SetNavBarHasShadow(this, false);

		ToolbarItems.Add(new ToolbarItem("Cancel", null, OnCancel));

		_search = new SearchBar
		{
			Placeholder = "Search by number or name…",
			Margin = new Thickness(16, 4, 16, 8),
			TextColor = ThemeColors.OnSurface,
			PlaceholderColor = ThemeColors.OnSurfaceVariant,
			BackgroundColor = ThemeColors.SurfaceSecondary
		};
		_search.TextChanged += (_, e) =>
		{
			_query = e.NewTextValue ?? "";
			RebuildList();
		};

		var card = new Border
		{
			Margin = new Thickness(16, 0, 16, 16),
			Padding = new Thickness(0, 0, 0, 0),
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = _listHost,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 2),
				Radius = 12,
				Opacity = 1
			}
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 0,
				Children =
				{
					new VerticalStackLayout
					{
						Padding = new Thickness(20, 12, 20, 8),
						Spacing = 6,
						Children =
						{
							new Label
							{
								Text = "Choose a tank",
								FontSize = 22,
								FontAttributes = FontAttributes.Bold,
								TextColor = ThemeColors.OnSurface
							},
							new Label
							{
								Text = "Tap a tank to select it, or Cancel to keep the current tank.",
								FontSize = 14,
								TextColor = ThemeColors.OnSurfaceVariant,
								LineBreakMode = LineBreakMode.WordWrap
							}
						}
					},
					_search,
					card
				}
			}
		};

		RebuildList();
	}

	void RebuildList()
	{
		_listHost.Children.Clear();
		var q = _query.Trim();
		var matches = Enumerable.Range(1, 100).Where(n =>
		{
			if (q.Length == 0)
				return true;
			if (n.ToString().Contains(q, StringComparison.OrdinalIgnoreCase))
				return true;
			return $"Tank {n}".Contains(q, StringComparison.OrdinalIgnoreCase);
		}).ToList();

		if (matches.Count == 0)
		{
			_listHost.Children.Add(new Label
			{
				Text = "No tanks match your search.",
				FontSize = 15,
				TextColor = ThemeColors.OnSurfaceVariant,
				HorizontalTextAlignment = TextAlignment.Center,
				Margin = new Thickness(20, 24, 20, 24)
			});
			return;
		}

		for (var i = 0; i < matches.Count; i++)
		{
			var n = matches[i];
			var isLast = i == matches.Count - 1;
			_listHost.Children.Add(BuildRow(n));
			if (!isLast)
			{
				_listHost.Children.Add(new BoxView
				{
					HeightRequest = 1,
					Color = ThemeColors.Divider,
					Margin = new Thickness(16, 0, 0, 0)
				});
			}
		}
	}

	Border BuildRow(int n)
	{
		var isCurrent = n == _initialTank;
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			Padding = new Thickness(16, 14, 16, 14),
			ColumnSpacing = 12,
			BackgroundColor = isCurrent ? AppConstants.Primary.MultiplyAlpha(0.08f) : Colors.Transparent
		};

		var title = new Label
		{
			Text = $"Tank {n}",
			FontSize = 17,
			TextColor = ThemeColors.OnSurface,
			VerticalOptions = LayoutOptions.Center
		};

		var right = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
		if (isCurrent)
		{
			right.Children.Add(new Border
			{
				Padding = new Thickness(8, 4),
				BackgroundColor = AppConstants.Primary.MultiplyAlpha(0.15f),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 6 },
				Content = new Label
				{
					Text = "Last used",
					FontSize = 11,
					FontAttributes = FontAttributes.Bold,
					TextColor = AppConstants.Primary
				}
			});
		}

		right.Children.Add(new Label
		{
			Text = "\u203A",
			FontSize = 22,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center
		});

		grid.Children.Add(title);
		grid.Children.Add(right);
		Grid.SetColumn(right, 1);

		var border = new Border
		{
			BackgroundColor = Colors.Transparent,
			StrokeThickness = 0,
			Content = grid
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => OnPick(n);
		border.GestureRecognizers.Add(tap);
		return border;
	}

	void OnPick(int n)
	{
		if (_dismissed)
			return;
		_dismissed = true;
		_onSelected(n);
		_ = DismissAsync();
	}

	async Task DismissAsync()
	{
		if (Navigation.ModalStack.Count > 0)
			await Navigation.PopModalAsync();
	}

	async void OnCancel()
	{
		if (_dismissed)
			return;
		if (Navigation.ModalStack.Count > 0)
			await Navigation.PopModalAsync();
	}
}
