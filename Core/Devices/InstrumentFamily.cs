namespace HannaUIDemo.Core.Devices;

/// <summary>Display metadata for one instrument family (picker, home chips, cloud sync).</summary>
public sealed record InstrumentFamily(
	InstrumentKind Kind,
	string PickerTitle,
	string PickerSubtitle,
	string? PickerThumbText,
	string? PickerIcon,
	bool PickerUsesTealAccent,
	string MeasureNavigationTitleKey,
	string OpeningMessage);
