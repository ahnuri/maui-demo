namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Shared visual helpers for the Halo 2 calibration UI.
///
/// <see cref="BufferBeaker"/> renders one calibration-buffer tile (beaker icon + pH label):
///   - <c>calibrated=true</c>  → blue beaker, label visible (this point has been measured)
///   - <c>calibrated=false</c> → grey beaker, label hidden (pending calibration)
///
/// Used both by the calibration summary card on the measure screen and the calibration page.
/// Width thresholds switch font size between hero (>70 → 17pt) and inline (≤70 → 11pt).
/// </summary>
public static class Halo2CalibrationUi
{
	public const string BeakerBlueIcon = "halo2_beaker_blue.png";
	public const string BeakerGreyIcon = "halo2_beaker_grey.png";

	/// <summary>
	/// Renders a beaker tile sized to <paramref name="width"/>×<paramref name="height"/>.
	/// pH label is overlaid centered (slightly offset vertically to land inside the beaker glass).
	/// </summary>
	public static View BufferBeaker(string phValue, double width, double height, bool calibrated = true)
	{
		var source = calibrated ? BeakerBlueIcon : BeakerGreyIcon;
		var showValue = calibrated && !string.IsNullOrWhiteSpace(phValue);

		return new Grid
		{
			WidthRequest = width,
			HeightRequest = height,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Image
				{
					Source = source,
					Aspect = Aspect.AspectFit,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill
				},
				new Label
				{
					Text = showValue ? phValue : string.Empty,
					IsVisible = showValue,
					FontSize = width > 70 ? 17 : 11,
					FontAttributes = FontAttributes.Bold,
					TextColor = Colors.White,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(0, height * 0.14, 0, 0)
				}
			}
		};
	}

}
