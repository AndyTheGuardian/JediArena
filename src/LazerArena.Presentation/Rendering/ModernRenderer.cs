using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Settings;
using P = LazerArena.Presentation.Rendering.ModernPalette;

namespace LazerArena.Presentation.Rendering;

/// <summary>
/// Moderner Stil: 16-Bit-Retro, Palette 256 aus 4096 Farben (F4). Das Spielfeld zeigt die gewaehlte Arena (OP-11):
/// „Original“ mit Leuchteffekten und Dithering, die uebrigen im SNES-Look (Konturen, harte Schattierung,
/// transparente Blitze, kein Dithering). Menues sehen in allen Arenen gleich aus.
/// </summary>
public sealed class ModernRenderer : StyleRenderer
{
    private static readonly Rgb Ink = SnesArena.Outline;
    private static readonly Rgb[] ArmorP1 = [Rgb.FromBytes(14, 34, 70), Rgb.FromBytes(28, 66, 120), Rgb.FromBytes(52, 104, 170), Rgb.FromBytes(110, 160, 220)];
    private static readonly Rgb[] ArmorP2 = [Rgb.FromBytes(50, 10, 40), Rgb.FromBytes(100, 26, 80), Rgb.FromBytes(160, 50, 130), Rgb.FromBytes(220, 120, 190)];
    private static readonly Rgb[] Helmet = [Rgb.FromBytes(70, 76, 96), Rgb.FromBytes(130, 138, 160), Rgb.FromBytes(190, 196, 214), Rgb.FromBytes(240, 244, 255)];
    private const int HiltLength = 11;
    private const float OriginalBoltGlow = 1.7f;    // Leuchtstaerke der Blitze in der Arena „Original“
    private const float OriginalBladeGlow = 1.4f;   // Leuchtstaerke der Klingen in der Arena „Original“
    private const float StormRingGlow = 0.8f;       // dezentes Leuchten des roten Gewitterrings
    private const int StormPulseFrames = 50;        // Pulsdauer des roten Lichts im Gewitter (bei 60 Bildern/s)
    private const float SnesGlowFactor = 0.75f;     // SNES-Arenen: 75 % des Original-Leuchtens

    private readonly SnesArena? _arena;
    private readonly PaletteQuantizer? _arenaQuantizer;
    private PixelCanvas? _background;
    private readonly FlightStarfield _titleStars = new(110, 21);
    private PixelCanvas? _arenaStatic;
    private ScreenLayout? _arenaLayout;

    public ModernRenderer(ArenaKind arena = ArenaKind.Original)
    {
        Arena = arena;
        _arena = SnesArena.Create(arena);
        if (_arena is not null) _arenaQuantizer = new PaletteQuantizer(_arena.PaletteColors);
    }

    public ArenaKind Arena { get; }

    /// <summary>Farben, die diese Arena im Spiel verwenden darf (fuer Tests).</summary>
    public bool IsArenaColor(Rgb c) => _arenaQuantizer?.Contains(c) ?? ModernPalette.IsPaletteColor(c);

    protected override void BeginGameFrame(PixelCanvas canvas, ScreenLayout layout)
    {
        if (_arena is null)
        {
            BeginFrame(canvas, layout);
            return;
        }
        if (_arenaStatic is null || _arenaLayout != layout)
        {
            _arenaStatic = new PixelCanvas(canvas.Width, canvas.Height);
            _arena.DrawStatic(_arenaStatic, layout);
            _arenaLayout = layout;
        }
        canvas.CopyFrom(_arenaStatic);
    }

    protected override void FinishGame(PixelCanvas canvas)
    {
        if (_arena is null)
        {
            Finish(canvas);
            return;
        }
        _arena.PostProcess(canvas, CurrentFrame);
        _arenaQuantizer!.Quantize(canvas, ditherStrength: 0);   // SNES-Look: Farbbaender statt Dithering
    }

    public override GraphicsStyle Style => GraphicsStyle.Modern;
    protected override Rgb HintColor => P.Dim;

    private static Rgb Team(PlayerId id) => id == PlayerId.One ? P.P1 : P.P2;
    private static Rgb Team(BoltOwner owner) => owner switch { BoltOwner.Player1 => P.P1, BoltOwner.Player2 => P.P2, _ => P.Neutral };

    protected override void BeginFrame(PixelCanvas canvas, ScreenLayout layout)
    {
        if (_background is null || _background.Width != canvas.Width || _background.Height != canvas.Height)
            _background = CreateNebula(canvas.Width, canvas.Height);
        canvas.CopyFrom(_background);
    }

    protected override void Finish(PixelCanvas canvas) => ModernPalette.Quantize(canvas);

    /// <summary>Weltraum-Hintergrund: weiche Rauschwolken in Violett/Tuerkis und Sterne (einmal je Groesse berechnet).</summary>
    private static PixelCanvas CreateNebula(int w, int h)
    {
        var canvas = new PixelCanvas(w, h);
        var noiseA = new SmoothNoise(w / 40 + 2, h / 40 + 2, 11);
        var noiseB = new SmoothNoise(w / 25 + 2, h / 25 + 2, 23);
        for (int y = 0; y < h; y++)
        {
            float t = y / (float)h;
            for (int x = 0; x < w; x++)
            {
                float a = MathF.Max(0, noiseA.Sample(x / (float)w, y / (float)h) * 2 - 1);
                float b = MathF.Max(0, noiseB.Sample(x / (float)w, y / (float)h) * 2 - 1);
                canvas[x, y] = new Rgb(8 + 70 * a * a + 10 * t, 8 + 20 * a * a + 60 * b * b, 22 + 70 * a * a + 70 * b * b + 20 * (1 - t));
            }
        }
        var random = new Random(w * 31 + h);
        for (int i = 0; i < w * h / 220; i++)
        {
            int v = new[] { 90, 140, 200, 255 }[random.Next(4)];
            canvas.Set(random.Next(w), random.Next(h), new Rgb(v, v, MathF.Min(255, v + 30)));
        }
        return canvas;
    }

    // ------------------------------------------------------------------ Spielfeld

