namespace HannaUIDemo.Features.Halo2;

public sealed class Halo2SettingsPage : ContentPage
{
	static readonly Color LabCanvas = Color.FromArgb("#0A0F1C");
	static readonly Color LabCard = Color.FromArgb("#18181B");
	static readonly Color LabCardElevated = Color.FromArgb("#27272A");
	static readonly Color LabBorder = Color.FromArgb("#FFFFFF").MultiplyAlpha(0.10f);
	static readonly Color LabMuted = Color.FromArgb("#A1A1AA");
	static readonly Color CyanAccent = Color.FromArgb("#22D3EE");
	static readonly Color Emerald = Color.FromArgb("#34D399");

	readonly Halo2SettingsViewModel _viewModel;
	readonly List<Border> _measureModeChips = [];
	readonly List<Border> _temperatureUnitChips = [];
	readonly Label _deviceSubtitle = new() { FontSize = 13, TextColor = LabMuted, LineBreakMode = LineBreakMode.WordWrap };

	public Halo2SettingsPage(Halo2SettingsViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;
		Title = "Device Settings";
		BackgroundColor = LabCanvas;
		Halo2Routes.ConfigureSubPageChrome(this);
		Shell.SetBackgroundColor(this, LabCanvas);
		Shell.SetForegroundColor(this, Colors.White);
		Shell.SetTitleColor(this, Colors.White);

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20, 8, 20, 32),
				Spacing = 20,
				Children =
				{
					DeviceSummary(),
					Group(
						ActionRow("Last Calibration", "Calibrate", Emerald, async () => await _viewModel.OpenCalibrationCommand.ExecuteAsync(null)),
						LogActionsRow()),
					Group(
						SettingsRow("Measure Mode", BuildMeasureModeSegment()),
						SettingsRow("Resolution", BuildResolutionSegment()),
						SettingsRow("Temperature Units", BuildTemperatureUnitSegment()),
						SettingsRow("Temperature Compensation", ValueWithChevron("ATC"))),
					SectionTitle("VIEW"),
					Group(
						SettingsRow("View Mode", BuildViewModeSegment()),
						SettingsRow("Display", BuildDisplaySegment()),
						SettingsRow("Graph Display", BuildGraphDisplaySegment()),
						SettingsRow("Stability Criteria", BuildStabilitySegment())),
					SectionTitle("CALIBRATION"),
					Group(
						SettingsRow("Calibration Buffers", ValueWithChevron("Hanna")),
						SettingsRow("Calibration Reminder", ValueWithChevron("None", disabled: true), disabled: true),
						SettingsRow("Alarm", ValueWithChevron("Off")))
				}
			}
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.RefreshFromPreferences();
		_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
	}

	Border DeviceSummary()
	{
		var icon = new Border
		{
			WidthRequest = 56,
			HeightRequest = 56,
			Padding = 6,
			BackgroundColor = CyanAccent.MultiplyAlpha(0.12f),
			Stroke = CyanAccent.MultiplyAlpha(0.25f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = new Image
			{
				Source = Halo2SettingsViewModel.DeviceIcon,
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			}
		};

		var calibratedPill = new Border
		{
			Padding = new Thickness(10, 4),
			BackgroundColor = Emerald.MultiplyAlpha(0.15f),
			Stroke = Emerald.MultiplyAlpha(0.35f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 10 },
			HorizontalOptions = LayoutOptions.Start,
			Content = new Label
			{
				Text = "CALIBRATED",
				FontSize = 10,
				FontAttributes = FontAttributes.Bold,
				TextColor = Emerald,
				CharacterSpacing = 0.5
			}
		};

		var details = new VerticalStackLayout
		{
			Spacing = 8,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = Halo2SettingsViewModel.DeviceName,
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					TextColor = Colors.White,
					LineBreakMode = LineBreakMode.WordWrap
				},
				calibratedPill,
				_deviceSubtitle
			}
		};

		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 14,
			Padding = new Thickness(18, 16),
			Children = { icon, details }
		};
		Grid.SetColumn(details, 1);

		return Card(grid, accentGlow: true);
	}

	static Label SectionTitle(string text) => new()
	{
		Text = text,
		FontSize = 12,
		FontAttributes = FontAttributes.Bold,
		TextColor = LabMuted,
		CharacterSpacing = 1.2,
		Margin = new Thickness(4, 0, 0, -6)
	};

	static Border Group(params View[] rows)
	{
		var stack = new VerticalStackLayout { Spacing = 0 };
		for (var i = 0; i < rows.Length; i++)
		{
			if (rows[i] is Border border)
			{
				border.StrokeThickness = i < rows.Length - 1 ? 0.5 : 0;
				border.Stroke = LabBorder;
			}
			stack.Children.Add(rows[i]);
		}

		return Card(stack);
	}

	static Border ActionRow(string title, string action, Color actionColor, Func<Task>? onTap = null)
	{
		var grid = BaseRowGrid();
		grid.Children.Add(new Label { Text = title, FontSize = 16, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center });
		var actionLabel = new Label
		{
			Text = action,
			FontSize = 15,
			FontAttributes = FontAttributes.Bold,
			TextColor = actionColor,
			VerticalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		grid.Children.Add(actionLabel);
		Grid.SetColumn(actionLabel, 1);

		var border = RowBorder(grid);
		if (onTap is not null)
		{
			var tap = new TapGestureRecognizer();
			tap.Tapped += async (_, _) => await onTap();
			border.GestureRecognizers.Add(tap);
		}
		return border;
	}

	static Border LogActionsRow()
	{
		var grid = BaseRowGrid();
		grid.Children.Add(new Label { Text = "Log", FontSize = 16, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center });

		var actions = new HorizontalStackLayout
		{
			Spacing = 8,
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				LogPill("Clear"),
				LogPill("Save", primary: true),
				LogPill("Share")
			}
		};
		grid.Children.Add(actions);
		Grid.SetColumn(actions, 1);
		return RowBorder(grid);
	}

	static Border LogPill(string text, bool primary = false) => new()
	{
		Padding = new Thickness(14, 8),
		BackgroundColor = primary ? CyanAccent.MultiplyAlpha(0.18f) : LabCardElevated,
		Stroke = primary ? CyanAccent.MultiplyAlpha(0.4f) : LabBorder,
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 10 },
		Content = new Label
		{
			Text = text,
			FontSize = 13,
			FontAttributes = FontAttributes.Bold,
			TextColor = primary ? CyanAccent : Colors.White
		}
	};

	static Border SettingsRow(string title, View trailing, bool disabled = false)
	{
		var grid = BaseRowGrid();
		grid.Children.Add(new Label
		{
			Text = title,
			FontSize = 16,
			TextColor = disabled ? LabMuted.MultiplyAlpha(0.55f) : Colors.White,
			VerticalOptions = LayoutOptions.Center
		});
		trailing.VerticalOptions = LayoutOptions.Center;
		grid.Children.Add(trailing);
		Grid.SetColumn(trailing, 1);
		return RowBorder(grid);
	}

	static Grid BaseRowGrid() => new()
	{
		ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)),
		ColumnSpacing = 12,
		Padding = new Thickness(16, 14),
		MinimumHeightRequest = 56
	};

	static Border RowBorder(View content) => new()
	{
		BackgroundColor = Colors.Transparent,
		StrokeThickness = 0,
		Content = content
	};

	static View ValueWithChevron(string value, bool disabled = false) => new HorizontalStackLayout
	{
		Spacing = 4,
		VerticalOptions = LayoutOptions.Center,
		HorizontalOptions = LayoutOptions.End,
		Children =
		{
			new Label
			{
				Text = value,
				FontSize = 15,
				TextColor = disabled ? LabMuted.MultiplyAlpha(0.55f) : LabMuted,
				VerticalOptions = LayoutOptions.Center
			},
			new Label { Text = "\u203A", FontSize = 20, TextColor = CyanAccent, VerticalOptions = LayoutOptions.Center }
		}
	};

	static Border Card(View content, bool accentGlow = false) => new()
	{
		BackgroundColor = LabCard,
		Stroke = accentGlow ? CyanAccent.MultiplyAlpha(0.2f) : LabBorder,
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 20 },
		Content = content,
		Shadow = accentGlow
			? new Shadow
			{
				Brush = new SolidColorBrush(CyanAccent.MultiplyAlpha(0.12f)),
				Offset = new Point(0, 4),
				Radius = 14,
				Opacity = 1
			}
			: null
	};

	static Border BuildChipShell(Grid grid) => new()
	{
		BackgroundColor = LabCardElevated,
		Stroke = LabBorder,
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 12 },
		Padding = 3,
		HorizontalOptions = LayoutOptions.Fill,
		Content = grid
	};

	Border BuildMeasureModeSegment() => BuildInteractiveSegment(
		_measureModeChips,
		["pH", "mV", "Both"],
		IndexFromPreference(Halo2Preferences.GetPrimaryDisplay()),
		idx =>
		{
			_viewModel.SetPrimaryDisplayCommand.Execute(PreferenceFromIndex(idx));
			_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
		});

	Border BuildTemperatureUnitSegment() => BuildInteractiveSegment(
		_temperatureUnitChips,
		["°C", "°F"],
		Halo2Preferences.UseFahrenheit() ? 1 : 0,
		idx =>
		{
			_viewModel.SetTemperatureUnitCommand.Execute(idx == 1);
			_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
		});

	static Border BuildResolutionSegment() => BuildStaticSegment(["0.1", "0.01", "0.001"], selected: 1);

	static Border BuildViewModeSegment() => BuildStaticSegment(["Basic", "GLP", "Full", "Graph", "Table"], selected: 0, compact: true);

	static Border BuildDisplaySegment() => BuildStaticSegment(["All Data", "Tagged"], selected: 0);

	static Border BuildGraphDisplaySegment() => BuildStaticSegment(["pH", "Temp", "Both"], selected: 2);

	static Border BuildStabilitySegment() => BuildStaticSegment(["Slow", "Medium", "Fast"], selected: 1);

	Border BuildInteractiveSegment(List<Border> chipStore, string[] labels, int selected, Action<int> onSelect)
	{
		chipStore.Clear();
		var grid = new Grid { ColumnSpacing = 2 };
		foreach (var _ in labels)
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

		for (var i = 0; i < labels.Length; i++)
		{
			var idx = i;
			var (chip, label) = CreateChip(labels[i], i == selected);
			var tap = new TapGestureRecognizer();
			tap.Tapped += (_, _) =>
			{
				onSelect(idx);
				RefreshChipSelection(chipStore, idx);
			};
			chip.GestureRecognizers.Add(tap);
			grid.Children.Add(chip);
			Grid.SetColumn(chip, i);
			chipStore.Add(chip);
		}

		return BuildChipShell(grid);
	}

	static Border BuildStaticSegment(string[] labels, int selected, bool compact = false)
	{
		var grid = new Grid { ColumnSpacing = 2 };
		foreach (var _ in labels)
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

		for (var i = 0; i < labels.Length; i++)
		{
			var (chip, _) = CreateChip(labels[i], i == selected, compact);
			grid.Children.Add(chip);
			Grid.SetColumn(chip, i);
		}

		var shell = BuildChipShell(grid);
		if (compact)
			shell.HorizontalOptions = LayoutOptions.Fill;
		return shell;
	}

	static (Border Chip, Label Label) CreateChip(string text, bool active, bool compact = false)
	{
		var label = new Label
		{
			Text = text,
			FontSize = compact ? 12 : 13,
			FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
			TextColor = active ? CyanAccent : LabMuted,
			HorizontalTextAlignment = TextAlignment.Center
		};
		var chip = new Border
		{
			Padding = new Thickness(compact ? 8 : 10, 8),
			BackgroundColor = active ? CyanAccent.MultiplyAlpha(0.18f) : Colors.Transparent,
			Stroke = active ? CyanAccent.MultiplyAlpha(0.35f) : Colors.Transparent,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 9 },
			Content = label
		};
		return (chip, label);
	}

	void RefreshChipSelection(List<Border> chips, int selectedIndex)
	{
		for (var j = 0; j < chips.Count; j++)
		{
			var chip = chips[j];
			if (chip.Content is not Label lbl)
				continue;
			var active = j == selectedIndex;
			chip.BackgroundColor = active ? CyanAccent.MultiplyAlpha(0.18f) : Colors.Transparent;
			chip.Stroke = active ? CyanAccent.MultiplyAlpha(0.35f) : Colors.Transparent;
			lbl.FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
			lbl.TextColor = active ? CyanAccent : LabMuted;
		}
	}

	static int IndexFromPreference(string v) => v.ToLowerInvariant() switch
	{
		"mv" => 1,
		"both" => 2,
		_ => 0
	};

	static string PreferenceFromIndex(int i) => i switch
	{
		1 => "mv",
		2 => "both",
		_ => "ph"
	};
}
