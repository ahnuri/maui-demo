namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Shared in-memory log history entries for home and detail pages.</summary>
public static class LogHistoryData
{
	static List<LogEntryViewModel> _entries = [];

	public static IReadOnlyList<LogEntryViewModel> Entries => _entries;

	public static void RemoveEntries(IEnumerable<LogEntryViewModel> entries)
	{
		foreach (var entry in entries.ToList())
			_entries.Remove(entry);
	}

	public static void EnsureInitialized()
	{
		if (_entries.Count > 0)
			return;

		LogHistoryCatalog.Rebuild();
		_entries = LogHistoryCatalog.Entries.ToList();
	}

	public static void ReloadFromCatalog()
	{
		LogHistoryCatalog.Rebuild();
		_entries = LogHistoryCatalog.Entries.ToList();
	}
}
