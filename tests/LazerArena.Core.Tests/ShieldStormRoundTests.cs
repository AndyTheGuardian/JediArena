using System.Numerics;
using LazerArena.Core;

namespace LazerArena.Core.Tests;

public class ShieldTests
{
    [Fact, Trait("Regel", "SC-1")]
    public void SC1_Alle5SekundenWaechstEinSegmentNach_VorneZuerst_MitteZuerst()
    {
        var sim = TestArena.Playing();
        TestArena.DestroySegments(sim, PlayerId.One, (0, 3), (0, 8), (1, 8));
        var shield = sim.ShieldFor(PlayerId.One);
        bool Alive(int row, int index) => shield.Segments.Single(s => s.Row == row && s.Index == index).Alive;

        TestArena.RunSeconds(sim, 4.9f);
        Assert.False(Alive(0, 8));
        TestArena.RunSeconds(sim, 0.2f);
        Assert.True(Alive(0, 8));
        Assert.False(Alive(0, 3));

        TestArena.RunSeconds(sim, 5f);
        Assert.True(Alive(0, 3));
        Assert.False(Alive(1, 8));

        TestArena.RunSeconds(sim, 5f);
        Assert.True(Alive(1, 8));
    }

    [Fact, Trait("Regel", "SC-1")]
    public void SC1_BeiGleichemAbstandZurMitte_WirdDasLinkereZuerstErsetzt()
    {
        var sim = TestArena.Playing();
        TestArena.DestroySegments(sim, PlayerId.One, (0, 7), (0, 8));
        TestArena.RunSeconds(sim, 5.05f);
        var shield = sim.ShieldFor(PlayerId.One);
        Assert.True(shield.Segments.Single(s => s.Row == 0 && s.Index == 7).Alive);
        Assert.False(shield.Segments.Single(s => s.Row == 0 && s.Index == 8).Alive);
    }

    [Fact, Trait("Regel", "SC-2")]
    public void SC2_SegmentWaechstNichtNach_SolangeEinBlitzDurchVerlaeuft()
    {
        var config = GameConfig.Default;
        var shield = new Shield(PlayerId.One, new Vector2(0.5f, 1.7f), config, ShieldShape.Straight);
        var blocked = shield.Segments.Single(s => s.Row == 0 && s.Index == 7);
        var other = shield.Segments.Single(s => s.Row == 0 && s.Index == 2);
        blocked.Alive = false;
        other.Alive = false;

        var regrown = shield.UpdateRegrowth(5.01f, rect => rect == blocked.Bounds);

        Assert.Same(other, regrown);
        Assert.False(blocked.Alive);
    }

    [Fact, Trait("Regel", "SC-2")]
    public void SC2_SindAlleKandidatenBlockiert_WaechstNichtsNach()
    {
        var shield = new Shield(PlayerId.One, new Vector2(0.5f, 1.7f), GameConfig.Default, ShieldShape.Straight);
        shield.Segments[0].Alive = false;
        Assert.Null(shield.UpdateRegrowth(6f, _ => true));
        Assert.False(shield.Segments[0].Alive);
    }
}

public class StormTests
{
    private static GameConfig StormConfig => TestArena.Quiet() with { StormIntervalMin = 15, StormIntervalMax = 30 };

