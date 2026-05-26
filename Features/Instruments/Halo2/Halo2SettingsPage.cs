using HannaUIDemo.Core.Localization;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Halo2;

public sealed class Halo2SettingsPage : ContentPage
{
	static Color LabCanvas => ThemeColors.LabCanvas;
	static Color LabCard => ThemeColors.LabCard;
	static Color LabCardElevated => ThemeColors.LabCardElevated;
	static Color LabBorder => ThemeColors.LabBorder;
	static Color LabMuted => ThemeColors.LabMuted;
	static Color LabPrimaryText => ThemeColors.LabPrimaryText;
	static Color CyanAccent => ThemeColors.LabAccentCyan;
	static Color Emerald => ThemeColors.LabEmerald;

	readonly Halo2SettingsViewModel _viewModel;
	readonly List<Border> _measureModeChips = [];
	readonly List<Border> _temperatureUnitChips = [];
	readonly Label _deviceSubtitle = new() { FontSize = 13, TextColor = LabMuted, LineBreakMode = LineBreakMode.WordWrap };

	public Halo2SettingsPage(Halo2SettingsViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;
		var loc = viewModel.Loc;
		Title = loc.T("Halo_Settings_PageTitle");
		ApplyChrome();
		Halo2Routes.ConfigureSubPageChrome(this);

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20, 8, 20, 32),
				Spacing = 20,
				Children =
				{
					DeviceSummary(loc),
					Group(
						ActionRow(loc.T("Halo_Settings_LastCalibration"), loc.T("Halo_Tab_Calibrate"), Emerald, async () => await _viewModel.OpenCalibrationCommand.ExecuteAsync(null)),
						LogActionsRow(loc)),
					Group(
						SettingsRow(loc.T("Halo_Settings_MeasureMode"), BuildMeasureModeSegment(loc)),
						SettingsRow(loc.T("Halo_Settings_Resolution"), BuildResolutionSegment()),
						SettingsRow(loc.T("Halo_Settings_TemperatureUnits"), BuildTemperatureUnitSegment(loc)),
						SettingsRow(loc.T("Halo_Settings_TemperatureCompensation"), ValueWithChevron(loc.T("Halo_Settings_Atc")))),
					SectionTitle(loc.T("Halo_Settings_ViewSection")),
					Group(
						SettingsRow(loc.T("Halo_Settings_ViewMode"), BuildViewModeSegment(loc)),
						SettingsRow(loc.T("Halo_Settings_Display"), BuildDisplaySegment(loc)),
						SettingsRow(loc.T("Halo_Settings_GraphDisplay"), BuildGraphDisplaySegment(loc)),
						SettingsRow(loc.T("Halo_Settings_StabilityCriteria"), BuildStabilitySegment(loc))),
					SectionTitle(loc.T("Halo_Settings_CalibrationSection")),
					Group(
						SettingsRow(loc.T("Halo_Settings_CalibrationBuffers"), ValueWithChevron(loc.T("Halo_Settings_BufferHanna"))),
						SettingsRow(loc.T("Halo_Settings_CalibrationReminder"), ValueWithChevron(loc.T("Common_None"), disabled: true), disabled: true),
						SettingsRow(loc.T("Common_Alarm"), ValueWithChevron(loc.T("Common_Off"))))
				}
			}
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ApplyChrome();
		_viewModel.RefreshFromPreferences();
		_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
	}

	public void ApplyTheme() => ApplyChrome();

	void ApplyChrome()
	{
		BackgroundColor = LabCanvas;
		ShellChrome.ApplyLab(this);
	}

	Border DeviceSummary(LocalizationService loc)
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
				// DeviceIcon is intentionally a const asset name (not a translatable label).
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
				Text = loc.T("Halo_Settings_Calibrated"),
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
					Text = _viewModel.DeviceName,
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					TextColor = LabPrimaryText,
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
		grid.Children.Add(new Label { Text = title, FontSize = 16, TextColor = LabPrimaryText, VerticalOptions = LayoutOptions.Center });
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

	static Border LogActionsRow(LocalizationService loc)
	{
		var grid = BaseRowGrid();
		grid.Children.Add(new Label { Text = loc.T("Halo_Log_Section"), FontSize = 16, TextColor = LabPrimaryText, VerticalOptions = LayoutOptions.Center });

		var actions = new HorizontalStackLayout
		{
			Spacing = 8,
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				LogPill(loc.T("Halo_Log_Clear")),
				LogPill(loc.T("Halo_Log_Save"), primary: true),
				LogPill(loc.T("Halo_Log_Share"))
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
			TextColor = primary ? CyanAccent : LabPrimaryText
		}
	};

	static Border SettingsRow(string title, View trailing, bool disabled = false)
	{
		var grid = BaseRowGrid();
		grid.Children.Add(new Label
		{
			Text = title,
			FontSize = 16,
			TextColor = disabled ? LabMuted.MultiplyAlpha(0.55f) : LabPrimaryText,
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

	static Border Card(View content, bool accentGlow = false)
	{
		var card = new Border
		{
			BackgroundColor = LabCard,
			Stroke = accentGlow ? CyanAccent.MultiplyAlpha(0.2f) : LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 20 },
			Content = content
		};

		if (accentGlow)
		{
			card.Shadow = new Shadow
			{
				Brush = new SolidColorBrush(CyanAccent.MultiplyAlpha(0.12f)),
				Offset = new Point(0, 4),
				Radius = 14,
				Opacity = 1
			};
		}

		return card;
	}

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

	Border BuildMeasureModeSegment(LocalizationService loc) => BuildInteractiveSegment(
		_measureModeChips,
		[loc.T("Halo_Mode_Ph"), loc.T("Halo_Mode_Mv"), loc.T("Halo_Mode_Both")],
		IndexFromPreference(Halo2Preferences.GetPrimaryDisplay()),
		idx =>
		{
			_viewModel.SetPrimaryDisplayCommand.Execute(PreferenceFromIndex(idx));
			_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
		});

	Border BuildTemperatureUnitSegment(LocalizationService loc) => BuildInteractiveSegment(
		_temperatureUnitChips,
		[loc.T("Halo_TemperatureUnit_Celsius"), loc.T("Halo_TemperatureUnit_Fahrenheit")],
		Halo2Preferences.UseFahrenheit() ? 1 : 0,
		idx =>
		{
			_viewModel.SetTemperatureUnitCommand.Execute(idx == 1);
			_deviceSubtitle.Text = _viewModel.DeviceSubtitle;
		});

	// Numeric resolution values are universal (not translatable).
	static Border BuildResolutionSegment() => BuildStaticSegment(["0.1", "0.01", "0.001"], selected: 1);

	static Border BuildViewModeSegment(LocalizationService loc) => BuildStaticSegment(
		[
			loc.T("Halo_Settings_ViewMode_Basic"),
			loc.T("Halo_Settings_ViewMode_Glp"),
			loc.T("Halo_Settings_ViewMode_Full"),
			loc.T("Halo_Settings_ViewMode_Graph"),
			loc.T("Halo_Settings_ViewMode_Table"),
		],
		selected: 0,
		compact: true);

	static Border BuildDisplaySegment(LocalizationService loc) => BuildStaticSegment(
		[loc.T("Halo_Settings_Display_AllData"), loc.T("Halo_Settings_Display_Tagged")],
		selected: 0);

	static Border BuildGraphDisplaySegment(LocalizationService loc) => BuildStaticSegment(
		[loc.T("Halo_Settings_Graph_Ph"), loc.T("Halo_Settings_Graph_Temp"), loc.T("Halo_Settings_Graph_Both")],
		selected: 2);

	static Border BuildStabilitySegment(LocalizationService loc) => BuildStaticSegment(
		[loc.T("Halo_Settings_Stability_Slow"), loc.T("Halo_Settings_Stability_Medium"), loc.T("Halo_Settings_Stability_Fast")],
		selected: 1);

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
