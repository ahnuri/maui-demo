using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Core.Devices;

/// <summary>Resolves Maui image asset names and thumb text for instrument rows.</summary>
public static class DeviceIconResolver
{
	public static string? ResolveIcon(InstrumentKind? kind, string name) => kind switch
	{
		InstrumentKind.Photometer => "tab_photometer.png",
		InstrumentKind.Multimeter => "tab_multimeter.png",
		InstrumentKind.Halo2 => "halo2_device_icon.png",
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
			return "halo2_device_icon.png";
		if (n.Contains("97115") || n.Contains("PHOTO") || n.Contains("PT1"))
			return "tab_photometer.png";
		if (n.Contains("98494") || n.Contains("98X") || n.Contains("MULTI"))
			return "tab_multimeter.png";
		return null;
	}
}
