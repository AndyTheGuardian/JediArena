using System.Globalization;
using System.Reflection;
using System.Text.Json;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Localization;

/// <summary>SO-5: Automatisch -> Deutsch bei deutscher Systemsprache, sonst Englisch. Manuell fest einstellbar.</summary>
public static class LanguageResolver
{
    public const string German = "de";
    public const string English = "en";

    public static string Resolve(LanguageSetting setting, CultureInfo systemCulture) => setting switch
    {
        LanguageSetting.German => German,
        LanguageSetting.English => English,
        _ => systemCulture.TwoLetterISOLanguageName == German ? German : English,
    };
}

/// <summary>Liefert Texte aus den eingebetteten Ressourcendateien Strings.&lt;sprache&gt;.json (SO-5).</summary>
public sealed class Localizer
{
    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> Cache = [];
    private readonly IReadOnlyDictionary<string, string> _texts;
    private readonly IReadOnlyDictionary<string, string> _fallback;

    public Localizer(string language)
    {
        Language = language;
        _texts = LoadTable(language);
        _fallback = LoadTable(LanguageResolver.English);
    }

    public string Language { get; }

    public static IReadOnlyCollection<string> AvailableLanguages => [LanguageResolver.German, LanguageResolver.English];

    public string this[string key] => _texts.TryGetValue(key, out var text) ? text
        : _fallback.TryGetValue(key, out var fallback) ? fallback
        : key;

    public static IReadOnlyCollection<string> KeysOf(string language) => LoadTable(language).Keys.ToList();

    private static IReadOnlyDictionary<string, string> LoadTable(string language)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(language, out var cached)) return cached;
            var assembly = typeof(Localizer).Assembly;
            string resource = $"LazerArena.Presentation.Localization.Strings.{language}.json";
            using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Sprachdatei fehlt: {resource}");
            var table = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? [];
            Cache[language] = table;
            return table;
        }
    }
}
