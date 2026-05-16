namespace HannaUIDemo.Theme;

/// <summary>Pushes semantic palette into <see cref="Application.Resources"/> so XAML can bind with DynamicResource (AppThemeColor is not reliably compiled in all MAUI XAML pipelines).</summary>
public static class SemanticResources
{
	public static void Update(Application app)
	{
		var r = app.Resources;
		r["PageBackground"] = ThemeColors.PageBackground;
		r["AppBackground"] = ThemeColors.PageBackground;
		r["StoreGroupedBackground"] = ThemeColors.StoreGroupedBackground;
		r["Surface"] = ThemeColors.Surface;
		r["SurfaceSecondary"] = ThemeColors.SurfaceSecondary;
		r["OnSurface"] = ThemeColors.OnSurface;
		r["OnSurfaceVariant"] = ThemeColors.OnSurfaceVariant;
		r["OnSurfaceMuted"] = ThemeColors.OnSurfaceMuted;
		r["Divider"] = ThemeColors.Divider;
		r["SubtleTeal"] = ThemeColors.SubtleTeal;
		r["SubtleGreen"] = ThemeColors.SubtleGreen;
		r["ChipUnselected"] = ThemeColors.ChipUnselected;
		r["ChipUnselectedText"] = ThemeColors.ChipUnselectedText;
		r["OverlayScrim"] = ThemeColors.OverlayScrim;
		r["HandleBar"] = ThemeColors.HandleBar;
		r["GlassStroke"] = ThemeColors.GlassStroke;
		r["GlassFill"] = ThemeColors.GlassFill;
		r["LinkBlue"] = ThemeColors.LinkBlue;
		r["SoftShadow"] = ThemeColors.SoftShadow;
		r["TabBarBackground"] = ThemeColors.Surface;
		r["TabBarUnselected"] = ThemeColors.TabUnselected;
	}
}
