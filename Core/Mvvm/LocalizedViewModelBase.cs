using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>ViewModel base that refreshes localized strings when culture changes.</summary>
public abstract partial class LocalizedViewModelBase : PageViewModelBase
{
	/// <summary>
	/// Active localization service. Exposed publicly (rather than protected) so cross-module
	/// helpers (e.g. <c>MultimeterMeasureModule.TryRefreshNavigation</c>) can resolve strings
	/// against the same instance the ViewModel uses.
	/// </summary>
	public LocalizationService Loc { get; }

	protected LocalizedViewModelBase()
	{
		Loc = Application.Current is App app
			? app.Services.GetRequiredService<LocalizationService>()
			: throw new InvalidOperationException("Application host is not initialized.");
		LocalizationService.CultureChanged += OnCultureChanged;
	}

	void OnCultureChanged(object? sender, EventArgs e) => ApplyLocalization();

	public override void RefreshForTheme()
	{
		base.RefreshForTheme();
		ApplyLocalization();
	}

	/// <summary>Override to push translated values into bindable properties.</summary>
	protected virtual void ApplyLocalization() { }
}