    [Fact, Trait("Regel", "LG-1"), Trait("Regel", "LG-2")]
    public void LG1_LG2_AnkuendigungNachZufallszeit_GewitterEineSekundeSpaeter()
    {
        var sim = TestArena.Playing(config: StormConfig);   // FixedRandom 0.5 -> 22,5 s
        var events = TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Warning, 30);
        Assert.InRange(sim.RoundTime, 22.4f, 22.6f);
        Assert.Contains(events, e => e.Type == GameEventType.StormWarning);

        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Active, 2);
        Assert.InRange(sim.RoundTime, 23.4f, 23.6f);
    }

    [Fact, Trait("Regel", "LG-3"), Trait("Regel", "LG-4")]
    public void LG3_DreiSekunden_JeRichtungHoechstensEinNeutralerBlitz_DichtNachgeschossen()
    {
        var sim = TestArena.Playing(config: StormConfig, random: new SeededRandom(21));
        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Active, 40);
        var fired = new List<Bolt>();
        var seen = new HashSet<Bolt>();
        var freeSince = new Dictionary<PlayerId, float>();
        float duration = 0, longestGap = 0;

        while (sim.Storm.Phase == StormPhase.Active && sim.Phase == MatchPhase.Playing && duration < 5)
        {
            sim.Step();
            duration += sim.Config.TickSeconds;
            fired.AddRange(sim.Bolts.Where(seen.Add));
            foreach (var target in new[] { PlayerId.One, PlayerId.Two })
            {
                int active = sim.Bolts.Count(b => b.Owner == BoltOwner.Neutral && b.Target == target);
                Assert.True(active <= 1, $"{active} Gewitter-Blitze Richtung {target}");
                if (active == 0) freeSince.TryAdd(target, duration);
                else if (freeSince.Remove(target, out var since)) longestGap = MathF.Max(longestGap, duration - since);
            }
        }

        Assert.All(fired, b => Assert.Equal(BoltOwner.Neutral, b.Owner));
        Assert.Contains(fired, b => b.Target == PlayerId.One);
        Assert.Contains(fired, b => b.Target == PlayerId.Two);
        Assert.True(fired.Count >= 4, $"nur {fired.Count} Blitze");
        Assert.True(longestGap <= sim.Config.StormRefireMax + 2 * sim.Config.TickSeconds, $"Pause {longestGap:F2} s");
        Assert.All(fired, b => Assert.InRange(MathF.Abs(MathF.Atan2(b.Direction.X, MathF.Abs(b.Direction.Y))), 0, 60.01f * MathF.PI / 180));
    }

    [Fact, Trait("Regel", "LG-3")]
    public void LG3_GewitterEndetNachDreiSekunden()
    {
        var sim = TestArena.Playing(config: StormConfig with { DockRadius = 0, ShieldRegrowInterval = 0.01f },
            random: new SeededRandom(21));
        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Active, 40);
        float start = sim.RoundTime;
        TestArena.RunUntil(sim, s => s.Storm.Phase != StormPhase.Active || s.Phase != MatchPhase.Playing, 5);
        if (sim.Phase == MatchPhase.Playing)
            Assert.InRange(sim.RoundTime - start, 2.95f, 3.05f);
    }

    [Fact, Trait("Regel", "LG-4")]
    public void LG4_NeutralerBlitzKannAndocken_UndGehoertDannNichtDemVerteidiger()
    {
        var sim = TestArena.Playing();
        var bolt = TestArena.Shoot(sim, BoltOwner.Neutral, sim.Drone.Position, new Vector2(0, 1));
        TestArena.RunSeconds(sim, 0.5f);
        Assert.Equal(PlayerId.One, bolt.DockedAt);
        Assert.Equal(BoltOwner.Neutral, bolt.Owner);
    }

    [Fact, Trait("Regel", "LG-5")]
    public void LG5_NeutraleBlitzeZaehlenNichtZumLimit()
    {
        var sim = TestArena.Playing();
        TestArena.Shoot(sim, BoltOwner.Neutral, sim.Drone.Position, new Vector2(0.3f, 1));
        TestArena.Shoot(sim, BoltOwner.Neutral, sim.Drone.Position, new Vector2(-0.3f, -1));
        Assert.Equal(0, sim.OwnedBoltCount(PlayerId.One));
        Assert.True(sim.Fire(PlayerId.One));
    }

    [Fact, Trait("Regel", "LG-6")]
    public void LG6_WaehrendDesGewittersKannNichtGeschossenWerden_InDerAnkuendigungSchon()
    {
        var sim = TestArena.Playing(config: StormConfig with { DockRadius = 0 }, random: new SeededRandom(21));
        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Warning, 40);
        TestArena.SetSword(sim, PlayerId.One, 50);
        Assert.True(sim.Fire(PlayerId.One));

        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Active, 2);
        Assert.False(sim.Fire(PlayerId.One));
        Assert.False(sim.Fire(PlayerId.Two));

        TestArena.RunUntil(sim, s => s.Storm.Phase == StormPhase.Idle, 4);
        Assert.True(sim.Fire(PlayerId.Two));
    }
}

public class RoundTests
{
    private static void ScorePoint(ArenaSimulation sim, PlayerId scorer)
    {
        TestArena.RunUntil(sim, s => s.Phase == MatchPhase.Playing, 10);
        var victim = scorer.Opponent();
        TestArena.SetSword(sim, victim, 60);
        TestArena.OpenCenterLane(sim, victim);
        TestArena.SetSword(sim, scorer, 0);
        Assert.True(sim.Fire(scorer));
        TestArena.RunUntil(sim, s => s.Phase != MatchPhase.Playing, 2);
    }

