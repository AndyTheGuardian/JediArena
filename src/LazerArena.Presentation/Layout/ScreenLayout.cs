using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Layout;

public enum ScreenOrientation { Portrait, Landscape }

public readonly record struct IntRect(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public bool Contains(float x, float y) => x >= X && x < Right && y >= Y && y < Bottom;
}

/// <summary>
/// Aufteilung des logischen Pixelbilds. Die kurze Bildschirmseite hat im klassischen Stil 120, im modernen 270
/// logische Pixel; die lange Seite richtet sich nach dem Seitenverhaeltnis des Geraets.
/// Das Spielfeld bleibt immer hochkant (SF-1); im Querformat liegen Infobereiche links und rechts.
/// </summary>
public sealed record ScreenLayout(
    int Width, int Height, ScreenOrientation Orientation, IntRect Field,
    IntRect TopHud, IntRect BottomHud, IntRect LeftPanel, IntRect RightPanel, IntRect PauseButton)
{
    public static int ShortSide(GraphicsStyle style) => style == GraphicsStyle.Classic ? 120 : 270;

    /// <summary>Ganzzahliger Vergroesserungsfaktor fuer die Anzeige auf dem echten Bildschirm.</summary>
    public int PixelScale(int screenWidth, int screenHeight) =>
        Math.Max(1, Math.Min(screenWidth / Width, screenHeight / Height));

    public static ScreenLayout Create(GraphicsStyle style, int screenWidth, int screenHeight, float fieldAspect, bool twoPlayers)
    {
        int shortSide = ShortSide(style);
        bool classic = style == GraphicsStyle.Classic;
        if (screenHeight >= screenWidth)
        {
            int height = (int)MathF.Round(shortSide * (float)screenHeight / screenWidth);
            return Portrait(shortSide, height, classic, fieldAspect, twoPlayers);
        }
        int width = (int)MathF.Round(shortSide * (float)screenWidth / screenHeight);
        return Landscape(width, shortSide, classic, fieldAspect);
    }

    private static ScreenLayout Portrait(int w, int h, bool classic, float aspect, bool twoPlayers)
    {
        int hud = (int)MathF.Round(w * 0.1625f);
        int margin = classic ? 4 : 8;
        int fieldW = w - 2 * margin;
        int fieldH = (int)MathF.Round(fieldW / aspect);
        if (fieldH > h - 2 * hud)
        {
            fieldH = h - 2 * hud;
            fieldW = (int)MathF.Round(fieldH * aspect);
        }
        var field = new IntRect((w - fieldW) / 2, (h - fieldH) / 2, fieldW, fieldH);
        var top = new IntRect(0, 0, w, hud);
        var bottom = new IntRect(0, h - hud, w, hud);
        int pauseSize = hud;
        // 1 Spieler: Pause oben rechts; 2 Spieler: unten rechts (beide erreichen den Rand)
        var pause = twoPlayers
            ? new IntRect(w - pauseSize, h - hud, pauseSize, hud)
            : new IntRect(w - pauseSize, 0, pauseSize, hud);
        return new ScreenLayout(w, h, ScreenOrientation.Portrait, field, top, bottom, default, default, pause);
    }

    private static ScreenLayout Landscape(int w, int h, bool classic, float aspect)
    {
        int margin = classic ? 2 : 6;
        int fieldH = h - 2 * margin;
        int fieldW = (int)MathF.Round(fieldH * aspect);
        var field = new IntRect((w - fieldW) / 2, margin, fieldW, fieldH);
        int panelW = field.X - 12;
        var left = new IntRect(6, 6, panelW, h / 2 - 8);
        var right = new IntRect(field.Right + 6, h / 2 + 2, panelW, h / 2 - 8);
        int pauseSize = classic ? 14 : 30;
        var pause = new IntRect(w - pauseSize - 4, 4, pauseSize, pauseSize);
        return new ScreenLayout(w, h, ScreenOrientation.Landscape, field, default, default, left, right, pause);
    }
}