    protected override void DrawFieldBackground(PixelCanvas canvas, FieldMapper map)
    {
        if (_arena is not null)
        {
            _arena.DrawAnimated(canvas, _arenaLayout!, CurrentFrame);
            _arena.DrawStormLights(canvas, _arenaLayout!, CurrentFrame, StormActive && StormLights);
            return;
        }
        var f = map.Field;
        const int tile = 18;
        for (int y = f.Y; y < f.Bottom; y++)
            for (int x = f.X; x < f.Right; x++)
            {
                int lx = (x - f.X) % tile, ly = (y - f.Y) % tile;
                int b = 26 + (int)(10 * (1 - MathF.Abs((y - f.Y) / (float)f.Height - 0.5f) * 2));
                canvas[x, y] = lx == 0 || ly == 0 ? Rgb.FromBytes(b + 14, b + 18, b + 30)
                    : lx == tile - 1 || ly == tile - 1 ? Rgb.FromBytes(b - 10, b - 9, b - 2)
                    : Rgb.FromBytes(b, b + 3, b + 14);
            }
        var wall = Rgb.FromBytes(70, 80, 110);
        var wallLight = Rgb.FromBytes(140, 150, 190);
        canvas.FillRect(f.X - 3, f.Y, f.X - 1, f.Bottom - 1, wall);
        canvas.FillRect(f.Right, f.Y, f.Right + 2, f.Bottom - 1, wall);
        canvas.Line(f.X - 2, f.Y, f.X - 2, f.Bottom - 1, wallLight);
        canvas.Line(f.Right + 1, f.Y, f.Right + 1, f.Bottom - 1, wallLight);
        int my = f.Y + f.Height / 2;
        canvas.GlowLine(f.X, my, f.Right, my, Rgb.FromBytes(40, 60, 120), 2, 3, 1);
        for (int x = f.X; x < f.Right; x += 6) canvas.Line(x, my, x + 2, my, Rgb.FromBytes(80, 110, 180));
    }

    protected override void DrawShield(PixelCanvas canvas, FieldMapper map, Shield shield, PlayerId owner)
    {
        if (_arena is not null)
        {
            DrawSnesShield(canvas, map, shield, _arena.Shield(owner));
            return;
        }
        var col = Team(owner);
        foreach (var s in shield.Segments.Where(s => Visible(s, owner)))
        {
            var (x0, y0, x1, y1) = map.ToScreen(s.Bounds);
            x0 += 1; x1 -= 1;   // Fuge nur in der Grafik (A5)
            float f = s.Row == 0 ? 1f : 0.7f;
            canvas.GlowRect(x0, y0, x1, y1, col.Scale(1 / 3f), 2, 1);
            canvas.FillRect(x0, y0, x1, y1, col.Scale(0.55f * f));
            canvas.Line(x0, y0, x1, y0, new Rgb(MathF.Min(255, col.R * f + 60), MathF.Min(255, col.G * f + 60), MathF.Min(255, col.B * f + 60)));
            canvas.Line(x0, y1, x1, y1, col.Scale(0.3f * f));
        }
    }

    protected override void DrawPlayer(PixelCanvas canvas, FieldMapper map, Player player, GameConfig config)
    {
        var c = map.ToScreen(player.Position);
        float r = map.Field.Width * 0.045f;
        var (hand, tip) = SwordPoints(map, player, config);
        // Auf ganze Pixel legen: sonst haengt die Klingenbreite vom Bruchteil der Position ab (oben breiter als unten)
        hand = Snap(hand);
        tip = Snap(tip);
        var dir = Vector2.Normalize(tip - hand);
        var col = Team(player.Id);
        float swordGlow = OriginalBladeGlow * Look.SwordGlow / 100f;   // OP-15
        float visorY = c.Y + player.Forward * r * 0.35f;

        if (_arena is not null)   // SNES: Kontur, 4 Schattierungsstufen, deckende Klinge
        {
            canvas.FillEllipse(c.X, c.Y, r * 2f + 1, r * 1.05f + 1, Ink);
            canvas.PosterizedEllipse(c.X, c.Y, r * 2f, r * 1.05f, player.Id == PlayerId.One ? ArmorP1 : ArmorP2);
            canvas.FillEllipse(c.X, c.Y, r * 0.95f + 1, r * 0.95f + 1, Ink);
            canvas.PosterizedEllipse(c.X, c.Y, r * 0.95f, r * 0.95f, Helmet);
            canvas.Line(c.X - r * 0.5f, visorY, c.X + r * 0.5f, visorY, Rgb.FromBytes(20, 20, 40));
            var bladeStart = Snap(DrawHilt(canvas, hand, dir, outline: true));
            // Leuchten wie in der Arena „Original“, mit 75 % der Staerke
            canvas.GlowLine(bladeStart.X, bladeStart.Y, tip.X, tip.Y, col, 4, 3, swordGlow * SnesGlowFactor);
            DrawBlade(canvas, bladeStart, tip, col);
            return;
        }

        canvas.ShadedEllipse(c.X, c.Y, r * 2f, r * 1.05f, player.Id == PlayerId.One ? P.P1Armor : P.P2Armor);
        canvas.ShadedEllipse(c.X, c.Y, r * 0.95f, r * 0.95f, Rgb.FromBytes(200, 205, 220));
        canvas.Line(c.X - r * 0.5f, visorY, c.X + r * 0.5f, visorY, Rgb.FromBytes(30, 30, 50));
        var start = Snap(DrawHilt(canvas, hand, dir, outline: false));
        canvas.GlowLine(start.X, start.Y, tip.X, tip.Y, col, 4, 3, swordGlow);
        DrawBlade(canvas, start, tip, col);
    }

    /// <summary>Deckende Klinge: 3 Pixel in hellem Teamton, in der Mitte fast weiss.</summary>
    private static readonly Rgb White = Rgb.FromBytes(255, 255, 255);
    private const float BladeEdgeWhite = 0.45f;   // Anteil Weiss am Rand von Klinge und Blitz
    private const float BladeCoreWhite = 0.92f;   // fast weisser Kern
    public const int BladeWidth = 3;
    public const int BoltWidth = 2;               // Blitze: wie die Klinge, nur etwas schmaler

    private static void DrawBlade(PixelCanvas canvas, Vector2 start, Vector2 tip, Rgb col)
    {
        canvas.Line(start.X, start.Y, tip.X, tip.Y, Rgb.Lerp(col, White, BladeEdgeWhite), BladeWidth);
        canvas.Line(start.X, start.Y, tip.X, tip.Y, Rgb.Lerp(col, White, BladeCoreWhite));
    }

