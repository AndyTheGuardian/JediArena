using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace LazerArena.Game;

/// <summary>Plattformabhaengige Dienste (Desktop/Android).</summary>
public interface IPlatformServices
{
    string SettingsPath { get; }
    string ScreenshotDirectory { get; }
    /// <summary>SO-7: Kamera-Aussparung oben in der aktuellen Ausrichtung (Geraetepixel; Hoehe 0 = keine).</summary>
    Cutout Cutout => default;
    void Quit();
}

/// <summary>
/// Kamera-Aussparung am oberen Rand: <paramref name="Top"/> = Hoehe in Geraetepixeln, <paramref name="Centered"/> = liegt
/// sie mittig? Nur mittige Aussparungen werden ueberzeichnet (dort ist die obere Leiste frei); sonst beginnt das Bild darunter.
/// </summary>
public readonly record struct Cutout(int Top, bool Centered)
{
    /// <summary>So viele Geraetepixel bleiben oben frei (nicht mittige Aussparung).</summary>
    public int Reserved => Centered ? 0 : Top;

    /// <summary>So weit reicht eine ueberzeichnete Aussparung ins Bild.</summary>
    public int Overdrawn => Centered ? Top : 0;

    /// <summary>Mittig heisst: alle Aussparungen am oberen Rand liegen in den mittleren 40 % der Breite.</summary>
    public static bool IsCentered(int screenWidth, IEnumerable<(int Left, int Right)> topCutouts) =>
        topCutouts.All(c => c.Left >= screenWidth * 0.3f && c.Right <= screenWidth * 0.7f);
}

public enum PointerPhase { Down, Move, Up }

/// <summary>Beruehrung in logischen Pixeln des aktuellen Layouts.</summary>
public readonly record struct PointerEvent(int Id, PointerPhase Phase, float X, float Y);

/// <summary>Uebersetzt Touch (und auf dem Desktop die Maus) in logische Koordinaten.</summary>
public sealed class PointerInput
{
    public List<PointerEvent> Read(Vector2 offset, float scale)
    {
        var events = new List<PointerEvent>();
        foreach (var touch in TouchPanel.GetState())
        {
            var p = (touch.Position - offset) / scale;
            var phase = touch.State switch
            {
                TouchLocationState.Pressed => PointerPhase.Down,
                TouchLocationState.Released => PointerPhase.Up,
                _ => PointerPhase.Move,
            };
            if (touch.State != TouchLocationState.Invalid)
                events.Add(new PointerEvent(touch.Id, phase, p.X, p.Y));
        }
        return events;
    }
}
