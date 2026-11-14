using System.Numerics;

namespace LazerArena.Core;

public enum PlayerId { One = 0, Two = 1 }

/// <summary>Besitzer eines Blitzes; Neutral = Lasergewitter (LG-4).</summary>
public enum BoltOwner { Player1, Player2, Neutral }

public enum MatchPhase { Countdown, Playing, RoundOver, MatchOver }

public enum StormPhase { Idle, Warning, Active }

public static class PlayerIdExtensions
{
    public static PlayerId Opponent(this PlayerId id) => id == PlayerId.One ? PlayerId.Two : PlayerId.One;

    public static BoltOwner AsOwner(this PlayerId id) => id == PlayerId.One ? BoltOwner.Player1 : BoltOwner.Player2;

    public static PlayerId? AsPlayer(this BoltOwner owner) => owner switch
    {
        BoltOwner.Player1 => PlayerId.One,
        BoltOwner.Player2 => PlayerId.Two,
        _ => null,
    };
}

/// <summary>Zufallsquelle mit Startwert (SO-4).</summary>
public interface IRandomSource
{
    float Range(float min, float max);
    bool Chance(float probability);
}

public sealed class SeededRandom(int seed) : IRandomSource
{
    private readonly Random _random = new(seed);

    public float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

    public bool Chance(float probability) => _random.NextDouble() < probability;
}

/// <summary>Achsenparalleles Rechteck (Schildsegmente).</summary>
public readonly record struct RectF(float Left, float Top, float Right, float Bottom)
{
    public Vector2 Center => new((Left + Right) / 2, (Top + Bottom) / 2);

    public bool Contains(Vector2 p) => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
}

public static class Angles
{
    public static float ToRadians(float degrees) => degrees * MathF.PI / 180f;

    public static float ToDegrees(float radians) => radians * 180f / MathF.PI;

    public static float MoveTowards(float current, float target, float maxDelta) =>
        MathF.Abs(target - current) <= maxDelta ? target : current + MathF.Sign(target - current) * maxDelta;
}

public enum GameEventType
{
    BoltFired, BoltThrownBack, BoltDocked, BoltFizzled, ShieldSegmentDestroyed, ShieldSegmentRegrown,
    PlayerHit, StormWarning, StormStarted, StormEnded, CountdownStarted, CountdownTwo, CountdownOne, RoundStarted, MatchWon,
}

/// <summary>Ereignis fuer Ton und Darstellung.</summary>
public readonly record struct GameEvent(GameEventType Type, PlayerId? Player = null);
