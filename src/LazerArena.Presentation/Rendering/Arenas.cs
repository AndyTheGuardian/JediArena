using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Rendering;

/// <summary>
/// Eine Arena im SNES-Look (OP-11): Hintergrund, Boden, Waende, Schild- und HUD-Farben. Gemeinsam ist allen:
/// kein Dithering, harte Schattierungsstufen, Konturen und transparente Blitze (zeichnet der <see cref="ModernRenderer"/>).
/// Der unbewegte Teil wird einmal je Bildschirmgroesse gezeichnet, der bewegte Teil in jedem Bild.
/// </summary>
internal abstract class SnesArena
{
    public static readonly Rgb Outline = Rgb.FromBytes(8, 8, 20);

    public abstract ArenaKind Kind { get; }
    public abstract IReadOnlyList<int> PaletteColors { get; }
    public bool Animated => ArenaInfo.IsAnimated(Kind);

    /// <summary>Hintergrund, Boden und Waende ohne Bewegung (wird zwischengespeichert).</summary>
    public abstract void DrawStatic(PixelCanvas c, ScreenLayout layout);

    /// <summary>Bewegter Bodenanteil, unter Schild, Figuren und Blitzen.</summary>
    public virtual void DrawAnimated(PixelCanvas c, ScreenLayout layout, int frame) { }

    /// <summary>Wirkt nach allem anderen auf das ganze Bild (z. B. Bildzeilen).</summary>
    public virtual void PostProcess(PixelCanvas c, int frame) { }

    public abstract (Rgb Light, Rgb Mid, Rgb Dark) Shield(PlayerId id);

    /// <summary>Hintergrund einer HUD-Leiste oder Seitentafel; <paramref name="lineAtBottom"/>: Zierlinie zum Spielfeld hin unten.</summary>
    public abstract void DrawBar(PixelCanvas bar, bool lineAtBottom);

    public abstract Rgb EmptyPip { get; }
    public abstract Rgb Dim { get; }
    public abstract Rgb Button { get; }
    public virtual bool EnergyBars => false;
    public virtual Func<int, int, Rgb>? LabelColor => null;

    public static SnesArena? Create(ArenaKind kind) => kind switch
    {
        ArenaKind.SpaceStation => new SpaceStationArena(),
        ArenaKind.SpaceFlight => new SpaceFlightArena(),
        ArenaKind.ArcadeChrome => new ArcadeChromeArena(),
        ArenaKind.EnergyCore => new EnergyCoreArena(),
        ArenaKind.Hologram => new HologramArena(),
        ArenaKind.LavaForge => new LavaForgeArena(),
        _ => null,
    };

    protected static Rgb C(int r, int g, int b) => Rgb.FromBytes(r, g, b);
}

