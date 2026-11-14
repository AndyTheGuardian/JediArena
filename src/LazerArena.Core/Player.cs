using System.Numerics;

namespace LazerArena.Core;

/// <summary>Feststehende Spielfigur mit drehbarem Laserschwert (SP-1, SP-2).</summary>
public sealed class Player
{
    private readonly GameConfig _config;

    public Player(PlayerId id, GameConfig config)
    {
        Id = id;
        _config = config;
        float yRatio = id == PlayerId.One ? config.Player1YRatio : config.Player2YRatio;
        Position = new Vector2(config.FieldWidth / 2, yRatio * config.FieldHeight);
    }

    public PlayerId Id { get; }
    public Vector2 Position { get; }
    public float SwordAngle { get; private set; }
    public float TargetAngle { get; private set; }
    public float Cooldown { get; internal set; }
    public int Score { get; internal set; }

    /// <summary>Richtung zum Gegner: Spieler 1 nach oben (y negativ), Spieler 2 nach unten.</summary>
    public float Forward => Id == PlayerId.One ? -1f : 1f;

    public Vector2 SwordDirection => DirectionFor(SwordAngle);

    public Vector2 SwordTip => Position + SwordDirection * (_config.SwordHandOffset + _config.SwordLength);

    /// <summary>Winkel aus Sicht des Spielers: 0 = zum Gegner, positiv = nach rechts (aus seiner Sicht).</summary>
    public Vector2 DirectionFor(float angleDegrees)
    {
        float a = Angles.ToRadians(angleDegrees);
        return Id == PlayerId.One
            ? new Vector2(MathF.Sin(a), -MathF.Cos(a))
            : new Vector2(-MathF.Sin(a), MathF.Cos(a));
    }

    /// <summary>Umkehrung von <see cref="DirectionFor"/>.</summary>
    public float AngleFor(Vector2 direction) => Id == PlayerId.One
        ? Angles.ToDegrees(MathF.Atan2(direction.X, -direction.Y))
        : Angles.ToDegrees(MathF.Atan2(-direction.X, direction.Y));

    public void SetTargetAngle(float degrees) =>
        TargetAngle = Math.Clamp(degrees, -_config.SwordMaxAngle, _config.SwordMaxAngle);

    public void UpdateSword(float dt, SwordTurning turning) =>
        SwordAngle = turning == SwordTurning.Instant
            ? TargetAngle
            : Angles.MoveTowards(SwordAngle, TargetAngle, _config.SwordLimitedTurnRate * dt);

    internal void ResetForRound()
    {
        SwordAngle = 0;
        TargetAngle = 0;
        Cooldown = 0;
    }
}
