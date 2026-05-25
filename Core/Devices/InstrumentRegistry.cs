namespace HannaUIDemo.Core.Devices;

/// <summary>Central catalog of instrument families and their UI metadata.</summary>
public static class InstrumentRegistry
{
	// Device-picker icons (PickerIcon). When set, the picker uses an Image control;
	// when null, it falls back to PickerThumbText as a tinted text badge.
	// Halo 2 already used its product shot; we now do the same for the other two so the
	// picker is visually consistent across all instruments.
	static readonly InstrumentFamily[] Families =
	[
		new(
			InstrumentKind.Photometer,
			"HI97115 - Meter",
			"Wireless photometer",
			"HI",
			"photometer_hi97115.png",
			PickerUsesTealAccent: true,
			MeasureNavigationTitleKey: "HI97115 - Meter",
			OpeningMessage: "Opening photometer…"),
		new(
			InstrumentKind.Multimeter,
			"HI98x94 - Multiparameter",
			"Log recall & download",
			"94",
			"hi98494_multimeter_icon.png",
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