/// <summary>Arena 2 „Raumstation“: Metallplatten mit Nieten, Warnstreifen-Waende, Stahl-HUD mit Schrauben.</summary>
internal sealed class SpaceStationArena : SnesArena
{
    public override ArenaKind Kind => ArenaKind.SpaceStation;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.SpaceStation;
    public override Rgb EmptyPip => C(40, 44, 56);
    public override Rgb Dim => C(220, 226, 240);
    public override Rgb Button => C(70, 76, 92);

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(160, 240, 255), C(50, 160, 210), C(16, 70, 110))
        : (C(255, 180, 220), C(200, 60, 150), C(90, 20, 70));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout)
    {
        c.Clear(C(20, 22, 30));
        var f = layout.Field;
        for (int ty = f.Y; ty < f.Bottom; ty += 16)
            for (int tx = f.X; tx < f.Right; tx += 16)
            {
                int x1 = Math.Min(tx + 15, f.Right - 1), y1 = Math.Min(ty + 15, f.Bottom - 1);
                bool even = ((tx - f.X) / 16 + (ty - f.Y) / 16) % 2 == 0;
                c.FillRect(tx, ty, x1, y1, even ? C(58, 64, 78) : C(52, 58, 72));
                c.FillRect(tx, ty, x1, ty, C(84, 92, 110));
                c.FillRect(tx, ty, tx, y1, C(84, 92, 110));
                c.FillRect(tx, y1, x1, y1, C(30, 34, 44));
                c.FillRect(x1, ty, x1, y1, C(30, 34, 44));
                foreach (var (rx, ry) in new[] { (3, 3), (12, 3), (3, 12), (12, 12) })
                    if (tx + rx < f.Right && ty + ry < f.Bottom) c.Set(tx + rx, ty + ry, C(120, 128, 146));
            }
        foreach (int x0 in new[] { f.X - 8, f.Right })   // Waende mit Warnstreifen
        {
            for (int y = f.Y; y < f.Bottom; y++)
                for (int x = x0; x < x0 + 8; x++)
                    c.Set(x, y, (x + y) / 4 % 2 == 0 ? C(230, 180, 30) : C(30, 26, 20));
            c.RectOutline(x0, f.Y, x0 + 7, f.Bottom - 1, Outline);
        }
        int my = f.Y + f.Height / 2;
        for (int x = f.X; x < f.Right; x += 8) c.FillRect(x, my - 1, Math.Min(x + 4, f.Right - 1), my, C(90, 200, 120));
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom)
    {
        int w = bar.Width, h = bar.Height;
        bar.BandedGradient(0, 0, w - 1, h - 1, C(110, 118, 136), C(50, 56, 70), 6);
        bar.RectOutline(0, 0, w - 1, h - 1, Outline);
        bar.FillRect(1, 1, w - 2, 1, C(170, 178, 196));
        foreach (int sx in new[] { 4, w - 7 })
            foreach (int sy in new[] { 4, h - 7 })
            {
                bar.FillEllipse(sx + 1, sy + 1, 1.5f, 1.5f, C(200, 206, 220));
                bar.Set(sx + 2, sy + 2, C(40, 44, 56));
            }
    }
}

/// <summary>
/// Flug durchs All: Sterne kommen aus einem Mittelpunkt, fliegen nach aussen, werden dabei schneller, heller
/// und ziehen kurze Streifen (Arena „Sternenflug“ und Titelbild).
/// </summary>
internal sealed class FlightStarfield
{
    private const float FramesPerApproach = 240f;   // 4 s bei 60 Bildern/s
    private static readonly Rgb[] Levels = [Rgb.FromBytes(70, 70, 110), Rgb.FromBytes(120, 120, 170), Rgb.FromBytes(180, 180, 225), Rgb.FromBytes(255, 255, 255)];
    private readonly (Vector2 Dir, float Phase)[] _stars;

    public FlightStarfield(int count, int seed)
    {
        var random = new Random(seed);
        _stars = Enumerable.Range(0, count).Select(_ =>
        {
            float a = (float)(random.NextDouble() * Math.Tau), spread = 0.15f + (float)random.NextDouble() * 0.85f;
            return (new Vector2(MathF.Cos(a), MathF.Sin(a)) * spread, (float)random.NextDouble());
        }).ToArray();
    }

    public void Draw(PixelCanvas c, Vector2 center, float spread, int frame)
    {
        foreach (var (dir, phase) in _stars)
        {
            float p = (phase + frame / FramesPerApproach) % 1f;
            Vector2 Project(float progress) => center + dir / (1f - 0.94f * progress) * spread;
            var pos = Project(p);
            float z = 1f - 0.94f * p;
            int level = Math.Min(3, (int)((1 - z) * 4.2f));
            if (level >= 2)
            {
                var before = Project(MathF.Max(0, p - 0.012f));
                c.Line(before.X, before.Y, pos.X, pos.Y, Levels[level - 1]);
            }
            if (z < 0.2f) c.FillRect((int)pos.X, (int)pos.Y, (int)pos.X + 1, (int)pos.Y + 1, Levels[level]);
            else c.Set((int)pos.X, (int)pos.Y, Levels[level]);
        }
    }
}

