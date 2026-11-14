using LazerArena.Core;

namespace LazerArena.Core.Tests;

public class FieldAndPlayerTests
{
    [Fact, Trait("Regel", "SF-1")]
    public void SF1_Spieler1StehtUnten_Spieler2Oben_BeideMittig()
    {
        var sim = TestArena.Create();
        var p1 = sim.PlayerFor(PlayerId.One);
        var p2 = sim.PlayerFor(PlayerId.Two);

        Assert.True(p1.Position.Y > sim.Drone.Y);
        Assert.True(p2.Position.Y < sim.Drone.Y);
        Assert.Equal(sim.Config.FieldWidth / 2, p1.Position.X);
        Assert.Equal(sim.Config.FieldWidth / 2, p2.Position.X);
    }

    [Fact, Trait("Regel", "SF-2")]
    public void SF2_DrohneBleibtImFeld_AufFesterHoehe_UndWechseltDieRichtung()
    {
        var config = GameConfig.Default with { StormIntervalMin = 10_000, StormIntervalMax = 10_000 };
        var sim = new ArenaSimulation(config, GameOptions.Default, seed: 42);
        float y = sim.Drone.Y;
        int directionChanges = 0;
        float lastVelocity = sim.Drone.VelocityX;

        for (int i = 0; i < 60 * 60; i++)
        {
            sim.Step();
            Assert.InRange(sim.Drone.X, config.DroneRadius, config.FieldWidth - config.DroneRadius);
            Assert.Equal(y, sim.Drone.Y);
            if (MathF.Sign(sim.Drone.VelocityX) != MathF.Sign(lastVelocity)) directionChanges++;
            lastVelocity = sim.Drone.VelocityX;
        }
        Assert.True(directionChanges >= 5, $"nur {directionChanges} Richtungswechsel");
    }

    [Theory, Trait("Regel", "SF-3")]
    [InlineData(ShieldShape.Straight)]
    [InlineData(ShieldShape.Curved)]
    public void SF3_SchildHat2Mal16Segmente(ShieldShape shape)
    {
        var sim = TestArena.Create(GameOptions.Default with { ShieldShape = shape });
        foreach (var id in new[] { PlayerId.One, PlayerId.Two })
        {
            var segments = sim.ShieldFor(id).Segments;
            Assert.Equal(32, segments.Count);
            Assert.Equal(16, segments.Count(s => s.Row == 0));
            Assert.All(segments, s => Assert.True(s.Alive));
        }
    }

    [Fact, Trait("Regel", "SF-3")]
    public void SF3_GeradesSchild_ReihenLiegenWaagerecht_UeberGanzeBreite()
    {
        var sim = TestArena.Create();
        var front = sim.ShieldFor(PlayerId.One).Segments.Where(s => s.Row == 0).ToList();
        Assert.Single(front.Select(s => s.Bounds.Top).Distinct());
        Assert.Equal(0, front.Min(s => s.Bounds.Left), 3);
        Assert.Equal(sim.Config.FieldWidth, front.Max(s => s.Bounds.Right), 3);
    }

    [Fact, Trait("Regel", "SF-3")]
    public void SF3_GebogenesSchild_MitteLiegtWeiterVorneAlsDieRaender()
    {
        var sim = TestArena.Create(GameOptions.Default with { ShieldShape = ShieldShape.Curved });
        var front = sim.ShieldFor(PlayerId.One).Segments.Where(s => s.Row == 0).OrderBy(s => s.Index).ToList();
        // Spieler 1 schaut nach oben: "weiter vorne" = kleineres Y
        Assert.True(front[7].Bounds.Top < front[0].Bounds.Top);
        Assert.True(front[8].Bounds.Top < front[15].Bounds.Top);
    }