    private static Vector2 Snap(Vector2 p) => new(MathF.Round(p.X), MathF.Round(p.Y));

    /// <summary>Schlichter Schwertgriff ohne seitliche Teile: Griff mit Rillen und Endkappe; liefert den Klingenansatz.</summary>
    private static Vector2 DrawHilt(PixelCanvas canvas, Vector2 hand, Vector2 dir, bool outline)
    {
        var start = hand - dir * 2;
        var end = hand + dir * (HiltLength - 2);
        if (outline) canvas.Line(start.X, start.Y, end.X, end.Y, Ink, 5);
        canvas.Line(start.X, start.Y, end.X, end.Y, Rgb.FromBytes(150, 150, 165), 3);
        var across = new Vector2(-dir.Y, dir.X);
        for (int k = 1; k < HiltLength - 3; k += 3)
        {
            var p = start + dir * k;
            canvas.Line(p.X - across.X, p.Y - across.Y, p.X + across.X, p.Y + across.Y, Rgb.FromBytes(70, 70, 85));
        }
        var cap = start + dir * 1.5f;
        canvas.Line(start.X, start.Y, cap.X, cap.Y, Rgb.FromBytes(205, 205, 220), 3);
        return end + dir;
    }

    /// <summary>SNES-Schild: Segmente mit Kontur, heller Ober-/Linkskante, dunkler Unter-/Rechtskante.</summary>
    private void DrawSnesShield(PixelCanvas canvas, FieldMapper map, Shield shield, (Rgb Light, Rgb Mid, Rgb Dark) colors)
    {
        var alive = shield.Segments.Where(s => Visible(s, shield.Owner)).Select(s => (s.Row, Box: map.ToScreen(s.Bounds))).ToList();
        foreach (var (_, (x0, y0, x1, y1)) in alive) canvas.RectOutline(x0 - 1, y0 - 1, x1 + 1, y1 + 1, Ink);
        foreach (var (row, (x0, y0, x1Full, y1)) in alive)
        {
            int x1 = x1Full - 1;   // 1 Pixel Kontur als Fuge zwischen den Segmenten (nur Grafik, A5)
            float f = row == 0 ? 1f : 0.75f;
            canvas.FillRect(x0, y0, x1, y1, colors.Mid.Scale(f));
            canvas.FillRect(x0, y0, x1, y0, colors.Light.Scale(f));
            canvas.FillRect(x0, y0, x0, y1, colors.Light.Scale(f));
            canvas.FillRect(x0, y1, x1, y1, colors.Dark.Scale(f));
            canvas.FillRect(x1, y0, x1, y1, colors.Dark.Scale(f));
        }
    }

    protected override void DrawBolt(PixelCanvas canvas, IReadOnlyList<Vector2> path, BoltOwner owner, bool docked, int seed)
    {
        var col = Team(owner);
        // OP-14: zackig mit Seitenaesten (Standard) oder gleichmaessiges Zickzack ohne Aeste wie in den ersten Versionen
        bool lightning = Bolts == BoltShape.Lightning;
        var points = lightning
            ? Zigzag.Along(path, docked ? 9 : 8, docked ? 6.3f : 5.4f, seed)
            : Zigzag.Along(path, docked ? 9 : 8, docked ? 4.5f : 3.6f, seed, irregular: false);
        var branches = lightning ? Zigzag.Branches(points, docked ? 3 : 2, 12.6f, seed) : [];
        float boltGlow = OriginalBoltGlow * Look.BoltGlow / 100f;   // OP-15
        float glowFactor = _arena is not null ? SnesGlowFactor : 1f;
        // BL-8: wie die Lichtschwerter (deckender heller Rand, fast weisser deckender Kern), nur schmaler;
        // transparent ist nur das Leuchten aussen
        foreach (var branch in branches)
        {
            canvas.GlowPolyline(branch, col, 1, 3, 1.2f * Look.BoltGlow / 100f * glowFactor);
            canvas.Polyline(branch, Rgb.Lerp(col, White, BladeEdgeWhite));
        }
        canvas.GlowPolyline(points, col, docked ? 4 : 3, 3, boltGlow * glowFactor);
        canvas.Polyline(points, Rgb.Lerp(col, White, BladeEdgeWhite), BoltWidth);
        canvas.Polyline(points, Rgb.Lerp(col, White, BladeCoreWhite));
        if (docked)
        {
            if (_arena is not null) SnesSpark(canvas, path[^1]);
            else Spark(canvas, path[^1]);
        }
    }

    private static void SnesSpark(PixelCanvas canvas, Vector2 p)
    {
        var col = Rgb.FromBytes(255, 245, 200);
        for (int a = 0; a < 360; a += 45)
        {
            float len = a % 90 == 0 ? 6 : 3;
            canvas.Line(p.X, p.Y, p.X + MathF.Cos(a * MathF.PI / 180) * len, p.Y + MathF.Sin(a * MathF.PI / 180) * len, col);
        }
    }

    private static void Spark(PixelCanvas canvas, Vector2 p)
    {
        var col = Rgb.FromBytes(255, 240, 180);
        for (int a = 0; a < 360; a += 45)
        {
            float len = a % 90 == 0 ? 6 : 3;
            float ex = p.X + MathF.Cos(a * MathF.PI / 180) * len, ey = p.Y + MathF.Sin(a * MathF.PI / 180) * len;
            canvas.GlowLine(p.X, p.Y, ex, ey, col, 1, 2, 1.5f);
            canvas.Line(p.X, p.Y, ex, ey, col);
        }
    }

