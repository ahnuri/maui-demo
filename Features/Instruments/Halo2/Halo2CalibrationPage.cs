using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>Modern Halo 2 five-point pH calibration flow.</summary>
public sealed class Halo2CalibrationPage : ContentPage
{
	readonly LocalizationService _loc;
	readonly string _deviceName;
	readonly string _currentBuffer;

	static Color LabCanvas => ThemeColors.LabCanvas;
	static Color LabCard => ThemeColors.LabCard;
	static Color LabCardElevated => ThemeColors.LabCardElevated;
	static Color LabBorder => ThemeColors.LabBorder;
	static Color LabMuted => ThemeColors.LabMuted;
	static Color LabPrimaryText => ThemeColors.LabPrimaryText;
	static Color LabSecondaryText => ThemeColors.LabSecondaryText;
	static Color LabEmerald => ThemeColors.LabEmerald;
	static Color LabChipDisabled => ThemeColors.LabChipDisabled;

	static readonly CalibrationPoint[] Points =
	[
		new(Halo2CalibrationDemoData.Points[0], true),
		new(Halo2CalibrationDemoData.Points[1], true),
		new(Halo2CalibrationDemoData.Points[2], true),
		new(Halo2CalibrationDemoData.Points[3], true),
		new(Halo2CalibrationDemoData.Points[4], false)
	];

	readonly List<CalibrationSlot> _slots = [];
	readonly Label _stepEyebrow;
	readonly Label _stepTitle;
	readonly Label _stepBody;
	readonly Label _readingStatus;
	readonly Button _confirmButton;
	Button _saveButton = null!;

	bool _confirmed;

	public Halo2CalibrationPage(Halo2CalibrationViewModel viewModel)
	{
		BindingContext = viewModel;
		_loc = viewModel.Loc;
		_deviceName = _loc.T("Halo_Calibration_DeviceNameDemo");
		_currentBuffer = _loc.T("Halo_Calibration_CurrentBufferValue");
		Title = _loc.T("Halo_Calibration_PageTitle");
		ApplyChrome();
		Halo2Routes.ConfigureSubPageChrome(this);
		Shell.SetNavBarIsVisible(this, false);

		_stepEyebrow = LabelText(_loc.T("Halo_Calibration_BufferRecognized"), 12, LabMuted, bold: true);
		_stepTitle = LabelText(_loc.T("Halo_Calibration_CalibratingWithBufferFormat", _currentBuffer), 24, LabPrimaryText, bold: true, lineBreak: LineBreakMode.WordWrap);
		_stepBody = LabelText(_loc.T("Halo_Calibration_StableHint"), 15, LabSecondaryText, lineBreak: LineBreakMode.WordWrap);
		_readingStatus = LabelText(_loc.T("Halo_Stability_Stable"), 13, LabEmerald, bold: true);

		_confirmButton = PrimaryButton(_loc.T("Halo_Calibration_ConfirmBuffer"), AppConstants.Primary);
		_confirmButton.Clicked += (_, _) => SetConfirmed(true);

		Content = BuildLayout();
		SetConfirmed(false);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ApplyChrome();
	}

	public void ApplyTheme() => ApplyChrome();

	void ApplyChrome()
	{
		BackgroundColor = LabCanvas;
		ShellChrome.ApplyLab(this);
	}

	View BuildLayout()
	{
		var root = new Grid
		{
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			],
			BackgroundColor = LabCanvas
		};

		var topBar = BuildTopBar();
		root.Children.Add(topBar);
		root.SetRow(topBar, 0);

