using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Devices;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Shared icons and accent styling for log history device groups.</summary>
public static class LogDeviceVisuals
{
	/// <summary>Brand-aligned accent used across Halo, photometer, and multimeter UI.</summary>
	public static Color Accent => AppConstants.Primary;

	public static Color AccentBackground => Accent.MultiplyAlpha(0.12f);

	// Canonical product-shot icons (registered in csproj with BaseSize 120x120). Centralised in
	// DeviceIconResolver so every surface — picker sheet, home tiles, log-history cards, nav
	// title strips — renders the same artwork.
	public static string IconAsset(InstrumentKind kind) => kind switch
	{
		InstrumentKind.Halo2 => DeviceIconResolver.Halo2Icon,
		InstrumentKind.Photometer => DeviceIconResolver.PhotometerIcon,
		InstrumentKind.Multimeter => DeviceIconResolver.MultimeterIcon,
		_ => "measure_icon"
	};

	public static ImageSource IconSource(InstrumentKind kind) =>
		ImageSource.FromFile(IconAsset(kind));

	/// <summary>Cloud upload icon on log rows — green when uploaded, grey when pending.</summary>
	public static Color CloudUploadedIcon => AppConstants.Success;

	public static Color CloudPendingIcon => ThemeColors.MutedSignalDot;

	// PNG icons used on log-history rows for the cloud-sync status pill.
	// Underscore filenames (not hyphens) because Resizetizer rejects '-' in image asset names.
	public const string SyncedIconAsset = "sync_green.png";
	public const string NotSyncedIconAsset = "sync_gray.png";

	/// <summary>Returns the matching cloud-sync image filename for a session/tank row.</summary>
	public static string CloudSyncIcon(bool isUploadedToCloud) =>
		isUploadedToCloud ? SyncedIconAsset : NotSyncedIconAsset;
}
