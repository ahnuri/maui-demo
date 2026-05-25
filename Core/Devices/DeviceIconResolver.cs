using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Core.Devices;

/// <summary>
/// Resolves Maui image asset names and thumb text for instrument rows.
/// Single source of truth for the three canonical product-shot icons:
///   • Photometer  → <see cref="PhotometerIcon"/>
///   • Multimeter  → <see cref="MultimeterIcon"/>
///   • Halo 2      → <see cref="Halo2Icon"/>
/// </summary>
public static class DeviceIconResolver
{
	public const string PhotometerIcon = "photometer_hi97115.png";
	public const string MultimeterIcon = "hi98494_multimeter_icon.png";
	public const string Halo2Icon = "halo2_device_icon.png";

	public static string? ResolveIcon(InstrumentKind? kind, string name) => kind switch
	{
		InstrumentKind.Photometer => PhotometerIcon,
		InstrumentKind.Multimeter => MultimeterIcon,
		InstrumentKind.Halo2 => Halo2Icon,
		_ => InferIcon(name)
	};

	public static string? ResolveThumb(InstrumentKind? kind, string name)
	{
		if (kind is not null || InferIcon(name) is not null)
			return null;

		return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
	}

	static string? InferIcon(string name)
	{
		var n = name.ToUpperInvariant();
		if (n.Contains("HALO"))
			return Halo2Icon;
		if (n.Contains("97115") || n.Contains("PHOTO") || n.Contains("PT1"))
			return PhotometerIcon;
		if (n.Contains("98494") || n.Contains("98X") || n.Contains("MULTI"))
			return MultimeterIcon;
		return null;
	}
}
