namespace LazerArena.Core;

public enum SwordTurning { Instant, Limited }          // OP-4
public enum DockedBoltBehavior { Fizzle, ThrowBack }    // OP-5
public enum ShieldShape { Straight, Curved }            // OP-6
public enum WallBehavior { Bounce, Extinguish }         // OP-9
public enum AiLevel { Easy, Medium, Hard }              // OP-3, KI-1

/// <summary>Spielrelevante Optionen (Abschnitt 10). Darstellungsoptionen liegen nicht im Kern.</summary>
public sealed record GameOptions
{
    public static GameOptions Default { get; } = new();

    public SwordTurning SwordTurning { get; init; } = SwordTurning.Limited;
    public DockedBoltBehavior DockedBolt { get; init; } = DockedBoltBehavior.Fizzle;
    public ShieldShape ShieldShape { get; init; } = ShieldShape.Straight;
    public WallBehavior Walls { get; init; } = WallBehavior.Bounce;
}
