using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Helpers;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Theme;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace HannaUIDemo.Features.Instruments.Multimeter;

/// <summary>HI98494 / HI98594 log recall — list, LOT/LOD filters, download and share.</summary>
public partial class MultimeterLogRecallView : ContentView
{
	readonly MultimeterLogRecallViewModel _viewModel;
	readonly LocalizationService _loc;

	public MultimeterLogRecallView()
	{
		_viewModel = AppServices.Get<MultimeterLogRecallViewModel>();
		_loc = _viewModel.Loc;
		BindingContext = _viewModel;
		InitializeComponent();
		_viewModel.LogsChanged += (_, _) => Rebuild();
		_viewModel.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName is nameof(MultimeterLogRecallViewModel.ActiveFilter)
			    or nameof(MultimeterLogRecallViewModel.IsSyncing)
			    or nameof(MultimeterLogRecallViewModel.TotalLogs)
			    or nameof(MultimeterLogRecallViewModel.LotLogCount)
			    or nameof(MultimeterLogRecallViewModel.LodLogCount))
				Rebuild();
		};
		Rebuild();
	}

	public void ApplyTheme() => _viewModel.RefreshForTheme();

	void Rebuild()
	{
		Root.Children.Clear();
		Root.Children.Add(BuildSummaryCard());
		Root.Children.Add(new BoxView { HeightRequest = 14 });
		Root.Children.Add(BuildFilterRow());
		Root.Children.Add(new BoxView { HeightRequest = 16 });
		Root.Children.Add(BuildSectionHeader());

		var logs = _viewModel.VisibleLogs.ToList();
		if (logs.Count == 0)
		{
			Root.Children.Add(BuildEmptyState());
			return;
		}

		foreach (var log in logs)
		{
			Root.Children.Add(new BoxView { HeightRequest = 10 });
			Root.Children.Add(BuildLogCard(log));
		}
	}

	Border BuildSummaryCard()
	{
		var stats = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 8
		};
		var totalCell = BuildStatCell(_viewModel.TotalLogs.ToString(), _loc.T("Multimeter_LogRecall_StatsTotal"));
		stats.Children.Add(totalCell);
		Grid.SetColumn(totalCell, 0);
		var lotCell = BuildStatCell(_viewModel.LotLogCount.ToString(), _loc.T("Multimeter_LogRecall_StatsLot"), AppConstants.Primary);
		stats.Children.Add(lotCell);
		Grid.SetColumn(lotCell, 1);
		var lodCell = BuildStatCell(_viewModel.LodLogCount.ToString(), _loc.T("Multimeter_LogRecall_StatsLod"), MultimeterLogItemViewModel.LodAccent);
		stats.Children.Add(lodCell);
		Grid.SetColumn(lodCell, 2);

		// Compact "Sync from meter" chip — smaller padding/font/corner so it doesn't
		// dominate the log-recall card header.
		var syncBtn = new Border
		{
			Padding = new Thickness(10, 6),
			Stroke = AppConstants.Primary,
			StrokeThickness = 1.2,
			BackgroundColor = Colors.Transparent,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};
		var syncInner = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
		if (_viewModel.IsSyncing)
		{
			syncInner.Children.Add(new ActivityIndicator
			{
				IsRunning = true,
				Color = AppConstants.Primary,
				WidthRequest = 14,
				HeightRequest = 14,
				VerticalOptions = LayoutOptions.Center
			});
		}
		else
		{
			syncInner.Children.Add(new Label
			{
				Text = "\u21BB",
				FontSize = 14,
				TextColor = AppConstants.Primary,
				VerticalOptions = LayoutOptions.Center
			});
		}

		syncInner.Children.Add(new Label
		{
			Text = _viewModel.IsSyncing ? _loc.T("Multimeter_LogRecall_Syncing") : _loc.T("Multimeter_LogRecall_SyncButton"),
			FontAttributes = FontAttributes.Bold,
			FontSize = 12,
			TextColor = AppConstants.Primary,
			VerticalOptions = LayoutOptions.Center
		});
		syncBtn.Content = syncInner;
		var syncTap = new TapGestureRecognizer();
		syncTap.Tapped += async (_, _) => await _viewModel.SyncLogsCommand.ExecuteAsync(null);
		syncBtn.GestureRecognizers.Add(syncTap);
		SemanticProperties.SetDescription(syncBtn, _loc.T("Multimeter_LogRecall_SyncHint"));

		var header = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			Margin = new Thickness(0, 0, 0, 14)
		};
		var titleCol = new VerticalStackLayout { Spacing = 4 };
		titleCol.Children.Add(new Label
		{
			Text = _loc.T("Multimeter_LogRecall_Title"),
			FontSize = 22,
			FontAttributes = FontAttributes.Bold,
			TextColor = ThemeColors.OnSurface
		});
		titleCol.Children.Add(new Label
		{
			Text = _loc.T("Multimeter_LogRecall_Description"),
			FontSize = 13,
			TextColor = ThemeColors.OnSurfaceVariant,
			LineBreakMode = LineBreakMode.WordWrap
		});
		header.Children.Add(titleCol);
		header.Children.Add(syncBtn);
		Grid.SetColumn(syncBtn, 1);

		var body = new VerticalStackLayout { Spacing = 0, Padding = 18 };
		body.Children.Add(header);
		body.Children.Add(stats);

		return new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = AppConstants.RadiusCard },
			Content = body,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 3),
				Radius = 12,
				Opacity = 1
			}
		};
	}

	static View BuildStatCell(string value, string label, Color? accent = null) =>
		new VerticalStackLayout
		{
			Spacing = 2,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = value,
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					TextColor = accent ?? ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = label,
					FontSize = 11,
					FontAttributes = FontAttributes.Bold,
					TextColor = accent?.MultiplyAlpha(0.85f) ?? ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};

	Grid BuildFilterRow()
	{
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)),
			ColumnSpacing = 10
		};
		var allChip = BuildFilterChip(_loc.T("Multimeter_LogRecall_FilterAll"), "All", MultimeterLogFilter.All, _viewModel.TotalLogs, AppConstants.Primary);
		grid.Children.Add(allChip);
		Grid.SetColumn(allChip, 0);
		var lotChip = BuildFilterChip(_loc.T("Multimeter_LogRecall_StatsLot"), "LOT", MultimeterLogFilter.Lot, _viewModel.LotLogCount, AppConstants.Primary);
		grid.Children.Add(lotChip);
		Grid.SetColumn(lotChip, 1);
		var lodChip = BuildFilterChip(_loc.T("Multimeter_LogRecall_StatsLod"), "LOD", MultimeterLogFilter.Lod, _viewModel.LodLogCount, MultimeterLogItemViewModel.LodAccent);
		grid.Children.Add(lodChip);
		Grid.SetColumn(lodChip, 2);
		return grid;
	}

	Border BuildFilterChip(string label, MultimeterLogFilter filter, int count, Color accent) =>
		BuildFilterChip(label, label, filter, count, accent);

	Border BuildFilterChip(string label, string filterId, MultimeterLogFilter filter, int count, Color accent)
	{
		var selected = _viewModel.ActiveFilter == filter;
		var chip = new Border
		{
			Padding = new Thickness(12, 10),
			BackgroundColor = selected ? accent.MultiplyAlpha(0.12f) : ThemeColors.SurfaceSecondary,
			Stroke = selected ? accent : ThemeColors.Divider,
			StrokeThickness = selected ? 1.5 : 1,
			StrokeShape = new RoundRectangle { CornerRadius = 12 }
		};
		chip.Content = new VerticalStackLayout
		{
			Spacing = 2,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = label,
					FontAttributes = FontAttributes.Bold,
					FontSize = 14,
					TextColor = selected ? accent : ThemeColors.OnSurface,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = _loc.T("Multimeter_LogRecall_FileCountFormat", count),
					FontSize = 11,
					TextColor = selected ? accent.MultiplyAlpha(0.8f) : ThemeColors.OnSurfaceVariant,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => _viewModel.SetFilterCommand.Execute(filterId);
		chip.GestureRecognizers.Add(tap);
		return chip;
	}

	Label BuildSectionHeader() =>
		new()
		{
			Text = _loc.T("Multimeter_LogRecall_SectionTitle"),
			FontAttributes = FontAttributes.Bold,
			FontSize = 16,
			TextColor = ThemeColors.OnSurface,
			Margin = new Thickness(2, 0, 0, 4)
		};

	View BuildEmptyState() =>
		new Border
		{
			Margin = new Thickness(0, 24, 0, 0),
			Padding = 24,
			BackgroundColor = ThemeColors.SurfaceSecondary,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = new Label
			{
				Text = _loc.T("Multimeter_LogRecall_EmptyHint"),
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = ThemeColors.OnSurfaceVariant,
				FontSize = 14
			}
		};

	Border BuildLogCard(MultimeterLogItemViewModel log)
	{
		var typeBadge = new Border
		{
			WidthRequest = 44,
			Padding = new Thickness(0, 4),
			BackgroundColor = log.FileTypeBackground,
			Stroke = log.FileTypeAccent.MultiplyAlpha(0.25f),
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 8 },
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Start,
			Content = new Label
			{
				Text = log.FileTypeLabel,
				FontSize = 11,
				FontAttributes = FontAttributes.Bold,
				TextColor = log.FileTypeForeground,
				HorizontalTextAlignment = TextAlignment.Center
			}
		};

		var status = new Label
		{
			Text = log.StatusGlyph,
			FontSize = 16,
			TextColor = log.StatusColor,
			VerticalOptions = LayoutOptions.Center
		};

		var titleRow = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Auto)),
			ColumnSpacing = 10
		};
		titleRow.Children.Add(typeBadge);
		var titleLbl = new Label
		{
			Text = log.Title,
			FontAttributes = FontAttributes.Bold,
			FontSize = 16,
			TextColor = ThemeColors.OnSurface,
			LineBreakMode = LineBreakMode.TailTruncation,
			MaxLines = 1,
			VerticalOptions = LayoutOptions.Center
		};
		titleRow.Children.Add(titleLbl);
		Grid.SetColumn(titleLbl, 1);
		titleRow.Children.Add(status);
		Grid.SetColumn(status, 2);
		var chevron = new Label
		{
			Text = "\u203A",
			FontSize = 22,
			TextColor = ThemeColors.OnSurfaceVariant,
			VerticalOptions = LayoutOptions.Center
		};
		titleRow.Children.Add(chevron);
		Grid.SetColumn(chevron, 3);

		var metaGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection(
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)),
			Margin = new Thickness(0, 8, 0, 0)
		};
		var dates = new VerticalStackLayout { Spacing = 4 };
		dates.Children.Add(new Label
		{
			Text = _loc.T("Multimeter_LogRecall_StartFormat", log.StartRecorded),
			FontSize = 12,
			TextColor = ThemeColors.OnSurfaceVariant
		});
		dates.Children.Add(new Label
		{
			Text = _loc.T("Multimeter_LogRecall_StopFormat", log.StopRecorded),
			FontSize = 12,
			TextColor = ThemeColors.OnSurfaceVariant
		});
		metaGrid.Children.Add(dates);
		var countLbl = new Label
		{
			Text = _loc.T("Multimeter_LogRecall_RecordCountFormat", log.RecordCount),
			FontAttributes = FontAttributes.Bold,
			FontSize = 13,
			TextColor = ThemeColors.OnSurface,
			VerticalOptions = LayoutOptions.End
		};
		metaGrid.Children.Add(countLbl);
		Grid.SetColumn(countLbl, 1);

		var paramWrap = new FlexLayout
		{
			Wrap = FlexWrap.Wrap,
			Direction = FlexDirection.Row,
			AlignItems = FlexAlignItems.Start,
			Margin = new Thickness(0, 10, 0, 0)
		};
		foreach (var param in log.Parameters)
		{
			paramWrap.Children.Add(new Border
			{
				Margin = new Thickness(0, 0, 6, 6),
				Padding = new Thickness(8, 4),
				BackgroundColor = ThemeColors.SurfaceSecondary,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 8 },
				Content = new Label
				{
					Text = param.Display,
					FontSize = 11,
					TextColor = ThemeColors.OnSurface
				}
			});
		}

		var stack = new VerticalStackLayout { Spacing = 0, Padding = 16 };
		stack.Children.Add(titleRow);
		stack.Children.Add(metaGrid);
		stack.Children.Add(new Label
		{
			Text = _loc.T("Multimeter_LogRecall_ParametersLabel"),
			FontSize = 11,
			FontAttributes = FontAttributes.Bold,
			TextColor = ThemeColors.OnSurfaceMuted,
			Margin = new Thickness(0, 10, 0, 4)
		});
		stack.Children.Add(paramWrap);

		if (log.IsDownloaded)
		{
			stack.Children.Add(new Label
			{
				Text = _loc.T("Multimeter_LogRecall_DownloadedBadge"),
				FontSize = 11,
				TextColor = AppConstants.Success,
				Margin = new Thickness(0, 10, 0, 0)
			});
		}

		var card = new Border
		{
			BackgroundColor = ThemeColors.Surface,
			Stroke = ThemeColors.Divider,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = AppConstants.RadiusCardSmall },
			Content = stack,
			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(ThemeColors.SoftShadow),
				Offset = new Point(0, 2),
				Radius = 8,
				Opacity = 1
			}
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await OnLogTappedAsync(log);
		card.GestureRecognizers.Add(tap);
		SemanticProperties.SetDescription(card, _loc.T("Multimeter_LogRecall_LogCardHint", log.Title));
		return card;
	}

	async Task OnLogTappedAsync(MultimeterLogItemViewModel log)
	{
		var page = ViewNavigation.FindHostPage(this);
		if (page is null)
			return;

		var downloadLabel = _loc.T("Multimeter_LogRecall_ActionDownload");
		var shareLabel = _loc.T("Multimeter_LogRecall_ActionShare");
		var action = await page.DisplayActionSheetAsync(
			log.Title,
			_loc.T("Common_Cancel"),
			null,
			downloadLabel,
			shareLabel);

		if (string.Equals(action, downloadLabel, StringComparison.Ordinal))
			await _viewModel.DownloadLogCommand.ExecuteAsync(log);
		else if (string.Equals(action, shareLabel, StringComparison.Ordinal))
			await _viewModel.ShareLogCommand.ExecuteAsync(log);
	}
}
