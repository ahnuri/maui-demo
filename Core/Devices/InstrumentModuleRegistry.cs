namespace HannaUIDemo.Core.Devices;

/// <summary>
/// Read-only map of instrument families and their feature capabilities in this app.
/// Use when building menus, onboarding, or feature flags per device type.
/// </summary>
public static class InstrumentModuleRegistry
{
	public static IReadOnlyList<InstrumentModuleDescriptor> All { get; } =
	[
		new(
			InstrumentKind.Photometer,
			ProductLine: "HI97115 Photometer",
			HasMeasureTab: true,
			HasLogHistoryTab: true,
			GroupsLogsByTank: true,
			MeasureUsesCustomShellChrome: true),
		new(
			InstrumentKind.Multimeter,
			ProductLine: "HI98x94 Multiparameter",
			HasMeasureTab: true,
			HasLogHistoryTab: true,
			GroupsLogsByTank: false,
			MeasureUsesCustomShellChrome: false),
		new(
			InstrumentKind.Halo2,
			ProductLine: "Halo 2",
			HasMeasureTab: true,
			HasLogHistoryTab: true,
			GroupsLogsByTank: false,
			MeasureUsesCustomShellChrome: false,
			UsesLabChrome: true)
	];

	public static InstrumentModuleDescriptor Get(InstrumentKind kind) =>
		All.First(m => m.Kind == kind);
}

/// <summary>Capabilities and labels for one instrument module vertical slice.</summary>
public sealed record InstrumentModuleDescriptor(
	InstrumentKind Kind,
	string ProductLine,
	bool HasMeasureTab,
	bool HasLogHistoryTab,
	bool GroupsLogsByTank,
	bool MeasureUsesCustomShellChrome,
	bool UsesLabChrome = false);
