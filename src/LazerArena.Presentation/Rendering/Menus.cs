using LazerArena.Core;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Rendering;

public enum TitleAction { OnePlayer, TwoPlayers, Tutorial, ToggleStyle, Options }

/// <summary>Eintrag im Optionsbildschirm: Text-Schluessel und Werte zum Durchschalten.</summary>
public sealed record OptionEntry(string LabelKey, Func<UserSettings, string> ValueKey, Func<UserSettings, int, UserSettings> Step);

/// <summary>Inhalt des Optionsbildschirms (Abschnitt 10). Musik nur im modernen Stil (OP-8).</summary>
public static class OptionsMenu
{
    public static IReadOnlyList<OptionEntry> Entries(UserSettings s)
    {
        var list = new List<OptionEntry>
        {
            new("opt.style", x => x.Style == GraphicsStyle.Classic ? "val.classic" : "val.modern",
                (x, d) => x with { Style = Cycle(x.Style, d) }),
            new("opt.language", x => x.Language switch
                {
                    LanguageSetting.German => "val.german",
                    LanguageSetting.English => "val.english",
                    _ => "val.automatic",
                }, (x, d) => x with { Language = Cycle(x.Language, d) }),
            new("opt.aiLevel", x => x.AiLevel switch { AiLevel.Easy => "val.easy", AiLevel.Hard => "val.hard", _ => "val.medium" },
                (x, d) => x with { AiLevel = Cycle(x.AiLevel, d) }),
            new("opt.dockedBolt", x => x.DockedBolt == DockedBoltBehavior.Fizzle ? "val.fizzle" : "val.throwBack",
                (x, d) => x with { DockedBolt = Cycle(x.DockedBolt, d) }),
            s.Style == GraphicsStyle.Classic
                ? new("opt.bolts", x => BoltKey(x.ClassicBolts), (x, d) => x with { ClassicBolts = Cycle(x.ClassicBolts, d) })
                : new("opt.bolts", x => BoltKey(x.ModernBolts), (x, d) => x with { ModernBolts = Cycle(x.ModernBolts, d) }),
            new("opt.shieldShape", x => x.ShieldShape == ShieldShape.Straight ? "val.straight" : "val.curved",
                (x, d) => x with { ShieldShape = Cycle(x.ShieldShape, d) }),
            new("opt.walls", x => x.Walls == WallBehavior.Bounce ? "val.bounce" : "val.extinguish",
                (x, d) => x with { Walls = Cycle(x.Walls, d) }),
            new("opt.tvEffect", x => x.TvEffect ? "val.on" : "val.off", (x, _) => x with { TvEffect = !x.TvEffect }),
            new("opt.sound", x => x.Sound ? "val.on" : "val.off", (x, _) => x with { Sound = !x.Sound }),
            new("opt.voice", x => x.VoiceStyle switch { VoiceKind.Computer => "val.voiceComputer", VoiceKind.Soft => "val.voiceSoft", _ => "val.off" },
                (x, d) => x with { VoiceStyle = Cycle(x.VoiceStyle, d) }),
        };
        if (s.Style == GraphicsStyle.Modern)
        {
            list.Insert(1, new("opt.arena", x => "arena." + x.Arena, (x, d) => x with { Arena = Cycle(x.Arena, d) }));
            if (s.Arena == ArenaKind.SpaceStation)   // OP-18: nur die Raumstation hat Rundumleuchten
                list.Insert(2, StormLights);
            list.Add(new("opt.music", x => x.Music ? "val.on" : "val.off", (x, _) => x with { Music = !x.Music }));
            list.Add(new("opt.gameMusic", x => x.GameMusic switch
                {
                    GameTrack.Orbit => "track.orbit",
                    GameTrack.Nebula => "track.nebula",
                    GameTrack.Hyperdrive => "track.hyperdrive",
                    GameTrack.SunsetRun => "track.sunsetRun",
                    GameTrack.LaserNights => "track.laserNights",
                    GameTrack.PixelPulse => "track.pixelPulse",
                    GameTrack.Starlight => "track.starlight",
                    _ => "val.off",
                }, (x, d) => x with { GameMusic = Cycle(x.GameMusic, d) }));
        }
        return list;
    }

    public static readonly OptionEntry StormLights =
        new("opt.stormLights", x => x.StormLights ? "val.on" : "val.off", (x, _) => x with { StormLights = !x.StormLights });