    protected override void DrawDrone(PixelCanvas canvas, FieldMapper map, Vector2 c, bool storm)
    {
        float r = DroneRadius(map);
        var eye = storm ? Rgb.FromBytes(255, 60, 40) : Rgb.FromBytes(255, 200, 60);
        // Im Gewitter pulsiert das rote Licht (Ring und Auge), etwa 1,2-mal je Sekunde
        float pulse = storm ? 0.55f + 0.45f * (0.5f + 0.5f * MathF.Sin(CurrentFrame * MathF.Tau / StormPulseFrames)) : 1f;
        // Drohne der Arena „Original“ in allen Arenen
        if (storm)
        {
            // dezentes Leuchten um den roten Ring (OP-15: Staerke einstellbar)
            canvas.GlowRing(c.X, c.Y, r * StormRingFactor, Rgb.FromBytes(255, 60, 40), 2f, StormRingGlow * pulse * Look.RingGlow / 100f);
            canvas.EllipseOutline(c.X, c.Y, r * StormRingFactor, r * StormRingFactor, Rgb.Lerp(Rgb.FromBytes(150, 40, 30), Rgb.FromBytes(255, 90, 60), pulse), 1);
        }
        float angle = RingAngle();
        foreach (var n in Nozzles(c, r * 1.55f, angle))
        {
            canvas.GlowCircle(n.X, n.Y, 2, Rgb.FromBytes(255, 140, 40), 2, 1.6f);
            canvas.FillEllipse(n.X, n.Y, 1.5f, 1.5f, Rgb.FromBytes(90, 95, 120));
        }
        canvas.EllipseOutline(c.X, c.Y, r * 1.6f, r * 1.6f, Rgb.FromBytes(110, 120, 150), 2);
        foreach (var mark in RingMarks(c, r * 1.6f, angle)) canvas.Line(mark.A.X, mark.A.Y, mark.B.X, mark.B.Y, Rgb.FromBytes(190, 200, 230));
        canvas.ShadedEllipse(c.X, c.Y, r, r, Rgb.FromBytes(150, 160, 185));
        canvas.GlowCircle(c.X, c.Y, 2, eye, 2.5f, 1.8f * pulse * Look.EyeGlow / 100f);
        canvas.FillEllipse(c.X, c.Y, 1.5f, 1.5f, Rgb.FromBytes(255, 255, 220));
    }

