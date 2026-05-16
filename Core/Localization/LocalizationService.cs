using System.Globalization;

namespace HannaUIDemo.Core.Localization;

public sealed class LocalizationService
{
	public const string PreferenceKey = "app_ui_language";

	static readonly IReadOnlyDictionary<string, string> Autonyms =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["en"] = "English",
			["it"] = "Italiano",
			["fr"] = "Français",
			["es"] = "Español",
			["de"] = "Deutsch",
			["pt"] = "Português",
			["nl"] = "Nederlands",
		};

	static readonly IReadOnlyDictionary<string, string> CultureMap =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["en"] = "en-US",
			["it"] = "it-IT",
			["fr"] = "fr-FR",
			["es"] = "es-ES",
			["de"] = "de-DE",
			["pt"] = "pt-PT",
			["nl"] = "nl-NL",
		};

	public string CurrentLanguageCode { get; private set; } = "en";

	public void ApplyStoredLanguage()
	{
		var stored = Preferences.Get(PreferenceKey, "en");
		SetLanguage(stored, persist: false);
	}

	public void SetLanguage(string twoLetterCode, bool persist = true)
	{
		var code = TranslationStore.NormalizeLang(twoLetterCode);
		CurrentLanguageCode = code;
		if (persist)
			Preferences.Set(PreferenceKey, code);

		var cultureName = CultureMap.TryGetValue(code, out var mapped) ? mapped : "en-US";
		var culture = CultureInfo.GetCultureInfo(cultureName);
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;

		CultureChanged?.Invoke(this, EventArgs.Empty);
	}

	public static event EventHandler? CultureChanged;

	public string T(string key) =>
		TranslationStore.Get(CurrentLanguageCode, key);

	public string T(string key, params object[] args) =>
		TranslationStore.Get(CurrentLanguageCode, key, args);

	public string GetAutonym(string? languageCode)
	{
		var code = TranslationStore.NormalizeLang(languageCode);
		return Autonyms.TryGetValue(code, out var name) ? name : Autonyms["en"];
	}

	public IReadOnlyList<(string Code, string Autonym)> GetLanguageOptions() =>
		TranslationStore.SupportedLanguageCodes
			.Select(c => (c, Autonyms[c]))
			.ToList();
}