    [Theory, Trait("Regel", "SF-3")]
    [InlineData(ShieldShape.Straight)]
    [InlineData(ShieldShape.Curved)]
    public void SF3_SchwertspitzeRagtUeberDieVordereSchildreiheHinaus(ShieldShape shape)
    {
        var sim = TestArena.Create(GameOptions.Default with { ShieldShape = shape });
        var p1 = sim.PlayerFor(PlayerId.One);
        var p2 = sim.PlayerFor(PlayerId.Two);
        var front1 = sim.ShieldFor(PlayerId.One).Segments.Where(s => s.Row == 0);
        var front2 = sim.ShieldFor(PlayerId.Two).Segments.Where(s => s.Row == 0);

        Assert.True(p1.SwordTip.Y < front1.Min(s => s.Bounds.Top));
        Assert.True(p2.SwordTip.Y > front2.Max(s => s.Bounds.Bottom));
    }

    [Theory, Trait("Regel", "SF-3"), Trait("Regel", "AD-1")]
    [InlineData(ShieldShape.Straight)]
    [InlineData(ShieldShape.Curved)]
    public void SF3_AndockzoneRagtBeiJedemSchwertwinkelVorDasSchild(ShieldShape shape)
    {
        var sim = TestArena.Create(GameOptions.Default with { ShieldShape = shape });
        foreach (var id in new[] { PlayerId.One, PlayerId.Two })
        {
            var player = sim.PlayerFor(id);
            var front = sim.ShieldFor(id).Segments.Where(s => s.Row == 0).ToList();
            for (float angle = -60; angle <= 60; angle += 5)
            {
                TestArena.SetSword(sim, id, angle);
                var tip = player.SwordTip;
                var segment = front.Where(s => tip.X >= s.Bounds.Left && tip.X <= s.Bounds.Right)
                    .OrderBy(s => s.Index).FirstOrDefault();
                if (segment is null) continue;   // Spitze seitlich neben dem gebogenen Schild
                // vorderster Punkt der Andockzone (Richtung Feldmitte) muss vor der Schildkante liegen
                float zoneFront = tip.Y + player.Forward * sim.Config.DockRadius;
                float shieldFront = id == PlayerId.One ? segment.Bounds.Top : segment.Bounds.Bottom;
                Assert.True(player.Forward * (zoneFront - shieldFront) > 0, $"{id} bei {angle}°");
            }
        }
    }

    [Theory, Trait("Regel", "AD-2")]
    [InlineData(ShieldShape.Straight)]
    [InlineData(ShieldShape.Curved)]
    public void AD2_AufDieSpitzeGezielterBlitzDocktAuchBeiSteilemSchwertAn(ShieldShape shape)
    {
        var sim = TestArena.Playing(GameOptions.Default with { ShieldShape = shape });
        TestArena.SetSword(sim, PlayerId.Two, 60);
        var tip = sim.PlayerFor(PlayerId.Two).SwordTip;
        var bolt = TestArena.Shoot(sim, BoltOwner.Player1, new System.Numerics.Vector2(tip.X, sim.Drone.Y),
            new System.Numerics.Vector2(0, -1));

        TestArena.RunSeconds(sim, 0.5f);

        Assert.Equal(PlayerId.Two, bolt.DockedAt);
        Assert.All(sim.ShieldFor(PlayerId.Two).Segments, s => Assert.True(s.Alive));
    }

    [Fact, Trait("Regel", "SP-1")]
    public void SP1_FigurBewegtSichNicht()
    {
        var sim = new ArenaSimulation(seed: 3);
        var before = sim.PlayerFor(PlayerId.One).Position;
        sim.SetSwordTarget(PlayerId.One, 45);
        TestArena.RunSeconds(sim, 10);
        Assert.Equal(before, sim.PlayerFor(PlayerId.One).Position);
    }

    [Theory, Trait("Regel", "SP-2")]
    [InlineData(80, 60)]
    [InlineData(-75, -60)]
    [InlineData(30, 30)]
    public void SP2_SchwertwinkelWirdAufPlusMinus60Begrenzt(float requested, float expected)
    {
        var sim = TestArena.Create(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.One, requested);
        sim.Step();
        Assert.Equal(expected, sim.PlayerFor(PlayerId.One).SwordAngle);
    }

    [Fact, Trait("Regel", "SP-2"), Trait("Regel", "OP-4")]
    public void SP2_DrehungSofort_SchwertFolgtImSelbenSchritt()
    {
        var sim = TestArena.Create(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.One, 50);
        sim.Step();
        Assert.Equal(50, sim.PlayerFor(PlayerId.One).SwordAngle);
    }

