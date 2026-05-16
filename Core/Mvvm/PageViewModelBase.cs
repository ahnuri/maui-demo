using CommunityToolkit.Mvvm.ComponentModel;

namespace HannaUIDemo.Core.Mvvm;

/// <summary>Base for screen ViewModels — override <see cref="RefreshForTheme"/> when theme changes affect bound colors or data.</summary>
public abstract partial class PageViewModelBase : ObservableObject
{
	public virtual void RefreshForTheme() { }
}
