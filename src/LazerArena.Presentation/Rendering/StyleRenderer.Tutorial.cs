using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;

namespace LazerArena.Presentation.Rendering;

/// <summary>
/// SO-6: Animierte Anleitung. Sechs Seiten mit je einer kleinen, fest ablaufenden Demo-Szene (keine echte Simulation),
/// einer Ueberschrift und einem kurzen Text. Gezeichnet mit den Bausteinen des jeweiligen Stils.
/// </summary>
public abstract partial class StyleRenderer
{
    public const int TutorialPages = 6;

    /// <summary>Laenge einer Demo-Schleife in Bildern (4 s bei 60 Bildern/s).</summary>
    public const int TutorialLoopFrames = 240;

    /// <summary>Knoepfe der Anleitung: [0] = zurueck, [1] = weiter (auf der letzten Seite: fertig).</summary>
    public IReadOnlyList<IntRect> TutorialNavRects(ScreenLayout layout)
    {
        int w = layout.Width, h = layout.Height;
        int bh = Style == Settings.GraphicsStyle.Classic ? 14 : 29;
        int bw = (w - 3 * Margin(layout)) / 2;
        int y = h - bh - Margin(layout);
        return [new IntRect(Margin(layout), y, bw, bh), new IntRect(w - Margin(layout) - bw, y, bw, bh)];
    }

    private int Margin(ScreenLayout layout) => Style == Settings.GraphicsStyle.Classic ? 4 : 12;

    /// <summary>Szenenbereich (im Seitenverhaeltnis des Spielfelds) und Textbereich einer Anleitungsseite.</summary>
    public (IntRect Scene, IntRect Text) TutorialAreas(ScreenLayout layout)
    {
        int w = layout.Width, h = layout.Height, m = Margin(layout);
        int titleH = (Style == Settings.GraphicsStyle.Classic ? 14 : 30) + TutorialTitleY(layout) - TutorialTitleBase;
        int navTop = TutorialNavRects(layout)[0].Y;
        float aspect = GameConfig.Default.FieldAspect;
        if (layout.Orientation == ScreenOrientation.Landscape)
        {
            int sh = navTop - titleH - 2 * m;
            int sw = (int)(sh * aspect);
            var scene = new IntRect(w / 4 - sw / 2, titleH + m, sw, sh);
            return (scene, new IntRect(w / 2, titleH + m, w / 2 - m, sh));
        }
        int sceneH = (int)((navTop - titleH) * 0.58f);
        int sceneW = (int)(sceneH * aspect);
        var s = new IntRect(w / 2 - sceneW / 2, titleH + m, sceneW, sceneH);
        return (s, new IntRect(m, s.Bottom + m, w - 2 * m, navTop - s.Bottom - 2 * m));
    }

    public void RenderTutorial(PixelCanvas canvas, ScreenLayout layout, int page, Localizer t, int frame)
    {
        CurrentFrame = frame;
        BeginFrame(canvas, layout);
        var (scene, text) = TutorialAreas(layout);
        DrawTutorialFrame(canvas, scene);
        DrawTutorialScene(canvas, scene, page, frame % TutorialLoopFrames);
        _hiddenSegments = null;

        int titleScale = FittingScale(t[$"tut.{page + 1}.title"], layout.Width - 8, Style == Settings.GraphicsStyle.Classic ? 1 : 2);
        BitmapFont.Draw(canvas, t[$"tut.{page + 1}.title"], layout.Width / 2, TutorialTitleY(layout),
            TutorialTitleColor, titleScale, TextAlign.Center);
        int lineH = BitmapFont.GlyphHeight + (Style == Settings.GraphicsStyle.Classic ? 3 : 5);
        int textScale = Style == Settings.GraphicsStyle.Classic ? 1 : (WrapText(t[$"tut.{page + 1}.text"], text.Width, 2).Count * (2 * lineH) <= text.Height ? 2 : 1);
        var lines = WrapText(t[$"tut.{page + 1}.text"], text.Width, textScale);
        int y = text.Y + Math.Max(0, (text.Height - lines.Count * lineH * textScale) / 2);
        foreach (var line in lines)
        {
            BitmapFont.Draw(canvas, line, text.X + text.Width / 2, y, TutorialTextColor, textScale, TextAlign.Center);
            y += lineH * textScale;
        }

        var nav = TutorialNavRects(layout);
        DrawTutorialButton(canvas, nav[0], page == 0 ? t["menu.back"] : t["tut.prev"], false);
        DrawTutorialButton(canvas, nav[1], page == TutorialPages - 1 ? t["tut.done"] : t["tut.next"], true);
        int dotsY = nav[0].Y - (Style == Settings.GraphicsStyle.Classic ? 5 : 10);
        for (int i = 0; i < TutorialPages; i++)   // Seitenpunkte
        {
            int dx = layout.Width / 2 + (i - TutorialPages / 2) * (Style == Settings.GraphicsStyle.Classic ? 5 : 10) + 2;
            int r = Style == Settings.GraphicsStyle.Classic ? 1 : 2;
            canvas.FillRect(dx - r, dotsY - r, dx + r, dotsY + r, i == page ? TutorialTitleColor : HintColor);
        }
        Finish(canvas);
    }

