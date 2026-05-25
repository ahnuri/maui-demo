namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Maps a probe-condition percent (0–100) to the matching <c>probe_conditioning_*p.png</c>
/// asset. Eleven images live in <c>Resources/Images/probe_conditions/</c> in 10% steps
/// (0p, 10p, 20p, … 100p) plus @2x variants, registered via <see cref="MauiImage"/> in the csproj.
///
/// Bucketing: percent is rounded to the nearest 10, so 0–4 → 0p, 5–14 → 10p, …, 95–100 → 100p.
/// </summary>
public static class Halo2ProbeConditionIcons
{
	/// <summary>Returns the asset name for the given percent, e.g. 94 → "probe_conditioning_90p.png".</summary>
	public static string ImageForPercent(int percent)
	{
		var bucket = (int)Math.Round(Math.Clamp(percent, 0, 100) / 10.0) * 10;
		return $"probe_conditioning_{bucket}p.png";
	}

	/// <summary>
	/// Builds an <see cref="Image"/> bound to the bucketed asset. Defaults (18×22) suit the
	/// inline metric row; pass larger size for the device-card hero (~32×80).
	/// </summary>
	public static Image CreateGlyph(int percent, double width = 18, double height = 22) => new()
	{
		Source = ImageForPercent(percent),
		WidthRequest = width,
		HeightRequest = height,
		Aspect = Aspect.AspectFit,
		VerticalOptions = LayoutOptions.Center
	};
}
