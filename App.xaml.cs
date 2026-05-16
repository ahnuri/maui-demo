using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Settings;
using HannaUIDemo.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		Services = services;
		services.GetRequiredService<LocalizationService>().ApplyStoredLanguage();
		InitializeComponent();
		SemanticResources.Update(this);
		RequestedThemeChanged += OnRequestedThemeChanged;
	}

	public IServiceProvider Services { get; }

	void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
	{
		SemanticResources.Update(this);
		foreach (var window in Windows)
		{
			if (window.Page is AppShell appShell)
				appShell.ApplyTheme();

			if (window.Page is Shell { CurrentPage: { } current } && current.Navigation?.NavigationStack is { } stack)
			{
				foreach (var p in stack)
				{
					switch (p)
					{
						case DevicePage device:
							device.ApplyTheme();
							break;
						case SettingsPage settings:
							settings.ApplyTheme();
							break;
					}
				}
			}
		}
	}

	protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}
