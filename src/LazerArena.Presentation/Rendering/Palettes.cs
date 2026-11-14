namespace LazerArena.Presentation.Rendering;

/// <summary>Farben des klassischen Stils: wenige flache Farben im Stil des Atari VCS.</summary>
public static class ClassicPalette
{
    public static readonly Rgb Black = Rgb.FromBytes(0, 0, 0);
    public static readonly Rgb Wall = Rgb.FromBytes(110, 110, 110);
    public static readonly Rgb Lane = Rgb.FromBytes(40, 40, 40);
    public static readonly Rgb P1 = Rgb.FromBytes(84, 138, 255);
    public static readonly Rgb P1Dark = Rgb.FromBytes(24, 59, 157);
    public static readonly Rgb P2 = Rgb.FromBytes(236, 112, 72);
    public static readonly Rgb P2Dark = Rgb.FromBytes(148, 40, 16);
    public static readonly Rgb Drone = Rgb.FromBytes(252, 224, 112);
    public static readonly Rgb DroneStorm = Rgb.FromBytes(236, 72, 72);
    public static readonly Rgb Neutral = Rgb.FromBytes(236, 236, 236);
    public static readonly Rgb BladeP1 = Rgb.FromBytes(160, 220, 255);
    public static readonly Rgb BladeP2 = Rgb.FromBytes(255, 170, 140);
    public static readonly Rgb Text = Rgb.FromBytes(236, 236, 236);
    public static readonly Rgb Grey = Rgb.FromBytes(140, 140, 140);
    public static readonly Rgb DarkGrey = Rgb.FromBytes(70, 70, 70);
    public static readonly Rgb Highlight = Rgb.FromBytes(252, 224, 112);

    public static readonly Rgb[] LogoBands =
    [
        Rgb.FromBytes(252, 252, 84), Rgb.FromBytes(252, 188, 116), Rgb.FromBytes(252, 148, 92), Rgb.FromBytes(236, 112, 72),
        Rgb.FromBytes(200, 72, 72), Rgb.FromBytes(176, 60, 140), Rgb.FromBytes(120, 72, 200),
    ];
}

/// <summary>
/// Farben des modernen Stils. Jedes fertige Bild wird mit geordnetem Dithering (Bayer 4x4) auf eine feste Palette
/// von 256 Farben abgebildet. Alle 256 stammen aus einer 12-Bit-Grundpalette (16 Stufen je Kanal = 4096 Farben),
/// wie bei Heimcomputern der 16-Bit-Zeit (F4). Die Auswahl wurde aus gerenderten Spielszenen ermittelt.
/// </summary>
public static class ModernPalette
{
    public const int MasterLevelsPerChannel = 16;   // 4 Bit je Kanal
    public static int MasterColorCount => MasterLevelsPerChannel * MasterLevelsPerChannel * MasterLevelsPerChannel;
    public const int MaxColors = 256;
    private const float DitherStrength = 22f;

