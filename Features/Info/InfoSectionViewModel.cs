using System.Collections.ObjectModel;

namespace HannaUIDemo.Features.Info;

/// <summary>Grouped device information (details, software, connection).</summary>
public sealed class InfoSectionViewModel
{
	public required string Icon { get; init; }
	public required string Title { get; init; }
	public ObservableCollection<InfoRowViewModel> Rows { get; } = new();
	public bool ShowFirmwareBanner { get; init; }
}