    protected override void DrawStormBanner(PixelCanvas canvas, FieldMapper map, bool twoPlayers, Localizer t)
    {
        var f = map.Field;
        int my = f.Y + f.Height / 2;
        const int height = 26;
        var band = new PixelCanvas(f.Width, height);
        band.Clear(Rgb.FromBytes(60, 10, 10));
        // Warnband: diagonale gelb-schwarze Streifen oben und unten
        var yellow = Rgb.FromBytes(255, 200, 40);
        var black = Rgb.FromBytes(20, 12, 8);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < f.Width; x++)
            {
                var c = (x + y) / 5 % 2 == 0 ? yellow : black;
                band[x, y] = c;
                band[x, height - 1 - y] = (x - y + 100) / 5 % 2 == 0 ? yellow : black;
            }
        int scale = FittingScale(t["hud.storm"], f.Width - 6, 2);   // Querformat: schmales Feld
        BitmapFont.Draw(band, t["hud.storm"], f.Width / 2, scale == 2 ? 6 : 9, Rgb.FromBytes(255, 230, 120), scale, TextAlign.Center, P.Shadow);
        // Abstand so gross, dass die Drohne mit Warnring vollstaendig frei steht
        int gap = (int)MathF.Ceiling(DroneRadius(map) * StormRingFactor) + 6;
        canvas.Blit(band, f.X, my + gap);
        if (twoPlayers) canvas.Blit(band, f.X, my - gap - height, rotate180: true);
    }

    private const float StormRingFactor = 2.4f;
    private const float RingTurnsPerSecond = 1f / 3f;   // aeusserer Drohnenring: eine Umdrehung in 3 s

    /// <summary>Drehwinkel des aeusseren Drohnenrings im aktuellen Bild (im Titel steht er still).</summary>
    private float RingAngle() => CurrentFrame / 60f * RingTurnsPerSecond * MathF.Tau;

    private static IEnumerable<Vector2> Nozzles(Vector2 c, float radius, float angle) =>
        new[] { 45, 135, 225, 315 }.Select(a => c + new Vector2(MathF.Cos(a * MathF.PI / 180 + angle), MathF.Sin(a * MathF.PI / 180 + angle)) * radius);

    /// <summary>Kurze Querstriche auf dem Ring, damit die Drehung sichtbar ist.</summary>
    private static IEnumerable<(Vector2 A, Vector2 B)> RingMarks(Vector2 c, float radius, float angle) =>
        new[] { 0, 90, 180, 270 }.Select(a =>
        {
            var dir = new Vector2(MathF.Cos(a * MathF.PI / 180 + angle), MathF.Sin(a * MathF.PI / 180 + angle));
            return (c + dir * (radius - 1.5f), c + dir * (radius + 1.5f));
        });

    private static float DroneRadius(FieldMapper map) => map.Field.Width * 0.045f;

    // ------------------------------------------------------------------ HUD

    private static void Panel(PixelCanvas p)
    {
        p.Clear(P.Panel);
        p.Line(0, 0, p.Width - 1, 0, P.PanelLine);
        p.Line(0, p.Height - 1, p.Width - 1, p.Height - 1, P.PanelLine);
    }

    private static void Pips(PixelCanvas canvas, int x, int y, int score, Rgb col, int total = 3)
    {
        for (int i = 0; i < total; i++)
        {
            int cx = x + i * 12;
            if (i < score) canvas.GlowCircle(cx, y, 3, col, 2, 1.2f);
            for (int dy = -4; dy <= 4; dy++)
            {
                int half = 4 - Math.Abs(dy);
                if (i < score) canvas.FillRect(cx - half, y + dy, cx + half, y + dy, col);
                else { canvas.Set(cx - half, y + dy, P.Dim); canvas.Set(cx + half, y + dy, P.Dim); }
            }
        }
    }

    /// <summary>Raute mit Kontur (SNES-Punkteanzeige).</summary>
    private static void SnesPips(PixelCanvas canvas, int x, int y, int score, Rgb col, Rgb empty)
    {
        for (int i = 0; i < 3; i++)
        {
            int cx = x + i * 13;
            for (int dy = -5; dy <= 5; dy++)
            {
                int half = 5 - Math.Abs(dy);
                canvas.FillRect(cx - half, y + dy, cx + half, y + dy, Ink);
                if (half > 0) canvas.FillRect(cx - half + 1, y + dy, cx + half - 1, y + dy, i < score ? col : empty);
            }
            if (i < score) canvas.Set(cx - 1, y - 2, Rgb.FromBytes(255, 255, 255));
        }
    }

    /// <summary>Energieleiste in 3 Abschnitten (Arcade-Chrom): je Punkt ein gefuellter Abschnitt.</summary>
    private static void EnergyBar(PixelCanvas canvas, int x, int y, int width, int score, Rgb col, Rgb empty)
    {
        canvas.FillRect(x - 2, y - 2, x + width + 1, y + 13, Ink);
        canvas.RectOutline(x - 2, y - 2, x + width + 1, y + 13, Rgb.FromBytes(220, 226, 240));
        int seg = width / 3;
        for (int i = 0; i < 3; i++)
        {
            int sx = x + i * seg;
            canvas.FillRect(sx + 1, y, sx + seg - 2, y + 11, i < score ? col : empty);
            if (i >= score) continue;
            canvas.FillRect(sx + 1, y, sx + seg - 2, y, Rgb.Lerp(col, Rgb.FromBytes(255, 255, 255), 0.6f));
            canvas.FillRect(sx + 1, y + 11, sx + seg - 2, y + 11, col.Scale(0.5f));
        }
    }

    private void SnesPause(PixelCanvas canvas, IntRect a)
    {
        int s = Math.Min(a.Width, a.Height) - 6, x = a.X + (a.Width - s) / 2, y = a.Y + (a.Height - s) / 2;
        canvas.FillRect(x, y, x + s, y + s, _arena!.Button);
        canvas.RectOutline(x, y, x + s, y + s, Ink);
        canvas.FillRect(x + s / 3, y + 5, x + s / 3 + 1, y + s - 5, Rgb.FromBytes(240, 244, 255));
        canvas.FillRect(x + 2 * s / 3 - 1, y + 5, x + 2 * s / 3, y + s - 5, Rgb.FromBytes(240, 244, 255));
    }

    private void DrawSnesHudBand(PixelCanvas band, string label, int score, PlayerId owner, bool pause, string extra)
    {
        var arena = _arena!;
        int w = band.Width, h = band.Height;
        arena.DrawBar(band, lineAtBottom: true);
        var team = Team(owner);
        int labelY = arena.EnergyBars ? 8 : (h - 14) / 2;
        BitmapFont.Draw(band, label, 10, labelY, arena.LabelColor ?? ((_, _) => team), 2, outline: Ink);
        int after = 10 + BitmapFont.MeasureWidth(label, 2) + 16;
        if (arena.EnergyBars) EnergyBar(band, after - 4, 12, Math.Min(120, w - after - 50), score, team, arena.EmptyPip);
        else SnesPips(band, after, h / 2, score, team, arena.EmptyPip);
        if (pause) SnesPause(band, new IntRect(w - 34, (h - 26) / 2, 26, 26));
        if (extra.Length > 0)
            BitmapFont.Draw(band, extra, w - 40, arena.EnergyBars ? h - 12 : (h - 7) / 2, arena.Dim, align: TextAlign.Right, outline: Ink);
    }

    private void DrawSnesSidePanel(PixelCanvas panel, string label, int score, PlayerId owner, string extra)
    {
        var arena = _arena!;
        arena.DrawBar(panel, lineAtBottom: false);
        var team = Team(owner);
        int cx = panel.Width / 2;
        BitmapFont.Draw(panel, label, cx, 8, arena.LabelColor ?? ((_, _) => team), 2, TextAlign.Center, outline: Ink);
        if (arena.EnergyBars) EnergyBar(panel, 12, 30, panel.Width - 24, score, team, arena.EmptyPip);
        else SnesPips(panel, cx - 13, 36, score, team, arena.EmptyPip);
        if (extra.Length > 0) BitmapFont.Draw(panel, extra, cx, 52, arena.Dim, align: TextAlign.Center, outline: Ink);
    }

    protected override void DrawHudBand(PixelCanvas band, string label, int score, PlayerId owner, bool pause, string extra)
    {
        if (_arena is not null)
        {
            DrawSnesHudBand(band, label, score, owner, pause, extra);
            return;
        }
        int w = band.Width, h = band.Height;
        Panel(band);
        // Punkte neben der Beschriftung: die Bildschirmmitte oben ist auf vielen Handys die Kameraaussparung
        BitmapFont.Draw(band, label, 8, (h - 14) / 2, Team(owner), 2, shadow: P.Shadow);
        Pips(band, 8 + BitmapFont.MeasureWidth(label, 2) + 14, h / 2, score, Team(owner));
        if (pause) DrawPauseIcon(band, new IntRect(w - 30, (h - 22) / 2, 22, 22));
        if (extra.Length > 0) BitmapFont.Draw(band, extra, w - 36, (h - 7) / 2, P.Dim, align: TextAlign.Right, shadow: P.Shadow);
    }

    protected override void DrawSidePanel(PixelCanvas panel, string label, int score, PlayerId owner, string extra)
    {
        if (_arena is not null)
        {
            DrawSnesSidePanel(panel, label, score, owner, extra);
            return;
        }
        Panel(panel);
        int cx = panel.Width / 2;
        BitmapFont.Draw(panel, label, cx, 6, Team(owner), 2, TextAlign.Center, P.Shadow);
        Pips(panel, cx - 12, 36, score, Team(owner));
        if (extra.Length > 0) BitmapFont.Draw(panel, extra, cx, 50, Team(owner), align: TextAlign.Center, shadow: P.Shadow);
    }

    protected override void DrawPauseIcon(PixelCanvas canvas, IntRect a)
    {
        if (_arena is not null)
        {
            SnesPause(canvas, a);
            return;
        }
        int s = Math.Min(a.Width, a.Height) - 4, x = a.X + (a.Width - s) / 2, y = a.Y + (a.Height - s) / 2;
        canvas.RoundedRect(x, y, x + s, y + s, 4, Rgb.FromBytes(40, 45, 80), Rgb.FromBytes(110, 120, 180));
        canvas.FillRect(x + s / 3, y + 5, x + s / 3 + 1, y + s - 5, P.Text);
        canvas.FillRect(x + 2 * s / 3 - 1, y + 5, x + 2 * s / 3, y + s - 5, P.Text);
    }

    protected override void DrawCenterMessage(PixelCanvas canvas, ScreenLayout layout, string text, string? sub, bool twoPlayers)
    {
        var f = layout.Field;
        int scale = BitmapFont.MeasureWidth(text, 3) <= f.Width - 10 ? 3 : 2;
        var box = new PixelCanvas(f.Width - 8, sub is null ? 7 * scale + 14 : 7 * scale + 28);
        box.Clear(Rgb.FromBytes(10, 12, 30));
        box.RoundedRect(0, 0, box.Width - 1, box.Height - 1, 6, null, P.P1);
        BitmapFont.Draw(box, text, box.Width / 2, 7, (row, ry) => Rgb.Lerp(Rgb.FromBytes(255, 250, 170), Rgb.FromBytes(255, 90, 40), ry / (7f * scale)),
            scale, TextAlign.Center, P.Shadow);
        if (sub is not null) BitmapFont.Draw(box, sub, box.Width / 2, 7 * scale + 12, P.Dim, align: TextAlign.Center);
        int my = f.Y + f.Height / 2;
        canvas.Blit(box, f.X + 4, my + 14);
        if (twoPlayers) canvas.Blit(box, f.X + 4, my - 14 - box.Height, rotate180: true);
    }

    protected override (int Width, int Height, int Gap) PauseButtonSize(int fieldWidth) => (fieldWidth - 50, 29, 10);

    protected override void DrawPauseMenu(PixelCanvas canvas, ScreenLayout layout, Localizer t, UserSettings settings)
    {
        var f = layout.Field;
        var rects = PauseMenuRects(layout);
        var entries = PauseMenu.Entries(settings);
        var rows = PauseOptionRects(layout, entries.Count);
        var all = rects.Concat(rows).ToList();
        int left = all.Min(r => r.X) - 13, right = all.Max(r => r.Right) + 12, top = f.Y + f.Height / 2 - 40;
        int bottom = all.Max(r => r.Bottom) + 12;
        canvas.GlowRect(left, top, right, bottom, P.P1.Scale(0.4f), 4, 1);
        canvas.RoundedRect(left, top, right, bottom, 8, Rgb.FromBytes(10, 12, 30), P.P1);
        BitmapFont.Draw(canvas, t["hud.paused"], f.X + f.Width / 2, top + 10, Rgb.FromBytes(255, 220, 120),
            FittingScale(t["hud.paused"], right - left - 8, 3), TextAlign.Center, P.Shadow);
        Button(canvas, rects[0], t["pause.resume"], selected: true);
        Button(canvas, rects[1], t["pause.menu"], selected: false);
        for (int i = 0; i < entries.Count; i++)
            DrawOptionRow(canvas, rows[i], t[entries[i].LabelKey], t[entries[i].ValueKey(settings)], sel: false, last: i == entries.Count - 1);
    }

    // ------------------------------------------------------------------ Siegerbildschirm

    public override void RenderVictory(PixelCanvas canvas, ScreenLayout layout, VictoryInfo info, Localizer t, int frame)
    {
        BeginFrame(canvas, layout);
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int w = layout.Width, h = layout.Height;
        var gold = info.HumanWon ? Rgb.FromBytes(255, 200, 60) : Rgb.FromBytes(130, 135, 150);
        float tx = land ? w * 0.3f : w / 2f, ty = land ? h * 0.52f : h * 0.42f;
        float size = land ? h * 0.36f : w * 0.36f;
        DrawMenuStars(canvas, layout, frame);
        // V1.0: Pixelbild der Heldin statt Pokal (Sieg: Schwert erhoben, Niederlage: kniend)
        var art = PixelArt.Get(info.HumanWon ? ArtId.Victory : ArtId.Defeat, land);
        int artBottom = land ? (info.HumanWon ? h - 18 : h - 40) : (info.HumanWon ? (int)(h * 0.65f) : (int)(h * 0.6f));
        ty = artBottom - art.Height / 2f;

        if (info.HumanWon)   // drehende Lichtstrahlen
            for (int i = 0; i < 12; i++)
            {
                float a = frame * 0.01f + i * MathF.PI / 6;
                canvas.GlowLine(tx, ty, tx + MathF.Cos(a) * size * 2.2f, ty + MathF.Sin(a) * size * 2.2f, gold.Scale(0.35f), 6, 5, 0.8f);
            }
        if (info.HumanWon)   // Funkeln
            for (int i = 0; i < 6; i++)
            {
                float phase = (frame * 0.05f + i * 1.7f) % 6.28f;
                float sx = tx + MathF.Cos(i * 2.1f) * size * 1.2f, sy = ty + MathF.Sin(i * 1.3f) * size * 1.1f;
                float k = MathF.Max(0, MathF.Sin(phase));
                canvas.GlowCircle(sx, sy, 1, Rgb.FromBytes(255, 255, 220), 2, 1.5f * k);
                canvas.Line(sx - 4 * k, sy, sx + 4 * k, sy, Rgb.FromBytes(255, 255, 220));
                canvas.Line(sx, sy - 4 * k, sx, sy + 4 * k, Rgb.FromBytes(255, 255, 220));
            }

        string title = t[info.HumanWon ? "victory.title" : "defeat.title"];
        int textX = land ? (int)(w * 0.7f) : w / 2;
        int titleY = land ? h / 2 - 70 : (int)(h * 0.1f);
        int titleScale = FittingScale(title, land ? (int)(w * 0.55f) : w - 16, 5);
        var top = info.HumanWon ? Rgb.FromBytes(255, 250, 170) : Rgb.FromBytes(200, 205, 220);
        var bottom = info.HumanWon ? Rgb.FromBytes(255, 90, 40) : Rgb.FromBytes(90, 95, 120);
        BitmapFont.Draw(canvas, title, textX, titleY, (_, ry) => Rgb.Lerp(top, bottom, ry / (7f * titleScale)), titleScale,
            TextAlign.Center, Rgb.FromBytes(40, 10, 30));

        int infoY = land ? h / 2 + 4 : (int)(h * 0.66f);
        string winner = WinnerLine(info, t);
        BitmapFont.Draw(canvas, winner, textX, infoY, P.Text, FittingScale(winner, land ? (int)(w * 0.55f) : w - 16, 2), TextAlign.Center, P.Shadow);
        string score = $"{info.WinnerScore} : {info.LoserScore}";
        BitmapFont.Draw(canvas, score, textX, infoY + 26, gold, 3, TextAlign.Center, P.Shadow);
        if (info.ShowContinue && frame / 30 % 2 == 0)
            BitmapFont.Draw(canvas, t["hud.tapToContinue"], textX, land ? h - 30 : h - 40, P.Dim, align: TextAlign.Center, shadow: P.Shadow);
        Finish(canvas);
        art.DrawBottomCentered(canvas, (int)tx, artBottom);   // nach der Farbreduktion: Bildfarben sind schon Palettenfarben, kein Dithering
    }

    /// <summary>V1.0: Sternenflug wie im Titel auch in Optionen, Sieg und Niederlage.</summary>
    private void DrawMenuStars(PixelCanvas canvas, ScreenLayout layout, int frame) =>
        _titleStars.Draw(canvas, new Vector2(layout.Width / 2f, layout.Height / 2f), Math.Min(layout.Width, layout.Height) * 0.16f, frame);

    /// <summary>Pokal: Schale mit Stern, Henkel, Stiel und Sockel, von links oben beleuchtet.</summary>

    // ------------------------------------------------------------------ Titel

    public override IReadOnlyList<IntRect> TitleItemRects(ScreenLayout layout)
    {
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int w = layout.Width, h = layout.Height;
        return land
            ? Enumerable.Range(0, 5).Select(i => new IntRect(w - 255, 40 + i * 40, 225, 29)).ToList()
            : Enumerable.Range(0, 5).Select(i => new IntRect(45, h * 392 / 585 + i * 34, w - 90, 29)).ToList();
    }

    public override void RenderTitle(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int frame)
    {
        BeginFrame(canvas, layout);
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int w = layout.Width, h = layout.Height;
        // Flug durchs All hinter dem Titel (wie in der Arena „Sternenflug“)
        _titleStars.Draw(canvas, new Vector2(land ? w * 0.28f : w / 2f, land ? h * 0.55f : h * 0.42f), Math.Min(w, h) * 0.16f, frame);
        int sc = land ? 6 : 8;
        int logoX = land ? (w - 255) / 2 : w / 2;
        int[] logoYs = land ? [28, 84] : [h * 60 / 585, h * 130 / 585];
        foreach (var (word, y) in new[] { ("LAZER", logoYs[0]), ("ARENA", logoYs[1]) })
        {
            int x = logoX - BitmapFont.MeasureWidth(word, sc) / 2;
            var mask = new PixelCanvas(w, h);
            BitmapFont.Draw(mask, word, x, y, Rgb.FromBytes(255, 255, 255), sc);
            for (int py = Math.Max(0, y - 6); py < Math.Min(h, y + 7 * sc + 6); py++)
                for (int px = Math.Max(0, x - 6); px < Math.Min(w, x + BitmapFont.MeasureWidth(word, sc) + 6); px++)
                {
                    bool near = false;
                    for (int dy = -2; dy <= 2 && !near; dy++)
                        for (int dx = -2; dx <= 2 && !near; dx++)
                            near = px + dx >= 0 && px + dx < w && py + dy >= 0 && py + dy < h && mask[px + dx, py + dy].R > 0;
                    if (near) canvas[px, py] = Rgb.FromBytes(20, 6, 30);
                }
            BitmapFont.Draw(canvas, word, x, y,
                (_, ry) => Rgb.Lerp(Rgb.FromBytes(255, 250, 170), Rgb.FromBytes(255, 70, 40), MathF.Min(1, ry / (7f * sc))), sc);
        }
        BitmapFont.Draw(canvas, t["title.tagline"], logoX, land ? 140 : h * 200 / 585, Rgb.FromBytes(150, 200, 255),
            align: TextAlign.Center, shadow: P.Shadow);

        var items = TitleItems(settings, t);
        var rects = TitleItemRects(layout);
        for (int i = 0; i < items.Length; i++)
            Button(canvas, rects[i], items[i], selected: i == 0);
        NoteIcon(canvas, 10, h - 22);
        BitmapFont.Draw(canvas, t[settings.Music ? "status.musicOn" : "status.musicOff"], 26, h - 18, P.Dim);
        BitmapFont.Draw(canvas, "V1.3", w - 8, h - 18, Rgb.FromBytes(80, 90, 130), align: TextAlign.Right);
        Finish(canvas);
        // V1.0: Heldin im Kampf mit der Drohne in der Mitte (nach der Farbreduktion, ohne Dithering)
        var art = PixelArt.Get(ArtId.Title, land);
        art.DrawBottomCentered(canvas, logoX, land ? h - 26 : h * 386 / 585);
    }

    protected override void DrawTutorialFrame(PixelCanvas canvas, IntRect scene)
    {
        _titleStars.Draw(canvas, new Vector2(canvas.Width / 2f, canvas.Height / 2f), Math.Min(canvas.Width, canvas.Height) * 0.16f, CurrentFrame);
        canvas.GlowRect(scene.X - 4, scene.Y - 4, scene.Right + 3, scene.Bottom + 3, P.P1.Scale(0.3f), 3, 1);
        canvas.RoundedRect(scene.X - 4, scene.Y - 4, scene.Right + 3, scene.Bottom + 3, 5, Rgb.FromBytes(16, 18, 38), Rgb.FromBytes(70, 80, 130));
        int my = scene.Y + scene.Height / 2;
        for (int x = scene.X; x < scene.Right; x += 6) canvas.Line(x, my, x + 2, my, Rgb.FromBytes(60, 80, 140));
    }

    protected override void DrawTutorialButton(PixelCanvas canvas, IntRect r, string text, bool primary) => Button(canvas, r, text, primary);

    private static void Button(PixelCanvas canvas, IntRect r, string text, bool selected)
    {
        if (selected) canvas.GlowRect(r.X, r.Y, r.Right - 1, r.Bottom - 1, P.P1.Scale(0.6f), 3, 1);
        canvas.RoundedRect(r.X, r.Y, r.Right - 1, r.Bottom - 1, 6,
            selected ? Rgb.FromBytes(24, 50, 90) : Rgb.FromBytes(20, 22, 44), selected ? P.P1 : Rgb.FromBytes(70, 80, 130));
        int scale = FittingScale(text, r.Width - 8, 2);
        BitmapFont.Draw(canvas, text, r.X + r.Width / 2, r.Y + (scale == 2 ? 8 : 11), selected ? P.Text : P.Dim, scale, TextAlign.Center, P.Shadow);
    }

    private static void NoteIcon(PixelCanvas canvas, int x, int y)
    {
        canvas.FillEllipse(x + 2.5f, y + 11, 2.5f, 2f, P.P1);
        canvas.Line(x + 5, y + 11, x + 5, y, P.P1);
        canvas.Line(x + 5, y, x + 10, y + 3, P.P1);
    }

    // ------------------------------------------------------------------ Optionen

    /// <summary>Optionswerte in doppelter Schrift (14 Pixel): ganzzahlig vergroessert, daher gleichmaessige Buchstaben.</summary>
    private const int ValueScale = 2;

    private const int OptionsTop = 54;   // erste Optionszeile unter der Ueberschrift
    private const int OptionsTitleY = 18;

    public override IReadOnlyList<IntRect> OptionRowRects(ScreenLayout layout, int count)
    {
        int top = OptionsTop + layout.TopBelowCutout(OptionsTitleY) - OptionsTitleY;   // SO-7: unter die Kamera-Aussparung
        int step = Math.Min(50, (layout.Height - top - 54) / Math.Max(1, count));
        return Enumerable.Range(0, count).Select(i => new IntRect(16, top + i * step, layout.Width - 32, step - 4)).ToList();
    }

    public override IntRect OptionsBackRect(ScreenLayout layout) => new(16, layout.Height - 46, (layout.Width - 48) / 2, 29);

    public override IntRect OptionsResetRect(ScreenLayout layout) =>
        new(layout.Width / 2 + 8, layout.Height - 46, (layout.Width - 48) / 2, 29);

    public override void RenderOptions(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int selected,
        bool advanced = false, bool resetArmed = false, int frame = 0)
    {
        BeginFrame(canvas, layout);
        DrawMenuStars(canvas, layout, frame);
        int w = layout.Width;
        BitmapFont.Draw(canvas, t[advanced ? "menu.advanced" : "menu.options"], w / 2, layout.TopBelowCutout(OptionsTitleY), Rgb.FromBytes(255, 220, 120), 3, TextAlign.Center, Rgb.FromBytes(60, 10, 30));
        var entries = OptionEntries(settings, advanced);
        var rows = OptionRowRects(layout, entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            string? mark = entries[i].LabelKey == "opt.arena" && ArenaInfo.IsAnimated(settings.Arena) ? t["opt.arenaAnimated"] : null;
            DrawOptionRow(canvas, rows[i], t[entries[i].LabelKey], t[entries[i].ValueKey(settings)], i == selected, mark, last: i == entries.Count - 1);
        }
        Button(canvas, OptionsBackRect(layout), t["menu.back"], selected: false);
        Button(canvas, OptionsResetRect(layout), t[resetArmed ? "menu.resetConfirm" : "menu.reset"], selected: resetArmed);
        Finish(canvas);
    }

    private static readonly Rgb RowFillSelected = Rgb.FromBytes(24, 50, 90);
    private static readonly Rgb RowLine = Rgb.FromBytes(60, 70, 120), ArrowDim = Rgb.FromBytes(90, 100, 150);
    private static readonly Rgb ValueColor = Rgb.FromBytes(200, 210, 235), MarkColor = Rgb.FromBytes(255, 220, 120);

    /// <summary>
    /// Optionszeile (Entwurf B, V1.1): keine Kaesten, Trennlinien zwischen den Zeilen. Beschriftung klein oben,
    /// Wert in doppelter Schrift linksbuendig darunter; die gewaehlte Zeile ist hinterlegt und links markiert.
    /// </summary>
    private static void DrawOptionRow(PixelCanvas canvas, IntRect r, string label, string value, bool sel, string? mark = null,
        bool last = false)
    {
        int valueH = BitmapFont.GlyphHeight * ValueScale;
        if (sel)
        {
            canvas.BlendRect(r.X, r.Y, r.Right - 1, r.Bottom - 1, RowFillSelected, 0.8f);
            canvas.FillRect(r.X, r.Y, r.X + 2, r.Bottom - 1, P.P1);
        }
        if (!last) canvas.FillRect(r.X + 6, r.Bottom + 1, r.Right - 7, r.Bottom + 1, RowLine);
        BitmapFont.Draw(canvas, label, r.X + 10, r.Y + 4, P.Dim);
        if (mark is not null) BitmapFont.Draw(canvas, mark, r.Right - 8, r.Y + 4, MarkColor, align: TextAlign.Right);
        int vy = r.Bottom - 3 - valueH;
        BitmapFont.Draw(canvas, value, r.X + 10, vy, sel ? P.Text : ValueColor, ValueScale, shadow: P.Shadow);
        var arrow = sel ? P.P1 : ArrowDim;
        Triangle(canvas, r.Right - 34, vy + valueH / 2, -1, 5, arrow);
        Triangle(canvas, r.Right - 12, vy + valueH / 2, 1, 5, arrow);
    }

    /// <summary>Kleines gefuelltes Dreieck als Pfeil; <paramref name="direction"/> −1 = links, 1 = rechts.</summary>
    private static void Triangle(PixelCanvas canvas, int x, int y, int direction, int size, Rgb color)
    {
        for (int i = 0; i < size; i++)
        {
            int col = x + direction * (i - size / 2);
            int half = size - 1 - i;
            canvas.FillRect(col, y - half, col, y + half, color);
        }
    }
}

