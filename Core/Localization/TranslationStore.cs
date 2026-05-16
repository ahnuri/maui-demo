namespace HannaUIDemo.Core.Localization;

/// <summary>Neutral (English) and satellite string tables keyed by two-letter ISO language codes.</summary>
static class TranslationStore
{
	internal static readonly string[] SupportedLanguageCodes = ["en", "it", "fr", "es", "de", "pt", "nl"];

	static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Table =
		new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
		{
			["en"] = En(),
			["it"] = It(),
			["fr"] = Fr(),
			["es"] = Es(),
			["de"] = De(),
			["pt"] = Pt(),
			["nl"] = Nl(),
		};

	static IReadOnlyDictionary<string, string> En() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Home",
		["Shell_Measure"] = "Measure",
		["Shell_LogHistory"] = "Log History",
		["Shell_Information"] = "Information",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Help",
		["Flyout_VersionFormat"] = "Version: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Privacy Policy",
		["Flyout_LanguageLine"] = "Language: {0}",
		["Page_Language_Title"] = "Language",
		["Page_Language_Subtitle"] = "Choose the language for the app interface.",
		["Page_Language_Done"] = "Done",
		["PageToolbar_Info"] = "Device Information",
		["PageToolbar_Help"] = "Help & Support",
	};

	static IReadOnlyDictionary<string, string> It() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Home",
		["Shell_Measure"] = "Misura",
		["Shell_LogHistory"] = "Cronologia log",
		["Shell_Information"] = "Informazioni",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Guida",
		["Flyout_VersionFormat"] = "Versione: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Informativa sulla privacy",
		["Flyout_LanguageLine"] = "Lingua: {0}",
		["Page_Language_Title"] = "Lingua",
		["Page_Language_Subtitle"] = "Scegli la lingua dell'interfaccia.",
		["Page_Language_Done"] = "Fine",
		["PageToolbar_Info"] = "Informazioni dispositivo",
		["PageToolbar_Help"] = "Guida e supporto",
	};

	static IReadOnlyDictionary<string, string> Fr() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Accueil",
		["Shell_Measure"] = "Mesure",
		["Shell_LogHistory"] = "Historique des journaux",
		["Shell_Information"] = "Informations",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Aide",
		["Flyout_VersionFormat"] = "Version : {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Politique de confidentialité",
		["Flyout_LanguageLine"] = "Langue : {0}",
		["Page_Language_Title"] = "Langue",
		["Page_Language_Subtitle"] = "Choisissez la langue de l'application.",
		["Page_Language_Done"] = "OK",
		["PageToolbar_Info"] = "Informations appareil",
		["PageToolbar_Help"] = "Aide et support",
	};

	static IReadOnlyDictionary<string, string> Es() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Inicio",
		["Shell_Measure"] = "Medición",
		["Shell_LogHistory"] = "Historial de registros",
		["Shell_Information"] = "Información",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Ayuda",
		["Flyout_VersionFormat"] = "Versión: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Política de privacidad",
		["Flyout_LanguageLine"] = "Idioma: {0}",
		["Page_Language_Title"] = "Idioma",
		["Page_Language_Subtitle"] = "Elija el idioma de la interfaz.",
		["Page_Language_Done"] = "Listo",
		["PageToolbar_Info"] = "Información del dispositivo",
		["PageToolbar_Help"] = "Ayuda y soporte",
	};

	static IReadOnlyDictionary<string, string> De() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Start",
		["Shell_Measure"] = "Messung",
		["Shell_LogHistory"] = "Protokollverlauf",
		["Shell_Information"] = "Informationen",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Hilfe",
		["Flyout_VersionFormat"] = "Version: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Datenschutzrichtlinie",
		["Flyout_LanguageLine"] = "Sprache: {0}",
		["Page_Language_Title"] = "Sprache",
		["Page_Language_Subtitle"] = "Wählen Sie die Sprache der Benutzeroberfläche.",
		["Page_Language_Done"] = "Fertig",
		["PageToolbar_Info"] = "Geräteinformationen",
		["PageToolbar_Help"] = "Hilfe und Support",
	};

	static IReadOnlyDictionary<string, string> Pt() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Início",
		["Shell_Measure"] = "Medição",
		["Shell_LogHistory"] = "Histórico de registos",
		["Shell_Information"] = "Informações",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Ajuda",
		["Flyout_VersionFormat"] = "Versão: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Política de privacidade",
		["Flyout_LanguageLine"] = "Idioma: {0}",
		["Page_Language_Title"] = "Idioma",
		["Page_Language_Subtitle"] = "Escolha o idioma da interface.",
		["Page_Language_Done"] = "Concluído",
		["PageToolbar_Info"] = "Informações do dispositivo",
		["PageToolbar_Help"] = "Ajuda e suporte",
	};

	static IReadOnlyDictionary<string, string> Nl() => new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["Shell_Home"] = "Home",
		["Shell_Measure"] = "Meting",
		["Shell_LogHistory"] = "Loggeschiedenis",
		["Shell_Information"] = "Informatie",
		["Shell_HannaCloud"] = "Hanna Cloud",
		["Shell_Help"] = "Help",
		["Flyout_VersionFormat"] = "Versie: {0}",
		["Flyout_Copyright"] = "© 2014–{0} Hanna Instruments Inc.",
		["Flyout_PrivacyPolicy"] = "Privacybeleid",
		["Flyout_LanguageLine"] = "Taal: {0}",
		["Page_Language_Title"] = "Taal",
		["Page_Language_Subtitle"] = "Kies de taal van de app.",
		["Page_Language_Done"] = "Gereed",
		["PageToolbar_Info"] = "Apparaatinformatie",
		["PageToolbar_Help"] = "Help en ondersteuning",
	};

	internal static string Get(string languageCode, string key) =>
		Get(languageCode, key, Array.Empty<object>());

	internal static string Get(string languageCode, string key, params object[] args)
	{
		var lang = NormalizeLang(languageCode);
		if (!Table.TryGetValue(lang, out var dict))
			dict = Table["en"];
		if (!dict.TryGetValue(key, out var template))
		{
			if (!Table["en"].TryGetValue(key, out template))
				return key;
		}
		return args.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, template, args) : template;
	}

	internal static string NormalizeLang(string? code)
	{
		if (string.IsNullOrWhiteSpace(code))
			return "en";
		var two = code.Trim().Split('-', '_')[0];
		return Table.ContainsKey(two) ? two : "en";
	}
}
