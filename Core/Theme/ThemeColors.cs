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

	/// <summary>Flyout nav icon badge behind single-color SVG icons.</summary>
	public static Color FlyoutIconBadge => IsDark ? Color.FromRgb(22, 92, 86) : Color.FromRgb(224, 242, 254);
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

	// ── Halo / multiparameter lab dashboard (Halo 2 measure, device settings) ──

	public static Color LabCanvas => IsDark ? Color.FromArgb("#0A0F1C") : Color.FromRgb(242, 244, 248);
	public static Color LabCard => IsDark ? Color.FromArgb("#18181B") : Color.FromRgb(255, 255, 255);
	public static Color LabCardElevated => IsDark ? Color.FromArgb("#27272A") : Color.FromRgb(248, 250, 252);
	public static Color LabRowStripe => IsDark ? Color.FromArgb("#27272A").MultiplyAlpha(0.5f) : Color.FromRgb(241, 245, 249);
	public static Color LabBorder => IsDark ? Color.FromArgb("#FFFFFF").MultiplyAlpha(0.10f) : Divider;
	public static Color LabMuted => IsDark ? Color.FromArgb("#A1A1AA") : OnSurfaceVariant;
	public static Color LabPrimaryText => IsDark ? Colors.White : OnSurface;
	public static Color LabSecondaryText => IsDark ? Color.FromArgb("#E4E4E7") : OnSurfaceMuted;
	public static Color LabAccentCyan => Color.FromArgb("#22D3EE");
	public static Color LabAccentOrange => Color.FromArgb("#FB923C");
	public static Color LabEmerald => Color.FromArgb("#34D399");
	public static Color LabEmeraldMuted => LabEmerald.MultiplyAlpha(0.12f);
	public static Color LabIconButtonFill => IsDark ? Colors.White.MultiplyAlpha(0.05f) : SurfaceSecondary;
	public static Color LabGradientStop => IsDark ? Color.FromArgb("#27272A") : SurfaceSecondary;

	/// <summary>Live readings card gradient end.</summary>
	public static Color LabGradientEnd => IsDark ? Color.FromArgb("#09090B") : Color.FromRgb(255, 255, 255);

	/// <summary>Graph plot area fill.</summary>
	public static Color LabGraphPlotFill => IsDark ? Color.FromArgb("#09090B") : Color.FromRgb(248, 250, 252);

	/// <summary>Selected mode chip fill on the measure mode bar.</summary>
	public static Color LabModeChipActive => IsDark ? LabCanvas : LabCardElevated;

	/// <summary>Disabled chip / button surface.</summary>
	public static Color LabChipDisabled => IsDark ? Color.FromArgb("#3F3F46") : ChipUnselected;

	/// <summary>Data table column header background (measure history grid).</summary>
	public static Color LabTableHeaderBackground => IsDark ? Color.FromArgb("#0F172A") : Color.FromRgb(226, 232, 240);

	/// <summary>Data table column header text.</summary>
	public static Color LabTableHeaderText => IsDark ? Colors.White : Color.FromRgb(15, 23, 42);

	// Status / semantic (readable on light and dark surfaces)
	public static Color LabWarning => Color.FromArgb("#FBBF24");
	public static Color LabWarningMuted => LabWarning.MultiplyAlpha(0.12f);
	public static Color LabDanger => Color.FromArgb("#EF4444");
	public static Color LabDangerSoft => Color.FromArgb("#FCA5A5");
	public static Color LabDangerMuted => LabDanger.MultiplyAlpha(0.12f);
	public static Color LabSuccess => Color.FromArgb("#34D399");
	public static Color LabPhAcidic => Color.FromArgb("#EF4444");
	public static Color LabPhAcidicMid => Color.FromArgb("#F97316");
	public static Color LabPhNeutral => Color.FromArgb("#22C55E");
	public static Color LabPhBasic => Color.FromArgb("#A855F7");
	public static Color LabPhAlkaline => Color.FromArgb("#C026D3");

	// ── Home dashboard hero (multi-device lab banner) ──

	public static Color LabHeroGradientStart => IsDark ? Color.FromArgb("#07111F") : Color.FromRgb(224, 242, 254);
	public static Color LabHeroGradientMid => IsDark ? Color.FromArgb("#0F1B2D") : Color.FromRgb(240, 249, 255);
	public static Color LabHeroGradientEnd => IsDark ? Color.FromArgb("#0B3A4A") : Color.FromRgb(186, 230, 253);
	public static Color LabHeroText => IsDark ? Color.FromArgb("#F8FAFC") : Color.FromRgb(15, 23, 42);
	public static Color LabHeroMuted => IsDark ? Color.FromArgb("#9FB3C8") : Color.FromRgb(71, 85, 105);
	public static Color LabHeroTileBackground => IsDark ? Color.FromArgb("#15243A") : Color.FromRgb(255, 255, 255);
	public static Color LabHeroTileStroke => IsDark ? Color.FromArgb("#28465B") : Color.FromRgb(203, 213, 225);
	public static Color LabHeroBadgeFill => IsDark ? Color.FromArgb("#193649") : Color.FromRgb(224, 242, 254);
	public static Color LabHeroBadgeStroke => IsDark ? Color.FromArgb("#2E6B83") : Color.FromRgb(125, 211, 252);
	public static Color LabHeroDeviceFrame => IsDark ? Color.FromArgb("#102A3A") : Color.FromRgb(241, 245, 249);
	public static Color LabHeroDeviceStroke => IsDark ? Color.FromArgb("#295E74") : Color.FromRgb(203, 213, 225);

	/// <summary>Flyout footer badge behind the Hanna logo (keeps logo readable).</summary>
	public static Color FlyoutLogoBadge => IsDark ? Colors.Black : Color.FromRgb(15, 23, 42);

	/// <summary>Flyout drawer canvas (deep navy in dark mode).</summary>
	public static Color FlyoutBackground => IsDark ? Color.FromRgb(11, 18, 32) : Color.FromRgb(248, 250, 252);

	/// <summary>Signed-in profile card fill on the flyout.</summary>
	public static Color FlyoutProfileCard => IsDark ? Color.FromRgb(22, 36, 52) : Color.FromRgb(255, 255, 255);

	/// <summary>Active flyout row highlight.</summary>
	public static Color FlyoutActiveRow => IsDark ? Color.FromRgba(14, 165, 198, 38) : Color.FromRgba(14, 165, 198, 20);

	/// <summary>Flyout settings / footer menu group.</summary>
	public static Color FlyoutMenuGroup => IsDark ? Color.FromRgb(18, 28, 42) : Color.FromRgb(241, 245, 249);
}
