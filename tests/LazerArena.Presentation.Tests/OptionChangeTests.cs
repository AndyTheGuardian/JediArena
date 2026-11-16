using System.Numerics;
using LazerArena.Core;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

public class OptionChangeTests
{
    [Theory, Trait("Regel", "OP-4")]
    [InlineData(GraphicsStyle.Classic)]
    [InlineData(GraphicsStyle.Modern)]
    public void OP4_SchwertdrehungIstImmerBegrenztUndKeineOptionMehr(GraphicsStyle style)
    {
        var settings = new UserSettings { Style = style };
        Assert.DoesNotContain(OptionsMenu.Entries(settings), e => e.LabelKey == "opt.swordTurning");
        Assert.Equal(SwordTurning.Limited, settings.ToGameOptions().SwordTurning);
    }

    [Fact, Trait("Regel", "OP-14")]
    public void OP14_BlitzformJeStilGetrennt_StandardKlassischGleichmaessigModernZackig()
    {
        var classic = new UserSettings { Style = GraphicsStyle.Classic };
        var modern = new UserSettings { Style = GraphicsStyle.Modern };
        Assert.Equal(BoltShape.Regular, classic.Bolts);
        Assert.Equal(BoltShape.Lightning, modern.Bolts);

        var classicEntry = OptionsMenu.Entries(classic).Single(e => e.LabelKey == "opt.bolts");
        var changed = classicEntry.Step(classic, 1);
        Assert.Equal(BoltShape.Lightning, changed.ClassicBolts);
        Assert.Equal(BoltShape.Lightning, changed.ModernBolts);   // unveraendert
        Assert.Equal("val.boltLightning", classicEntry.ValueKey(changed));

        var modernEntry = OptionsMenu.Entries(modern).Single(e => e.LabelKey == "opt.bolts");
        var changedModern = modernEntry.Step(modern, 1);
        Assert.Equal(BoltShape.Regular, changedModern.ModernBolts);
        Assert.Equal(BoltShape.Regular, changedModern.ClassicBolts);
    }

    [Fact, Trait("Regel", "OP-14")]
    public void OP14_GleichmaessigesZickzackWechseltDieSeiteBeiJedemPunkt()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(0, 100);
        var regular = Zigzag.Along([a, b], 4, 2, seed: 3, irregular: false);
        var sides = regular.Skip(1).SkipLast(1).Select(p => MathF.Sign(p.X)).ToList();
        Assert.True(sides.Count > 10);
        for (int i = 1; i < sides.Count; i++)
            Assert.Equal(-sides[i - 1], sides[i]);
        Assert.All(regular, p => Assert.InRange(MathF.Abs(p.X), 0, 2.001f));
    }

    [Fact, Trait("Regel", "OP-11")]
    public void OP11_AnimierteArenenNurMitDemWortMarkiert()
    {
        foreach (var language in new[] { "de", "en" })
        {
            var t = new Localizer(language);
            Assert.DoesNotContain("*", t["opt.arenaAnimated"]);
            foreach (var arena in Enum.GetValues<ArenaKind>())
                Assert.DoesNotContain("*", t["arena." + arena]);
            Assert.Equal(language == "de" ? "ANIMIERT" : "ANIMATED", t["opt.arenaAnimated"]);
        }
    }

    [Fact, Trait("Regel", "RS-4")]
    public void RS4_NachDemCountdownStehtGoKurzImBild()
    {
        Assert.Equal("LOS!", new Localizer("de")["hud.go"]);
        Assert.Equal("GO!", new Localizer("en")["hud.go"]);
        Assert.InRange(StyleRenderer.GoDisplaySeconds, 0.4f, 1f);
    }
}
