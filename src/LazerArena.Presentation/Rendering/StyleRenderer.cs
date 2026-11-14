using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Rendering;

/// <summary>
/// Gemeinsamer Ablauf beider Grafikstile (SO-2: Stil aendert nur Darstellung). Die Stile liefern die
/// Zeichenbausteine; Spielzustand kommt ausschliesslich lesend aus der Simulation.
/// </summary>
public abstract class StyleRenderer
{
    public abstract GraphicsStyle Style { get; }

    // ------------------------------------------------------------------ Spiel

    public void RenderGame(PixelCanvas canvas, ScreenLayout layout, GameSession session, Localizer t, int frame)
    {
        var sim = session.Simulation;
        CurrentFrame = frame;
        BeginGameFrame(canvas, layout);
        var map = new FieldMapper(layout.Field, sim.Config);
        DrawFieldBackground(canvas, map);
        if (sim.Storm.Phase == StormPhase.Active)   // liegt auf dem Boden, unter Blitzen und Drohne
            DrawStormBanner(canvas, map, session.TwoPlayers, t);
        foreach (var id in new[] { PlayerId.Two, PlayerId.One })
            DrawShield(canvas, map, sim.ShieldFor(id), id);
        foreach (var id in new[] { PlayerId.Two, PlayerId.One })
            DrawPlayer(canvas, map, sim.PlayerFor(id), sim.Config);

        var drone = map.ToScreen(sim.Drone.Position);
        foreach (var (bolt, index) in sim.Bolts.Select((b, i) => (b, i)))
        {
            var path = bolt.PathPoints(sim.Drone, sim.Players, sim.Config.FieldWidth).Select(map.ToScreen).ToList();
            DrawBolt(canvas, path, bolt.Owner, bolt.State == BoltState.Docked, frame * 31 + index * 977);
        }
        bool storm = sim.Storm.Phase == StormPhase.Active || (sim.Storm.Phase == StormPhase.Warning && frame / 6 % 2 == 0);
        DrawDrone(canvas, map, drone, storm);

        DrawHud(canvas, layout, session, t);
        DrawOverlay(canvas, layout, session, t);
        FinishGame(canvas);
    }

    private void DrawHud(PixelCanvas canvas, ScreenLayout layout, GameSession session, Localizer t)
    {
        var sim = session.Simulation;
        string p2Label = session.TwoPlayers ? t["hud.p2"] : t["hud.ai"];
        string level = session.TwoPlayers ? "" : t[LevelKey(session.AiLevel)];
        int p1Score = sim.PlayerFor(PlayerId.One).Score, p2Score = sim.PlayerFor(PlayerId.Two).Score;

        if (layout.Orientation == ScreenOrientation.Portrait)
        {
            var top = new PixelCanvas(layout.TopHud.Width, layout.TopHud.Height);
            DrawHudBand(top, p2Label, p2Score, PlayerId.Two, pause: !session.TwoPlayers, extra: level);
            canvas.Blit(top, layout.TopHud.X, layout.TopHud.Y, rotate180: session.TwoPlayers);
            var bottom = new PixelCanvas(layout.BottomHud.Width, layout.BottomHud.Height);
            DrawHudBand(bottom, t["hud.p1"], p1Score, PlayerId.One, pause: session.TwoPlayers, extra: "");
            canvas.Blit(bottom, layout.BottomHud.X, layout.BottomHud.Y);
            return;
        }

        var left = new PixelCanvas(layout.LeftPanel.Width, layout.LeftPanel.Height);
        DrawSidePanel(left, p2Label, p2Score, PlayerId.Two, level);
        canvas.Blit(left, layout.LeftPanel.X, layout.LeftPanel.Y, rotate180: session.TwoPlayers);
        var right = new PixelCanvas(layout.RightPanel.Width, layout.RightPanel.Height);
        DrawSidePanel(right, t["hud.p1"], p1Score, PlayerId.One, "");
        canvas.Blit(right, layout.RightPanel.X, layout.RightPanel.Y);
        if (!session.TwoPlayers)
        {
            int hx = layout.LeftPanel.X + layout.LeftPanel.Width / 2;
            string[] hints = [t["hint.swipe"], t["hint.sword"], t["hint.tap"], t["hint.fire"]];
            for (int i = 0; i < hints.Length; i++)
                BitmapFont.Draw(canvas, hints[i], hx, layout.Height / 2 + 8 + i * 10 + (i >= 2 ? 4 : 0), HintColor, align: TextAlign.Center);
        }
        DrawPauseIcon(canvas, layout.PauseButton);
    }

