using System.Text.Json;
using System.Text.Json.Serialization;
using LazerArena.Core;

namespace LazerArena.Presentation.Settings;

public enum GraphicsStyle { Classic, Modern }    // OP-1
public enum LanguageSetting { Automatic, German, English }   // OP-2
public enum GameTrack { Off, Orbit, Nebula, Hyperdrive, SunsetRun, LaserNights, PixelPulse, Starlight }       // OP-10
public enum VoiceKind { Off, Computer, Soft }                                           // OP-13
public enum BoltShape { Regular, Lightning }                                            // OP-14

/// <summary>OP-11: Arena (Aussehen des Spielfelds im modernen Stil).</summary>
public enum ArenaKind { Original, SpaceStation, SpaceFlight, ArcadeChrome, EnergyCore, Hologram, LavaForge }

public static class ArenaInfo
{
    /// <summary>Arenen mit bewegtem Hintergrund (im Optionsmenue mit „animiert“ markiert).</summary>
    public static bool IsAnimated(ArenaKind arena) =>
        arena is ArenaKind.SpaceFlight or ArenaKind.EnergyCore or ArenaKind.Hologram or ArenaKind.LavaForge;
}

/// <summary>Alle Optionen aus Abschnitt 10 des Regelwerks mit ihren Standardwerten.</summary>
public sealed record UserSettings
{
    public GraphicsStyle Style { get; init; } = GraphicsStyle.Modern;                  // OP-1
    public LanguageSetting Language { get; init; } = LanguageSetting.Automatic;        // OP-2
    public AiLevel AiLevel { get; init; } = AiLevel.Medium;                            // OP-3
    public DockedBoltBehavior DockedBolt { get; init; } = DockedBoltBehavior.Fizzle;   // OP-5
    public ShieldShape ShieldShape { get; init; } = ShieldShape.Straight;              // OP-6
    public bool Sound { get; init; } = true;                                           // OP-7
    public bool Music { get; init; } = true;                                           // OP-8
    public WallBehavior Walls { get; init; } = WallBehavior.Bounce;                    // OP-9
    public GameTrack GameMusic { get; init; } = GameTrack.Orbit;                      // OP-10
    /// <summary>OP-18: Rote Rundumleuchten der Raumstation im Gewitter.</summary>
    public bool StormLights { get; init; } = true;
    public ArenaKind Arena { get; init; } = ArenaKind.Original;                         // OP-11
    public bool TvEffect { get; init; } = false;                                        // OP-12: Bildzeilen + Roehrenleuchten
    public VoiceKind VoiceStyle { get; init; } = VoiceKind.Computer;                   // OP-13: Stimme
    public BoltShape ClassicBolts { get; init; } = BoltShape.Regular;                   // OP-14: Blitzform je Stil
    public BoltShape ModernBolts { get; init; } = BoltShape.Lightning;
    public AdvancedSettings Advanced { get; init; } = AdvancedSettings.Default;            // OP-15: versteckt

    /// <summary>OP-13: Ansagen an, sobald eine Stimme gewaehlt ist.</summary>
    public bool Voice => VoiceStyle != VoiceKind.Off;

    /// <summary>OP-14: Blitzform des aktuellen Grafikstils.</summary>
    public BoltShape Bolts => Style == GraphicsStyle.Classic ? ClassicBolts : ModernBolts;

    /// <summary>SO-2: Nur spielrelevante Optionen gehen in die Logik; der Grafikstil nie. OP-4: Schwertdrehung immer begrenzt.</summary>
    public GameOptions ToGameOptions() => new()
    {
        SwordTurning = SwordTurning.Limited,
        DockedBolt = DockedBolt,
        ShieldShape = ShieldShape,
        Walls = Walls,
    };

    /// <summary>OP-8: Titelmusik gibt es nur im modernen Stil.</summary>
    public bool MusicActive => Music && Style == GraphicsStyle.Modern;

    /// <summary>OP-10: Spielmusik gibt es nur im modernen Stil.</summary>
    public bool GameMusicActive => GameMusic != GameTrack.Off && Style == GraphicsStyle.Modern;
}

/// <summary>
/// OP-19: Jeder Grafikstil hat einen eigenen, vollstaendigen Satz Optionen. <see cref="Style"/> ist der aktive Stil;
/// beim Wechsel des Stils wird dessen gespeicherter Satz aktiv.
/// </summary>
public sealed record SettingsProfiles(GraphicsStyle Style, UserSettings Classic, UserSettings Modern)
{
    public SettingsProfiles() : this(GraphicsStyle.Modern, new UserSettings { Style = GraphicsStyle.Classic }, new UserSettings()) { }

    /// <summary>Optionen des aktiven Stils.</summary>
    public UserSettings Active => For(Style);

    public UserSettings For(GraphicsStyle style) => style == GraphicsStyle.Classic ? Classic : Modern;

    /// <summary>
    /// Uebernimmt geaenderte Optionen. Hat sich der Stil geaendert, bleibt der bisherige Satz unveraendert und der
    /// gespeicherte Satz des neuen Stils wird aktiv; sonst werden die Optionen im aktiven Satz gespeichert.
    /// </summary>
    public SettingsProfiles Apply(UserSettings changed)
    {
        if (changed.Style != Style) return this with { Style = changed.Style };
        return Style == GraphicsStyle.Classic ? this with { Classic = changed } : this with { Modern = changed };
    }

    /// <summary>Aeltere Dateien (ein Satz fuer beide Stile): beide Saetze starten mit denselben Werten.</summary>
    public static SettingsProfiles FromSingle(UserSettings s) =>
        new(s.Style, s with { Style = GraphicsStyle.Classic }, s with { Style = GraphicsStyle.Modern });
}

public interface ISettingsStore
{
    SettingsProfiles Load();
    void Save(SettingsProfiles profiles);
}

/// <summary>Speichert die Optionen beider Stile dauerhaft als JSON-Datei (Abschnitt 10, OP-19).</summary>
public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public SettingsProfiles Load()
    {
        try
        {
            if (!File.Exists(filePath)) return new SettingsProfiles();
            string json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(nameof(SettingsProfiles.Classic), out _))
            {
                var p = JsonSerializer.Deserialize<SettingsProfiles>(json, JsonOptions) ?? new SettingsProfiles();
                // Jeder Satz traegt seinen eigenen Stil, auch wenn die Datei von Hand veraendert wurde
                return p with { Classic = p.Classic with { Style = GraphicsStyle.Classic }, Modern = p.Modern with { Style = GraphicsStyle.Modern } };
            }
            return SettingsProfiles.FromSingle(JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? new UserSettings());
        }
        catch (JsonException)
        {
            return new SettingsProfiles();   // beschaedigte Datei: mit Standardwerten weiter
        }
    }

    public void Save(SettingsProfiles profiles)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(profiles, JsonOptions));
    }
}
