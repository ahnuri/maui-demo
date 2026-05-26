namespace HannaUIDemo.Theme;

/// <summary>
/// Single source of truth for every design-system value in the Hanna Lab demo —
/// font families, font sizes, spacing, radii, stroke widths, icon sizes, and
/// icon-button sizes. Pair with <see cref="ThemeColors"/> for colors.
///
/// ── How to use ─────────────────────────────────────────────────────────────
/// C#:
///   var padding = Tokens.Spacing.Lg;
///   label.FontSize = Tokens.FontSize.Body;
///   image.WidthRequest = Tokens.IconSize.Md;
///
/// XAML (after the matching keys are merged via <c>Resources/Styles/Tokens.xaml</c>):
///   FontSize="{StaticResource FontSizeBody}"
///   Padding="{StaticResource SpacingLg}"
///   CornerRadius="{StaticResource RadiusMd}"
///
/// ── Naming conventions ─────────────────────────────────────────────────────
///  • Scale step suffixes: Xs (extra-small) → Sm → Md → Lg → Xl → Xxl → Xxxl
///  • Semantic suffixes used where they read more naturally than the scale step:
///      FontSize.Body / Title / LargeTitle / Display / MeasureHero
///      Radius.Pill / Hero
///      IconButton.Sm/Md/Lg + Avatar (circular - radius = width/2)
///
/// Don't introduce new magic numbers in views — add a token here first.
/// </summary>
public static class Tokens
{
	// ── Fonts ─────────────────────────────────────────────────────────────
	/// <summary>Registered font family names (see <c>MauiProgram.ConfigureFonts</c>).</summary>
	public static class FontFamily
	{
		public const string Regular = "OpenSansRegular";
		public const string Semibold = "OpenSansSemibold";
	}

	/// <summary>
	/// Numeric font-size scale. Captures the observed usage clusters in the codebase:
	/// 11/12/13/14/15/16/17/18/20/22 cover ~95% of all label sizes.
	/// </summary>
	public static class FontSize
	{
		/// <summary>11 pt — fine-print labels, table-row metadata.</summary>
		public const double Caption = 11;
		/// <summary>12 pt — secondary captions, status pills.</summary>
		public const double Small = 12;
		/// <summary>13 pt — secondary body, supporting text.</summary>
		public const double Body = 13;
		/// <summary>14 pt — default body text (Styles.xaml default).</summary>
		public const double BodyLarge = 14;
		/// <summary>15 pt — list-row title.</summary>
		public const double Subhead = 15;
		/// <summary>16 pt — emphasised body / form label.</summary>
		public const double SubheadLarge = 16;
		/// <summary>17 pt — navigation title.</summary>
		public const double Title = 17;
		/// <summary>18 pt — page section title.</summary>
		public const double SectionTitle = 18;
		/// <summary>20 pt — appbar title / hero card title.</summary>
		public const double LargeTitle = 20;
		/// <summary>22 pt — accent number on hero tiles.</summary>
		public const double Hero = 22;
		/// <summary>26 pt — splash / dashboard headline.</summary>
		public const double Display = 26;
		/// <summary>46 pt — Halo 2 live-reading hero number.</summary>
		public const double MeasureHero = 46;
	}

	// ── Spacing ───────────────────────────────────────────────────────────
	/// <summary>
	/// Padding / margin / spacing scale on a 4-pt base grid. Top values used in the
	/// codebase: 8, 10, 12, 14, 16, 20, 24 — the scale below covers all of them.
	/// </summary>
	public static class Spacing
	{
		/// <summary>4 — tight inline spacing between glyph and label.</summary>
		public const double Xs = 4;
		/// <summary>6 — micro padding inside compact chips.</summary>
		public const double Xxs = 6;
		/// <summary>8 — minimum card-internal gap; default row spacing.</summary>
		public const double Sm = 8;
		/// <summary>10 — chip / button internal padding.</summary>
		public const double SmPlus = 10;
		/// <summary>12 — card content padding.</summary>
		public const double Md = 12;
		/// <summary>14 — section card padding.</summary>
		public const double MdPlus = 14;
		/// <summary>16 — screen edge padding ("screen gutter").</summary>
		public const double Lg = 16;
		/// <summary>20 — large hero internal padding.</summary>
		public const double Xl = 20;
		/// <summary>24 — section-to-section separation.</summary>
		public const double Xxl = 24;
		/// <summary>28 — bottom-of-screen safe padding.</summary>
		public const double Xxxl = 28;
	}

