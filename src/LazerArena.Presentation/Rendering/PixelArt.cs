namespace LazerArena.Presentation.Rendering;

/// <summary>Bilder im modernen Stil (Sieg, Niederlage, Titel), erzeugt mit tools/art/convert_art.py.</summary>
public enum ArtId { Victory, Defeat, Title }

/// <summary>
/// Vorberechnete Pixelgrafik mit Farben aus <see cref="ModernPalette.Colors"/> und durchsichtigem Hintergrund.
/// Dateiformat .lzpx: „LZPX“, Breite und Hoehe (uint16 LE), dann je Pixel ein Paletteindex (uint16 LE, 0xFFFF = durchsichtig).
/// </summary>
public sealed class PixelArt
{
    private const ushort Transparent = 0xFFFF;
    private static readonly Dictionary<string, PixelArt> Cache = [];
    private readonly ushort[] _pixels;

    public PixelArt(int width, int height, ushort[] pixels)
    {
        if (pixels.Length != width * height) throw new ArgumentException("Pixelzahl passt nicht zur Groesse", nameof(pixels));
        Width = width;
        Height = height;
        _pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }

    public bool IsTransparent(int x, int y) => _pixels[y * Width + x] == Transparent;

    /// <summary>Grafik fuer die Ausrichtung (hochkant/quer haben eigene, ungestreckte Groessen).</summary>
    public static PixelArt Get(ArtId id, bool landscape)
    {
        string name = $"{id.ToString().ToLowerInvariant()}-{(landscape ? "quer" : "hoch")}";
        lock (Cache)
        {
            if (!Cache.TryGetValue(name, out var art)) Cache[name] = art = Load(name);
            return art;
        }
    }

    public static PixelArt Parse(byte[] data)
    {
        if (data.Length < 8 || data[0] != 'L' || data[1] != 'Z' || data[2] != 'P' || data[3] != 'X')
            throw new InvalidDataException("Keine LZPX-Datei");
        int w = BitConverter.ToUInt16(data, 4), h = BitConverter.ToUInt16(data, 6);
        if (data.Length != 8 + w * h * 2) throw new InvalidDataException("LZPX-Datei unvollstaendig");
        var pixels = new ushort[w * h];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = BitConverter.ToUInt16(data, 8 + i * 2);
            if (pixels[i] != Transparent && pixels[i] >= ModernPalette.Colors.Length)
                throw new InvalidDataException("Farbindex ausserhalb der Palette");
        }
        return new PixelArt(w, h, pixels);
    }

    private static PixelArt Load(string name)
    {
        using var stream = typeof(PixelArt).Assembly.GetManifestResourceStream($"LazerArena.Presentation.Art.{name}.lzpx")
            ?? throw new FileNotFoundException($"Bild {name} fehlt");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return Parse(memory.ToArray());
    }

    /// <summary>Zeichnet die Grafik mit der Mitte ihrer Unterkante bei (<paramref name="centerX"/>, <paramref name="bottom"/>).</summary>
    public void DrawBottomCentered(PixelCanvas canvas, int centerX, int bottom) => Draw(canvas, centerX - Width / 2, bottom - Height);

    public void Draw(PixelCanvas canvas, int left, int top)
    {
        var colors = ModernPalette.Colors;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                ushort i = _pixels[y * Width + x];
                if (i == Transparent) continue;
                int c = colors[i];
                canvas.Set(left + x, top + y, Rgb.FromBytes((c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF));
            }
    }
}
