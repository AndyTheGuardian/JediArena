namespace LazerArena.Presentation.Settings;

/// <summary>
/// OP-15: Erweiterte Optionen (versteckt: Optionen-Knopf im Titel lange halten). Feineinstellungen fuer den
/// TV-Effekt (OP-12) und das Leuchten von Klingen, Blitzen und Drohne. Reine Darstellung, nie Spiellogik (SO-2).
/// Prozentwerte sind ganze Zahlen; die Leuchtstaerken sind relativ zum Standard (100 %).
/// </summary>
public sealed record AdvancedSettings
{
    public enum ScanlineWidth { Thin, Wide }
    public enum GlowSoftness { Sharp, Medium, Soft }

    public int TvScanlineDarkness { get; init; } = 50;   // Abdunklung der Bildzeilen in %
    public ScanlineWidth TvScanlineWidth { get; init; } = ScanlineWidth.Thin;   // 1/3 oder 1/2 der Pixelzeile
    public int TvGlow { get; init; } = 48;               // Roehrenleuchten in %
    public int TvGlowSpread { get; init; } = 2;          // Ueberstand des Leuchtens in % der Bildgroesse
    public GlowSoftness TvGlowSoftness { get; init; } = GlowSoftness.Medium;   // Weichheit des Leuchtens
    public int TvVignette { get; init; } = 45;           // Abdunklung der Ecken in %
    public int SwordGlow { get; init; } = 100;           // Leuchten der Laserschwerter, relativ in %
    public int BoltGlow { get; init; } = 100;            // Leuchten der Blitze, relativ in %
    public int RingGlow { get; init; } = 100;            // Leuchten des roten Drohnenrings im Gewitter, relativ in %
    public int EyeGlow { get; init; } = 100;             // Leuchten des Drohnen-„Auges“, relativ in %

    public static AdvancedSettings Default { get; } = new();

    /// <summary>Anteil der dunklen Linie an einer logischen Pixelzeile.</summary>
    public int ScanlineRowParts => TvScanlineWidth == ScanlineWidth.Thin ? 3 : 2;

    /// <summary>Verkleinerung des Leuchtbildes: kleiner = weicheres, breiteres Leuchten.</summary>
    public int GlowDownscale => TvGlowSoftness switch { GlowSoftness.Sharp => 2, GlowSoftness.Soft => 4, _ => 3 };
}
