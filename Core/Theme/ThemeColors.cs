namespace HannaUIDemo.Theme;

/// <summary>Colors for C#-built UI — mirrors Colors.xaml semantic palette.</summary>
public static class ThemeColors
{
	public static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

	public static Color PageBackground => IsDark ? Color.FromRgb(11, 18, 32) : Color.FromRgb(244, 246, 248);

	/// <summary>iOS App Store–style grouped background (light grey canvas / near-black in dark).</summary>
	public static Color StoreGroupedBackground => IsDark ? Color.FromRgb(0, 0, 0) : Color.FromRgb(242, 242, 247);
	public static Color Surface => IsDark ? Color.FromRgb(26, 36, 51) : Color.FromRgb(255, 255, 255);
	public static Color SurfaceSecondary => IsDark ? Color.FromRgb(37, 47, 63) : Color.FromRgb(248, 250, 252);
	public static Color OnSurface => IsDark ? Color.FromRgb(241, 245, 249) : Color.FromRgb(15, 23, 42);
	public static Color OnSurfaceVariant => IsDark ? Color.FromRgb(148, 163, 184) : Color.FromRgb(100, 116, 139);
	public static Color OnSurfaceMuted => IsDark ? Color.FromRgb(203, 213, 225) : Color.FromRgb(71, 85, 105);
	public static Color Divider => IsDark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240);
	public static Color SubtleTeal => IsDark ? Color.FromRgb(19, 78, 74) : Color.FromRgb(224, 242, 254);
	public static Color SubtleGreen => IsDark ? Color.FromRgb(20, 83, 45) : Color.FromRgb(232, 245, 233);
	public static Color ChipUnselected => IsDark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240);
	public static Color ChipUnselectedText => IsDark ? Color.FromRgb(241, 245, 249) : Color.FromRgb(15, 23, 42);
	public static Color OverlayScrim => IsDark ? Color.FromRgba(0, 0, 0, 180) : Color.FromRgba(0, 0, 0, 153);
	public static Color HandleBar => IsDark ? Color.FromRgb(71, 85, 105) : Color.FromRgb(203, 213, 225);
	public static Color GlassStroke => IsDark ? Color.FromRgba(255, 255, 255, 85) : Color.FromRgba(255, 255, 255, 102);
	public static Color GlassFill => IsDark ? Color.FromRgba(26, 36, 51, 51) : Color.FromRgba(255, 255, 255, 38);
	public static Color TileIconBg => IsDark ? Color.FromRgb(22, 78, 99) : Color.FromRgb(224, 242, 254);
	public static Color ResultIconBgNeutral => IsDark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240);
	public static Color CloseButtonBg => IsDark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240);
	public static Color SliderTrackBg => IsDark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240);
	public static Color LinkBlue => IsDark ? Color.FromRgb(125, 211, 252) : Color.FromRgb(21, 101, 192);
	public static Color TabUnselected => IsDark ? Color.FromRgb(148, 163, 184) : Color.FromRgb(100, 116, 139);
	public static Color SoftShadow => IsDark ? Color.FromRgba(0, 0, 0, 102) : Color.FromRgba(0, 0, 0, 24);
	public static Color MutedSignalDot => IsDark ? Color.FromRgb(100, 116, 139) : Color.FromRgb(189, 189, 189);

	/// <summary>Primary brand at ~10% opacity (pills, badges).</summary>
	public static Color PrimarySubtleFill => IsDark ? Color.FromRgba(14, 165, 198, 38) : Color.FromRgba(14, 165, 198, 26);

	/// <summary>Primary brand at ~7% opacity (scan banners).</summary>
	public static Color PrimarySubtleBanner => IsDark ? Color.FromRgba(14, 165, 198, 31) : Color.FromRgba(14, 165, 198, 18);

	/// <summary>Primary brand at ~18% opacity (banner strokes).</summary>
	public static Color PrimarySubtleStroke => IsDark ? Color.FromRgba(14, 165, 198, 64) : Color.FromRgba(14, 165, 198, 46);
}
