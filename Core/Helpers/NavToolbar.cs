using HannaUIDemo;
using HannaUIDemo.Core.Constants;
using HannaUIDemo.Core.Localization;
using HannaUIDemo.Features.Device;
using HannaUIDemo.Features.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HannaUIDemo.Core.Helpers;

public static class NavToolbar
{
	/// <summary>Landing tab: Bluetooth scan and settings in the nav bar.</summary>
	public static void ConfigureLanding(ContentPage page)
	{
		if (Application.Current is not App app)
			return;

		var loc = app.Services.GetRequiredService<LocalizationService>();
		page.Title = loc.T("Shell_Home");
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateBluetoothScanItem(page, app, loc));
		page.ToolbarItems.Add(CreateSettingsItem(page, app, loc));
	}

	public static void Configure(ContentPage page, string titleLocalizationKey, bool includeDisconnect = false, Func<Task>? onDisconnectAsync = null)
	{
		if (Application.Current is not App app)
			return;

		var loc = app.Services.GetRequiredService<LocalizationService>();
		page.Title = loc.T(titleLocalizationKey);
		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateSettingsItem(page, app, loc));

		if (includeDisconnect && onDisconnectAsync is not null)
			page.ToolbarItems.Add(CreateDisconnectItem(loc, onDisconnectAsync));
	}

	/// <summary>
	/// Pushed detail pages with a fixed title (not Shell tab labels).
	/// When <paramref name="iconSource"/> is supplied the title is rendered as
	/// <c>[icon]  title</c> (with <paramref name="subtitle"/> stacked under the title if present).
	/// </summary>
	public static void ConfigureDetail(ContentPage page, string title, string? subtitle = null, string? iconSource = null)
	{
		if (Application.Current is not App app)
			return;

		var hasIcon = !string.IsNullOrWhiteSpace(iconSource);
		var hasSubtitle = !string.IsNullOrWhiteSpace(subtitle);

		if (!hasIcon && !hasSubtitle)
		{
			// Cheapest path — let Shell render its standard title label.
			Shell.SetTitleView(page, null);
			page.Title = title;
		}
		else
		{
			// Custom TitleView replaces the platform title; clear page.Title so platforms
			// (Android in particular) don't render both the string and the view.
			page.Title = string.Empty;
			Shell.SetTitleView(page, CreateTitleStack(title, subtitle, iconSource));
		}

		page.ToolbarItems.Clear();
		page.ToolbarItems.Add(CreateSettingsItem(page, app, app.Services.GetRequiredService<LocalizationService>()));
	}

	static View CreateTitleStack(string title, string? subtitle, string? iconSource)
	{
		var titleLabel = new Label
		{
			Text = title,
			FontSize = 17,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalOptions = LayoutOptions.Center,
			LineBreakMode = LineBreakMode.TailTruncation,
			MaxLines = 1
		};
		titleLabel.SetDynamicResource(Label.TextColorProperty, "OnSurface");

		View textBlock;
		if (!string.IsNullOrWhiteSpace(subtitle))
		{
			var subtitleLabel = new Label
			{
				Text = subtitle,
				FontSize = 12,
				HorizontalTextAlignment = TextAlignment.Center,
				LineBreakMode = LineBreakMode.TailTruncation,
				MaxLines = 2
			};
			subtitleLabel.SetDynamicResource(Label.TextColorProperty, "OnSurfaceVariant");

			textBlock = new VerticalStackLayout
			{
				Spacing = 1,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children = { titleLabel, subtitleLabel }
			};
		}
		else
		{
			textBlock = titleLabel;
		}

		if (string.IsNullOrWhiteSpace(iconSource))
			return textBlock;

		// Icon + text composition for "device_icon  device name" headers on per-device
		// detail screens (log history per family, photometer measure landing, etc.).
		var icon = new Image
		{
			Source = iconSource,
			Aspect = Aspect.AspectFit,
			WidthRequest = 22,
			HeightRequest = 22,
			VerticalOptions = LayoutOptions.Center
		};

		return new HorizontalStackLayout
		{
			Spacing = 8,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { icon, textBlock }
		};
	}

	static ToolbarItem CreateBluetoothScanItem(ContentPage page, App app, LocalizationService loc) => new()
	{
		Text = loc.T("Toolbar_ScanDevices"),
		Order = ToolbarItemOrder.Primary,
		Priority = 0,
		IconImageSource = "bluetooth_icon",
		Command = new Command(async () =>
		{
			await page.Navigation.PushAsync(app.Services.GetRequiredService<DevicePage>());
		})
	};

	static ToolbarItem CreateSettingsItem(ContentPage page, App app, LocalizationService loc) => new()
	{
		Text = loc.T("Toolbar_Profile"),
		Order = ToolbarItemOrder.Primary,
		Priority = 1,
		IconImageSource = "tab_profile",
		// IconImageSource = new FontImageSource
		// {
		// 	Glyph = "\u2699",
		// 	Size = 20,
		// 	Color = AppConstants.Primary
		// },
		Command = new Command(() => _ = ProfileNavigation.OpenProfileAsync(page))
	};

	static ToolbarItem CreateDisconnectItem(LocalizationService loc, Func<Task> onDisconnectAsync) => new()
	{
		Text = loc.T("Toolbar_Disconnect"),
		Order = ToolbarItemOrder.Primary,
		Priority = 2,
		Command = new Command(() => _ = onDisconnectAsync())
	};

	internal static ToolbarItem CreateProfileItem(ContentPage page, App app) =>
		CreateSettingsItem(page, app, app.Services.GetRequiredService<LocalizationService>());
}
