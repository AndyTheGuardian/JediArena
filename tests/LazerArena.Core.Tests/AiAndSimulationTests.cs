using LazerArena.Core;
using LazerArena.Core.Ai;

namespace LazerArena.Core.Tests;

public class AiTests
{
    [Fact, Trait("Regel", "KI-1")]
    public void KI1_DreiStufenMitEigenemProfil()
    {
        var profiles = Enum.GetValues<AiLevel>().Select(AiProfile.For).ToList();
        Assert.Equal(3, profiles.Count);
        Assert.Equal(3, profiles.Distinct().Count());
    }

    [Fact, Trait("Regel", "KI-2")]
    public void KI2_Reaktionszeiten400_250_120ms_GenauigkeitUndSchussfrequenzSteigen()
    {
        var easy = AiProfile.For(AiLevel.Easy);
        var medium = AiProfile.For(AiLevel.Medium);
        var hard = AiProfile.For(AiLevel.Hard);

        Assert.Equal(0.400f, easy.ReactionSeconds);
        Assert.Equal(0.250f, medium.ReactionSeconds);
        Assert.Equal(0.120f, hard.ReactionSeconds);
        Assert.True(easy.AimSpreadDegrees > medium.AimSpreadDegrees && medium.AimSpreadDegrees > hard.AimSpreadDegrees);
        Assert.True(easy.ShotIntervalSeconds > medium.ShotIntervalSeconds && medium.ShotIntervalSeconds > hard.ShotIntervalSeconds);
    }

    [Theory, Trait("Regel", "KI-3")]
    [InlineData(AiLevel.Easy)]
    [InlineData(AiLevel.Hard)]
    public void KI3_KiHaeltSichAnDieselbenRegeln(AiLevel level)
    {
        var sim = new ArenaSimulation(GameConfig.Default, GameOptions.Default, seed: 7);
        var ai = new AiController(PlayerId.Two, level, new SeededRandom(8));
        var shotTimes = new List<float>();
        float time = 0, lastAngle = 0;

        for (int i = 0; i < 60 * 90 && sim.Phase != MatchPhase.MatchOver; i++)
        {
            ai.Update(sim, sim.Config.TickSeconds);
            sim.Step();
            time += sim.Config.TickSeconds;
            if (sim.Events.Any(e => e.Type is GameEventType.BoltFired or GameEventType.BoltThrownBack && e.Player == PlayerId.Two))
                shotTimes.Add(time);

            var me = sim.PlayerFor(PlayerId.Two);
            if (sim.Phase != MatchPhase.Playing) lastAngle = me.SwordAngle;   // RS-2 setzt zwischen den Runden zurueck
            Assert.InRange(me.SwordAngle, -60f, 60f);
            Assert.True(MathF.Abs(me.SwordAngle - lastAngle) <= 4.001f, "Drehrate ueberschritten");
            Assert.True(sim.OwnedBoltCount(PlayerId.Two) <= 2);
            lastAngle = me.SwordAngle;
        }

        Assert.NotEmpty(shotTimes);
        for (int i = 1; i < shotTimes.Count; i++)
            Assert.True(shotTimes[i] - shotTimes[i - 1] >= 0.8f - 0.001f, "Abklingzeit unterschritten");
    }

    [Fact, Trait("Regel", "KI-2")]
    public void KI2_SchwereKiWehrtEinenDirektenSchussAb()
    {
        var sim = TestArena.Playing();
        var ai = new AiController(PlayerId.Two, AiLevel.Hard, new FixedRandom());
        TestArena.SetSword(sim, PlayerId.One, 12);
        TestArena.SetSword(sim, PlayerId.Two, -40);
        sim.Fire(PlayerId.One);

        bool docked = false;
        for (int i = 0; i < 60 && !docked; i++)
        {
            ai.Update(sim, sim.Config.TickSeconds);
            sim.Step();
            docked = sim.Events.Any(e => e.Type == GameEventType.BoltDocked && e.Player == PlayerId.Two);
        }
        Assert.True(docked);
    }
}

public class SimulationTests
{
    [Fact, Trait("Regel", "SO-1")]
    public void SO1_PauseHaeltAllesAn()
    {
        var sim = new ArenaSimulation(seed: 5);
        TestArena.RunSeconds(sim, 4);
        sim.Pause();
        float x = sim.Drone.X, t = sim.RoundTime;
        TestArena.RunSeconds(sim, 2);
        Assert.Equal(x, sim.Drone.X);
        Assert.Equal(t, sim.RoundTime);
        Assert.False(sim.Fire(PlayerId.One));

        sim.Resume();
        TestArena.RunSeconds(sim, 0.5f);
        Assert.True(sim.RoundTime > t);
    }

    [Fact, Trait("Regel", "SO-3")]
    public void SO3_UpdateZerlegtEchtzeitInFesteSchritte()
    {
        var a = new ArenaSimulation(seed: 9);
        var b = new ArenaSimulation(seed: 9);
        for (int i = 0; i < 300; i++) a.Step();
        for (int i = 0; i < 100; i++) b.Update(0.05f);   // gleiche Gesamtzeit, andere Bildrate

        Assert.Equal(a.Drone.X, b.Drone.X, 4);
        Assert.Equal(a.RoundTime, b.RoundTime, 3);
    }

    [Fact, Trait("Regel", "SO-3"), Trait("Regel", "SO-4")]
    public void SO4_GleicherStartwert_GleicherSpielverlauf()
    {
        static string Play(int seed)
        {
            var sim = new ArenaSimulation(GameConfig.Default, GameOptions.Default, seed);
            var ai1 = new AiController(PlayerId.One, AiLevel.Medium, new SeededRandom(seed + 1));
            var ai2 = new AiController(PlayerId.Two, AiLevel.Medium, new SeededRandom(seed + 2));
            for (int i = 0; i < 60 * 60; i++)
            {
                ai1.Update(sim, sim.Config.TickSeconds);
                ai2.Update(sim, sim.Config.TickSeconds);
                sim.Step();
            }
            return $"{sim.Drone.X:F5}|{sim.PlayerFor(PlayerId.One).Score}|{sim.PlayerFor(PlayerId.Two).Score}|{sim.Bolts.Count}|{sim.RoundTime:F3}";
        }

        Assert.Equal(Play(11), Play(11));
        Assert.NotEqual(Play(11), Play(12));
    }
}