    private void DrawOverlay(PixelCanvas canvas, ScreenLayout layout, GameSession session, Localizer t)
    {
        var sim = session.Simulation;
        if (sim.Paused)
        {
            DrawPauseMenu(canvas, layout, t);
            return;
        }
        string? text = sim.Phase switch
            {
                MatchPhase.Countdown => ((int)MathF.Ceiling(sim.PhaseTimeRemaining)).ToString(),
                MatchPhase.RoundOver => t["hud.hit"],
                MatchPhase.MatchOver => $"{WinnerLabel(session, t)} {t["hud.wins"]}",
                _ => null,
            };
        if (text is null) return;
        string? sub = null;
        DrawCenterMessage(canvas, layout, text, sub, session.TwoPlayers);
    }

    private static string WinnerLabel(GameSession session, Localizer t) =>
        session.Simulation.Winner == PlayerId.One ? t["hud.p1"] : session.TwoPlayers ? t["hud.p2"] : t["hud.ai"];

    protected static string LevelKey(AiLevel level) =>
        level switch { AiLevel.Easy => "val.easy", AiLevel.Hard => "val.hard", _ => "val.medium" };

    /// <summary>
    /// Pausenmenue (SO-1): [0] = Weiter, [1] = Hauptmenue, mittig. Im Querformat ist das Spielfeld schmal;
    /// dort nutzt das Menue die Bildschirmbreite, damit die Beschriftungen passen.
    /// </summary>
    public IReadOnlyList<IntRect> PauseMenuRects(ScreenLayout layout)
    {
        var f = layout.Field;
        int area = layout.Orientation == ScreenOrientation.Landscape ? Math.Min(layout.Width - 20, f.Width * 2 + 40) : f.Width;
        var (w, h, gap) = PauseButtonSize(area);
        int x = f.X + f.Width / 2 - w / 2, y = f.Y + f.Height / 2 + gap;
        return [new IntRect(x, y, w, h), new IntRect(x, y + h + gap, w, h)];
    }

    /// <summary>Groesste Schriftgroesse (hoechstens <paramref name="preferred"/>), bei der der Text in die Breite passt.</summary>
    protected static int FittingScale(string text, int width, int preferred)
    {
        for (int scale = preferred; scale > 1; scale--)
            if (BitmapFont.MeasureWidth(text, scale) <= width) return scale;
        return 1;
    }

    protected abstract (int Width, int Height, int Gap) PauseButtonSize(int fieldWidth);
    protected abstract void DrawPauseMenu(PixelCanvas canvas, ScreenLayout layout, Localizer t);

    // ------------------------------------------------------------------ Siegerbildschirm

    /// <summary>Siegerbildschirm (RS-5) mit Pokal; bei einer Niederlage gegen die KI grauer Pokal.</summary>
    public abstract void RenderVictory(PixelCanvas canvas, ScreenLayout layout, VictoryInfo info, Localizer t, int frame);

    protected static string WinnerLine(VictoryInfo info, Localizer t) => $"{info.WinnerLabel} {t["hud.wins"]}";

    // ------------------------------------------------------------------ Menues

    public abstract IReadOnlyList<IntRect> TitleItemRects(ScreenLayout layout);
    public abstract void RenderTitle(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int frame);

