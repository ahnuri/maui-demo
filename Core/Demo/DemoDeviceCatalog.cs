using HannaUIDemo.Core.Devices;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Core.Demo;

/// <summary>
/// Central demo device definitions shared by Devices, Cloud Sync, and Measure flows.
/// Replace with a real device repository when connecting to hardware or Hanna Cloud APIs.
/// </summary>
public static class DemoDeviceCatalog
{
	/// <summary>Default connected instrument IDs for the demo session.</summary>
	public static IReadOnlyCollection<string> DefaultConnectedIds { get; } =
		["hi97115", "hi98494", "halo2"];

	/// <summary>Instruments shown in the Connected section when their ID is in <see cref="DefaultConnectedIds"/>.</summary>
	public static IEnumerable<DeviceListItem> ConnectedDevices(IReadOnlySet<string> connectedIds) =>
		AllCatalog().Where(d => connectedIds.Contains(d.Id));

	/// <summary>Previously paired instruments not currently connected.</summary>
	public static IEnumerable<DeviceListItem> AssociatedDevices(IReadOnlySet<string> connectedIds) =>
		AssociatedCatalog().Where(d => !connectedIds.Contains(d.Id));

	/// <summary>Nearby BLE discoveries from a scan.</summary>
	public static IEnumerable<DeviceListItem> AvailableDevices() => AvailableCatalog();

	static IEnumerable<DeviceListItem> AllCatalog() =>
		ConnectedCatalog().Concat(AssociatedCatalog()).Concat(AvailableCatalog());

	static IEnumerable<DeviceListItem> ConnectedCatalog() =>
	[
		Build("hi97115", "HI97115 PMeter3", "Model: HI97105", "1.4.2", 92, "2 min ago",
			InstrumentKind.Photometer),
		Build("hi98494", "HI98494 - MM3", "Model: HI98494", "2.1.0", 78, "8 min ago",
			InstrumentKind.Multimeter),
		Build("halo2", "HI12322 Halo22", "Model: HI12322", "3.0.1", 44, "Just now",
			InstrumentKind.Halo2),
	];

	static IEnumerable<DeviceListItem> AssociatedCatalog() =>
	[
		Build("hi97115-pt1", "HI97115 Meter1", null, null, null, null, null),
		Build("hi9810391", "HI9810391 Halo2", null, null, null, null, null),
	];

	static IEnumerable<DeviceListItem> AvailableCatalog() =>
	[
		Build("hi98494-ak1", "HI98494 MultiM1", null, null, null, null, null, strong: true),
		Build("hi9810392", "HI9810392 Halo2", null, null, null, null, null, strong: false, signalKey: "Device_Signal_Low"),
		Build("hi97115-mm2", "HI97115 Pmeter1", null, null, null, null, null, strong: true),
	];

	static DeviceListItem Build(
		string id,
		string name,
		string serial,
		string? firmware,
		int? battery,
		string? lastSeen,
		InstrumentKind? kind,
		bool strong = true,
		string signalKey = "Device_Signal_Strong")
	{
		var item = new DeviceListItem
		{
			Id = id,
			Name = name,
			Serial = serial,
			Firmware = firmware,
			BatteryPercent = battery,
			LastSeen = lastSeen,
			SignalTextKey = signalKey,
			IsStrongSignal = strong,
			InstrumentKind = kind,
			DeviceIcon = DeviceIconResolver.ResolveIcon(kind, name),
			ThumbText = DeviceIconResolver.ResolveThumb(kind, name)
		};
		item.RefreshChrome();
		return item;
	}
}