/// <summary>Arena 3 „Sternenflug“: durchsichtige Glas-Arena, darunter ein Flug durchs All (Sterne stroemen aus der Mitte).</summary>
internal sealed class SpaceFlightArena : SnesArena
{
    private readonly FlightStarfield _stars = new(90, 3);

    public override ArenaKind Kind => ArenaKind.SpaceFlight;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.SpaceFlight;
    public override Rgb EmptyPip => C(30, 30, 70);
    public override Rgb Dim => C(170, 190, 240);
    public override Rgb Button => C(30, 40, 90);

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(200, 250, 255), C(80, 200, 240), C(20, 90, 140))
        : (C(255, 200, 240), C(230, 80, 180), C(110, 20, 90));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout) =>
        c.BandedGradient(0, 0, c.Width - 1, c.Height - 1, C(4, 2, 16), C(14, 6, 34), 8);

    public override void DrawAnimated(PixelCanvas c, ScreenLayout layout, int frame)
    {
        var f = layout.Field;
        _stars.Draw(c, new Vector2(f.X + f.Width / 2f, f.Y + f.Height / 2f), f.Width * 0.134f, frame);
        // Glasboden mit Gitter ueber den Sternen, Leuchtschienen
        c.BlendRect(f.X, f.Y, f.Right - 1, f.Bottom - 1, C(20, 30, 70), 0.47f);
        for (int x = f.X; x < f.Right; x += 24) c.BlendRect(x, f.Y, x, f.Bottom - 1, C(80, 120, 220), 0.27f);
        for (int y = f.Y; y < f.Bottom; y += 24) c.BlendRect(f.X, y, f.Right - 1, y, C(80, 120, 220), 0.27f);
        foreach (int x0 in new[] { f.X - 3, f.Right })
        {
            c.FillRect(x0, f.Y, x0 + 2, f.Bottom - 1, C(120, 200, 255));
            c.FillRect(x0 + 1, f.Y, x0 + 1, f.Bottom - 1, C(230, 250, 255));
        }
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom)
    {
        bar.BlendRect(0, 0, bar.Width - 1, bar.Height - 1, C(10, 10, 40), 0.67f);
        int y = lineAtBottom ? bar.Height - 1 : 0;
        bar.FillRect(0, y, bar.Width - 1, y, C(120, 200, 255));
    }
}

/// <summary>Arena 4 „Arcade-Chrom“: Schachbrettboden, Chromschienen, Chrom-Schrift und Energieleisten statt Rauten.</summary>
internal sealed class ArcadeChromeArena : SnesArena
{
    private static readonly Rgb[] Chrome =
        [C(255, 255, 255), C(230, 236, 250), C(200, 210, 235), C(150, 160, 200), C(230, 200, 120), C(250, 220, 140), C(255, 240, 180)];

    public override ArenaKind Kind => ArenaKind.ArcadeChrome;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.ArcadeChrome;
    public override Rgb EmptyPip => C(40, 40, 60);
    public override Rgb Dim => C(200, 210, 235);
    public override Rgb Button => C(60, 60, 100);
    public override bool EnergyBars => true;
    public override Func<int, int, Rgb>? LabelColor => (row, _) => Chrome[Math.Min(row, Chrome.Length - 1)];

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(210, 250, 255), C(70, 190, 240), C(20, 80, 130))
        : (C(255, 210, 240), C(230, 70, 170), C(100, 20, 80));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout)
    {
        c.BandedGradient(0, 0, c.Width - 1, c.Height - 1, C(10, 20, 60), C(60, 10, 50), 10);
        var f = layout.Field;
        for (int ty = f.Y; ty < f.Bottom; ty += 16)
            for (int tx = f.X; tx < f.Right; tx += 16)
            {
                bool even = ((tx - f.X) / 16 + (ty - f.Y) / 16) % 2 == 0;
                int x1 = Math.Min(tx + 15, f.Right - 1), y1 = Math.Min(ty + 15, f.Bottom - 1);
                c.FillRect(tx, ty, x1, y1, even ? C(26, 70, 96) : C(18, 48, 70));
                if (even) c.FillRect(tx, ty, x1, ty, C(40, 96, 124));
            }
        foreach (int x0 in new[] { f.X - 4, f.Right })
            c.BandedGradient(x0, f.Y, x0 + 3, f.Bottom - 1, C(240, 240, 255), C(90, 100, 140), 6);
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom) =>
        bar.BandedGradient(0, 0, bar.Width - 1, bar.Height - 1, C(40, 40, 80), C(10, 10, 30), 4);
}