    /// <summary>OP-16: „Standard“ setzt alle normalen Optionen zurueck; Grafikstil und erweiterte Optionen bleiben erhalten.</summary>
    public static UserSettings Reset(UserSettings s) => new() { Style = s.Style, Advanced = s.Advanced };

    private static string BoltKey(BoltShape shape) => shape == BoltShape.Regular ? "val.boltRegular" : "val.boltLightning";

    private static T Cycle<T>(T value, int direction) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        int i = Array.IndexOf(values, value);
        return values[((i + direction) % values.Length + values.Length) % values.Length];
    }
}

/// <summary>
/// OP-15: Versteckte erweiterte Optionen (Optionen-Knopf im Titel lange halten). Linke Haelfte verringert,
/// rechte erhoeht; Zahlen bleiben in ihrem Bereich stehen statt umzulaufen.
/// </summary>
public static class AdvancedMenu
{
    public const float HoldSeconds = 1.0f;   // so lange muss der Optionen-Knopf gehalten werden

    public static IReadOnlyList<OptionEntry> Entries(UserSettings s) =>
    [
        Percent("adv.tvScanlines", a => a.TvScanlineDarkness, (a, v) => a with { TvScanlineDarkness = v }, 5, 0, 100),
        new("adv.tvScanlineWidth", x => x.Advanced.TvScanlineWidth == AdvancedSettings.ScanlineWidth.Thin ? "val.thin" : "val.wide",
            (x, d) => x with { Advanced = x.Advanced with { TvScanlineWidth = Cycle(x.Advanced.TvScanlineWidth, d) } }),
        Percent("adv.tvGlow", a => a.TvGlow, (a, v) => a with { TvGlow = v }, 4, 0, 100),
        Percent("adv.tvGlowSpread", a => a.TvGlowSpread, (a, v) => a with { TvGlowSpread = v }, 1, 0, 10),
        new("adv.tvGlowSoftness", x => x.Advanced.TvGlowSoftness switch
            {
                AdvancedSettings.GlowSoftness.Sharp => "val.sharp",
                AdvancedSettings.GlowSoftness.Soft => "val.soft",
                _ => "val.medium",
            }, (x, d) => x with { Advanced = x.Advanced with { TvGlowSoftness = Cycle(x.Advanced.TvGlowSoftness, d) } }),
        Percent("adv.tvVignette", a => a.TvVignette, (a, v) => a with { TvVignette = v }, 5, 0, 100),
        Percent("adv.swordGlow", a => a.SwordGlow, (a, v) => a with { SwordGlow = v }, 10, 0, 200),
        Percent("adv.boltGlow", a => a.BoltGlow, (a, v) => a with { BoltGlow = v }, 10, 0, 200),
        Percent("adv.ringGlow", a => a.RingGlow, (a, v) => a with { RingGlow = v }, 10, 0, 200),
        Percent("adv.eyeGlow", a => a.EyeGlow, (a, v) => a with { EyeGlow = v }, 10, 0, 200),
    ];

    /// <summary>OP-16: „Standard“ setzt nur die erweiterten Optionen zurueck.</summary>
    public static UserSettings Reset(UserSettings s) => s with { Advanced = AdvancedSettings.Default };

    /// <summary>Prozentwert als Text, z. B. „48%“ (ohne Uebersetzung, der Localizer gibt unbekannte Schluessel zurueck).</summary>
    private static OptionEntry Percent(string label, Func<AdvancedSettings, int> get, Func<AdvancedSettings, int, AdvancedSettings> set,
        int step, int min, int max) =>
        new(label, x => $"{get(x.Advanced)}%",
            (x, d) => x with { Advanced = set(x.Advanced, Math.Clamp(get(x.Advanced) + d * step, min, max)) });

    private static T Cycle<T>(T value, int direction) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        int i = Array.IndexOf(values, value);
        return values[((i + direction) % values.Length + values.Length) % values.Length];
    }
}

/// <summary>
/// SO-1/OP-17: Im Pausenmenue lassen sich Spielmusik (nur moderner Stil, OP-8), Stimme und in der Raumstation
/// die Rundumleuchten (OP-18) umschalten.
/// Dieselben Eintraege wie im Optionsbildschirm.
/// </summary>
public static class PauseMenu
{
    private static readonly string[] Keys = ["opt.gameMusic", "opt.voice", "opt.stormLights"];

    public static IReadOnlyList<OptionEntry> Entries(UserSettings s) =>
        OptionsMenu.Entries(s).Where(e => Keys.Contains(e.LabelKey)).OrderBy(e => Array.IndexOf(Keys, e.LabelKey)).ToList();
}
