using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Settings;
using P = LazerArena.Presentation.Rendering.ClassicPalette;

namespace LazerArena.Presentation.Rendering;

/// <summary>Klassischer Stil: grobe Pixel und wenige flache Farben wie beim Atari VCS (abgenommene Mockups).</summary>
public sealed class ClassicRenderer : StyleRenderer
{
    private static readonly string[] DroneSprite =
        ["....#....", "..#####..", ".##...##.", ".#..#..#.", "##.###.##", ".#..#..#.", ".##...##.", "..#####..", "....#...."];

    public override GraphicsStyle Style => GraphicsStyle.Classic;
    protected override Rgb HintColor => P.Grey;

    private static Rgb Team(PlayerId id) => id == PlayerId.One ? P.P1 : P.P2;
    private static Rgb Team(BoltOwner owner) => owner switch { BoltOwner.Player1 => P.P1, BoltOwner.Player2 => P.P2, _ => P.Neutral };

    protected override void BeginFrame(PixelCanvas canvas, ScreenLayout layout) => canvas.Clear(P.Black);
    protected override void Finish(PixelCanvas canvas) { }

    protected override void DrawFieldBackground(PixelCanvas canvas, FieldMapper map)
    {
        var f = map.Field;
        canvas.FillRect(f.X - 2, f.Y, f.X - 1, f.Bottom - 1, P.Wall);
        canvas.FillRect(f.Right, f.Y, f.Right + 1, f.Bottom - 1, P.Wall);
        int laneY = f.Y + f.Height / 2 + 6;
        for (int x = f.X; x < f.Right; x += 4) canvas.Set(x, laneY, P.Lane);
    }

    protected override void DrawShield(PixelCanvas canvas, FieldMapper map, Shield shield, PlayerId owner)
    {
        var (color, dark) = owner == PlayerId.One ? (P.P1, P.P1Dark) : (P.P2, P.P2Dark);
        foreach (var s in shield.Segments.Where(s => s.Alive))
        {
            var (x0, y0, x1, y1) = map.ToScreen(s.Bounds);
            canvas.FillRect(x0, y0, x1 - 1, y1, s.Row == 0 ? color : dark);   // 1 Pixel Fuge (nur Grafik, A5)
        }
    }

    protected override void DrawPlayer(PixelCanvas canvas, FieldMapper map, Player player, GameConfig config)
    {
        var c = map.ToScreen(player.Position);
        int cx = (int)MathF.Round(c.X), cy = (int)MathF.Round(c.Y);
        canvas.FillRect(cx - 4, cy - 1, cx + 4, cy + 1, Team(player.Id));
        canvas.FillRect(cx - 1, cy - 2, cx + 1, cy + 2, P.Text);
        var (hand, tip) = SwordPoints(map, player, config);
        canvas.Line(hand.X, hand.Y, tip.X, tip.Y, player.Id == PlayerId.One ? P.BladeP1 : P.BladeP2);
    }

    protected override void DrawBolt(PixelCanvas canvas, IReadOnlyList<Vector2> path, BoltOwner owner, bool docked, int seed)
    {
        var color = Team(owner);
        canvas.Polyline(Zigzag.Along(path, 4, docked ? 2.3f : 2f, seed), color);   // 10 % kleiner
        if (!docked) return;
        var tip = path[^1];
        canvas.FillRect((int)tip.X - 1, (int)tip.Y - 1, (int)tip.X + 1, (int)tip.Y + 1, color);
    }

    protected override void DrawDrone(PixelCanvas canvas, FieldMapper map, Vector2 center, bool storm)
    {
        var color = storm ? P.DroneStorm : P.Drone;
        int cx = (int)MathF.Round(center.X), cy = (int)MathF.Round(center.Y);
        for (int y = 0; y < DroneSprite.Length; y++)
            for (int x = 0; x < DroneSprite[y].Length; x++)
                if (DroneSprite[y][x] == '#') canvas.Set(cx - 4 + x, cy - 4 + y, color);
    }

    protected override void DrawStormBanner(PixelCanvas canvas, FieldMapper map, bool twoPlayers, Localizer t)
    {
        var f = map.Field;
        int my = f.Y + f.Height / 2;
        var band = new PixelCanvas(f.Width, 9);
        band.Clear(P.P2Dark);
        BitmapFont.Draw(band, t["hud.stormShort"], f.Width / 2, 1, P.Text, align: TextAlign.Center);
        canvas.Blit(band, f.X, my + 8);
        if (twoPlayers) canvas.Blit(band, f.X, my - 16, rotate180: true);
    }

    protected override void DrawHudBand(PixelCanvas band, string label, int score, PlayerId owner, bool pause, string extra)
    {
        int w = band.Width, h = band.Height;
        // Punktestand klein neben der Beschriftung: die Bildschirmmitte oben ist auf vielen Handys die Kameraaussparung
        BitmapFont.Draw(band, label, 3, (h - 7) / 2, Team(owner));
        BitmapFont.Draw(band, score.ToString(), 3 + BitmapFont.MeasureWidth(label) + 6, (h - 7) / 2, P.Text);
        if (pause) DrawPauseIcon(band, new IntRect(w - h, 0, h, h));
        if (extra.Length > 0) BitmapFont.Draw(band, extra, (pause ? w - h : w) - 2, (h - 7) / 2, P.Grey, align: TextAlign.Right);
    }

