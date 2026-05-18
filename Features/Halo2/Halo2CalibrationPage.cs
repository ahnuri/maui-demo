using HannaUIDemo.Core.Constants;

namespace HannaUIDemo.Features.Halo2;

/// <summary>Halo 2 buffer calibration flow (presentation built in code; state in <see cref="Halo2CalibrationViewModel"/>).</summary>
public sealed class Halo2CalibrationPage : ContentPage
{
	static readonly string[] Buffers = ["4.01", "6.01", "7.01", "9.01", "12.01"];

	public Halo2CalibrationPage(Halo2CalibrationViewModel viewModel)
	{
		BindingContext = viewModel;
		Title = "Calibrate";
		SetDynamicResource(BackgroundColorProperty, "PageBackground");
		Halo2Routes.ConfigureSubPageChrome(this);

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16, 12, 16, 28),
				Spacing = 16,
				Children =
				{
					BuildReadingPanel(),
					BuildInstructionPanel(),
					BuildBufferSelector(),
					BuildCurrentCalibration()
				}
			}
		};
	}

	static Border BuildReadingPanel()
	{
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)),
			Padding = 18,
			ColumnSpacing = 16
		};

		grid.Children.Add(new VerticalStackLayout
		{
			Spacing = 4,
			Children =
			{
				new Label { Text = "Halo Demo-1", FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = ThemeColors.OnSurface },
				new Label { Text = "Stable reading", FontSize = 13, TextColor = AppConstants.Success }
			}
		});

		var reading = new HorizontalStackLayout
		{
			Spacing = 6,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "7.01", FontSize = 34, TextColor = ThemeColors.OnSurface },
				new Label { Text = "pH", FontSize = 18, TextColor = ThemeColors.OnSurface, VerticalOptions = LayoutOptions.Center },
				new Label { Text = "25.2 °C ATC", FontSize = 16, TextColor = ThemeColors.OnSurfaceVariant, VerticalOptions = LayoutOptions.Center }
			}
		};
		grid.Children.Add(reading);
		Grid.SetColumn(reading, 1);

		return Card(grid);
	}

	static Border BuildInstructionPanel()
	{
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 16,
			Padding = 18
		};
		grid.Children.Add(new VerticalStackLayout
		{
			Spacing = 8,
			Children =
			{
				new Label { Text = "Save Calibration", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = ThemeColors.OnSurface },
				new Label
				{
					Text = "Confirm the stable 7.01 buffer, then save calibration or continue with the next buffer point.",
					FontSize = 15,
					LineBreakMode = LineBreakMode.WordWrap,
					TextColor = ThemeColors.OnSurfaceMuted
				}
			}
		});

		var beaker = BufferBeaker("7.01", true);
		grid.Children.Add(beaker);
		Grid.SetColumn(beaker, 1);
		return Card(grid);
	}

	static Border BuildBufferSelector()
	{
		var stack = new VerticalStackLayout { Padding = 18, Spacing = 14 };
		stack.Children.Add(new Label { Text = "Five Point Calibration", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = ThemeColors.OnSurface });

		var points = new Grid { ColumnSpacing = 8 };
		for (var i = 0; i < Buffers.Length; i++)
		{
			points.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			var active = Buffers[i] == "7.01";
			var point = new Border
			{
				Padding = new Thickness(6, 10),
				BackgroundColor = active ? AppConstants.Primary.MultiplyAlpha(0.14f) : ThemeColors.SurfaceSecondary,
				Stroke = active ? AppConstants.Primary : ThemeColors.Divider,
				StrokeThickness = active ? 1.5 : 1,
				StrokeShape = new RoundRectangle { CornerRadius = 14 },
				Content = new VerticalStackLayout
				{
					Spacing = 4,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new Label { Text = "\u25F0", FontSize = 22, TextColor = active ? AppConstants.Primary : ThemeColors.OnSurfaceVariant, HorizontalTextAlignment = TextAlignment.Center },
						new Label { Text = Buffers[i], FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = active ? AppConstants.Primary : ThemeColors.OnSurface, HorizontalTextAlignment = TextAlignment.Center },
						new Label { Text = active ? "Ready" : "Pending", FontSize = 10, TextColor = ThemeColors.OnSurfaceVariant, HorizontalTextAlignment = TextAlignment.Center }
					}
				}
			};
			points.Children.Add(point);
			Grid.SetColumn(point, i);
		}
		stack.Children.Add(points);

		var actions = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 12
		};
		actions.Children.Add(OutlineButton("Clear Calibration", AppConstants.Primary));
		var confirm = FilledButton("Confirm Buffer", AppConstants.Success);
		actions.Children.Add(confirm);
		Grid.SetColumn(confirm, 1);
		stack.Children.Add(actions);

		return Card(stack);
	}

	static Border BuildCurrentCalibration()
	{
		var stack = new VerticalStackLayout { Padding = 18, Spacing = 14 };
		stack.Children.Add(new Label { Text = "Current Calibration", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = ThemeColors.OnSurface });

		var grid = new Grid { ColumnSpacing = 8 };
		for (var i = 0; i < Buffers.Length; i++)
		{
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			var saved = Buffers[i] == "7.01";
			var value = saved ? "-0.6 mV\n25.2 °C\n14/05/26\n4:27 PM" : "Empty";
			var tile = new Border
			{
				Padding = new Thickness(6, 10),
				BackgroundColor = saved ? AppConstants.Success.MultiplyAlpha(0.1f) : ThemeColors.SurfaceSecondary,
				Stroke = saved ? AppConstants.Success.MultiplyAlpha(0.6f) : ThemeColors.Divider,
				StrokeThickness = 1,
				StrokeShape = new RoundRectangle { CornerRadius = 14 },
				Content = new VerticalStackLayout
				{
					Spacing = 4,
					Children =
					{
						new Label { Text = Buffers[i], FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = saved ? AppConstants.Success : ThemeColors.OnSurface, HorizontalTextAlignment = TextAlignment.Center },
						new Label { Text = value, FontSize = 10, TextColor = ThemeColors.OnSurfaceVariant, HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap }
					}
				}
			};
			grid.Children.Add(tile);
			Grid.SetColumn(tile, i);
		}
		stack.Children.Add(grid);
		return Card(stack);
	}

	static Border BufferBeaker(string value, bool active) => new()
	{
		WidthRequest = 96,
		HeightRequest = 86,
		BackgroundColor = active ? AppConstants.Primary.MultiplyAlpha(0.18f) : ThemeColors.SurfaceSecondary,
		Stroke = active ? AppConstants.Primary : ThemeColors.Divider,
		StrokeThickness = 1.5,
		StrokeShape = new RoundRectangle { CornerRadius = 18 },
		Content = new VerticalStackLayout
		{
			Spacing = 2,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "\u25F0", FontSize = 26, TextColor = AppConstants.Primary, HorizontalTextAlignment = TextAlignment.Center },
				new Label { Text = value, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = AppConstants.Primary, HorizontalTextAlignment = TextAlignment.Center }
			}
		}
	};

	static Border OutlineButton(string text, Color color) => new()
	{
		HeightRequest = 48,
		Stroke = color.MultiplyAlpha(0.45f),
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 14 },
		Content = new Label { Text = text, TextColor = color, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
	};

	static Border FilledButton(string text, Color color) => new()
	{
		HeightRequest = 48,
		BackgroundColor = color,
		StrokeThickness = 0,
		StrokeShape = new RoundRectangle { CornerRadius = 14 },
		Content = new Label { Text = text, TextColor = Colors.White, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
	};

	static Border Card(View content) => new()
	{
		BackgroundColor = ThemeColors.Surface,
		StrokeThickness = 0,
		StrokeShape = new RoundRectangle { CornerRadius = 18 },
		Content = content,
		Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 10, Opacity = 1 }
	};
}
