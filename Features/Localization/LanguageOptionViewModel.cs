using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Features.Localization;

/// <summary>One language choice on the language picker screen.</summary>
public partial class LanguageOptionViewModel : ObservableObject
{
	public required string Code { get; init; }
	public required string DisplayName { get; init; }

	[ObservableProperty] private bool _isSelected;
}
