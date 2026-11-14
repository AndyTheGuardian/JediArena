using System.Numerics;
using LazerArena.Core;

namespace LazerArena.Core.Tests;

public class BoltTests
{
    [Fact, Trait("Regel", "BL-1")]
    public void BL1_BlitzWaechstSichtbar_Mit12FeldhoehenProSekunde()
    {
        var sim = TestArena.Playing();
        sim.Fire(PlayerId.One);
        sim.Step();
        float expected = 1.2f * sim.Config.FieldHeight / 60f;
        Assert.Equal(expected, sim.Bolts.Single().Length, 2);
    }

    [Fact, Trait("Regel", "BL-1")]
    public void BL1_BisZurGegnerischenGrundlinieEtwa04Sekunden()
    {
        var sim = TestArena.Playing();
        TestArena.SetSword(sim, PlayerId.Two, 60);
        TestArena.OpenCenterLane(sim, PlayerId.Two);
        sim.Fire(PlayerId.One);

        float t = 0;
        while (sim.Phase == MatchPhase.Playing && t < 2) { sim.Step(); t += sim.Config.TickSeconds; }
        Assert.InRange(t, 0.3f, 0.5f);
    }

    [Fact, Trait("Regel", "BL-2")]
    public void BL2_UrsprungWandertMitDerDrohne_RichtungBleibt()
    {
        var config = GameConfig.Default with { StormIntervalMin = 10_000, StormIntervalMax = 10_000, DroneSpeedMin = 0.4f };
        var sim = new ArenaSimulation(config, GameOptions.Default, new FixedRandom());
        TestArena.RunUntil(sim, s => s.Phase == MatchPhase.Playing, 5);
        TestArena.SetSword(sim, PlayerId.Two, 60);
        sim.Fire(PlayerId.One);
        var bolt = sim.Bolts.Single();
        var direction = bolt.Direction;
        float droneXBefore = sim.Drone.X;

        TestArena.RunSeconds(sim, 0.1f);

        Assert.NotEqual(droneXBefore, sim.Drone.X);
        Assert.Equal(sim.Drone.Position, bolt.Origin(sim.Drone));
        Assert.Equal(direction, bolt.Direction);
    }

    [Fact, Trait("Regel", "BL-3"), Trait("Regel", "OP-9")]
    public void BL3_Abknicken_PunktJenseitsDesRandesWirdGespiegelt()
    {
        var origin = new Vector2(0.9f, 1f);
        var direction = new Vector2(1, 0);
        var p = Bolt.PointAt(origin, direction, 0.3f, 1f, WallBehavior.Bounce);
        Assert.NotNull(p);
        Assert.Equal(0.8f, p.Value.X, 4);
    }

    [Fact, Trait("Regel", "BL-3"), Trait("Regel", "OP-9")]
    public void BL3_Erloeschen_BlitzVerschwindetAmRand()
    {
        var sim = TestArena.Playing(GameOptions.Default with { Walls = WallBehavior.Extinguish, SwordTurning = SwordTurning.Instant });
        TestArena.SetSword(sim, PlayerId.One, 60);
        sim.Fire(PlayerId.One);
        TestArena.RunUntil(sim, s => s.Bolts.Count == 0, 2);

        Assert.All(sim.ShieldFor(PlayerId.Two).Segments, s => Assert.True(s.Alive));
        Assert.Equal(0, sim.PlayerFor(PlayerId.One).Score);
    }

    [Fact, Trait("Regel", "BL-3"), Trait("Regel", "OP-9")]
    public void BL3_Abknicken_BlitzWaechstNachDemRandWeiterUndTrifftDasSchild()
    {
        var sim = TestArena.Playing(GameOptions.Default with { Walls = WallBehavior.Bounce });
        TestArena.SetSword(sim, PlayerId.One, 60);
        TestArena.SetSword(sim, PlayerId.Two, 60);
        sim.Fire(PlayerId.One);
        var events = TestArena.RunUntil(sim, s => s.Bolts.Count == 0, 2);

        Assert.Contains(events, e => e.Type == GameEventType.ShieldSegmentDestroyed && e.Player == PlayerId.Two);
        var path = Bolt.PointAt(sim.Drone.Position, sim.PlayerFor(PlayerId.One).SwordDirection, 0.7f,
            sim.Config.FieldWidth, WallBehavior.Bounce);
        Assert.InRange(path!.Value.X, 0f, sim.Config.FieldWidth);
    }

