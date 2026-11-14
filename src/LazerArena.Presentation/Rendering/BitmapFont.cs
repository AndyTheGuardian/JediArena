namespace LazerArena.Presentation.Rendering;

public enum TextAlign { Left, Center, Right }

/// <summary>Eigene 5x7-Pixelschrift (Grossbuchstaben, Ziffern, Umlaute, Satzzeichen).</summary>
public static class BitmapFont
{
    public const int GlyphWidth = 5;
    public const int GlyphHeight = 7;

    private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string>
    {
        ['A'] = ".###.|#...#|#...#|#####|#...#|#...#|#...#",
        ['B'] = "####.|#...#|#...#|####.|#...#|#...#|####.",
        ['C'] = ".###.|#...#|#....|#....|#....|#...#|.###.",
        ['D'] = "####.|#...#|#...#|#...#|#...#|#...#|####.",
        ['E'] = "#####|#....|#....|####.|#....|#....|#####",
        ['F'] = "#####|#....|#....|####.|#....|#....|#....",
        ['G'] = ".###.|#...#|#....|#.###|#...#|#...#|.####",
        ['H'] = "#...#|#...#|#...#|#####|#...#|#...#|#...#",
        ['I'] = ".###.|..#..|..#..|..#..|..#..|..#..|.###.",
        ['J'] = "..###|...#.|...#.|...#.|...#.|#..#.|.##..",
        ['K'] = "#...#|#..#.|#.#..|##...|#.#..|#..#.|#...#",
        ['L'] = "#....|#....|#....|#....|#....|#....|#####",
        ['M'] = "#...#|##.##|#.#.#|#.#.#|#...#|#...#|#...#",
        ['N'] = "#...#|#...#|##..#|#.#.#|#..##|#...#|#...#",
        ['O'] = ".###.|#...#|#...#|#...#|#...#|#...#|.###.",
        ['P'] = "####.|#...#|#...#|####.|#....|#....|#....",
        ['Q'] = ".###.|#...#|#...#|#...#|#.#.#|#..#.|.##.#",
        ['R'] = "####.|#...#|#...#|####.|#.#..|#..#.|#...#",
        ['S'] = ".####|#....|#....|.###.|....#|....#|####.",
        ['T'] = "#####|..#..|..#..|..#..|..#..|..#..|..#..",
        ['U'] = "#...#|#...#|#...#|#...#|#...#|#...#|.###.",
        ['V'] = "#...#|#...#|#...#|#...#|#...#|.#.#.|..#..",
        ['W'] = "#...#|#...#|#...#|#.#.#|#.#.#|#.#.#|.#.#.",
        ['X'] = "#...#|#...#|.#.#.|..#..|.#.#.|#...#|#...#",
        ['Y'] = "#...#|#...#|.#.#.|..#..|..#..|..#..|..#..",
        ['Z'] = "#####|....#|...#.|..#..|.#...|#....|#####",
        ['Ä'] = ".#.#.|.....|.###.|#...#|#####|#...#|#...#",
        ['Ö'] = ".#.#.|.....|.###.|#...#|#...#|#...#|.###.",
        ['Ü'] = ".#.#.|.....|#...#|#...#|#...#|#...#|.###.",
        ['0'] = ".###.|#...#|#..##|#.#.#|##..#|#...#|.###.",
        ['1'] = "..#..|.##..|..#..|..#..|..#..|..#..|.###.",
        ['2'] = ".###.|#...#|....#|...#.|..#..|.#...|#####",
        ['3'] = "#####|...#.|..#..|...#.|....#|#...#|.###.",
        ['4'] = "...#.|..##.|.#.#.|#..#.|#####|...#.|...#.",
        ['5'] = "#####|#....|####.|....#|....#|#...#|.###.",
        ['6'] = "..##.|.#...|#....|####.|#...#|#...#|.###.",
        ['7'] = "#####|....#|...#.|..#..|.#...|.#...|.#...",
        ['8'] = ".###.|#...#|#...#|.###.|#...#|#...#|.###.",
        ['9'] = ".###.|#...#|#...#|.####|....#|...#.|.##..",
        [':'] = ".....|..#..|..#..|.....|..#..|..#..|.....",
        ['!'] = "..#..|..#..|..#..|..#..|..#..|.....|..#..",
        ['-'] = ".....|.....|.....|#####|.....|.....|.....",
        ['.'] = ".....|.....|.....|.....|.....|.....|..#..",
        ['='] = ".....|.....|#####|.....|#####|.....|.....",
        ['>'] = ".#...|..#..|...#.|....#|...#.|..#..|.#...",
        ['<'] = "...#.|..#..|.#...|#....|.#...|..#..|...#.",
        [' '] = ".....|.....|.....|.....|.....|.....|.....",
        ['*'] = ".....|..#..|#.#.#|.###.|#.#.#|..#..|.....",   // Funkelstern: markiert animierte Arenen
    }.ToDictionary(kv => kv.Key, kv => kv.Value.Split('|'));

    public static bool Supports(char c) => Glyphs.ContainsKey(char.ToUpperInvariant(c));

    public static int MeasureWidth(string text, int scale = 1) =>
        text.Length == 0 ? 0 : (text.Length * (GlyphWidth + 1) - 1) * scale;

    /// <summary>Zeichnet Text; <paramref name="color"/> erhaelt Glyphenzeile (0..6) und y-Abstand zur Oberkante.</summary>
    public static void Draw(PixelCanvas canvas, string text, int x, int y, Func<int, int, Rgb> color, int scale = 1,
        TextAlign align = TextAlign.Left, Rgb? shadow = null, Rgb? outline = null)
    {
        int width = MeasureWidth(text, scale);
        x = align switch { TextAlign.Center => x - width / 2, TextAlign.Right => x - width, _ => x };
        if (outline is { } o)   // 1-Pixel-Kontur rundherum (SNES-Schrift)
            Rasterize(text, scale, (px, py, _) =>
            {
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        canvas.Set(x + px + dx, y + py + dy, o);
            });
        if (shadow is { } s)
            Rasterize(text, scale, (px, py, _) => canvas.Set(x + px + scale, y + py + scale, s));
        Rasterize(text, scale, (px, py, row) => canvas.Set(x + px, y + py, color(row, py)));
    }

    public static void Draw(PixelCanvas canvas, string text, int x, int y, Rgb color, int scale = 1,
        TextAlign align = TextAlign.Left, Rgb? shadow = null, Rgb? outline = null) =>
        Draw(canvas, text, x, y, (_, _) => color, scale, align, shadow, outline);

    private static void Rasterize(string text, int scale, Action<int, int, int> plot)
    {
        string upper = text.ToUpperInvariant();
        for (int i = 0; i < upper.Length; i++)
        {
            if (!Glyphs.TryGetValue(upper[i], out var rows)) rows = Glyphs[' '];
            for (int gy = 0; gy < GlyphHeight; gy++)
                for (int gx = 0; gx < GlyphWidth; gx++)
                {
                    if (rows[gy][gx] != '#') continue;
                    int ox = (i * (GlyphWidth + 1) + gx) * scale, oy = gy * scale;
                    for (int sy = 0; sy < scale; sy++)
                        for (int sx = 0; sx < scale; sx++)
                            plot(ox + sx, oy + sy, gy);
                }
        }
    }
}
