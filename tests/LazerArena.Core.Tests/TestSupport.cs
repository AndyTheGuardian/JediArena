using System.Numerics;
using LazerArena.Core;

namespace LazerArena.Core.Tests;

/// <summary>Zufall mit festem Ergebnis: Range liefert den Wert bei <see cref="Fraction"/>, Chance ist immer false.</summary>
public sealed class FixedRandom(float fraction = 0.5f) : IRandomSource
{
    public float Fraction { get; set; } = fraction;
    public float Range(float min, float max) => min + (max - min) * Fraction;
    public bool Chance(float probability) => false;
}

public static class TestArena
{
    /// <summary>Konfiguration fuer kontrollierte Tests: Drohne steht still in der Mitte, kein Gewitter.</summary>
    public static GameConfig Quiet(GameConfig? baseConfig = null) => (baseConfig ?? GameConfig.Default) with
    {
        DroneSpeedMin = 0,
        DroneSpeedMax = 0,
        StormIntervalMin = 10_000,
        StormIntervalMax = 10_000,
    };

    public static ArenaSimulation Create(GameOptions? options = null, GameConfig? config = null, IRandomSource? random = null) =>
        new(config ?? Quiet(), options ?? GameOptions.Default, random ?? new FixedRandom());

    /// <summary>Erzeugt eine Arena und ueberspringt den Countdown.</summary>
    public static ArenaSimulation Playing(GameOptions? options = null, GameConfig? config = null, IRandomSource? random = null)
    {
        var sim = Create(options, config, random);
        RunUntil(sim, s => s.Phase == MatchPhase.Playing, 10);
        return sim;
    }

    public static void RunSeconds(ArenaSimulation sim, float seconds)
    {
        int steps = (int)MathF.Round(seconds / sim.Config.TickSeconds);
        for (int i = 0; i < steps; i++) sim.Step();
    }

    /// <summary>Laeuft, bis die Bedingung gilt; sammelt alle Ereignisse.</summary>
    public static List<GameEvent> RunUntil(ArenaSimulation sim, Func<ArenaSimulation, bool> condition, float maxSeconds)
    {
        var events = new List<GameEvent>();
        int steps = (int)(maxSeconds / sim.Config.TickSeconds);
        for (int i = 0; i < steps && !condition(sim); i++)
        {
            sim.Step();
            events.AddRange(sim.Events);
        }
        Assert.True(condition(sim), "Bedingung wurde nicht erreicht");
        return events;
    }

    public static List<GameEvent> CollectEvents(ArenaSimulation sim, float seconds)
    {
        var events = new List<GameEvent>();
        int steps = (int)MathF.Round(seconds / sim.Config.TickSeconds);
        for (int i = 0; i < steps; i++)
        {
            sim.Step();
            events.AddRange(sim.Events);
        }
        return events;
    }

    /// <summary>Schwert sofort auf einen Winkel setzen (unabhaengig von der Drehoption).</summary>
    public static void SetSword(ArenaSimulation sim, PlayerId id, float angle)
    {
        sim.SetSwordTarget(id, angle);
        for (int i = 0; i < 60 && MathF.Abs(sim.PlayerFor(id).SwordAngle - angle) > 0.001f; i++)
            sim.PlayerFor(id).UpdateSword(sim.Config.TickSeconds, SwordTurning.Limited);
    }

    public static void DestroySegments(ArenaSimulation sim, PlayerId id, params (int Row, int Index)[] segments)
    {
        foreach (var (row, index) in segments)
            sim.ShieldFor(id).Segments.Single(s => s.Row == row && s.Index == index).Alive = false;
    }

    /// <summary>Oeffnet eine Schneise durch beide Reihen in der Feldmitte.</summary>
    public static void OpenCenterLane(ArenaSimulation sim, PlayerId id) =>
        DestroySegments(sim, id, (0, 7), (0, 8), (1, 7), (1, 8));

    public static Bolt Shoot(ArenaSimulation sim, BoltOwner owner, Vector2 origin, Vector2 direction)
    {
        var bolt = Bolt.FromPoint(owner, origin, direction);
        sim.AddBolt(bolt);
        return bolt;
    }
}
