using System.Numerics;
using LazerArena.Core;

namespace LazerArena.Core.Tests;

public class DockingTests
{
    private static ArenaSimulation DockedAtPlayer2(GameOptions? options = null)
    {
        var sim = TestArena.Playing(options);
        sim.Fire(PlayerId.One);   // Drohne mittig, beide Schwerter bei 0° -> trifft die Spitze von Spieler 2
        TestArena.RunUntil(sim, s => s.Bolts.Any(b => b.State == BoltState.Docked), 2);
        return sim;
    }

    [Fact, Trait("Regel", "AD-2")]
    public void AD2_BlitzDocktAnDerSchwertspitzeDesZielsAn_UndWaechstNichtWeiter()
    {
        var sim = DockedAtPlayer2();
        var bolt = sim.Bolts.Single();
        Assert.Equal(PlayerId.Two, bolt.DockedAt);
        float length = bolt.Length;

        TestArena.RunSeconds(sim, 0.5f);

        Assert.Equal(length, bolt.Length);
        Assert.All(sim.ShieldFor(PlayerId.Two).Segments, s => Assert.True(s.Alive));
        Assert.Equal(0, sim.PlayerFor(PlayerId.One).Score);
    }

    [Theory, Trait("Regel", "AD-1")]
    [InlineData(0.07f, true)]
    [InlineData(0.09f, false)]
    public void AD1_AndockzoneHatRadius8ProzentDerFeldbreite(float lateralOffset, bool expectDocked)
    {
        var sim = TestArena.Playing();
        TestArena.SetSword(sim, PlayerId.Two, 30);
        var tip = sim.PlayerFor(PlayerId.Two).SwordTip;
        var bolt = TestArena.Shoot(sim, BoltOwner.Player1, new Vector2(tip.X + lateralOffset, sim.Drone.Y), new Vector2(0, -1));

        TestArena.RunSeconds(sim, 0.6f);

        Assert.Equal(expectDocked, bolt.DockedAt == PlayerId.Two);
    }

    [Fact, Trait("Regel", "AD-2")]
    public void AD2_EigeneSchwertspitzeFaengtNichts()
    {
        var sim = TestArena.Playing();
        var tip = sim.PlayerFor(PlayerId.One).SwordTip;
        // Blitz von Spieler 2 laeuft nach oben weg von Spieler 1, direkt durch dessen Spitze
        var bolt = TestArena.Shoot(sim, BoltOwner.Player2, tip + new Vector2(0, 0.05f), new Vector2(0, -1));
        TestArena.RunSeconds(sim, 0.1f);
        Assert.NotEqual(PlayerId.One, bolt.DockedAt);
    }

    [Fact, Trait("Regel", "AD-3")]
    public void AD3_HaltezeitZwischen1Und2Sekunden_DanachErlischtDerBlitz()
    {
        var sim = DockedAtPlayer2();   // FixedRandom 0.5 -> 1,5 s
        var bolt = sim.Bolts.Single();
        Assert.InRange(bolt.HoldRemaining, 1f, 2f);

        TestArena.RunSeconds(sim, 1.4f);
        Assert.Equal(BoltState.Docked, bolt.State);

        var events = TestArena.CollectEvents(sim, 0.2f);
        Assert.Equal(BoltState.Gone, bolt.State);
        Assert.Contains(events, e => e.Type == GameEventType.BoltFizzled);
    }

