namespace HannaUIDemo.Core.Constants;

/// <summary>Layout metrics and brand accents (theme-independent).</summary>
public static class AppConstants
{
	public static readonly Color Primary = Color.FromArgb("#0EA5C6");
	public static readonly Color Success = Color.FromArgb("#22C55E");
	public static readonly Color GradientDarkStart = Color.FromArgb("#0B1220");
	public static readonly Color GradientDarkEnd = Color.FromArgb("#020617");

	public const string MeasureTabTitle = "Measure";

	public const double RadiusCard = 18;
	public const double RadiusCardSmall = 16;
	public const double RadiusTile = 14;
	public const double RadiusChip = 12;
	public const double RadiusButton = 14;
	public const double RadiusIconBox = 10;

	public const double SpacingScreen = 16;
	public const double SpacingSection = 24;
	public const double SpacingTile = 16;
	public const double SpacingTight = 12;
	public const double SpacingTiny = 8;

	public const double ButtonHeight = 54;
	public const double IconSizeMedium = 24;
	public const double IconSizeSmall = 20;
	public const double AvatarSizeSmall = 40;
	public const double AvatarSizeMedium = 44;

	public const double FontSizeAppBarTitle = 20;
	public const double FontSizeSectionTitle = 18;
	public const double FontSizeBody = 14;
	public const double FontSizeCaption = 12;

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
