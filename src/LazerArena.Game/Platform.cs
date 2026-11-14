using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace LazerArena.Game;

/// <summary>Plattformabhaengige Dienste (Desktop/Android).</summary>
public interface IPlatformServices
{
    string SettingsPath { get; }
    string ScreenshotDirectory { get; }
    void Quit();
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