/// <summary>Weiches Wertrauschen (bilinear mit Smoothstep) fuer den Nebel-Hintergrund.</summary>
internal sealed class SmoothNoise
{
    private readonly float[,] _grid;

    public SmoothNoise(int gw, int gh, int seed)
    {
        var random = new Random(seed);
        _grid = new float[gw + 1, gh + 1];
        for (int x = 0; x <= gw; x++)
            for (int y = 0; y <= gh; y++)
                _grid[x, y] = (float)random.NextDouble();
    }

    public float Sample(float u, float v)
    {
        float gx = u * (_grid.GetLength(0) - 1), gy = v * (_grid.GetLength(1) - 1);
        int x0 = Math.Min((int)gx, _grid.GetLength(0) - 2), y0 = Math.Min((int)gy, _grid.GetLength(1) - 2);
        float tx = Smooth(gx - x0), ty = Smooth(gy - y0);
        float a = _grid[x0, y0] + (_grid[x0 + 1, y0] - _grid[x0, y0]) * tx;
        float b = _grid[x0, y0 + 1] + (_grid[x0 + 1, y0 + 1] - _grid[x0, y0 + 1]) * tx;
        return a + (b - a) * ty;
    }

    private static float Smooth(float t) => t * t * (3 - 2 * t);
}
