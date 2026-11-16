using LazerArena.Presentation.Settings;

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

    /// <summary>
    /// V1.1: Wie viel groeber als die logischen Pixel des Stils der Effekt sein soll, damit er in beiden Stilen gleich
    /// aussieht (Klassisch 1, Modern 270/120 = 2,25).
    /// </summary>
    public static float CoarseFactor(GraphicsStyle style) =>
        Layout.ScreenLayout.ShortSide(style) / (float)Layout.ScreenLayout.ShortSide(GraphicsStyle.Classic);

    /// <summary>Anzahl der Bildzeilen: so viele, wie der klassische Stil bei gleicher Bildschirmhoehe haette.</summary>
    public static int ScanlineRows(int logicalHeight, GraphicsStyle style) =>
        Math.Max(1, (int)MathF.Round(logicalHeight / CoarseFactor(style)));

    /// <summary>
    /// Bildzeilen-Textur: Breite 1, Hoehe = logische Zeilen x <paramref name="rowParts"/>; wird auf die Bildhoehe
    /// gestreckt. Der letzte Teil jeder Zeile ist dunkel (OP-15: Staerke und Breite einstellbar).
    /// </summary>
    public static uint[] ScanlineTexture(int logicalRows, float darkness = ScanlineDarkness, int rowParts = RowParts)
    {
        var data = new uint[logicalRows * rowParts];
        uint dark = Premultiplied(0, 0, 0, darkness);
        for (int i = 0; i < data.Length; i++)
            data[i] = i % rowParts == rowParts - 1 ? dark : 0u;
        return data;
    }

    /// <summary>Vignette: Mitte unveraendert, zu den Ecken hin dunkler (gewoelbte Bildroehre).</summary>
    public static uint[] VignetteTexture(int size, float darkness = VignetteDarkness)
    {
        var data = new uint[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2 - 1, v = (y + 0.5f) / size * 2 - 1;
                data[y * size + x] = Premultiplied(0, 0, 0, VignetteAlpha(u, v, darkness));
            }
        return data;
    }

    /// <summary>Abdunklung an der Stelle (u, v) im Bereich -1..1.</summary>
    public static float VignetteAlpha(float u, float v, float darkness = VignetteDarkness)
    {
        float d = MathF.Sqrt(u * u * 0.6f + v * v * 0.6f);   // in den Ecken etwa 1,1
        float t = Math.Clamp((d - 0.55f) / 0.55f, 0f, 1f);
        return darkness * t * t;
    }

    /// <summary>RGBA in der Byte-Reihenfolge von MonoGame (R im niedrigsten Byte), Farbe mit Alpha vormultipliziert.</summary>
    public static uint Premultiplied(byte r, byte g, byte b, float alpha)
    {
        byte a = (byte)Math.Clamp((int)MathF.Round(alpha * 255), 0, 255);
        return ((uint)a << 24) | ((uint)(b * alpha) << 16) | ((uint)(g * alpha) << 8) | (uint)(r * alpha);
    }

    public static float AlphaOf(uint rgba) => (rgba >> 24) / 255f;
}