/// <summary>Arena 5 „Energiekern“: sehr schwache weisse Leiterbahnen mit wandernden Impulsen, Energiekanal in der Mitte.</summary>
internal sealed class EnergyCoreArena : SnesArena
{
    private List<List<Vector2>> _paths = [];
    private float[] _pathOffsets = [];

    public override ArenaKind Kind => ArenaKind.EnergyCore;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.EnergyCore;
    public override Rgb EmptyPip => C(40, 20, 60);
    public override Rgb Dim => C(200, 170, 240);
    public override Rgb Button => C(60, 30, 100);

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(180, 255, 240), C(40, 200, 190), C(10, 80, 90))
        : (C(255, 190, 250), C(200, 70, 220), C(80, 20, 100));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout)
    {
        c.BandedGradient(0, 0, c.Width - 1, c.Height - 1, C(20, 6, 40), C(6, 20, 40), 9);
        var f = layout.Field;
        c.FillRect(f.X, f.Y, f.Right - 1, f.Bottom - 1, C(16, 14, 34));
        _paths = CircuitPaths(f);
        var random = new Random(6);
        _pathOffsets = _paths.Select(_ => (float)random.NextDouble()).ToArray();
        var white = C(255, 255, 255);
        foreach (var path in _paths)   // Leiterbahnen: weiss, sehr schwach
        {
            c.BlendPolyline(path, white, 1, 0.1f);
            for (int i = 2; i < path.Count; i += 2)
                c.BlendRect((int)path[i].X - 1, (int)path[i].Y - 1, (int)path[i].X + 1, (int)path[i].Y + 1, white, 0.16f);
        }
        int my = f.Y + f.Height / 2;   // Energiekanal
        foreach (var (width, alpha) in new[] { (14, 0.16f), (8, 0.31f), (3, 0.63f) })
            c.BlendRect(f.X, my - width / 2, f.Right - 1, my + width / 2, C(120, 60, 255), alpha);
        foreach (int x0 in new[] { f.X - 3, f.Right })
            c.BandedGradient(x0, f.Y, x0 + 2, f.Bottom - 1, C(200, 120, 255), C(60, 220, 220), 8);
    }

    /// <summary>Weisse Impulse laufen die Bahnen entlang; nach dem Ende kurze Pause, dann von vorn.</summary>
    public override void DrawAnimated(PixelCanvas c, ScreenLayout layout, int frame)
    {
        var white = C(255, 255, 255);
        for (int i = 0; i < _paths.Count; i++)
        {
            var path = _paths[i];
            float length = 0;
            for (int k = 1; k < path.Count; k++) length += Vector2.Distance(path[k - 1], path[k]);
            float cycle = length + 40, head = (frame / 90f + _pathOffsets[i]) % 1f * cycle;
            for (int j = 0; j < 12; j++)
            {
                float pos = head - j * 1.5f;
                if (pos < 0 || pos > length || PointOn(path, pos) is not { } p) continue;
                c.Blend((int)p.X, (int)p.Y, white, 0.59f * (1 - j / 12f));
                if (j == 0) c.BlendRect((int)p.X - 1, (int)p.Y - 1, (int)p.X + 1, (int)p.Y + 1, white, 0.35f);
            }
        }
    }

    private static List<List<Vector2>> CircuitPaths(IntRect f)
    {
        var random = new Random(5);
        var paths = new List<List<Vector2>>();
        int count = Math.Max(8, f.Width * f.Height / 4700);
        for (int n = 0; n < count; n++)
        {
            int x = f.X + random.Next(0, Math.Max(1, f.Width / 8)) * 8, y = f.Y + random.Next(0, Math.Max(1, f.Height / 8)) * 8;
            var path = new List<Vector2> { new(x, y) };
            for (int s = random.Next(3, 8); s > 0; s--)
            {
                int nx = Math.Clamp(x + new[] { 0, 8, 16, -8 }[random.Next(4)], f.X, f.Right - 1);
                int ny = Math.Clamp(y + new[] { 8, 16, 0, -8 }[random.Next(4)], f.Y, f.Bottom - 1);
                path.Add(new Vector2(nx, y));
                path.Add(new Vector2(nx, ny));
                x = nx; y = ny;
            }
            paths.Add(path);
        }
        return paths;
    }

    private static Vector2? PointOn(List<Vector2> path, float distance)
    {
        for (int k = 1; k < path.Count; k++)
        {
            float seg = Vector2.Distance(path[k - 1], path[k]);
            if (distance <= seg) return seg == 0 ? path[k] : Vector2.Lerp(path[k - 1], path[k], distance / seg);
            distance -= seg;
        }
        return null;
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom)
    {
        bar.BandedGradient(0, 0, bar.Width - 1, bar.Height - 1, C(40, 14, 70), C(14, 6, 30), 4);
        int y = lineAtBottom ? bar.Height - 1 : 0;
        for (int x = 0; x < bar.Width; x += 6) bar.FillRect(x, y, x + 3, y, C(200, 120, 255));
    }
}