    [Fact, Trait("Regel", "AD-3")]
    public void AD3_AngedockterBlitzFolgtDemSchwert()
    {
        var sim = DockedAtPlayer2(GameOptions.Default with { SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.Two, 40);
        sim.Step();

        var path = sim.Bolts.Single().PathPoints(sim.Drone, sim.Players, sim.Config.FieldWidth);
        Assert.Equal(sim.PlayerFor(PlayerId.Two).SwordTip, path[^1]);
        Assert.Equal(sim.Drone.Position, path[0]);
    }

    [Fact, Trait("Regel", "AD-4")]
    public void AD4_HoechstensEinAngedockterBlitzProSchwert()
    {
        var sim = DockedAtPlayer2();
        TestArena.RunSeconds(sim, 0.8f);
        Assert.True(sim.Fire(PlayerId.One));
        TestArena.RunSeconds(sim, 0.5f);

        Assert.Single(sim.Bolts.Where(b => b.DockedAt == PlayerId.Two).ToList());
    }

    [Fact, Trait("Regel", "AD-5")]
    public void AD5_KlingeAusserhalbDerAndockzoneHatKeineWirkung()
    {
        // Schild hinter die Figuren legen, damit die Klinge frei liegt
        var config = TestArena.Quiet() with { StraightShieldRowsP1 = [0.97f, 0.975f] };
        var sim = TestArena.Playing(config: config);
        TestArena.SetSword(sim, PlayerId.Two, 50);
        var p2 = sim.PlayerFor(PlayerId.Two);
        var bladePoint = p2.Position + p2.SwordDirection * 0.12f;   // auf der Klinge, ausserhalb der Andockzone
        var bolt = TestArena.Shoot(sim, BoltOwner.Player1, new Vector2(bladePoint.X, sim.Drone.Y), new Vector2(0, -1));

        TestArena.RunSeconds(sim, 0.6f);

        Assert.Null(bolt.DockedAt);
        Assert.Equal(BoltState.Gone, bolt.State);   // durch die Klinge hindurch bis aus dem Feld
        Assert.Equal(0, sim.PlayerFor(PlayerId.One).Score);
    }

    [Fact, Trait("Regel", "AD-6")]
    public void AD6_AngedockterBlitzHindertNichtAmSchiessen()
    {
        var sim = DockedAtPlayer2();
        Assert.True(sim.Fire(PlayerId.Two));
        Assert.Contains(sim.Bolts, b => b.Owner == BoltOwner.Player2 && b.AnchoredToDrone);
    }

    [Fact, Trait("Regel", "AD-7"), Trait("Regel", "OP-5")]
    public void AD7_Verpufft_TippenFeuertNormalUndDerAngedockteBlitzBleibt()
    {
        var sim = DockedAtPlayer2(GameOptions.Default with { DockedBolt = DockedBoltBehavior.Fizzle });
        var docked = sim.Bolts.Single();
        sim.Fire(PlayerId.Two);
        Assert.Equal(BoltState.Docked, docked.State);
    }

    [Fact, Trait("Regel", "AD-7"), Trait("Regel", "OP-5")]
    public void AD7_Zurueckwerfen_BlitzZucktVonDerSchwertspitzeZumGegner()
    {
        var sim = DockedAtPlayer2(GameOptions.Default with { DockedBolt = DockedBoltBehavior.ThrowBack, SwordTurning = SwordTurning.Instant });
        sim.SetSwordTarget(PlayerId.Two, 20);
        sim.Step();
        var docked = sim.Bolts.Single();
        var p2 = sim.PlayerFor(PlayerId.Two);

        Assert.True(sim.Fire(PlayerId.Two));

        Assert.Equal(BoltState.Gone, docked.State);
        var thrown = sim.Bolts.Single(b => b.State == BoltState.Growing);
        Assert.False(thrown.AnchoredToDrone);
        Assert.Equal(BoltOwner.Player2, thrown.Owner);
        Assert.Equal(p2.SwordTip, thrown.Origin(sim.Drone));
        Assert.Equal(PlayerId.One, thrown.Target);
        Assert.Equal(1, sim.OwnedBoltCount(PlayerId.Two));
        sim.Step();
        Assert.Contains(sim.Events, e => e.Type == GameEventType.BoltThrownBack);
    }

    [Fact, Trait("Regel", "AD-7")]
    public void AD7_Zurueckwerfen_OhneTippenErlischtNachDerHaltezeit()
    {
        var sim = DockedAtPlayer2(GameOptions.Default with { DockedBolt = DockedBoltBehavior.ThrowBack });
        TestArena.RunSeconds(sim, 2.1f);
        Assert.Empty(sim.Bolts);
    }
}
