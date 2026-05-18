namespace HannaUIDemo.Core.Helpers;

/// <summary>External URLs and safe launcher helpers.</summary>
public static class AppLinks
{
	public const string HannaInstruments = "https://www.hannainst.com/";

	public static async Task OpenAsync(string url, Func<Task>? onFailure = null)
	{
		try
		{
			await Launcher.Default.OpenAsync(new Uri(url, UriKind.Absolute));
		}
		catch
		{
			if (onFailure is not null)
				await onFailure();
		}
	}
}
