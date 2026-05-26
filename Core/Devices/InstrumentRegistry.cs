using HannaUIDemo.Core.Localization;

namespace HannaUIDemo.Core.Devices;

/// <summary>
/// Central catalog of instrument families and their UI metadata.
///
/// All user-visible strings (display name, subtitle, opening message, nav title)
/// are stored as translation keys in <see cref="InstrumentFamily"/>. Use the
/// <see cref="GetDisplayName"/>, <see cref="GetSubtitle"/>, <see cref="GetOpeningMessage"/>
/// and <see cref="GetMeasureNavigationTitle"/> helpers to resolve them through
/// <see cref="LocalizationService"/>.
/// </summary>
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
			DisplayNameKey: "Instrument_Photometer_DisplayName",
			SubtitleKey: "Instrument_Photometer_Subtitle",
			PickerThumbText: "HI",
			PickerIcon: "photometer_hi97115.png",
			PickerUsesTealAccent: true,
			OpeningMessageKey: "Instrument_Photometer_OpeningMessage"),
		new(
			InstrumentKind.Multimeter,
			DisplayNameKey: "Instrument_Multimeter_DisplayName",
			SubtitleKey: "Instrument_Multimeter_Subtitle",
			PickerThumbText: "94",
			PickerIcon: "hi98494_multimeter_icon.png",
			PickerUsesTealAccent: false,
			OpeningMessageKey: "Instrument_Multimeter_OpeningMessage"),
		new(
			InstrumentKind.Halo2,
			DisplayNameKey: "Instrument_Halo2_DisplayName",
			SubtitleKey: "Instrument_Halo2_Subtitle",
			PickerThumbText: null,
			PickerIcon: "halo2_device_icon.png",
			PickerUsesTealAccent: false,
			OpeningMessageKey: "Instrument_Halo2_OpeningMessage")
	];

	public static IReadOnlyList<InstrumentFamily> All => Families;

	public static InstrumentFamily Get(InstrumentKind kind) =>
		Families.First(f => f.Kind == kind);

	/// <summary>Localized display name (e.g. "HI97115 - Meter").</summary>
	public static string GetDisplayName(InstrumentKind kind, LocalizationService loc) =>
		loc.T(Get(kind).DisplayNameKey);

	/// <summary>Localized one-line subtitle for the device picker / hub.</summary>
	public static string GetSubtitle(InstrumentKind kind, LocalizationService loc) =>
		loc.T(Get(kind).SubtitleKey);

	/// <summary>Localized "Opening &lt;device&gt;…" busy message.</summary>
	public static string GetOpeningMessage(InstrumentKind kind, LocalizationService loc) =>
		loc.T(Get(kind).OpeningMessageKey);

	/// <summary>
	/// Localized navigation-title text for the Measure tab when this instrument is selected.
	/// Halo 2 uses the global "Hanna Lab" title; the others use the device display name.
	/// </summary>
	public static string GetMeasureNavigationTitle(InstrumentKind kind, LocalizationService loc) =>
		kind switch
		{
			InstrumentKind.Halo2 => loc.T("Shell_Home"),
			_ => GetDisplayName(kind, loc)
		};
}
