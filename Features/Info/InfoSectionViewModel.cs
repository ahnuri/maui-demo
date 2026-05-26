using System.Collections.ObjectModel;
using HannaUIDemo.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Features.Info;

/// <summary>Grouped device information (details, software, connection).</summary>
public sealed class InfoSectionViewModel
{
	static LocalizationService Loc => ((App)Application.Current!).Services.GetRequiredService<LocalizationService>();

	public required string Icon { get; init; }
	public required string Title { get; init; }
	public ObservableCollection<InfoRowViewModel> Rows { get; } = new();
	public bool ShowFirmwareBanner { get; init; }
	public string FirmwareBannerLabel => Loc.T("Info_Banner_FirmwareUpdate");
}