	// ── Corner radii ──────────────────────────────────────────────────────
	/// <summary>
	/// Corner-radius scale. Card-style components cluster at 10/12/14/16/18; pill
	/// chips at 22; circular avatars use <c>width / 2</c> directly.
	/// </summary>
	public static class Radius
	{
		/// <summary>6 — small inline tag pills (e.g. record-count badge).</summary>
		public const double Xs = 6;
		/// <summary>8 — compact chip / segmented control.</summary>
		public const double Sm = 8;
		/// <summary>10 — list-row card.</summary>
		public const double Md = 10;
		/// <summary>12 — primary card.</summary>
		public const double Lg = 12;
		/// <summary>14 — tile / surface card.</summary>
		public const double Xl = 14;
		/// <summary>16 — small hero card.</summary>
		public const double Xxl = 16;
		/// <summary>18 — large hero / dashboard card.</summary>
		public const double Xxxl = 18;
		/// <summary>22 — pill button.</summary>
		public const double Pill = 22;
		/// <summary>28 — hero dashboard / sheet handle.</summary>
		public const double Hero = 28;
	}

	// ── Stroke widths ─────────────────────────────────────────────────────
	/// <summary>Stroke / border thicknesses for Borders, BoxView dividers, etc.</summary>
	public static class Stroke
	{
		/// <summary>0.5 — hairline (use sparingly; renders inconsistently on Android).</summary>
		public const double Hairline = 0.5;
		/// <summary>1 — default divider / card stroke.</summary>
		public const double Thin = 1;
		/// <summary>1.2 — slightly chunkier divider.</summary>
		public const double ThinPlus = 1.2;
		/// <summary>1.5 — selected card outline / button border.</summary>
		public const double Thick = 1.5;
		/// <summary>2 — focus / emphasis outline.</summary>
		public const double Heavy = 2;
	}

	// ── Icon image sizes ──────────────────────────────────────────────────
	/// <summary>
	/// Square icon dimensions for <see cref="Image"/> assets (WidthRequest = HeightRequest).
	/// Use the explicit <see cref="Avatar"/> sizes below for circular device-portrait icons.
	/// </summary>
	public static class IconSize
	{
		/// <summary>16 — inline status glyph.</summary>
		public const double Xs = 16;
		/// <summary>18 — inline metric icon (next to a 15pt label).</summary>
		public const double Sm = 18;
		/// <summary>20 — small device chip icon.</summary>
		public const double SmPlus = 20;
		/// <summary>22 — nav-bar device icon / sync indicator (default for log rows).</summary>
		public const double Md = 22;
		/// <summary>24 — toolbar icon.</summary>
		public const double MdPlus = 24;
		/// <summary>26 — settings-row leading icon.</summary>
		public const double Lg = 26;
		/// <summary>28 — tab-bar icon.</summary>
		public const double Xl = 28;
		/// <summary>32 — log-history card hero icon.</summary>
		public const double Xxl = 32;
		/// <summary>40 — small device-portrait.</summary>
		public const double Hero = 40;
		/// <summary>54 — beaker / large device portrait.</summary>
		public const double HeroLarge = 54;
	}

	// ── Icon buttons (square circular / rounded buttons containing an icon) ─
	/// <summary>
	/// Touch-target sizes for square / circular icon buttons. The corner radius
	/// is half the width when used as a circle (see <see cref="Avatar"/> for
	/// avatar-style circles). Minimum 36×36 per Apple HIG / Material spec.
	/// </summary>
	public static class IconButton
	{
		/// <summary>36 — compact toolbar icon button.</summary>
		public const double Sm = 36;
		/// <summary>38 — measure-card disconnect chip.</summary>
		public const double SmPlus = 38;
		/// <summary>40 — settings-row trailing button (also iOS default touch).</summary>
		public const double Md = 40;
		/// <summary>44 — default touch target (Apple HIG minimum).</summary>
		public const double Lg = 44;
	}

	// ── Avatar / circular device-portrait sizes ───────────────────────────
	/// <summary>Circular avatar sizes (radius = size / 2).</summary>
	public static class Avatar
	{
		public const double Sm = 40;
		public const double Md = 44;
		public const double Lg = 52;
		public const double Xl = 56;
	}

	// ── Buttons (height) ──────────────────────────────────────────────────
	public static class ButtonHeight
	{
		/// <summary>40 — compact action.</summary>
		public const double Sm = 40;
		/// <summary>48 — default CTA.</summary>
		public const double Md = 48;
		/// <summary>54 — large CTA (default in AppConstants.ButtonHeight).</summary>
		public const double Lg = 54;
	}
}
