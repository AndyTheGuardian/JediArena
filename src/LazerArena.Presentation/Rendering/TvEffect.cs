namespace LazerArena.Presentation.Rendering;

/// <summary>
/// Optionaler Roehrenfernseher-Effekt (OP-12): Bildzeilen, Leuchten und dunklere Ecken. Die Muster werden hier
/// plattformunabhaengig als RGBA-Daten erzeugt (vormultiplizierte Transparenz) und auf der Grafikkarte darueber gelegt.
/// </summary>
public static class TvEffect
{
    /// <summary>Jede logische Pixelzeile wird in 3 Teile geteilt: 2 hell, 1 dunkel (die „Luecke“ zwischen den Zeilen).</summary>
    public const int RowParts = 3;
    public const float ScanlineDarkness = 0.50f;
    public const float GlowStrength = 0.48f;
    public const float GlowSpread = 0.02f;      // Leuchten etwas groesser als das Bild
    public const float VignetteDarkness = 0.45f;

    /// <summary>Bildzeilen-Textur: Breite 1, Hoehe = logische Zeilen x 3; wird auf die Bildhoehe gestreckt.</summary>
    public static uint[] ScanlineTexture(int logicalRows)
    {
        var data = new uint[logicalRows * RowParts];
        uint dark = Premultiplied(0, 0, 0, ScanlineDarkness);
        for (int i = 0; i < data.Length; i++)
            data[i] = i % RowParts == RowParts - 1 ? dark : 0u;
        return data;
    }

    /// <summary>Vignette: Mitte unveraendert, zu den Ecken hin dunkler (gewoelbte Bildroehre).</summary>
    public static uint[] VignetteTexture(int size)
    {
        var data = new uint[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2 - 1, v = (y + 0.5f) / size * 2 - 1;
                data[y * size + x] = Premultiplied(0, 0, 0, VignetteAlpha(u, v));
            }
        return data;
    }

    /// <summary>Abdunklung an der Stelle (u, v) im Bereich -1..1.</summary>
    public static float VignetteAlpha(float u, float v)
    {
        float d = MathF.Sqrt(u * u * 0.6f + v * v * 0.6f);   // in den Ecken etwa 1,1
        float t = Math.Clamp((d - 0.55f) / 0.55f, 0f, 1f);
        return VignetteDarkness * t * t;
    }

    /// <summary>RGBA in der Byte-Reihenfolge von MonoGame (R im niedrigsten Byte), Farbe mit Alpha vormultipliziert.</summary>
    public static uint Premultiplied(byte r, byte g, byte b, float alpha)
    {
        byte a = (byte)Math.Clamp((int)MathF.Round(alpha * 255), 0, 255);
        return ((uint)a << 24) | ((uint)(b * alpha) << 16) | ((uint)(g * alpha) << 8) | (uint)(r * alpha);
    }

    public static float AlphaOf(uint rgba) => (rgba >> 24) / 255f;
}
