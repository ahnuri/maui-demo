namespace HannaUIDemo.Theme;

/// <summary>Applies navigation bar / page chrome for shell content pages.</summary>
public static class ShellChrome
{
	public static void ApplyStandard(ContentPage page)
	{
		page.BackgroundColor = ThemeColors.PageBackground;
		Shell.SetBackgroundColor(page, ThemeColors.PageBackground);
		Shell.SetForegroundColor(page, ThemeColors.OnSurface);
		Shell.SetTitleColor(page, ThemeColors.OnSurface);
		Shell.SetUnselectedColor(page, ThemeColors.OnSurfaceVariant);
	}

	public static void ApplyGrouped(ContentPage page)
	{
		page.BackgroundColor = ThemeColors.StoreGroupedBackground;
		Shell.SetBackgroundColor(page, ThemeColors.StoreGroupedBackground);
		Shell.SetForegroundColor(page, ThemeColors.OnSurface);
		Shell.SetTitleColor(page, ThemeColors.OnSurface);
		Shell.SetUnselectedColor(page, ThemeColors.OnSurfaceVariant);
	}

	public static void ApplyLab(ContentPage page)
	{
		page.BackgroundColor = ThemeColors.LabCanvas;
		Shell.SetBackgroundColor(page, ThemeColors.LabCanvas);
		Shell.SetForegroundColor(page, ThemeColors.LabPrimaryText);
		Shell.SetTitleColor(page, ThemeColors.LabPrimaryText);
		Shell.SetUnselectedColor(page, ThemeColors.LabMuted);
	}
}
