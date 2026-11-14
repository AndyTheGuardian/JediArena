using System.Text.Json;
using System.Text.Json.Serialization;
using LazerArena.Core;

namespace LazerArena.Presentation.Settings;

public enum GraphicsStyle { Classic, Modern }    // OP-1
public enum LanguageSetting { Automatic, German, English }   // OP-2
public enum GameTrack { Off, Orbit, Nebula, Hyperdrive, SunsetRun, LaserNights }       // OP-10

/// <summary>OP-11: Arena (Aussehen des Spielfelds im modernen Stil).</summary>
public enum ArenaKind { Original, SpaceStation, SpaceFlight, ArcadeChrome, EnergyCore, Hologram, LavaForge }

public static class ArenaInfo
{
    /// <summary>Arenen mit bewegtem Hintergrund (im Optionsmenue mit ✱ markiert).</summary>
    public static bool IsAnimated(ArenaKind arena) =>
        arena is ArenaKind.SpaceFlight or ArenaKind.EnergyCore or ArenaKind.Hologram or ArenaKind.LavaForge;
}

/// <summary>Alle Optionen aus Abschnitt 10 des Regelwerks mit ihren Standardwerten.</summary>
public sealed record UserSettings
{
    public GraphicsStyle Style { get; init; } = GraphicsStyle.Modern;                  // OP-1
    public LanguageSetting Language { get; init; } = LanguageSetting.Automatic;        // OP-2
    public AiLevel AiLevel { get; init; } = AiLevel.Medium;                            // OP-3
    public SwordTurning SwordTurning { get; init; } = SwordTurning.Limited;            // OP-4
    public DockedBoltBehavior DockedBolt { get; init; } = DockedBoltBehavior.Fizzle;   // OP-5
    public ShieldShape ShieldShape { get; init; } = ShieldShape.Straight;              // OP-6
    public bool Sound { get; init; } = true;                                           // OP-7
    public bool Music { get; init; } = true;                                           // OP-8
    public WallBehavior Walls { get; init; } = WallBehavior.Bounce;                    // OP-9
    public GameTrack GameMusic { get; init; } = GameTrack.Orbit;                      // OP-10
    public ArenaKind Arena { get; init; } = ArenaKind.Original;                         // OP-11
    public bool TvEffect { get; init; } = false;                                        // OP-12: Bildzeilen + Roehrenleuchten
    public bool Voice { get; init; } = true;                                            // OP-13: Computerstimme

    /// <summary>SO-2: Nur spielrelevante Optionen gehen in die Logik; der Grafikstil nie.</summary>
    public GameOptions ToGameOptions() => new()
    {
        SwordTurning = SwordTurning,
        DockedBolt = DockedBolt,
        ShieldShape = ShieldShape,
        Walls = Walls,
    };

    /// <summary>OP-8: Titelmusik gibt es nur im modernen Stil.</summary>
    public bool MusicActive => Music && Style == GraphicsStyle.Modern;

    /// <summary>OP-10: Spielmusik gibt es nur im modernen Stil.</summary>
    public bool GameMusicActive => GameMusic != GameTrack.Off && Style == GraphicsStyle.Modern;
}

public interface ISettingsStore
{
    UserSettings Load();
    void Save(UserSettings settings);
}

/// <summary>Speichert die Optionen dauerhaft als JSON-Datei (Abschnitt 10).</summary>
public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public UserSettings Load()
    {
        try
        {
            return File.Exists(filePath)
                ? JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(filePath), JsonOptions) ?? new UserSettings()
                : new UserSettings();
        }
        catch (JsonException)
        {
            return new UserSettings();   // beschaedigte Datei: mit Standardwerten weiter
        }
    }

    public void Save(UserSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