    protected override void DrawSidePanel(PixelCanvas panel, string label, int score, PlayerId owner, string extra)
    {
        int cx = panel.Width / 2;
        BitmapFont.Draw(panel, label, cx, 0, Team(owner), scale: 2, align: TextAlign.Center);
        BitmapFont.Draw(panel, score.ToString(), cx, 18, Team(owner), scale: 3, align: TextAlign.Center);
        if (extra.Length > 0) BitmapFont.Draw(panel, extra, cx, 43, Team(owner), align: TextAlign.Center);
    }

    protected override void DrawPauseIcon(PixelCanvas canvas, IntRect area)
    {
        int x = area.X + area.Width / 2 - 3, y = area.Y + area.Height / 2 - 3;
        canvas.FillRect(x, y, x + 1, y + 6, P.Grey);
        canvas.FillRect(x + 4, y, x + 5, y + 6, P.Grey);
    }

    protected override void DrawCenterMessage(PixelCanvas canvas, ScreenLayout layout, string text, string? sub, bool twoPlayers)
    {
        var f = layout.Field;
        int scale = BitmapFont.MeasureWidth(text, 2) <= f.Width - 4 ? 2 : 1;
        var box = new PixelCanvas(f.Width, sub is null ? 7 * scale + 6 : 7 * scale + 16);
        box.Clear(P.Black);
        BitmapFont.Draw(box, text, box.Width / 2, 3, P.Highlight, scale: scale, align: TextAlign.Center);
        if (sub is not null) BitmapFont.Draw(box, sub.Length * 6 <= f.Width ? sub : "", box.Width / 2, 7 * scale + 7, P.Grey, align: TextAlign.Center);
        int my = f.Y + f.Height / 2;
        canvas.Blit(box, f.X, my + 10);
        if (twoPlayers) canvas.Blit(box, f.X, my - 10 - box.Height, rotate180: true);
    }

    protected override (int Width, int Height, int Gap) PauseButtonSize(int fieldWidth) => (fieldWidth - 16, 14, 6);

    protected override void DrawPauseMenu(PixelCanvas canvas, ScreenLayout layout, Localizer t)
    {
        var f = layout.Field;
        var rects = PauseMenuRects(layout);
        int left = rects[0].X - 6, right = rects[0].Right + 5, top = f.Y + f.Height / 2 - 24;
        canvas.FillRect(left, top, right, rects[1].Bottom + 6, P.Black);
        canvas.RectOutline(left, top, right, rects[1].Bottom + 6, P.Grey);
        BitmapFont.Draw(canvas, t["hud.paused"], f.X + f.Width / 2, top + 6, P.Highlight,
            FittingScale(t["hud.paused"], right - left - 4, 2), TextAlign.Center);
        string[] labels = [t["pause.resume"], t["pause.menu"]];
        for (int i = 0; i < 2; i++)
        {
            var r = rects[i];
            canvas.RectOutline(r.X, r.Y, r.Right - 1, r.Bottom - 1, i == 0 ? P.Highlight : P.Grey);
            BitmapFont.Draw(canvas, labels[i], r.X + r.Width / 2, r.Y + 4, i == 0 ? P.Highlight : P.Text, align: TextAlign.Center);
        }
    }

    // ------------------------------------------------------------------ Siegerbildschirm

    private static readonly string[] TrophySprite =
    [
        "#..#########..#",
        "#.###########.#",
        "#.#####.#####.#",
        ".#####...#####.",
        "..###########..",
        "...#########...",
        "....#######....",
        ".....#####.....",
        "......###......",
        "......###......",
        "....#######....",
        "...#########...",
    ];