    [Fact, Trait("Regel", "RS-1")]
    public void RS1_NeutralerTreffer_GegnerDesGetroffenenPunktet()
    {
        var sim = TestArena.Playing();
        TestArena.SetSword(sim, PlayerId.One, 60);
        TestArena.OpenCenterLane(sim, PlayerId.One);
        TestArena.Shoot(sim, BoltOwner.Neutral, sim.Drone.Position, new Vector2(0, 1));
        TestArena.RunUntil(sim, s => s.Phase != MatchPhase.Playing, 2);

        Assert.Equal(1, sim.PlayerFor(PlayerId.Two).Score);
        Assert.Equal(PlayerId.One, sim.LastHitPlayer);
    }

    [Fact, Trait("Regel", "RS-2")]
    public void RS2_NeueRundeSetztAllesZurueck_PunkteBleiben()
    {
        var sim = TestArena.Playing(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        TestArena.DestroySegments(sim, PlayerId.One, (0, 1), (1, 1));
        ScorePoint(sim, PlayerId.One);
        TestArena.RunUntil(sim, s => s.Phase == MatchPhase.Countdown, 5);

        Assert.Equal(1, sim.PlayerFor(PlayerId.One).Score);
        Assert.Empty(sim.Bolts);
        Assert.Equal(0, sim.RoundTime);
        Assert.Equal(1f, sim.SpeedFactor);
        Assert.All(sim.Players, p => Assert.Equal(0, p.SwordAngle));
        Assert.All(sim.ShieldFor(PlayerId.One).Segments.Concat(sim.ShieldFor(PlayerId.Two).Segments), s => Assert.True(s.Alive));
        Assert.Equal(sim.Config.FieldWidth / 2, sim.Drone.X);
        Assert.Equal(StormPhase.Idle, sim.Storm.Phase);
    }

    [Fact, Trait("Regel", "RS-3")]
    public void RS3_WerZuerst3PunkteHat_GewinntDasMatch()
    {
        var sim = TestArena.Create(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        ScorePoint(sim, PlayerId.Two);
        ScorePoint(sim, PlayerId.Two);
        Assert.Equal(MatchPhase.RoundOver, sim.Phase);
        ScorePoint(sim, PlayerId.Two);

        Assert.Equal(MatchPhase.MatchOver, sim.Phase);
        Assert.Equal(PlayerId.Two, sim.Winner);
        Assert.False(sim.Fire(PlayerId.One));
    }

    [Fact, Trait("Regel", "RS-4")]
    public void RS4_AuchDieAllerersteRundeMeldetDenCountdownStart()
    {
        var sim = TestArena.Create();
        sim.Step();
        Assert.Contains(sim.Events, e => e.Type == GameEventType.CountdownStarted);
    }

    [Fact, Trait("Regel", "RS-4")]
    public void RS4_CountdownMeldetJedeSekunde_3_2_1_DannStart()
    {
        var sim = TestArena.Create();
        var times = new List<(GameEventType Type, float Time)>();
        float t = 0;
        while (sim.Phase == MatchPhase.Countdown && t < 5)
        {
            sim.Step();
            t += sim.Config.TickSeconds;
            times.AddRange(sim.Events.Select(e => (e.Type, t)));
        }
        var countdown = times.Where(x => x.Type is GameEventType.CountdownTwo or GameEventType.CountdownOne or GameEventType.RoundStarted).ToList();
        Assert.Equal([GameEventType.CountdownTwo, GameEventType.CountdownOne, GameEventType.RoundStarted], countdown.Select(x => x.Type));
        Assert.InRange(countdown[0].Time, 0.98f, 1.03f);
        Assert.InRange(countdown[1].Time, 1.98f, 2.03f);
        Assert.InRange(countdown[2].Time, 2.98f, 3.03f);
    }

    [Fact, Trait("Regel", "RS-4")]
    public void RS4_Countdown3Sekunden_WaehrenddessenKeinSchuss()
    {
        var sim = TestArena.Create();
        Assert.Equal(MatchPhase.Countdown, sim.Phase);
        Assert.False(sim.Fire(PlayerId.One));

        TestArena.RunSeconds(sim, 2.9f);
        Assert.Equal(MatchPhase.Countdown, sim.Phase);
        Assert.False(sim.Fire(PlayerId.Two));

        TestArena.RunSeconds(sim, 0.15f);
        Assert.Equal(MatchPhase.Playing, sim.Phase);
        Assert.True(sim.Fire(PlayerId.Two));
    }
}
