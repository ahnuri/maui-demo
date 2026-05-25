using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HannaUIDemo.Features.Instruments.Logs;

/// <summary>Expandable log section for one instrument family.</summary>
public partial class LogSectionViewModel : ObservableObject
{
	public required InstrumentKind Kind { get; init; }
	public required string Key { get; init; }
	public required string Title { get; init; }
	public required string Subtitle { get; init; }

	public ObservableCollection<LogEntryViewModel> VisibleEntries { get; } = new();

	public ImageSource DeviceIcon => LogDeviceVisuals.IconSource(Kind);

	public Color DeviceAccent => LogDeviceVisuals.Accent;

	public Color DeviceAccentBackground => LogDeviceVisuals.AccentBackground;

	public string LogCountLabel => $"{_allEntries.Count} saved";

	[ObservableProperty] private bool _isExpanded;

	public bool ShowExpandLink => _allEntries.Count > 3;

	public string ExpandLabel => IsExpanded
		? "Show less"
		: $"Show {_allEntries.Count - 3} more";

	readonly List<LogEntryViewModel> _allEntries = [];

	public IReadOnlyList<LogEntryViewModel> AllEntries => _allEntries;

	public int EntryCount => _allEntries.Count;

	public void SetEntries(IEnumerable<LogEntryViewModel> entries)
	{
		_allEntries.Clear();
		_allEntries.AddRange(entries);
		OnPropertyChanged(nameof(LogCountLabel));
		OnPropertyChanged(nameof(ShowExpandLink));
		OnPropertyChanged(nameof(EntryCount));
		UpdateVisibleEntries();
	}

	[RelayCommand]
	void ToggleExpanded()
	{
		IsExpanded = !IsExpanded;
		UpdateVisibleEntries();
	}

	partial void OnIsExpandedChanged(bool value)
	{
		UpdateVisibleEntries();
		OnPropertyChanged(nameof(ExpandLabel));
	}

	void UpdateVisibleEntries()
	{
		VisibleEntries.Clear();
		var slice = IsExpanded ? _allEntries : _allEntries.Take(3);
		foreach (var entry in slice)
			VisibleEntries.Add(entry);
		OnPropertyChanged(nameof(ExpandLabel));
	}
}
