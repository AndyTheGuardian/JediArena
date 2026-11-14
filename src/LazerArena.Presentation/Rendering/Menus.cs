using LazerArena.Core;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Rendering;

public enum TitleAction { OnePlayer, TwoPlayers, ToggleStyle, Options }

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
            new("opt.swordTurning", x => x.SwordTurning == SwordTurning.Instant ? "val.instant" : "val.limited",
                (x, d) => x with { SwordTurning = Cycle(x.SwordTurning, d) }),
            new("opt.dockedBolt", x => x.DockedBolt == DockedBoltBehavior.Fizzle ? "val.fizzle" : "val.throwBack",
                (x, d) => x with { DockedBolt = Cycle(x.DockedBolt, d) }),
            new("opt.shieldShape", x => x.ShieldShape == ShieldShape.Straight ? "val.straight" : "val.curved",
                (x, d) => x with { ShieldShape = Cycle(x.ShieldShape, d) }),
            new("opt.walls", x => x.Walls == WallBehavior.Bounce ? "val.bounce" : "val.extinguish",
                (x, d) => x with { Walls = Cycle(x.Walls, d) }),
            new("opt.tvEffect", x => x.TvEffect ? "val.on" : "val.off", (x, _) => x with { TvEffect = !x.TvEffect }),
            new("opt.sound", x => x.Sound ? "val.on" : "val.off", (x, _) => x with { Sound = !x.Sound }),
            new("opt.voice", x => x.Voice ? "val.on" : "val.off", (x, _) => x with { Voice = !x.Voice }),
        };
        if (s.Style == GraphicsStyle.Modern)
        {
            list.Insert(1, new("opt.arena", x => "arena." + x.Arena, (x, d) => x with { Arena = Cycle(x.Arena, d) }));
            list.Add(new("opt.music", x => x.Music ? "val.on" : "val.off", (x, _) => x with { Music = !x.Music }));
            list.Add(new("opt.gameMusic", x => x.GameMusic switch
                {
                    GameTrack.Orbit => "track.orbit",
                    GameTrack.Nebula => "track.nebula",
                    GameTrack.Hyperdrive => "track.hyperdrive",
                    GameTrack.SunsetRun => "track.sunsetRun",
                    GameTrack.LaserNights => "track.laserNights",
                    _ => "val.off",
                }, (x, d) => x with { GameMusic = Cycle(x.GameMusic, d) }));
        }
        return list;
    }

    private static T Cycle<T>(T value, int direction) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        int i = Array.IndexOf(values, value);
        return values[((i + direction) % values.Length + values.Length) % values.Length];
    }
}