    public abstract IReadOnlyList<IntRect> OptionRowRects(ScreenLayout layout, int count);
    public abstract IntRect OptionsBackRect(ScreenLayout layout);
    public abstract void RenderOptions(PixelCanvas canvas, ScreenLayout layout, UserSettings settings, Localizer t, int selected);

    protected static string[] TitleItems(UserSettings s, Localizer t) =>
    [
        t["menu.onePlayer"], t["menu.twoPlayers"],
        $"{t["menu.style"]}: {t[s.Style == GraphicsStyle.Classic ? "val.classic" : "val.modern"]}",
        t["menu.options"],
    ];

    /// <summary>Kleine Demo-Szene fuer den Titel: zwei Figuren, Drohne, ein Blitz und ein angedockter Blitz.</summary>
    protected void DrawTitleDemo(PixelCanvas canvas, IntRect rect)
    {
        var config = GameConfig.Default;
        var map = new FieldMapper(rect, config);
        var p1 = new Player(PlayerId.One, config);
        var p2 = new Player(PlayerId.Two, config);
        p1.SetTargetAngle(-25); p1.UpdateSword(1, SwordTurning.Instant);
        p2.SetTargetAngle(20); p2.UpdateSword(1, SwordTurning.Instant);
        DrawPlayer(canvas, map, p2, config);
        DrawPlayer(canvas, map, p1, config);
        var drone = map.ToScreen(new Vector2(0.55f, config.DroneYRatio * config.FieldHeight));
        DrawBolt(canvas, [drone, map.ToScreen(new Vector2(0.85f, 0.82f * config.FieldHeight))], BoltOwner.Player2, false, 3);
        DrawBolt(canvas, [drone, map.ToScreen(p2.SwordTip)], BoltOwner.Player1, true, 5);
        DrawDrone(canvas, map, drone, storm: false);
    }

    // ------------------------------------------------------------------ Bausteine je Stil

    protected abstract Rgb HintColor { get; }

    /// <summary>Bildzaehler des Spielbilds, das gerade gezeichnet wird (fuer Animationen).</summary>
    protected int CurrentFrame { get; private set; }

    /// <summary>Hintergrund eines Spielbilds; Menues nutzen weiterhin <see cref="BeginFrame"/>.</summary>
    protected virtual void BeginGameFrame(PixelCanvas canvas, ScreenLayout layout) => BeginFrame(canvas, layout);

    protected virtual void FinishGame(PixelCanvas canvas) => Finish(canvas);
    protected abstract void BeginFrame(PixelCanvas canvas, ScreenLayout layout);
    protected abstract void Finish(PixelCanvas canvas);
    protected abstract void DrawFieldBackground(PixelCanvas canvas, FieldMapper map);
    protected abstract void DrawShield(PixelCanvas canvas, FieldMapper map, Shield shield, PlayerId owner);
    protected abstract void DrawPlayer(PixelCanvas canvas, FieldMapper map, Player player, GameConfig config);
    protected abstract void DrawBolt(PixelCanvas canvas, IReadOnlyList<Vector2> path, BoltOwner owner, bool docked, int seed);
    protected abstract void DrawDrone(PixelCanvas canvas, FieldMapper map, Vector2 center, bool storm);
    protected abstract void DrawStormBanner(PixelCanvas canvas, FieldMapper map, bool twoPlayers, Localizer t);
    protected abstract void DrawHudBand(PixelCanvas band, string label, int score, PlayerId owner, bool pause, string extra);
    protected abstract void DrawSidePanel(PixelCanvas panel, string label, int score, PlayerId owner, string extra);
    protected abstract void DrawPauseIcon(PixelCanvas canvas, IntRect area);
    protected abstract void DrawCenterMessage(PixelCanvas canvas, ScreenLayout layout, string text, string? sub, bool twoPlayers);

    protected static (Vector2 Hand, Vector2 Tip) SwordPoints(FieldMapper map, Player player, GameConfig config) =>
        (map.ToScreen(player.Position + player.SwordDirection * config.SwordHandOffset), map.ToScreen(player.SwordTip));
}
