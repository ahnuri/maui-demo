using System.Windows.Input;
using HannaUIDemo.Core.Localization;

namespace HannaUIDemo.Features.Measure;

/// <summary>
/// Shell navigation surface exposed by <see cref="MeasureTabPage"/> to instrument measure modules.
/// Keeps photometer-specific chrome out of the tab host implementation details.
/// </summary>
public interface IMeasureTabNavigationHost
{
	/// <summary>The measure tab page used for Shell toolbar and title APIs.</summary>
	ContentPage Page { get; }

	void SetTitle(string title);

	void ClearTitleView();

	void SetTitleView(View titleView);

	void EnableFlyout();

	void DisableFlyout();

	void SetBackCommand(ICommand command);

	void ClearBackBehavior();

	void ClearToolbar();

	void AddToolbarItem(ToolbarItem item);

	LocalizationService Localization { get; }
}
