using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>
/// Service locator for Shell-hosted tab pages that are created by XAML without constructor injection.
/// Prefer constructor injection on pages registered in <see cref="MauiProgram"/>.
/// </summary>
public static class AppServices
{
	/// <summary>Resolves a registered service or ViewModel from the MAUI application host.</summary>
	public static T Get<T>() where T : notnull =>
		((App)Application.Current!).Services.GetRequiredService<T>();
}
