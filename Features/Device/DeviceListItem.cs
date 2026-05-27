using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Devices;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Device;

/// <summary>
/// One instrument row in the Devices list. Observable chrome updates when connection or theme changes.
/// </summary>
public partial class DeviceListItem : ObservableObject
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required string Id { get; init; }
	public required string Name { get; init; }
	public string? Serial { get; init; }
	public string? Firmware { get; init; }
	public int? BatteryPercent { get; init; }
	public string? LastSeen { get; init; }
	/// <summary>Maui image file name (e.g. tab_halo.png).</summary>
	public string? DeviceIcon { get; init; }
	public string? ThumbText { get; init; }
	/// <summary>Localization key for the signal strength label (e.g. "Device_Signal_Strong").</summary>
	public string SignalTextKey { get; init; } = "Device_Signal_Strong";
	public string SignalText => Loc.T(SignalTextKey);
	public bool IsStrongSignal { get; init; }
	public InstrumentKind? InstrumentKind { get; init; }

	[ObservableProperty] private bool _isConnected;

	[ObservableProperty] private Color _signalDotColor = Colors.Transparent;
	[ObservableProperty] private Color _signalTextColor = Colors.Transparent;
	[ObservableProperty] private Color _rowStrokeColor = Colors.Transparent;
	[ObservableProperty] private Color _connectionButtonBg = Colors.Transparent;
	[ObservableProperty] private Color _connectionButtonTextColor = Colors.Transparent;
	[ObservableProperty] private Color _connectionButtonBorderColor = Colors.Transparent;

	public bool ShowDeviceIcon => !string.IsNullOrEmpty(DeviceIcon);
	public bool ShowThumb => !ShowDeviceIcon && !string.IsNullOrWhiteSpace(ThumbText);
	public bool CanOpenMeasure => IsConnected && InstrumentKind is not null;
	public bool ShowMeasureLink => CanOpenMeasure;
	public bool ShowBattery => BatteryPercent is not null;

	public string SignalStatusText => IsConnected
		? Loc.T("Device_SignalConnectedFormat", SignalText)
		: SignalText;

	/// <summary>Plain serial number for the device row.</summary>
	public string SerialNumber => Serial ?? string.Empty;

	public bool ShowSerialLine => !string.IsNullOrWhiteSpace(SerialNumber);

	/// <summary>Firmware and last-active time on one subtle line (connected devices).</summary>
	public string SecondaryLine
	{
		get
		{
			if (!IsConnected)
				return string.Empty;

			var parts = new List<string>(2);
			if (!string.IsNullOrWhiteSpace(Firmware))
				parts.Add($"v{Firmware}");
			if (!string.IsNullOrWhiteSpace(LastSeen))
				parts.Add(LastSeen);
			return parts.Count > 0 ? string.Join(" · ", parts) : string.Empty;
		}
	}

	public bool ShowSecondaryLine => !string.IsNullOrEmpty(SecondaryLine);

	public string BatteryPillText => BatteryPercent is { } pct ? Loc.T("Common_PercentFormat", pct) : string.Empty;

	// ── Battery glyph (iOS-style outlined battery with inner fill) ─────────
	//
	// The battery icon in the connected row renders as:
	//   ┌────────┐▌
	//   │  78%   │
	//   └────────┘
	// The body is a fixed-width Border; an inner Border (anchored left, dynamic
	// width) acts as the fill bar; a small Border on the right is the battery
	// tip. The percentage text is overlaid on top of the fill at full opacity
	// so it stays legible regardless of how much of the battery is "full".

	/// <summary>Usable inner width of the battery body in pt (matches the XAML — 26pt inner area).</summary>
	const double BatteryBodyInnerWidth = 24;

	/// <summary>Width of the inner fill bar in pt, derived from <see cref="BatteryPercent"/>.</summary>
	public double BatteryFillWidth =>
		BatteryPercent is { } pct ? Math.Max(2, pct * BatteryBodyInnerWidth / 100.0) : 0;

	/// <summary>Accent color for the outline, tip, and percentage label.</summary>
	public Color BatteryAccentColor =>
		BatteryPercent is { } pct ? BatteryColorForLevel(pct) : ThemeColors.Divider;

	/// <summary>Soft fill behind the percentage label (accent at ~22% alpha).</summary>
	public Color BatteryFillColor => BatteryAccentColor.MultiplyAlpha(0.22f);

	/// <summary>Threshold buckets: ≥60% green, 25-59% amber, &lt;25% red.</summary>
	static Color BatteryColorForLevel(int pct)
	{
		if (pct >= 60)
			return AppConstants.Success;
		if (pct >= 25)
			return ThemeColors.LabWarning;
		return AppConstants.Error;
	}

	public string ConnectionButtonText => IsConnected
		? Loc.T("Device_Action_Disconnect")
		: Loc.T("Device_Action_Connect");

	partial void OnIsConnectedChanged(bool value) => RefreshChrome();

	public void RefreshChrome()
	{
		// Signal indicator: only the dot carries the strength color so the
		// status reads at a glance, while the label itself stays as standard
		// body text (OnSurface ≈ black in light mode, near-white in dark).
		//   Strong → success green dot (#22C55E)
		//   Low/Weak → warning amber dot (#FBBF24)
		SignalDotColor = IsStrongSignal ? AppConstants.Success : ThemeColors.LabWarning;
		SignalTextColor = ThemeColors.OnSurface;
		RowStrokeColor = IsConnected
			? AppConstants.Success.MultiplyAlpha(0.28f)
			: Colors.Transparent;

		if (IsConnected)
		{
			ConnectionButtonBg = Colors.Transparent;
			ConnectionButtonTextColor = ThemeColors.OnSurfaceVariant;
			ConnectionButtonBorderColor = ThemeColors.Divider;
		}
		else
		{
			ConnectionButtonBg = AppConstants.Primary;
			ConnectionButtonTextColor = Colors.White;
			ConnectionButtonBorderColor = AppConstants.Primary;
		}

		OnPropertyChanged(nameof(SignalStatusText));
		OnPropertyChanged(nameof(SerialNumber));
		OnPropertyChanged(nameof(ShowSerialLine));
		OnPropertyChanged(nameof(SecondaryLine));
		OnPropertyChanged(nameof(ShowSecondaryLine));
		OnPropertyChanged(nameof(BatteryPillText));
		OnPropertyChanged(nameof(BatteryFillWidth));
		OnPropertyChanged(nameof(BatteryAccentColor));
		OnPropertyChanged(nameof(BatteryFillColor));
		OnPropertyChanged(nameof(ConnectionButtonText));
		OnPropertyChanged(nameof(CanOpenMeasure));
		OnPropertyChanged(nameof(ShowMeasureLink));
	}
}

