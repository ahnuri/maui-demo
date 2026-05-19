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
		r["FlyoutIconBadge"] = ThemeColors.FlyoutIconBadge;
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
		r["PrimarySubtleFill"] = ThemeColors.PrimarySubtleFill;
		r["PrimarySubtleBanner"] = ThemeColors.PrimarySubtleBanner;
		r["PrimarySubtleStroke"] = ThemeColors.PrimarySubtleStroke;
		r["FlyoutLogoBadge"] = ThemeColors.FlyoutLogoBadge;
		r["FlyoutBackground"] = ThemeColors.FlyoutBackground;
		r["FlyoutProfileCard"] = ThemeColors.FlyoutProfileCard;
		r["FlyoutActiveRow"] = ThemeColors.FlyoutActiveRow;
		r["FlyoutMenuGroup"] = ThemeColors.FlyoutMenuGroup;

		// Halo 2 / lab measure surfaces
		r["LabCanvas"] = ThemeColors.LabCanvas;
		r["LabCard"] = ThemeColors.LabCard;
		r["LabCardElevated"] = ThemeColors.LabCardElevated;
		r["LabBorder"] = ThemeColors.LabBorder;
		r["LabMuted"] = ThemeColors.LabMuted;
		r["LabPrimaryText"] = ThemeColors.LabPrimaryText;
		r["LabSecondaryText"] = ThemeColors.LabSecondaryText;

		// Home hero banner
		r["LabHeroGradientStart"] = ThemeColors.LabHeroGradientStart;
		r["LabHeroGradientMid"] = ThemeColors.LabHeroGradientMid;
		r["LabHeroGradientEnd"] = ThemeColors.LabHeroGradientEnd;
		r["LabHeroText"] = ThemeColors.LabHeroText;
		r["LabHeroMuted"] = ThemeColors.LabHeroMuted;
		r["LabHeroTileBackground"] = ThemeColors.LabHeroTileBackground;
		r["LabHeroTileStroke"] = ThemeColors.LabHeroTileStroke;
		r["LabHeroBadgeFill"] = ThemeColors.LabHeroBadgeFill;
		r["LabHeroBadgeStroke"] = ThemeColors.LabHeroBadgeStroke;
		r["LabHeroDeviceFrame"] = ThemeColors.LabHeroDeviceFrame;
		r["LabHeroDeviceStroke"] = ThemeColors.LabHeroDeviceStroke;
	}
}