/// <summary>Arena 9 „Hologramm“: gruenes Holo-Gitter, das sehr dezent atmet, Bildzeilen wie auf einem Roehrenfernseher.</summary>
internal sealed class HologramArena : SnesArena
{
    public override ArenaKind Kind => ArenaKind.Hologram;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.Hologram;
    public override Rgb EmptyPip => C(10, 50, 30);
    public override Rgb Dim => C(120, 255, 170);
    public override Rgb Button => C(10, 60, 30);

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(180, 255, 255), C(60, 200, 220), C(20, 80, 100))
        : (C(255, 190, 240), C(220, 70, 170), C(90, 20, 70));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout)
    {
        c.Clear(C(0, 6, 4));
        var f = layout.Field;
        foreach (int x0 in new[] { f.X - 2, f.Right }) c.FillRect(x0, f.Y, x0 + 1, f.Bottom - 1, C(80, 255, 150));
        foreach (var (cx, cy) in new[] { (f.X, f.Y), (f.Right, f.Y), (f.X, f.Bottom), (f.Right, f.Bottom) })
        {
            c.FillRect(cx - 6, cy, cx + 6, cy, C(160, 255, 200));
            c.FillRect(cx, cy - 6, cx, cy + 6, C(160, 255, 200));
        }
    }

    public override void DrawAnimated(PixelCanvas c, ScreenLayout layout, int frame)
    {
        var f = layout.Field;
        float breath = 0.5f + 0.5f * MathF.Sin(frame * MathF.Tau / 120f);   // 2 s je Atemzug
        var grid = Rgb.Lerp(C(12, 66, 38), C(16, 82, 48), breath);
        for (int x = f.X; x < f.Right; x += 16)
        {
            c.BlendRect(x - 1, f.Y, x - 1, f.Bottom - 1, grid, 0.07f * breath);
            c.BlendRect(x + 1, f.Y, x + 1, f.Bottom - 1, grid, 0.07f * breath);
            c.FillRect(x, f.Y, x, f.Bottom - 1, grid);
        }
        for (int y = f.Y; y < f.Bottom; y += 16) c.FillRect(f.X, y, f.Right - 1, y, grid);
        int my = f.Y + f.Height / 2;
        c.FillRect(f.X, my, f.Right - 1, my, C(60, 220, 120));
    }

    public override void PostProcess(PixelCanvas c, int frame)
    {
        for (int y = 0; y < c.Height; y += 2) c.DarkenRow(y, 0.73f);
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom)
    {
        bar.Clear(C(0, 20, 12));
        bar.RectOutline(2, 2, bar.Width - 3, bar.Height - 3, C(60, 220, 120));
    }
}

