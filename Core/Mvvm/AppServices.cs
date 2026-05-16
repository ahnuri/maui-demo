using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>Resolves services from the MAUI <see cref="Application"/> host.</summary>
public static class AppServices
{
	public static T Get<T>() where T : notnull =>
		((App)Application.Current!).Services.GetRequiredService<T>();
}
