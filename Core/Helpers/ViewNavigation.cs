namespace HannaUIDemo.Helpers;

internal static class ViewNavigation
{
	internal static Page? FindHostPage(Element? element)
	{
		while (element != null && element is not Page)
			element = element.Parent;
		return element as Page;
	}
}