/// <summary>Arena 10 „Lava-Schmiede“: Basaltplatten, durch die Spalten glueht Lava, die sehr dezent pulsiert.</summary>
internal sealed class LavaForgeArena : SnesArena
{
    private static readonly Rgb[] LavaRamp = [C(110, 26, 8), C(140, 38, 10), C(170, 54, 12), C(200, 74, 16), C(225, 100, 22)];
    private (int X, int Y)[] _cracks = [];

    public override ArenaKind Kind => ArenaKind.LavaForge;
    public override IReadOnlyList<int> PaletteColors => ArenaPalettes.LavaForge;
    public override Rgb EmptyPip => C(40, 34, 40);
    public override Rgb Dim => C(255, 200, 140);
    public override Rgb Button => C(90, 40, 20);

    public override (Rgb, Rgb, Rgb) Shield(PlayerId id) => id == PlayerId.One
        ? (C(170, 240, 255), C(50, 160, 220), C(20, 70, 110))
        : (C(255, 190, 230), C(220, 60, 160), C(100, 20, 70));

    public override void DrawStatic(PixelCanvas c, ScreenLayout layout)
    {
        c.BandedGradient(0, 0, c.Width - 1, c.Height - 1, C(40, 8, 4), C(160, 50, 10), 10);
        var f = layout.Field;
        var isSlab = new bool[f.Width * f.Height];
        var random = new Random(10);
        for (int ty = f.Y; ty < f.Bottom; ty += 20)
            for (int tx = f.X; tx < f.Right; tx += 20)
            {
                int jx = random.Next(0, 2), jy = random.Next(0, 2);
                var slab = new[] { C(38, 34, 40), C(46, 40, 46), C(32, 28, 34) }[random.Next(3)];
                int x0 = tx + jx, y0 = ty + jy, x1 = Math.Min(tx + 19, f.Right - 1), y1 = Math.Min(ty + 19, f.Bottom - 1);
                c.FillRect(x0, y0, x1, y1, slab);
                c.FillRect(x0, y0, x1, y0, C(70, 62, 70));
                c.FillRect(x0, y1, x1, y1, C(20, 16, 22));
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                        isSlab[(y - f.Y) * f.Width + (x - f.X)] = true;
            }
        var cracks = new List<(int, int)>();
        for (int k = 0; k < isSlab.Length; k++)
            if (!isSlab[k]) cracks.Add((f.X + k % f.Width, f.Y + k / f.Width));
        _cracks = [.. cracks];
        foreach (int x0 in new[] { f.X - 6, f.Right })
        {
            c.FillRect(x0, f.Y, x0 + 5, f.Bottom - 1, C(60, 50, 56));
            c.FillRect(x0, f.Y, x0, f.Bottom - 1, C(120, 110, 120));
        }
    }

    /// <summary>Lava in den Spalten: zwei langsame Helligkeitswellen, in 5 Stufen (3 s je Durchlauf).</summary>
    public override void DrawAnimated(PixelCanvas c, ScreenLayout layout, int frame)
    {
        float phase = frame * MathF.Tau / 180f;
        foreach (var (x, y) in _cracks)
        {
            float v = 0.5f + 0.09f * MathF.Sin(phase + x * 0.045f + y * 0.03f) + 0.07f * MathF.Sin(-phase + y * 0.07f - x * 0.02f);
            c[x, y] = LavaRamp[Math.Clamp((int)(v * 5), 0, 4)];
        }
    }

    public override void DrawBar(PixelCanvas bar, bool lineAtBottom)
    {
        bar.BandedGradient(0, 0, bar.Width - 1, bar.Height - 1, C(80, 72, 80), C(30, 26, 32), 5);
        bar.RectOutline(0, 0, bar.Width - 1, bar.Height - 1, Outline);
        int y = lineAtBottom ? bar.Height - 2 : 1;
        bar.FillRect(0, y, bar.Width - 1, y, C(255, 140, 30));
    }
}
