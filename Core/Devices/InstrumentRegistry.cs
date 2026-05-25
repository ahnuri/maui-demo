namespace HannaUIDemo.Core.Devices;

/// <summary>Central catalog of instrument families and their UI metadata.</summary>
public static class InstrumentRegistry
{
	static readonly InstrumentFamily[] Families =
	[
		new(
			InstrumentKind.Photometer,
			"HI97115 - Meter",
			"Wireless photometer",
			"HI",
			null,
			PickerUsesTealAccent: true,
			MeasureNavigationTitleKey: "HI97115 - Meter",
			OpeningMessage: "Opening photometer…"),
		new(
			InstrumentKind.Multimeter,
			"HI98x94 - Multiparameter",
			"Log recall & download",
			"94",
			null,
			PickerUsesTealAccent: false,
			MeasureNavigationTitleKey: "HI98x94 - Multiparameter",
			OpeningMessage: "Opening multiparameter…"),
		new(
			InstrumentKind.Halo2,
			"Halo 2",
			"pH · mV · temperature",
			null,
			"halo2_device_icon.png",
			PickerUsesTealAccent: false,
			MeasureNavigationTitleKey: "Shell_Home",
			OpeningMessage: "Opening Halo 2…")
	];

	public static IReadOnlyList<InstrumentFamily> All => Families;

	public static InstrumentFamily Get(InstrumentKind kind) =>
		Families.First(f => f.Kind == kind);

	public static string GetOpeningMessage(InstrumentKind kind) => Get(kind).OpeningMessage;
}
