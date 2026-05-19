namespace HannaUIDemo.Features.Halo2;

/// <summary>Shared calibration visuals (buffer beaker tiles and icon asset names).</summary>
public static class Halo2CalibrationUi
{
	public const string BeakerBlueIcon = "halo2_beaker_blue.png";
	public const string BeakerGreyIcon = "halo2_beaker_grey.png";
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
