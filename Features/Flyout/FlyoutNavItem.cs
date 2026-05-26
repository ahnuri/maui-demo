using CommunityToolkit.Mvvm.ComponentModel;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Flyout;

public enum FlyoutNavAction
{
	ShellRoute,
	PushPage,
	SignOut,
}

/// <summary>Single row in the app flyout menu.</summary>
public partial class FlyoutNavItem : ObservableObject
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public ImageSource? IconSource { get; init; }
	public Color IconBadgeBackground { get; init; } = ThemeColors.FlyoutIconBadge;
	public double IconSize { get; init; } = 22;
	public FlyoutNavAction Action { get; init; } = FlyoutNavAction.ShellRoute;
	public string? ShellRoute { get; init; }
	public Type? PageType { get; init; }
	public bool IsDestructive { get; init; }
	public bool ShowChevron { get; init; } = true;

	[ObservableProperty] private bool _isSelected;
	[ObservableProperty] private Color _titleColor = ThemeColors.OnSurface;
	[ObservableProperty] private Color _rowBackground = Colors.Transparent;
	[ObservableProperty] private bool _showActiveBar;
}