    /// <summary>Die 256 Farben (0xRRGGBB); jeder Kanal ist ein Vielfaches von 17 (= 12-Bit-Farbe).</summary>
    public static readonly int[] Colors =
    [
        0x000000, 0xFFFFFF, 0x44DDFF, 0xFF44BB, 0xFFCC44, 0xEEEEFF, 0x778899, 0x111122,
        0x444477, 0x225588, 0x772255, 0xFFFFAA, 0xFF4422, 0xFFCC22, 0x99CCFF, 0x113355,
        0x111133, 0x445588, 0xCCCCDD, 0x9999BB, 0xFF8822, 0x667799, 0x441111, 0xFFEE77,
        0xFFDD77, 0x222222, 0x223344, 0x222233, 0x000022, 0x220022, 0x333333, 0x223333,
        0x002222, 0x223355, 0x220033, 0x332255, 0x002233, 0x332233, 0x333355, 0x222255,
        0x000033, 0x333377, 0x335577, 0x223377, 0x330000, 0x002255, 0x555577, 0x552255,
        0x8888AA, 0x225577, 0x330033, 0x003355, 0x777788, 0x555588, 0x227788, 0xFFAA77,
        0x332222, 0x225555, 0x552233, 0x553355, 0x33DDFF, 0xFF5533, 0xFFDD88, 0xFF8855,
        0x7788AA, 0x335588, 0x883377, 0x882277, 0x88AACC, 0x330055, 0x552222, 0xFFDDAA,
        0x8888CC, 0xFFCC88, 0x335555, 0xDDDDFF, 0x552277, 0x77FFFF, 0x330022, 0x882255,
        0xFF7733, 0xFFCC33, 0x003333, 0xFF88FF, 0xFFCC77, 0xFF7755, 0x7777AA, 0x33CCFF,
        0x005555, 0xFF55CC, 0x338888, 0x332277, 0xFF77FF, 0x55DDFF, 0xCCDDFF, 0x773377,
        0x555555, 0xAAAAAA, 0x88FFFF, 0x3388AA, 0xDDFFFF, 0xFFCCFF, 0xAAAACC, 0x55FFFF,
        0x220055, 0xFFAAFF, 0x227777, 0x337788, 0xDD55CC, 0x228888, 0xFF3322, 0x550033,
        0xCCFFFF, 0xAAFFFF, 0x55CCDD, 0x777777, 0x77DDFF, 0xFF33AA, 0x557788, 0xFF77CC,
        0xDD55AA, 0x888888, 0xFFDDFF, 0xFFDD33, 0xFF5522, 0x5577AA, 0xCCDDDD, 0x333388,
        0xFFFF88, 0xFF55DD, 0xCC55AA, 0xFF33CC, 0xFF8877, 0x33AACC, 0xAA3388, 0x88CCFF,
        0xDD77CC, 0x337777, 0xFF77DD, 0x553333, 0x33CCDD, 0x773355, 0x553377, 0xFF55FF,
        0x55CCFF, 0xAA3377, 0xCC33AA, 0x5577CC, 0x222277, 0xFFFFCC, 0xAA5588, 0x33AADD,
        0x3377AA, 0x77DDDD, 0x883388, 0x775577, 0x88AAAA, 0xCCCCCC, 0x557777, 0x3355AA,
        0x778888, 0xDD5555, 0x33FFFF, 0x7777CC, 0x775533, 0x773333, 0x7788CC, 0xFFAA55,
        0xFFFFDD, 0xDD77AA, 0xDD33AA, 0x2288AA, 0xDD8855, 0x885588, 0x3388CC, 0x553388,
        0x995533, 0x55AACC, 0xFF5555, 0xCC5555, 0x883344, 0xCC3388, 0x885566, 0xBB7733,
        0xAACCFF, 0x772211, 0xAA5555, 0xCCCC99, 0xAACCCC, 0x003377, 0xAA7766, 0xCC3322,
        0xFF7788, 0x772277, 0xDD3322, 0xBBAA88, 0xAA3333, 0xFFAAAA, 0x882211, 0x5588AA,
        0xCC8866, 0x88AADD, 0xCCCCFF, 0xAA2222, 0xAA55AA, 0xDD7788, 0x887777, 0xFF3333,
        0xDD5533, 0xBB88AA, 0x775555, 0xCC7766, 0xAA2277, 0x550000, 0xCC2222, 0x555533,
        0x887755, 0xDD33CC, 0xFFCCBB, 0xCCAABB, 0xAA8877, 0x22AACC, 0x55AADD, 0x5555AA,
        0x2277AA, 0xAAAAEE, 0x888866, 0xEE8833, 0xFFFF55, 0xFFDDCC, 0x005577, 0xDD8877,
        0xAA33AA, 0xAADDFF, 0x552200, 0x553322, 0xCC5588, 0xDDCCDD, 0x220000, 0xDD5577,
        0xDDDDCC, 0xEE88AA, 0xDDCCCC, 0x777755, 0xDD7733, 0x33AAAA, 0x77CCEE, 0x88CCDD,
        0x775588, 0x55DDDD, 0xDD7755, 0x770000, 0x333322, 0x77AADD, 0x773322, 0x8877BB,
    ];

    private static readonly Rgb[] PaletteRgb = Colors.Select(c => Rgb.FromBytes((c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF)).ToArray();
    private static readonly HashSet<int> ColorSet = [.. Colors];
    private static readonly Lazy<byte[]> Lookup = new(BuildLookup);

    /// <summary>Naechste Palettenfarbe fuer jede 15-Bit-Farbe (32768 Eintraege), einmalig berechnet.</summary>
    private static byte[] BuildLookup()
    {
        var lut = new byte[32768];
        for (int i = 0; i < lut.Length; i++)
        {
            float r = ((i >> 10) & 31) * 255f / 31, g = ((i >> 5) & 31) * 255f / 31, b = (i & 31) * 255f / 31;
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int k = 0; k < PaletteRgb.Length; k++)
            {
                float dr = r - PaletteRgb[k].R, dg = g - PaletteRgb[k].G, db = b - PaletteRgb[k].B;
                float d = 2 * dr * dr + 4 * dg * dg + 3 * db * db;   // grob wahrnehmungsgewichtet
                if (d < bestDistance) { bestDistance = d; best = k; }
            }
            lut[i] = (byte)best;
        }
        return lut;
    }

