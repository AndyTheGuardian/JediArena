using System.Numerics;

namespace LazerArena.Core;

public enum BoltState { Growing, Docked, Gone }

/// <summary>
/// Ein Blitz zuckt von seinem Ursprung (Drohne oder – beim Zurueckwerfen – Schwertspitze) in eine feste Richtung
/// und waechst sichtbar (BL-1). Der Ursprung wandert mit der Drohne mit (BL-2). Massgeblich ist die Spitze.
/// </summary>
public sealed class Bolt
{
    private readonly Vector2 _fixedOrigin;

    private Bolt(BoltOwner owner, Vector2 direction, bool anchoredToDrone, Vector2 fixedOrigin)
    {
        Owner = owner;
        Direction = Vector2.Normalize(direction);
        AnchoredToDrone = anchoredToDrone;
        _fixedOrigin = fixedOrigin;
    }

    public static Bolt FromDrone(BoltOwner owner, Vector2 direction) => new(owner, direction, true, default);

    public static Bolt FromPoint(BoltOwner owner, Vector2 origin, Vector2 direction) => new(owner, direction, false, origin);

    public BoltOwner Owner { get; }
    public Vector2 Direction { get; }
    public bool AnchoredToDrone { get; }
    public float Length { get; internal set; }
    public BoltState State { get; internal set; } = BoltState.Growing;

    /// <summary>Spieler, auf den der Blitz zulaeuft (an dessen Schwert er andocken kann).</summary>
    public PlayerId Target => Direction.Y < 0 ? PlayerId.Two : PlayerId.One;

    public PlayerId? DockedAt { get; internal set; }
    public float HoldRemaining { get; internal set; }

    public Vector2 Origin(Drone drone) => AnchoredToDrone ? drone.Position : _fixedOrigin;

    /// <summary>
    /// Punkt im Abstand <paramref name="distance"/> entlang des Blitzes. Bei <see cref="WallBehavior.Bounce"/>
    /// wird an den Seitenraendern gespiegelt (BL-3); null, wenn der Blitz bei <see cref="WallBehavior.Extinguish"/>
    /// den Rand erreicht hat.
    /// </summary>
    public static Vector2? PointAt(Vector2 origin, Vector2 direction, float distance, float fieldWidth, WallBehavior walls)
    {
        float x = origin.X + direction.X * distance;
        float y = origin.Y + direction.Y * distance;
        if (walls == WallBehavior.Extinguish)
            return x < 0 || x > fieldWidth ? null : new Vector2(x, y);
        return new Vector2(Fold(x, fieldWidth), y);
    }

    /// <summary>Eckpunkte des Blitzverlaufs fuer die Darstellung (Knicke an den Seitenraendern).</summary>
    public IReadOnlyList<Vector2> PathPoints(Drone drone, IReadOnlyList<Player> players, float fieldWidth)
    {
        var origin = Origin(drone);
        if (State == BoltState.Docked && DockedAt is { } p)
            return [origin, players[(int)p].SwordTip];

        var points = new List<Vector2> { origin };
        float travelled = 0;
        var pos = origin;
        var dir = Direction;
        while (travelled < Length)
        {
            float toWall = dir.X > 0 ? (fieldWidth - pos.X) / dir.X : dir.X < 0 ? -pos.X / dir.X : float.MaxValue;
            float step = MathF.Min(toWall, Length - travelled);
            pos += dir * step;
            travelled += step;
            points.Add(pos);
            if (travelled < Length)
                dir = new Vector2(-dir.X, dir.Y);
        }
        return points;
    }

    private static float Fold(float x, float width)
    {
        float period = 2 * width;
        float m = ((x % period) + period) % period;
        return m <= width ? m : period - m;
    }
}
