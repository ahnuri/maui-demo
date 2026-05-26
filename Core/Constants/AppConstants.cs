using HannaUIDemo.Theme;

namespace HannaUIDemo.Core.Constants;

/// <summary>
/// Brand colors and convenience aliases over <see cref="Tokens"/>. New UI code should
/// prefer <c>Tokens.*</c> directly (font sizes, spacing, radii, icon sizes, …) and
/// reach for <see cref="ThemeColors"/> for theme-aware colors. The named layout
/// constants below remain for backwards compatibility — they now forward to
/// <see cref="Tokens"/>.
/// </summary>
public static class AppConstants
{
	public static readonly Color Primary = Color.FromArgb("#0EA5C6");
	public static readonly Color Success = Color.FromArgb("#22C55E");
	public static readonly Color Error = Color.FromArgb("#EF4444");
	public static readonly Color GradientDarkStart = Color.FromArgb("#0B1220");
	public static readonly Color GradientDarkEnd = Color.FromArgb("#020617");

	public const string MeasureTabTitle = "Measure";

	// ── Aliases forwarding to Tokens (kept so existing call-sites compile) ────────
	public const double RadiusCard = Tokens.Radius.Xxxl;          // 18
	public const double RadiusCardSmall = Tokens.Radius.Xxl;       // 16
	public const double RadiusTile = Tokens.Radius.Xl;             // 14
	public const double RadiusChip = Tokens.Radius.Lg;             // 12
	public const double RadiusButton = Tokens.Radius.Xl;           // 14
	public const double RadiusIconBox = Tokens.Radius.Md;          // 10

	public const double SpacingScreen = Tokens.Spacing.Lg;         // 16
	public const double SpacingSection = Tokens.Spacing.Xxl;       // 24
	public const double SpacingTile = Tokens.Spacing.Lg;           // 16
	public const double SpacingTight = Tokens.Spacing.Md;          // 12
	public const double SpacingTiny = Tokens.Spacing.Sm;           // 8

	public const double ButtonHeight = Tokens.ButtonHeight.Lg;     // 54
	public const double IconSizeMedium = Tokens.IconSize.MdPlus;   // 24
	public const double IconSizeSmall = Tokens.IconSize.SmPlus;    // 20
	public const double AvatarSizeSmall = Tokens.Avatar.Sm;        // 40
	public const double AvatarSizeMedium = Tokens.Avatar.Md;       // 44

	public const double FontSizeAppBarTitle = Tokens.FontSize.LargeTitle;     // 20
	public const double FontSizeSectionTitle = Tokens.FontSize.SectionTitle;  // 18
	public const double FontSizeBody = Tokens.FontSize.BodyLarge;             // 14
	public const double FontSizeCaption = Tokens.FontSize.Small;              // 12

	/// <summary>MauiImage asset names (no path; extension omitted at runtime).</summary>
	public static class TabIcons
	{
		public const string Home = "tab_halo";
		public const string Measure = "tab_photometer";
		public const string Logs = "tab_logs";
		public const string Info = "tab_info";
		public const string Help = "tab_help";
	}
}