    public static readonly Rgb P1 = Rgb.FromBytes(60, 220, 255);
    public static readonly Rgb P1Armor = Rgb.FromBytes(40, 80, 140);
    public static readonly Rgb P2 = Rgb.FromBytes(255, 70, 190);
    public static readonly Rgb P2Armor = Rgb.FromBytes(120, 30, 90);
    public static readonly Rgb Neutral = Rgb.FromBytes(255, 210, 60);
    public static readonly Rgb Text = Rgb.FromBytes(235, 240, 255);
    public static readonly Rgb Dim = Rgb.FromBytes(120, 130, 160);
    public static readonly Rgb Panel = Rgb.FromBytes(14, 16, 34);
    public static readonly Rgb PanelLine = Rgb.FromBytes(60, 70, 120);
    public static readonly Rgb Shadow = Rgb.FromBytes(0, 0, 0);

    private static readonly int[,] Bayer = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };

    /// <summary>Bildet alle Pixel mit geordnetem Dithering auf die 256-Farben-Palette ab.</summary>
    public static void Quantize(PixelCanvas canvas)
    {
        var lut = Lookup.Value;
        var pixels = canvas.Pixels;
        for (int y = 0; y < canvas.Height; y++)
            for (int x = 0; x < canvas.Width; x++)
            {
                float offset = ((Bayer[y & 3, x & 3] + 0.5f) / 16f - 0.5f) * DitherStrength;
                ref var p = ref pixels[y * canvas.Width + x];
                int r = Channel(p.R + offset), g = Channel(p.G + offset), b = Channel(p.B + offset);
                p = PaletteRgb[lut[(r << 10) | (g << 5) | b]];
            }

        static int Channel(float v) => (int)(Math.Clamp(v, 0, 255) * 31 / 255 + 0.5f);
    }

    public static bool IsPaletteColor(Rgb c) => ColorSet.Contains(((int)c.R << 16) | ((int)c.G << 8) | (int)c.B);
}

/// <summary>
/// Bildet ein Bild auf eine feste Palette ab: naechste Farbe ueber eine 15-Bit-Nachschlagetabelle,
/// wahlweise mit geordnetem Dithering (Original-Arena) oder ohne (SNES-Arenen: Farbbaender statt Rastermuster).
/// </summary>
public sealed class PaletteQuantizer
{
    private static readonly int[,] Bayer = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };
    private readonly Rgb[] _palette;
    private readonly HashSet<int> _colors;
    private readonly Lazy<byte[]> _lookup;

    public PaletteQuantizer(IReadOnlyList<int> colors)
    {
        if (colors.Count > 256) throw new ArgumentException("Hoechstens 256 Farben", nameof(colors));
        Colors = colors;
        _palette = colors.Select(c => Rgb.FromBytes((c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF)).ToArray();
        _colors = [.. colors];
        _lookup = new Lazy<byte[]>(BuildLookup);
    }

    public IReadOnlyList<int> Colors { get; }

    public bool Contains(Rgb c) => _colors.Contains(((int)c.R << 16) | ((int)c.G << 8) | (int)c.B);

    public void Quantize(PixelCanvas canvas, float ditherStrength)
    {
        var lut = _lookup.Value;
        var pixels = canvas.Pixels;
        for (int y = 0; y < canvas.Height; y++)
            for (int x = 0; x < canvas.Width; x++)
            {
                float offset = ditherStrength > 0 ? ((Bayer[y & 3, x & 3] + 0.5f) / 16f - 0.5f) * ditherStrength : 0;
                ref var p = ref pixels[y * canvas.Width + x];
                p = _palette[lut[(Channel(p.R + offset) << 10) | (Channel(p.G + offset) << 5) | Channel(p.B + offset)]];
            }

        static int Channel(float v) => (int)(Math.Clamp(v, 0, 255) * 31 / 255 + 0.5f);
    }

    private byte[] BuildLookup()
    {
        var lut = new byte[32768];
        for (int i = 0; i < lut.Length; i++)
        {
            float r = ((i >> 10) & 31) * 255f / 31, g = ((i >> 5) & 31) * 255f / 31, b = (i & 31) * 255f / 31;
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int k = 0; k < _palette.Length; k++)
            {
                float dr = r - _palette[k].R, dg = g - _palette[k].G, db = b - _palette[k].B;
                float d = 2 * dr * dr + 4 * dg * dg + 3 * db * db;
                if (d < bestDistance) { bestDistance = d; best = k; }
            }
            lut[i] = (byte)best;
        }
        return lut;
    }
}

