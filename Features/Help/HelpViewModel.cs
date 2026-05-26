using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;
using HannaUIDemo.Features.Device;

namespace HannaUIDemo.Features.Help;

/// <summary>
/// Help tab ViewModel: static guidance sections and navigation to the Devices screen.
/// Uses [RelayCommand] for user actions; content is demo copy until wired to CMS or docs API.
/// </summary>
public partial class HelpViewModel : LocalizedViewModelBase
{
	public ObservableCollection<HelpItem> Items { get; } = new();

	public HelpViewModel() => LoadItems();

	public override void RefreshForTheme() => LoadItems();

	protected override void ApplyLocalization() => LoadItems();

	void LoadItems()
	{
		Items.Clear();
		AddSection("\u25A1", Loc.T("Help_Section_GettingStarted"));
		AddCard("\u224B", Loc.T("Help_GettingStarted_Connect_Title"), Loc.T("Help_GettingStarted_Connect_Body"));
		AddCard("\u2697", Loc.T("Help_GettingStarted_Measure_Title"), Loc.T("Help_GettingStarted_Measure_Body"));
		AddSection("\u26A0", Loc.T("Help_Section_Troubleshooting"));
		AddCard("\u26A0", Loc.T("Help_Troubleshooting_NotFound_Title"), Loc.T("Help_Troubleshooting_NotFound_Body"));
		AddCard("\u21BB", Loc.T("Help_Troubleshooting_Lost_Title"), Loc.T("Help_Troubleshooting_Lost_Body"));
		AddSection("\u2709", Loc.T("Help_Section_Support"));
		AddCard("\u2709", Loc.T("Help_Support_Contact_Title"), Loc.T("Help_Support_Contact_Body"));
	}

	void AddSection(string icon, string title) =>
		Items.Add(new HelpItem { IsSection = true, Icon = icon, Title = title });

	void AddCard(string icon, string title, string body) =>
		Items.Add(new HelpItem { Icon = icon, Title = title, Body = body });

	[RelayCommand]
	async Task OpenDevicesAsync()
	{
		if (Shell.Current?.CurrentPage?.Navigation is not { } nav)
			return;
		await nav.PushAsync(AppServices.Get<DevicePage>());
	}
}
