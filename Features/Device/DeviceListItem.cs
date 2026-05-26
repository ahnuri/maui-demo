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

	public string ConnectionButtonText => IsConnected
		? Loc.T("Device_Action_Disconnect")
		: Loc.T("Device_Action_Connect");

	partial void OnIsConnectedChanged(bool value) => RefreshChrome();

	public void RefreshChrome()
	{
		SignalDotColor = IsStrongSignal ? AppConstants.Success : ThemeColors.MutedSignalDot;
		SignalTextColor = IsConnected && IsStrongSignal
			? AppConstants.Success
			: ThemeColors.OnSurfaceVariant;
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
		OnPropertyChanged(nameof(ConnectionButtonText));
		OnPropertyChanged(nameof(CanOpenMeasure));
		OnPropertyChanged(nameof(ShowMeasureLink));
	}
}