    [Fact, Trait("Regel", "BL-3"), Trait("Regel", "BL-8")]
    public void BL8_DarstellungspfadHatEinenKnickAmRand()
    {
        var sim = TestArena.Playing();
        var bolt = TestArena.Shoot(sim, BoltOwner.Player1, new Vector2(0.9f, 1.5f), Vector2.Normalize(new Vector2(1, -1)));
        bolt.Length = 0.5f;
        var points = bolt.PathPoints(sim.Drone, sim.Players, sim.Config.FieldWidth);
        Assert.Equal(3, points.Count);
        Assert.Equal(1f, points[1].X, 4);
        Assert.True(points[2].X < 1f);
    }

    [Fact, Trait("Regel", "BL-4")]
    public void BL4_SchildsegmentWirdZerstoert_BlitzErlischt()
    {
        var sim = TestArena.Playing();
        TestArena.SetSword(sim, PlayerId.Two, 60);
        sim.Fire(PlayerId.One);
        TestArena.RunUntil(sim, s => s.Bolts.Count == 0, 2);

        Assert.Equal(1, sim.ShieldFor(PlayerId.Two).Segments.Count(s => !s.Alive));
        Assert.Equal(0, sim.PlayerFor(PlayerId.One).Score);
        Assert.Equal(MatchPhase.Playing, sim.Phase);
    }

    [Fact, Trait("Regel", "BL-5"), Trait("Regel", "RS-1")]
    public void BL5_TrefferAufDieFigur_SchuetzeBekommtEinenPunkt()
    {
        var sim = TestArena.Playing();
        TestArena.SetSword(sim, PlayerId.Two, 60);
        TestArena.OpenCenterLane(sim, PlayerId.Two);
        sim.Fire(PlayerId.One);
        var events = TestArena.RunUntil(sim, s => s.Phase != MatchPhase.Playing, 2);

        Assert.Equal(1, sim.PlayerFor(PlayerId.One).Score);
        Assert.Equal(0, sim.PlayerFor(PlayerId.Two).Score);
        Assert.Contains(events, e => e.Type == GameEventType.PlayerHit && e.Player == PlayerId.Two);
        Assert.Equal(MatchPhase.RoundOver, sim.Phase);
    }

    [Fact, Trait("Regel", "BL-6")]
    public void BL6_BlitzeKreuzenSichOhneWirkung()
    {
        var sim = TestArena.Playing(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        TestArena.SetSword(sim, PlayerId.One, 40);
        TestArena.SetSword(sim, PlayerId.Two, 40);
        sim.Fire(PlayerId.One);
        sim.Fire(PlayerId.Two);
        TestArena.RunSeconds(sim, 0.2f);

        Assert.Equal(2, sim.Bolts.Count);
        Assert.All(sim.Bolts, b => Assert.Equal(BoltState.Growing, b.State));
    }

    [Theory, Trait("Regel", "BL-7")]
    [InlineData(0f, 1.0f)]
    [InlineData(19.9f, 1.0f)]
    [InlineData(20.1f, 1.1f)]
    [InlineData(45f, 1.2f)]
    [InlineData(500f, 2.0f)]
    public void BL7_TempoSteigtAlle20SekundenUm10Prozent_Hoechstens200(float roundTime, float expected)
    {
        var config = TestArena.Quiet() with { ShieldRegrowInterval = 10_000 };
        var sim = TestArena.Playing(config: config);
        TestArena.RunSeconds(sim, roundTime);
        Assert.Equal(expected, sim.SpeedFactor, 3);
    }
}
