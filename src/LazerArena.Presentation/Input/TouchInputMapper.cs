using LazerArena.Core;
using LazerArena.Presentation.Layout;

namespace LazerArena.Presentation.Input;

public enum CommandType { RotateSword, Fire, Pause }

public readonly record struct InputCommand(CommandType Type, PlayerId Player = PlayerId.One, float DeltaDegrees = 0);

/// <summary>
/// Setzt Beruehrungen in Spielbefehle um (F3, SP-2, SP-3). Koordinaten in logischen Pixeln des Layouts.
/// 2 Spieler: untere Haelfte = Spieler 1, obere = Spieler 2. 1 Spieler: ganzer Bildschirm fuer Spieler 1 (A6).
/// Waagerechtes Wischen dreht das Schwert, kurzes Tippen feuert. Mehrere Finger gleichzeitig sind moeglich.
/// </summary>
public sealed class TouchInputMapper(ScreenLayout layout, bool twoPlayers)
{
    /// <summary>Wischen ueber die ganze Bildschirmbreite = 180°.</summary>
    public const float DegreesPerScreenWidth = 180f;
    public const float TapMaxSeconds = 0.35f;
    public const float TapSlopFraction = 0.04f;

    private readonly Dictionary<int, Touch> _touches = [];
    private readonly List<InputCommand> _commands = [];

    private sealed class Touch(PlayerId? player, bool onPause, float x, float y, double time)
    {
        public PlayerId? Player { get; } = player;
        public bool OnPause { get; } = onPause;
        public float StartX { get; } = x;
        public float StartY { get; } = y;
        public float LastX { get; set; } = x;
        public double StartTime { get; } = time;
        public bool Moved { get; set; }
    }

    public void TouchDown(int id, float x, float y, double timeSeconds)
    {
        bool onPause = layout.PauseButton.Contains(x, y);
        _touches[id] = new Touch(onPause ? null : ZoneOwner(y), onPause, x, y, timeSeconds);
    }

    public void TouchMove(int id, float x, float y)
    {
        if (!_touches.TryGetValue(id, out var touch)) return;
        if (Distance(touch.StartX, touch.StartY, x, y) > TapSlopFraction * layout.Width)
            touch.Moved = true;
        if (touch.Player is { } player && touch.Moved)
        {
            float delta = (x - touch.LastX) / layout.Width * DegreesPerScreenWidth;
            // Spieler 2 sitzt gegenueber: seine rechte Seite ist links auf dem Bildschirm
            if (player == PlayerId.Two) delta = -delta;
            if (delta != 0) _commands.Add(new InputCommand(CommandType.RotateSword, player, delta));
        }
        touch.LastX = x;
    }

    public void TouchUp(int id, float x, float y, double timeSeconds)
    {
        TouchMove(id, x, y);
        if (!_touches.Remove(id, out var touch)) return;
        bool isTap = !touch.Moved && timeSeconds - touch.StartTime <= TapMaxSeconds;
        if (!isTap) return;
        if (touch.OnPause) _commands.Add(new InputCommand(CommandType.Pause));
        else if (touch.Player is { } player) _commands.Add(new InputCommand(CommandType.Fire, player));
    }

    public IReadOnlyList<InputCommand> Drain()
    {
        var result = _commands.ToList();
        _commands.Clear();
        return result;
    }

    private PlayerId ZoneOwner(float y) => !twoPlayers || y >= layout.Height / 2f ? PlayerId.One : PlayerId.Two;

    private static float Distance(float x0, float y0, float x1, float y1) => MathF.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
}
