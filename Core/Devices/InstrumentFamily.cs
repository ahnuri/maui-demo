namespace HannaUIDemo.Core.Devices;

/// <summary>
/// Display metadata for one instrument family (device picker, home chips, cloud sync).
///
/// Text fields hold <see cref="HannaUIDemo.Core.Localization.TranslationStore"/> keys
/// (NOT raw strings). Resolve through <see cref="InstrumentRegistry"/> helpers, e.g.:
///
///   var title = InstrumentRegistry.GetDisplayName(kind, loc);
/// </summary>
public sealed record InstrumentFamily(
	InstrumentKind Kind,
	string DisplayNameKey,
	string SubtitleKey,
	string? PickerThumbText,
	string? PickerIcon,
	bool PickerUsesTealAccent,
	string OpeningMessageKey);