    [Fact, Trait("Regel", "SP-2"), Trait("Regel", "OP-4")]
    public void SP2_DrehungBegrenzt_Hoechstens240GradProSekunde()
    {
        var sim = TestArena.Create(GameOptions.Default with { SwordTurning = SwordTurning.Limited });
        sim.SetSwordTarget(PlayerId.One, 60);
        sim.Step();
        Assert.Equal(4f, sim.PlayerFor(PlayerId.One).SwordAngle, 3);   // 240 °/s * 1/60 s
        TestArena.RunSeconds(sim, 0.25f);
        Assert.Equal(60f, sim.PlayerFor(PlayerId.One).SwordAngle, 3);
    }

    [Fact, Trait("Regel", "SP-2")]
    public void SP2_WinkelIstAusSichtDesSpielers_PositivHeisstNachRechts()
    {
        var sim = TestArena.Create(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.One, 30);
        sim.SetSwordTarget(PlayerId.Two, 30);
        sim.Step();
        // Spieler 1 (schaut nach oben): rechts = +X. Spieler 2 (schaut nach unten): rechts = -X.
        Assert.True(sim.PlayerFor(PlayerId.One).SwordDirection.X > 0);
        Assert.True(sim.PlayerFor(PlayerId.Two).SwordDirection.X < 0);
        Assert.Equal(30f, sim.PlayerFor(PlayerId.Two).AngleFor(sim.PlayerFor(PlayerId.Two).SwordDirection), 3);
    }

    [Fact, Trait("Regel", "SP-3")]
    public void SP3_DrohneFeuertImSchwertwinkelDesSchuetzen()
    {
        var sim = TestArena.Playing(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.One, -25);
        sim.Step();

        Assert.True(sim.Fire(PlayerId.One));
        var bolt = Assert.Single(sim.Bolts);
        Assert.True(bolt.AnchoredToDrone);
        Assert.Equal(BoltOwner.Player1, bolt.Owner);
        Assert.Equal(sim.PlayerFor(PlayerId.One).SwordDirection.X, bolt.Direction.X, 4);
        Assert.Equal(sim.PlayerFor(PlayerId.One).SwordDirection.Y, bolt.Direction.Y, 4);
        Assert.Equal(PlayerId.Two, bolt.Target);
    }

    [Fact, Trait("Regel", "SP-4")]
    public void SP4_AbklingzeitVon08Sekunden()
    {
        var sim = TestArena.Playing();
        Assert.True(sim.Fire(PlayerId.One));
        TestArena.RunSeconds(sim, 0.75f);
        Assert.False(sim.Fire(PlayerId.One));
        TestArena.RunSeconds(sim, 0.1f);
        Assert.True(sim.Fire(PlayerId.One));
    }

    [Fact, Trait("Regel", "SP-4")]
    public void SP4_HoechstensZweiEigeneBlitzeGleichzeitig()
    {
        var config = TestArena.Quiet() with { ShotCooldown = 0 };
        var sim = TestArena.Playing(config: config);
        TestArena.SetSword(sim, PlayerId.Two, 60);   // Spitze aus der Bahn, damit nichts andockt

        Assert.True(sim.Fire(PlayerId.One));
        Assert.True(sim.Fire(PlayerId.One));
        Assert.False(sim.Fire(PlayerId.One));
        Assert.Equal(2, sim.OwnedBoltCount(PlayerId.One));
    }

    [Fact, Trait("Regel", "SP-4")]
    public void SP4_AmGegnerAngedockterBlitzZaehltMit()
    {
        var config = TestArena.Quiet() with { ShotCooldown = 0 };
        var sim = TestArena.Playing(config: config);
        sim.Fire(PlayerId.One);
        TestArena.RunUntil(sim, s => s.Bolts.Any(b => b.State == BoltState.Docked), 2);

        Assert.Equal(1, sim.OwnedBoltCount(PlayerId.One));
        Assert.True(sim.Fire(PlayerId.One));
        Assert.False(sim.Fire(PlayerId.One));
    }
}