    private int TutorialTitleBase => Style == Settings.GraphicsStyle.Classic ? 3 : 8;

    /// <summary>SO-7: Ueberschrift der Anleitung unter der Kamera-Aussparung.</summary>
    private int TutorialTitleY(ScreenLayout layout) => layout.TopBelowCutout(TutorialTitleBase);

    protected virtual Rgb TutorialTitleColor => Rgb.FromBytes(255, 220, 120);
    protected virtual Rgb TutorialTextColor => Rgb.FromBytes(230, 230, 240);

    /// <summary>Rahmen und Boden der Demo-Szene.</summary>
    protected abstract void DrawTutorialFrame(PixelCanvas canvas, IntRect scene);

    protected abstract void DrawTutorialButton(PixelCanvas canvas, IntRect r, string text, bool primary);

    /// <summary>Zerlegt einen Text in Zeilen, die in die Breite passen (Umbruch an Leerzeichen).</summary>
    public static IReadOnlyList<string> WrapText(string text, int width, int scale = 1)
    {
        var lines = new List<string>();
        string current = "";
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = current.Length == 0 ? word : current + " " + word;
            if (BitmapFont.MeasureWidth(candidate, scale) <= width || current.Length == 0) current = candidate;
            else { lines.Add(current); current = word; }
        }
        if (current.Length > 0) lines.Add(current);
        return lines;
    }

    // ------------------------------------------------------------------ Demo-Szenen

    private void DrawTutorialScene(PixelCanvas canvas, IntRect scene, int page, int t)
    {
        var config = GameConfig.Default;
        var map = new FieldMapper(scene, config);
        var p1 = new Player(PlayerId.One, config);
        var p2 = new Player(PlayerId.Two, config);
        var s1 = new Shield(PlayerId.One, p1.Position, config, ShieldShape.Straight);
        var s2 = new Shield(PlayerId.Two, p2.Position, config, ShieldShape.Straight);
        _hiddenSegments = [];
        float u = t / (float)TutorialLoopFrames;
        var drone = new Vector2(config.FieldWidth * (0.5f + 0.25f * MathF.Sin(u * MathF.Tau)), config.DroneYRatio * config.FieldHeight);
        bool storm = false;
        var bolts = new List<(Vector2 From, Vector2 To, BoltOwner Owner, bool Docked)>();
        Vector2? finger = null;
        bool tap = false;
        string? banner = null;

        void Aim(Player p, float angle) { p.SetTargetAngle(angle); p.UpdateSword(10, SwordTurning.Instant); }
        void Hide(Shield shield, Vector2 point)
        {
            var seg = shield.SegmentAt(point);
            if (seg is not null) _hiddenSegments!.Add((shield.Owner, seg.Row, seg.Index));
        }
        // Blitz von a nach b, der waehrend [start, end] (Anteil der Schleife) waechst; liefert den aktuellen Endpunkt oder null
        Vector2? Grow(Vector2 a, Vector2 b, float start, float end)
        {
            if (u < start) return null;
            float k = Math.Clamp((u - start) / (end - start), 0, 1);
            return a + (b - a) * k;
        }
        var fingerBase = new Vector2(config.FieldWidth / 2, config.FieldHeight * 1.04f);
        float rowH = config.ShieldRowHeightRatio * config.FieldHeight;
        float p2Front = (1 - config.StraightShieldRowsP1[0]) * config.FieldHeight - rowH / 2;   // Mitte der vorderen Reihe oben
        float p2Back = (1 - config.StraightShieldRowsP1[1]) * config.FieldHeight - rowH / 2;
        static Vector2 AtY(Vector2 from, Vector2 dir, float y) => from + dir * ((y - from.Y) / dir.Y);

        switch (page)
        {
            case 0:   // Schwert drehen: Wischen links/rechts
            {
                float angle = 45 * MathF.Sin(u * MathF.Tau * 2);
                Aim(p1, angle);
                Aim(p2, 0);
                finger = fingerBase + new Vector2(angle / 60f * 0.35f, 0);
                break;
            }
            case 1:   // Feuern: Tippen, Blitz in Schwertrichtung, Schildblock des Gegners zerstoert
            {
                Aim(p1, -25);
                Aim(p2, 10);
                var fireDrone = drone;
                var dir = p1.DirectionFor(-25);
                var target = AtY(fireDrone, dir, p2Front + rowH / 2);
                finger = fingerBase;
                tap = u > 0.1f && u < 0.2f;
                if (Grow(fireDrone, target, 0.18f, 0.45f) is { } end && u < 0.5f) bolts.Add((fireDrone, end, BoltOwner.Player1, false));
                if (u >= 0.45f) Hide(s2, AtY(fireDrone, dir, p2Front));
                break;
            }
            case 2:   // Blitze fangen: gegnerischer Blitz dockt an der Schwertspitze an
            {
                Aim(p1, 20 + 8 * MathF.Sin(u * MathF.Tau));
                Aim(p2, -15);
                var tip = p1.SwordTip;
                var from = new Vector2(config.FieldWidth * 0.5f, drone.Y);
                drone = from;
                if (Grow(from, tip, 0.1f, 0.35f) is { } end && u < 0.8f) bolts.Add((from, u >= 0.35f ? tip : end, BoltOwner.Player2, u >= 0.35f));
                break;
            }
            case 3:   // Schild: Blitz zerstoert einen Block, spaeter waechst er nach
            {
                Aim(p1, 35);
                Aim(p2, -5);
                var target = new Vector2(config.FieldWidth * 0.25f, config.StraightShieldRowsP1[0] * config.FieldHeight);
                var from = new Vector2(config.FieldWidth * 0.55f, drone.Y);
                drone = from;
                if (Grow(from, target, 0.1f, 0.3f) is { } end && u < 0.32f) bolts.Add((from, end, BoltOwner.Player2, false));
                if (u >= 0.3f && u < 0.75f) Hide(s1, target + new Vector2(0, 0.005f));
                if (u >= 0.75f && u < 0.85f && t / 6 % 2 == 0) Hide(s1, target + new Vector2(0, 0.005f));   // Nachwachsen blinkt
                break;
            }
            case 4:   // Blitzgewitter: Drohne blinkt, dann neutrale Blitze in zufaellige Richtungen
            {
                Aim(p1, 30 * MathF.Sin(u * MathF.Tau * 3));
                Aim(p2, -30 * MathF.Sin(u * MathF.Tau * 2));
                drone = new Vector2(config.FieldWidth / 2, drone.Y);
                storm = u < 0.25f ? t / 6 % 2 == 0 : u < 0.9f;
                if (u >= 0.25f && u < 0.9f)
                {
                    int burst = (int)((u - 0.25f) / 0.08f);
                    var random = new Random(burst * 7919 + 3);
                    float local = ((u - 0.25f) % 0.08f) / 0.08f;
                    foreach (int sign in new[] { -1, 1 })
                    {
                        float angle = (float)(random.NextDouble() * 120 - 60) * MathF.PI / 180;
                        var dir = new Vector2(MathF.Sin(angle), sign * MathF.Cos(angle));
                        bolts.Add((drone, drone + dir * (config.FieldHeight * 0.45f * local), BoltOwner.Neutral, false));
                    }
                }
                break;
            }
            default:   // Punkte: Blitz trifft den Gegner durch eine Luecke im Schild
            {
                Aim(p2, 25 * MathF.Sin(u * MathF.Tau));
                var from = new Vector2(config.FieldWidth * 0.4f, drone.Y);
                drone = from;
                var dir = Vector2.Normalize(p2.Position - from);
                Aim(p1, p1.AngleFor(dir));
                finger = fingerBase;
                tap = u > 0.05f && u < 0.15f;
                Hide(s2, AtY(from, dir, p2Front));   // Luecke im Schild
                Hide(s2, AtY(from, dir, p2Back));
                if (Grow(from, p2.Position, 0.15f, 0.45f) is { } end && u < 0.5f) bolts.Add((from, end, BoltOwner.Player1, false));
                if (u >= 0.45f) banner = u < 0.75f ? "1 : 0" : "3 : 0";
                break;
            }
        }

        foreach (var (id, shield) in new[] { (PlayerId.Two, s2), (PlayerId.One, s1) }) DrawShield(canvas, map, shield, id);
        DrawPlayer(canvas, map, p2, config);
        DrawPlayer(canvas, map, p1, config);
        foreach (var (from, to, owner, docked) in bolts)
            if (Vector2.Distance(from, to) > 0.01f)
                DrawBolt(canvas, [map.ToScreen(from), map.ToScreen(to)], owner, docked, CurrentFrame * 31 + (int)(to.X * 1000));
        DrawDrone(canvas, map, map.ToScreen(drone), storm);
        if (banner is not null)   // Punktestand in der Szene
        {
            BitmapFont.Draw(canvas, banner, scene.X + scene.Width / 2, scene.Y + scene.Height * 3 / 10, TutorialTitleColor,
                Style == Settings.GraphicsStyle.Classic ? 1 : 2, TextAlign.Center);
        }
        if (finger is { } f) DrawFinger(canvas, map.ToScreen(f), tap);
    }

    /// <summary>Fingerspitze als Kreis; beim Tippen mit Ring.</summary>
    private void DrawFinger(PixelCanvas canvas, Vector2 p, bool tap)
    {
        float r = Style == Settings.GraphicsStyle.Classic ? 2 : 5;
        var white = Rgb.FromBytes(255, 255, 255);
        canvas.FillEllipse(p.X, p.Y, r + 1, r + 1, Rgb.FromBytes(40, 40, 60));
        canvas.FillEllipse(p.X, p.Y, r, r, white);
        if (tap) canvas.EllipseOutline(p.X, p.Y, r * 2.2f, r * 2.2f, TutorialTitleColor, 1);
    }
}
