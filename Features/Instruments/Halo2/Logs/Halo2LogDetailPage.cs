using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Features.Instruments.Halo2;
using HannaUIDemo.Features.Instruments.Logs;
using HannaUIDemo.Theme;
using Microsoft.Maui.Controls.Shapes;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Halo 2 saved log detail — table, graph (WIP), and GLP/calibration views.</summary>
public sealed class Halo2LogDetailPage : ContentPage
{
	enum DetailView { Table, Graph, Calibration }
	enum DataFilter { All, Tagged }

	readonly Halo2LogDetailViewModel _viewModel;
	readonly VerticalStackLayout _contentHost;
	readonly Label _logFileNameLabel;
	readonly Label _recordedDateLabel;
	readonly List<Border> _modeButtons = [];

	DetailView _view = DetailView.Table;
	DataFilter _filter = DataFilter.All;

	public Halo2LogDetailViewModel ViewModel => _viewModel;

	public Halo2LogDetailPage(Halo2LogDetailViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;
		Title = "Log Details";
		Shell.SetNavBarIsVisible(this, true);
		Shell.SetNavBarHasShadow(this, false);
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

		_logFileNameLabel = new Label
		{
			FontAttributes = FontAttributes.Bold,
			FontSize = 20,
			LineBreakMode = LineBreakMode.TailTruncation,
			TextColor = ThemeColors.OnSurface
		};
		_logFileNameLabel.SetBinding(Label.TextProperty, nameof(Halo2LogDetailViewModel.LogFileName));

		_recordedDateLabel = new Label
		{
			FontSize = 14,
			LineBreakMode = LineBreakMode.WordWrap,
			TextColor = ThemeColors.OnSurfaceVariant
		};
		_recordedDateLabel.SetBinding(Label.TextProperty, nameof(Halo2LogDetailViewModel.RecordedDate));

		_contentHost = new VerticalStackLayout { Spacing = 12 };

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16, 12, 16, 28),
				Spacing = 14,
				Children =
				{
					BuildLogHeader(),
					BuildModeToolbar(),
					_contentHost
				}
			}
		};

		RebuildContent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		NavToolbar.ConfigureDetail(this, "Log Details", subtitle: null);
		ShellChrome.ApplyStandard(this);
	}

	public void ApplyTheme()
	{
		NavToolbar.ConfigureDetail(this, "Log Details", subtitle: null);
		ShellChrome.ApplyStandard(this);
		RebuildContent();
	}

	View BuildLogHeader()
	{
		var edit = new Label
		{
			Text = "\u270E",
			FontSize = 16,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.End,
			Padding = new Thickness(6, 0, 0, 0)
		};
		var editTap = new TapGestureRecognizer();
		editTap.SetBinding(TapGestureRecognizer.CommandProperty,
			new Binding(nameof(Halo2LogDetailViewModel.RenameLogCommand), source: _viewModel));
		edit.GestureRecognizers.Add(editTap);
		SemanticProperties.SetDescription(edit, "Rename log file");

		var nameRow = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)
			],
			ColumnSpacing = 2,
			VerticalOptions = LayoutOptions.Center
		};
		_logFileNameLabel.VerticalOptions = LayoutOptions.Center;
		nameRow.Children.Add(_logFileNameLabel);
		nameRow.Children.Add(edit);
		Grid.SetColumn(edit, 1);

		return new VerticalStackLayout
		{
			Spacing = 6,
			Children =
			{
				nameRow,
				_recordedDateLabel
			}
		};
	}

	ScrollView BuildModeToolbar()
	{
		var modes = new HorizontalStackLayout
		{
			Spacing = 8,
			Padding = new Thickness(0, 2),
			Children =
			{
				BuildModeButton(DetailView.Table, HaloMeasureModeIconKind.Table, "Table view"),
				BuildModeButton(DetailView.Graph, HaloMeasureModeIconKind.Chart, "Graph view"),
				BuildModeButton(DetailView.Calibration, HaloMeasureModeIconKind.Calibration, "GLP calibration data"),
				BuildShareButton()
			}
		};

		return new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			HeightRequest = 48,
			Content = modes
		};
	}

	Border BuildModeButton(DetailView mode, HaloMeasureModeIconKind icon, string semantic)
	{
		var active = _view == mode;
		var button = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			Padding = 10,
			BackgroundColor = active ? ThemeColors.Surface : ThemeColors.SurfaceSecondary,
			Stroke = active ? AppConstants.Primary : ThemeColors.Divider,
			StrokeThickness = active ? 2 : 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = Halo2MeasureModeIcons.Create(icon, () => active ? AppConstants.Primary : ThemeColors.OnSurfaceVariant, 22)
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			_view = mode;
			UpdateModeButtons();
			RebuildContent();
		};
		button.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(button, semantic);
		_modeButtons.Add(button);
		return button;
	}

	Border BuildShareButton()
	{
		var button = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			Padding = 10,
			BackgroundColor = ThemeColors.SurfaceSecondary,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new Label
			{
				Text = "\u21A7",
				FontSize = 18,
				TextColor = ThemeColors.OnSurfaceVariant,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		var shareTap = new TapGestureRecognizer();
		shareTap.SetBinding(TapGestureRecognizer.CommandProperty,
			new Binding(nameof(Halo2LogDetailViewModel.ExportLogCommand), source: _viewModel));
		button.GestureRecognizers.Add(shareTap);
		SemanticProperties.SetDescription(button, "Export log as PDF or CSV");
		return button;
	}

	void UpdateModeButtons()
	{
		var modes = new[] { DetailView.Table, DetailView.Graph, DetailView.Calibration };
		for (var i = 0; i < _modeButtons.Count && i < modes.Length; i++)
			ApplyModeButtonStyle(_modeButtons[i], _view == modes[i]);
	}

	static void ApplyModeButtonStyle(Border button, bool active)
	{
		button.BackgroundColor = active ? ThemeColors.Surface : ThemeColors.SurfaceSecondary;
		button.Stroke = active ? AppConstants.Primary : ThemeColors.Divider;
		button.StrokeThickness = active ? 2 : 1;
	}

	void RebuildContent()
	{
		_contentHost.Children.Clear();

		_contentHost.Children.Add(_view switch
		{
			DetailView.Table => BuildTableSection(),
			DetailView.Graph => BuildGraphSection(),
			_ => BuildCalibrationSection()
		});
	}

	View BuildDataFilterBar(bool showExport)
	{
		var row = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)
			],
			ColumnSpacing = 10,
			VerticalOptions = LayoutOptions.Center
		};

		var allChip = BuildFilterSegment("All Data", DataFilter.All);
		var taggedChip = BuildFilterSegment("Tagged Data", DataFilter.Tagged);

		var segmentGrid = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			],
			ColumnSpacing = 4
		};
		segmentGrid.Children.Add(allChip);
		Grid.SetColumn(allChip, 0);
		segmentGrid.Children.Add(taggedChip);
		Grid.SetColumn(taggedChip, 1);

		var segmentShell = new Border
		{
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			BackgroundColor = ThemeColors.SurfaceSecondary,
			StrokeShape = new RoundRectangle { CornerRadius = 10 },
			Padding = 4,
			HorizontalOptions = LayoutOptions.Fill,
			Content = segmentGrid
		};

		row.Children.Add(segmentShell);

		if (showExport)
		{
			var export = BuildIconActionButton("\u21A7");
			var exportTap = new TapGestureRecognizer();
			exportTap.SetBinding(TapGestureRecognizer.CommandProperty,
				new Binding(nameof(Halo2LogDetailViewModel.ExportLogCommand), source: _viewModel));
			export.GestureRecognizers.Add(exportTap);
			row.Children.Add(export);
			Grid.SetColumn(export, 1);
		}

		return row;
	}

	Border BuildFilterSegment(string text, DataFilter filter)
	{
		var active = _filter == filter;
		var chip = new Border
		{
			Padding = new Thickness(10, 9),
			MinimumHeightRequest = 36,
			BackgroundColor = active ? ThemeColors.Surface : Colors.Transparent,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 8 },
			HorizontalOptions = LayoutOptions.Fill,
			Content = new Label
			{
				Text = text,
				FontSize = 13,
				FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
				TextColor = active ? ThemeColors.OnSurface : ThemeColors.OnSurfaceVariant,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				LineBreakMode = LineBreakMode.NoWrap,
				MaxLines = 1
			}
		};
		var filterTap = new TapGestureRecognizer();
		filterTap.Tapped += (_, _) => SetDataFilter(filter);
		chip.GestureRecognizers.Add(filterTap);
		return chip;
	}

	void SetDataFilter(DataFilter filter)
	{
		if (_filter == filter)
			return;

		_filter = filter;
		RebuildContent();
	}

	static Border BuildIconActionButton(string glyph) =>
		new()
		{
			WidthRequest = 40,
			HeightRequest = 40,
			BackgroundColor = ThemeColors.SurfaceSecondary,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 10 },
			Content = new Label
			{
				Text = glyph,
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = ThemeColors.OnSurfaceVariant
			}
		};

	View BuildTableSection()
	{
		var stack = new VerticalStackLayout { Spacing = 12 };
		stack.Children.Add(BuildDataFilterBar(showExport: true));

		var allRows = Halo2LogDetailSampleData.Rows.ToList();
		var rows = _filter == DataFilter.Tagged
			? allRows.Where(r => r.isTagged).ToList()
			: allRows;

		if (rows.Count == 0)
		{
			stack.Children.Add(BuildEmptyFilterState("No tagged readings in this log."));
			return stack;
		}

		var table = new VerticalStackLayout { Spacing = 0 };
		table.Children.Add(BuildTableHeader());

		for (var i = 0; i < rows.Count; i++)
		{
			var recordNumber = allRows.IndexOf(rows[i]) + 1;
			table.Children.Add(BuildTableRow(rows[i], recordNumber, i % 2 == 1));
		}

		stack.Children.Add(new Border
		{
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			BackgroundColor = ThemeColors.Surface,
			Content = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
				Content = table
			}
		});

		return stack;
	}

	static Border BuildEmptyFilterState(string message) =>
		new()
		{
			Padding = 24,
			BackgroundColor = ThemeColors.SurfaceSecondary,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = new Label
			{
				Text = message,
				FontSize = 14,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = ThemeColors.OnSurfaceVariant
			}
		};

	Grid BuildTableHeader()
	{
		var header = new Grid
		{
			ColumnDefinitions = TableColumns(),
			ColumnSpacing = TableColumnSpacing,
			Padding = TableCellPadding,
			BackgroundColor = Color.FromArgb("#CBD5E1")
		};
		AddHeaderCell(header, "#rec", 0);
		AddHeaderCell(header, "pH", 1);
		AddHeaderCell(header, "mV", 2);
		AddHeaderCell(header, "Temp (°C)", 3);
		AddHeaderCell(header, "Date", 4);
		return header;
	}

	Grid BuildTableRow(Halo2LogTableRow row, int recordNumber, bool stripe)
	{
		var bg = row.isAlert
			? Color.FromArgb("#EF4444")
			: row.isTagged
				? Color.FromArgb("#22C55E")
				: stripe ? ThemeColors.SurfaceSecondary : ThemeColors.Surface;

		var grid = new Grid
		{
			ColumnDefinitions = TableColumns(),
			ColumnSpacing = TableColumnSpacing,
			Padding = TableCellPadding,
			BackgroundColor = bg
		};

		var textColor = row.isAlert || row.isTagged ? Colors.White : ThemeColors.OnSurface;
		AddDataCell(grid, recordNumber.ToString(), 0, textColor);
		AddDataCell(grid, row.Ph, 1, textColor);
		AddDataCell(grid, row.Mv, 2, textColor);
		AddDataCell(grid, row.Temp, 3, textColor);
		AddDataCell(grid, row.Date, 4, textColor, small: true);
		return grid;
	}

	const double TableColumnSpacing = 10;
	static readonly Thickness TableCellPadding = new(10, 8);

	static ColumnDefinitionCollection TableColumns() => new(
		new ColumnDefinition(new GridLength(40, GridUnitType.Absolute)),
		new ColumnDefinition(new GridLength(44, GridUnitType.Absolute)),
		new ColumnDefinition(new GridLength(52, GridUnitType.Absolute)),
		new ColumnDefinition(new GridLength(46, GridUnitType.Absolute)),
		new ColumnDefinition(new GridLength(148, GridUnitType.Absolute)));

	static void AddHeaderCell(Grid grid, string text, int col)
	{
		var label = new Label
		{
			Text = text,
			FontAttributes = FontAttributes.Bold,
			FontSize = 12,
			TextColor = ThemeColors.OnSurface,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};
		grid.Children.Add(label);
		Grid.SetColumn(label, col);
	}

	static void AddDataCell(Grid grid, string text, int col, Color color, bool small = false)
	{
		var label = new Label
		{
			Text = text,
			FontSize = small ? 11 : 12,
			TextColor = color,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation,
			MaxLines = 2
		};
		grid.Children.Add(label);
		Grid.SetColumn(label, col);
	}

	View BuildGraphSection()
	{
		var stack = new VerticalStackLayout { Spacing = 12 };
		stack.Children.Add(BuildDataFilterBar(showExport: false));

		stack.Children.Add(new Border
		{
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			BackgroundColor = ThemeColors.SurfaceSecondary,
			Padding = new Thickness(24, 48),
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Work in progress",
						FontAttributes = FontAttributes.Bold,
						FontSize = 17,
						HorizontalTextAlignment = TextAlignment.Center,
						TextColor = ThemeColors.OnSurface
					},
					new Label
					{
						Text = "Graph view for saved Halo 2 logs will be available in a future update.",
						FontSize = 14,
						HorizontalTextAlignment = TextAlignment.Center,
						LineBreakMode = LineBreakMode.WordWrap,
						TextColor = ThemeColors.OnSurfaceVariant
					}
				}
			}
		});

		return stack;
	}

	View BuildCalibrationSection()
	{
		var header = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			],
			ColumnSpacing = 8,
			Padding = new Thickness(14, 14),
			BackgroundColor = ThemeColors.SurfaceSecondary
		};
		AddCalibrationMetric(header, "Last Calibration:", Halo2CalibrationDemoData.LastCalibrationDisplay, 0);
		AddCalibrationMetric(header, "Offset:", Halo2CalibrationDemoData.OffsetDisplay, 1);
		AddCalibrationMetric(header, "Average Slope:", Halo2CalibrationDemoData.AverageSlopeDisplay, 2);

		var pointsGrid = new Grid
		{
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			],
			RowSpacing = 4,
			Padding = new Thickness(10, 16, 10, 18),
			HorizontalOptions = LayoutOptions.Center
		};

		var points = Halo2CalibrationDemoData.Points;
		var slopes = Halo2CalibrationDemoData.SegmentSlopes;
		var columnDefs = new ColumnDefinitionCollection();
		for (var i = 0; i < points.Count; i++)
		{
			columnDefs.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
			if (i < slopes.Count)
				columnDefs.Add(new ColumnDefinition(GridLength.Auto));
		}

		pointsGrid.ColumnDefinitions = columnDefs;

		var col = 0;
		for (var i = 0; i < points.Count; i++)
		{
			if (i > 0)
			{
				var slope = BuildCalibrationSlopeLabel(slopes[i - 1]);
				pointsGrid.Children.Add(slope);
				Grid.SetColumn(slope, col);
				Grid.SetRow(slope, 0);
				col++;
			}

			var point = BuildCalibrationPointColumn(points[i]);
			pointsGrid.Children.Add(point);
			Grid.SetColumn(point, col);
			Grid.SetRow(point, 1);
			col++;
		}

		return new Border
		{
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			BackgroundColor = ThemeColors.Surface,
			Content = new VerticalStackLayout
			{
				Spacing = 0,
				Children =
				{
					header,
					new BoxView { HeightRequest = 1, Color = ThemeColors.Divider },
					new ScrollView
					{
						Orientation = ScrollOrientation.Horizontal,
						HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
						Content = pointsGrid
					}
				}
			}
		};
	}

	static void AddCalibrationMetric(Grid grid, string caption, string value, int column)
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 4,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = caption,
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.WordWrap
				},
				new Label
				{
					Text = value,
					FontSize = 13,
					FontAttributes = FontAttributes.Bold,
					TextColor = ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.TailTruncation,
					MaxLines = 2
				}
			}
		};
		grid.Children.Add(stack);
		Grid.SetColumn(stack, column);
	}

	static View BuildCalibrationSlopeLabel(string slopePercent) =>
		new VerticalStackLayout
		{
			WidthRequest = 40,
			Spacing = 0,
			VerticalOptions = LayoutOptions.End,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Slope:",
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = slopePercent,
					FontSize = 12,
					FontAttributes = FontAttributes.Bold,
					TextColor = ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};

	static View BuildCalibrationPointColumn(Halo2CalibrationPoint point) =>
		new VerticalStackLayout
		{
			Spacing = 6,
			MinimumWidthRequest = 68,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				Halo2CalibrationUi.BufferBeaker(point.Ph, 54, 42, calibrated: true),
				new Label
				{
					Text = point.Millivolts,
					FontSize = 12,
					TextColor = ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = point.Temperature,
					FontSize = 12,
					TextColor = ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = Halo2CalibrationDemoData.PointDateDisplay,
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = Halo2CalibrationDemoData.PointTimeDisplay,
					FontSize = 11,
					TextColor = ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};
}