    public override void RenderVictory(PixelCanvas canvas, ScreenLayout layout, VictoryInfo info, Localizer t, int frame)
    {
        canvas.Clear(P.Black);
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int w = layout.Width, h = layout.Height;
        string title = t[info.HumanWon ? "victory.title" : "defeat.title"];
        int cx = land ? w / 3 : w / 2;
        int textX = land ? w * 3 / 4 : w / 2;
        BitmapFont.Draw(canvas, title, textX, land ? 18 : h * 24 / 260, (row, _) => info.HumanWon ? P.LogoBands[row] : P.Grey,
            FittingScale(title, land ? w / 2 - 8 : w - 8, 2), TextAlign.Center);

        int scale = 3, tw = TrophySprite[0].Length * scale;
        int tx = cx - tw / 2, ty = land ? h / 2 - TrophySprite.Length * scale / 2 : h * 70 / 260;
        var color = info.HumanWon ? P.Drone : P.Grey;
        for (int y = 0; y < TrophySprite.Length; y++)
            for (int x = 0; x < TrophySprite[y].Length; x++)
                if (TrophySprite[y][x] == '#')
                    canvas.FillRect(tx + x * scale, ty + y * scale, tx + x * scale + scale - 1, ty + y * scale + scale - 1,
                        y == 2 && x == 7 ? P.Text : color);
        if (info.HumanWon)   // blinkende Sterne
            for (int i = 0; i < 4; i++)
                if ((frame / 8 + i) % 3 == 0)
                {
                    int sx = cx + (i % 2 == 0 ? -1 : 1) * (tw / 2 + 6 + i * 2), sy = ty + 4 + i * 9;
                    canvas.Set(sx, sy, P.Text); canvas.Set(sx - 1, sy, P.Text); canvas.Set(sx + 1, sy, P.Text);
                    canvas.Set(sx, sy - 1, P.Text); canvas.Set(sx, sy + 1, P.Text);
                }

        int infoY = land ? h / 2 - 4 : h * 150 / 260;
        string winner = WinnerLine(info, t);
        BitmapFont.Draw(canvas, winner, textX, infoY, P.Text, align: TextAlign.Center);
        BitmapFont.Draw(canvas, $"{info.WinnerScore}:{info.LoserScore}", textX, infoY + 14, color, 2, TextAlign.Center);
        if (info.ShowContinue && frame / 30 % 2 == 0)
        {
            string tap = BitmapFont.MeasureWidth(t["hud.tapToContinue"]) <= (land ? w / 2 : w) - 4 ? t["hud.tapToContinue"] : t["hud.tapShort"];
            BitmapFont.Draw(canvas, tap, textX, h - 12, P.Grey, align: TextAlign.Center);
        }
    }

    // ------------------------------------------------------------------ Titel

    public override IReadOnlyList<IntRect> TitleItemRects(ScreenLayout layout)
    {
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int menuX = land ? layout.Width * 3 / 4 : layout.Width / 2;
        int menuY = land ? layout.Height / 4 : layout.Height * 172 / 260;
        return Enumerable.Range(0, 4).Select(i => new IntRect(menuX - 55, menuY + i * 16 - 4, 110, 15)).ToList();
    }

    public override void RenderTitle(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int frame)
    {
        canvas.Clear(P.Black);
        bool land = layout.Orientation == ScreenOrientation.Landscape;
        int w = layout.Width, h = layout.Height;
        int logoX = land ? w / 4 : w / 2;
        int logoY = land ? 14 : h * 26 / 260;
        BitmapFont.Draw(canvas, "LAZER", logoX, logoY, (row, _) => P.LogoBands[row], 3, TextAlign.Center);
        BitmapFont.Draw(canvas, "ARENA", logoX, logoY + 28, (row, _) => P.LogoBands[6 - row], 3, TextAlign.Center);
        var demo = land ? new IntRect(logoX - 30, 70, 60, 44) : new IntRect(20, h * 82 / 260, 80, h * 72 / 260);
        DrawTitleDemo(canvas, demo);

        var items = TitleItems(settings, t);
        var rects = TitleItemRects(layout);
        for (int i = 0; i < items.Length; i++)
        {
            var r = rects[i];
            var color = i == 0 ? P.Highlight : P.Grey;
            int cx = r.X + r.Width / 2, y = r.Y + 4;
            BitmapFont.Draw(canvas, items[i], cx, y, color, align: TextAlign.Center);
            if (i == 0) BitmapFont.Draw(canvas, ">", cx - BitmapFont.MeasureWidth(items[i]) / 2 - 9, y, color);
        }
        BitmapFont.Draw(canvas, t[settings.Sound ? "status.soundOn" : "status.soundOff"], 4, h - 10, P.Grey);
        BitmapFont.Draw(canvas, "V0.7", w - 4, h - 10, P.DarkGrey, align: TextAlign.Right);
    }

    // ------------------------------------------------------------------ Optionen

    public override IReadOnlyList<IntRect> OptionRowRects(ScreenLayout layout, int count)
    {
        int step = Math.Min(24, (layout.Height - 60) / Math.Max(1, count));
        return Enumerable.Range(0, count).Select(i => new IntRect(4, 34 + i * step - 2, layout.Width - 8, step)).ToList();
    }

    public override IntRect OptionsBackRect(ScreenLayout layout) => new(layout.Width / 2 - 30, layout.Height - 18, 60, 14);

    public override void RenderOptions(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int selected)
    {
        canvas.Clear(P.Black);
        int cx = layout.Width / 2;
        BitmapFont.Draw(canvas, t["menu.options"], cx, 10, P.Highlight, 2, TextAlign.Center);
        var entries = OptionsMenu.Entries(settings);
        var rows = OptionRowRects(layout, entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            var r = rows[i];
            string value = t[entries[i].ValueKey(settings)];
            BitmapFont.Draw(canvas, t[entries[i].LabelKey], cx, r.Y + 2, P.Grey, align: TextAlign.Center);
            bool sel = i == selected;
            BitmapFont.Draw(canvas, sel ? $"< {value} >" : value, cx, r.Y + 12, sel ? P.Highlight : P.Text, align: TextAlign.Center);
        }
        var back = OptionsBackRect(layout);
        BitmapFont.Draw(canvas, t["menu.back"], back.X + back.Width / 2, back.Y + 4, P.Grey, align: TextAlign.Center);
    }
}