		var body = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16, 14, 16, 28),
				Spacing = 16,
				Children =
				{
					BuildReadingCard(),
					BuildStepCard(),
					BuildCalibrationCard()
				}
			}
		};
		root.Children.Add(body);
		root.SetRow(body, 1);

		return root;
	}

	Grid BuildTopBar()
	{
		var bar = new Grid
		{
			HeightRequest = 58,
			Padding = new Thickness(16, 0),
			BackgroundColor = LabCard,
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			]
		};

		var cancel = LabelText(_loc.T("Common_Cancel"), 17, AppConstants.Primary);
		cancel.GestureRecognizers.Add(Tap(async () => await Shell.Current.GoToAsync("..")));
		bar.Children.Add(cancel);

		var title = LabelText(_loc.T("Halo_Calibration_PageTitle"), 20, LabPrimaryText, bold: true, horizontal: TextAlignment.Center);
		bar.Children.Add(title);
		bar.SetColumn(title, 1);

		_saveButton = PrimaryTextButton(_loc.T("Common_Save"));
		_saveButton.Clicked += async (_, _) => await Shell.Current.GoToAsync("..");
		bar.Children.Add(_saveButton);
		bar.SetColumn(_saveButton, 2);

		return bar;
	}

	Border BuildReadingCard()
	{
		var deviceRow = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)
			],
			ColumnSpacing = 12
		};

		deviceRow.Children.Add(new Border
		{
			WidthRequest = 48,
			HeightRequest = 48,
			Padding = 6,
			BackgroundColor = ThemeColors.PrimarySubtleFill,
			Stroke = ThemeColors.PrimarySubtleStroke,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = new Image { Source = "halo2_device_icon.png", Aspect = Aspect.AspectFit }
		});

		var deviceText = new VerticalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				LabelText(_deviceName, 18, LabPrimaryText, bold: true, lineBreak: LineBreakMode.TailTruncation),
				LabelText(_loc.T("Halo_Calibration_DeviceBatteryLine"), 13, LabMuted, lineBreak: LineBreakMode.TailTruncation)
			}
		};
		deviceRow.Children.Add(deviceText);
		deviceRow.SetColumn(deviceText, 1);

		var status = StatusPill(_loc.T("Halo_Calibration_Connected"), AppConstants.Success);
		deviceRow.Children.Add(status);
		deviceRow.SetColumn(status, 2);

		var readingGrid = new Grid
		{
			Margin = new Thickness(0, 18, 0, 0),
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			],
			ColumnSpacing = 12
		};

		readingGrid.Children.Add(ReadingTile(_loc.T("Halo_Mode_Ph"), _currentBuffer, _readingStatus));
		var tempTile = ReadingTile(_loc.T("Halo_Calibration_TemperatureLabel"), _loc.T("Halo_Calibration_TempSampleC"), LabelText(_loc.T("Halo_Settings_Atc"), 13, LabMuted, bold: true));
		readingGrid.Children.Add(tempTile);
		readingGrid.SetColumn(tempTile, 1);

		return Card(new VerticalStackLayout
		{
			Spacing = 0,
			Children = { deviceRow, readingGrid }
		});
	}

	Border BuildStepCard()
	{
		var content = new VerticalStackLayout { Spacing = 18 };

		var stepHeader = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)
			],
			ColumnSpacing = 14
		};

		stepHeader.Children.Add(new VerticalStackLayout
		{
			Spacing = 8,
			Children = { _stepEyebrow, _stepTitle, _stepBody }
		});

		var beaker = Halo2CalibrationUi.BufferBeaker(_currentBuffer, 82, 64, calibrated: true);
		stepHeader.Children.Add(beaker);
		stepHeader.SetColumn(beaker, 1);

		content.Children.Add(stepHeader);
		content.Children.Add(BuildProgressRail());

		var actions = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			],
			ColumnSpacing = 12
		};

		var clear = SecondaryButton(_loc.T("Halo_Calibration_ClearCalibration"));
		clear.Clicked += (_, _) => SetConfirmed(false);
		actions.Children.Add(clear);

		actions.Children.Add(_confirmButton);
		actions.SetColumn(_confirmButton, 1);
		content.Children.Add(actions);

		return Card(content);
	}

	View BuildProgressRail()
	{
		var row = new HorizontalStackLayout
		{
			Spacing = 6,
			HorizontalOptions = LayoutOptions.Fill
		};

		foreach (var point in Points)
		{
			var complete = point.IsSaved;
			row.Children.Add(new Border
			{
				HeightRequest = 8,
				WidthRequest = 44,
				BackgroundColor = complete ? AppConstants.Success : point.Buffer == _currentBuffer ? AppConstants.Primary : LabBorder,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 4 }
			});
		}

		return row;
	}

	Border BuildCalibrationCard()
	{
		var rail = new HorizontalStackLayout { Spacing = 10 };
		foreach (var point in Points)
		{
			var slot = BuildSlot(point);
			_slots.Add(slot);
			rail.Children.Add(slot.Root);
		}

		var content = new VerticalStackLayout
		{
			Spacing = 14,
			Children =
			{
				new Grid
				{
					ColumnDefinitions =
					[
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Auto)
					],
					Children =
					{
						LabelText(_loc.T("Halo_Calibration_CurrentCalibration"), 20, LabPrimaryText, bold: true),
						StatusPill(_loc.T("Halo_Calibration_PointsCountFormat", _loc.T("Halo_Calibration_PointsValue")), AppConstants.Primary)
					}
				},
				new ScrollView
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
					Content = rail
				}
			}
		};
		if (content.Children[0] is Grid header && header.Children.Count > 1)
			header.SetColumn(header.Children[1], 1);

		return Card(content);
	}

	CalibrationSlot BuildSlot(CalibrationPoint point)
	{
		var valueLabel = LabelText(string.Empty, 12, LabMuted, horizontal: TextAlignment.Center, lineBreak: LineBreakMode.WordWrap);
		var root = new Border
		{
			WidthRequest = 112,
			MinimumHeightRequest = 150,
			Padding = new Thickness(10),
			BackgroundColor = LabCardElevated,
			Stroke = LabBorder,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = new VerticalStackLayout
			{
				Spacing = 8,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					Halo2CalibrationUi.BufferBeaker(point.Buffer, 54, 42, point.IsSaved),
					valueLabel
				}
			}
		};

		var slot = new CalibrationSlot(root, valueLabel, point);
		ApplySlot(slot, point.IsSaved);
		return slot;
	}

	void SetConfirmed(bool confirmed)
	{
		_confirmed = confirmed;

		_stepEyebrow.Text = confirmed
			? _loc.T("Halo_Calibration_CompleteHeader")
			: _loc.T("Halo_Calibration_BufferRecognized");
		_stepEyebrow.TextColor = confirmed ? LabEmerald : LabMuted;
		_stepTitle.Text = confirmed
			? _loc.T("Halo_Calibration_Complete")
			: _loc.T("Halo_Calibration_CalibratingWithBufferFormat", _currentBuffer);
		_stepBody.Text = confirmed
			? _loc.T("Halo_Calibration_CompleteMessage")
			: _loc.T("Halo_Calibration_StableHint");

		_confirmButton.IsEnabled = !confirmed;
		_confirmButton.BackgroundColor = confirmed ? LabChipDisabled : AppConstants.Primary;
		_confirmButton.TextColor = confirmed ? LabMuted : Colors.White;
		_saveButton.IsEnabled = confirmed;
		_saveButton.TextColor = confirmed ? AppConstants.Primary : LabMuted;

		foreach (var slot in _slots)
			ApplySlot(slot, slot.Point.IsSaved || confirmed && slot.Point.Buffer == _currentBuffer);
	}

	void ApplySlot(CalibrationSlot slot, bool saved)
	{
		slot.Root.BackgroundColor = saved ? LabEmerald.MultiplyAlpha(0.08f) : LabCardElevated;
		slot.Root.Stroke = saved ? LabEmerald.MultiplyAlpha(0.42f) : LabBorder;
		slot.ValueLabel.TextColor = saved ? LabPrimaryText : LabMuted;
		slot.ValueLabel.Text = saved
			? $"{slot.Point.Millivolts}\n{slot.Point.Temperature}\n{Halo2CalibrationDemoData.PointDateDisplay}\n{Halo2CalibrationDemoData.PointTimeDisplay}"
			: _loc.T("Halo_Calibration_Empty");

		if (slot.Root.Content is VerticalStackLayout stack && stack.Children.Count > 0)
			stack.Children[0] = Halo2CalibrationUi.BufferBeaker(slot.Point.Buffer, 54, 42, saved);
	}

	static Border ReadingTile(string label, string value, View meta) => new()
	{
		Padding = new Thickness(14, 12),
		BackgroundColor = LabCardElevated,
		Stroke = LabBorder,
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 14 },
		Content = new VerticalStackLayout
		{
			Spacing = 8,
			Children =
			{
				LabelText(label, 12, LabMuted, bold: true),
				LabelText(value, 31, LabPrimaryText, lineBreak: LineBreakMode.TailTruncation),
				meta
			}
		}
	};

	static Border StatusPill(string text, Color color) => new()
	{
		Padding = new Thickness(9, 4),
		BackgroundColor = color.MultiplyAlpha(0.12f),
		Stroke = color.MultiplyAlpha(0.25f),
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 10 },
		VerticalOptions = LayoutOptions.Center,
		Content = LabelText(text, 10, color, bold: true, horizontal: TextAlignment.Center)
	};

	static Border Card(View content) => new()
	{
		Padding = new Thickness(16),
		BackgroundColor = LabCard,
		Stroke = LabBorder,
		StrokeThickness = 1,
		StrokeShape = new RoundRectangle { CornerRadius = 18 },
		Shadow = new Shadow { Brush = new SolidColorBrush(ThemeColors.SoftShadow), Offset = new Point(0, 2), Radius = 10, Opacity = 1 },
		Content = content
	};

	static Button PrimaryButton(string text, Color color) => new()
	{
		Text = text,
		HeightRequest = 48,
		CornerRadius = 14,
		FontAttributes = FontAttributes.Bold,
		FontSize = 15,
		BackgroundColor = color,
		TextColor = Colors.White
	};

	static Button SecondaryButton(string text) => new()
	{
		Text = text,
		HeightRequest = 48,
		CornerRadius = 14,
		FontAttributes = FontAttributes.Bold,
		FontSize = 15,
		BackgroundColor = LabCardElevated,
		TextColor = AppConstants.Primary,
		BorderColor = LabBorder,
		BorderWidth = 1
	};

	static Button PrimaryTextButton(string text) => new()
	{
		Text = text,
		BackgroundColor = Colors.Transparent,
		TextColor = AppConstants.Primary,
		FontSize = 17,
		HorizontalOptions = LayoutOptions.End,
		VerticalOptions = LayoutOptions.Center,
		Padding = new Thickness(0)
	};

	static Label LabelText(
		string text,
		double size,
		Color color,
		bool bold = false,
		TextAlignment horizontal = TextAlignment.Start,
		LineBreakMode lineBreak = LineBreakMode.NoWrap) => new()
	{
		Text = text,
		FontSize = size,
		TextColor = color,
		FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
		HorizontalTextAlignment = horizontal,
		VerticalTextAlignment = TextAlignment.Center,
		LineBreakMode = lineBreak,
		MaxLines = lineBreak == LineBreakMode.NoWrap ? 1 : int.MaxValue
	};

	static TapGestureRecognizer Tap(Func<Task> action)
	{
		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await action();
		return tap;
	}

	readonly record struct CalibrationPoint(Halo2CalibrationPoint Data, bool IsSaved)
	{
		public string Buffer => Data.Ph;
		public string Millivolts => Data.Millivolts;
		public string Temperature => Data.Temperature;
	}

	sealed record CalibrationSlot(Border Root, Label ValueLabel, CalibrationPoint Point);
}
