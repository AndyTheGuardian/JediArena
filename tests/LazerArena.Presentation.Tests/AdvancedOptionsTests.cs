using LazerArena.Core;
using LazerArena.Presentation.Audio;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

public class AdvancedOptionsTests
{
    private static readonly ScreenLayout Portrait = ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, 0.53f, twoPlayers: true);

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_StandardwerteEntsprechenDenBisherigenFestenWerten()
    {
        var a = new UserSettings().Advanced;
        Assert.Equal((int)MathF.Round(TvEffect.ScanlineDarkness * 100), a.TvScanlineDarkness);
        Assert.Equal((int)MathF.Round(TvEffect.GlowStrength * 100), a.TvGlow);
        Assert.Equal((int)MathF.Round(TvEffect.GlowSpread * 100), a.TvGlowSpread);
        Assert.Equal((int)MathF.Round(TvEffect.VignetteDarkness * 100), a.TvVignette);
        Assert.Equal(TvEffect.RowParts, a.ScanlineRowParts);
        Assert.Equal(3, a.GlowDownscale);
        Assert.Equal(100, a.SwordGlow);
        Assert.Equal(100, a.BoltGlow);
        Assert.Equal(100, a.RingGlow);
        Assert.Equal(100, a.EyeGlow);
    }

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_ProzentwerteBleibenImBereichUndZeigenProzent()
    {
        var entry = AdvancedMenu.Entries(new UserSettings()).Single(e => e.LabelKey == "adv.tvGlow");
        var s = new UserSettings();
        Assert.Equal("48%", entry.ValueKey(s));
        Assert.Equal(52, entry.Step(s, 1).Advanced.TvGlow);
        for (int i = 0; i < 40; i++) s = entry.Step(s, 1);
        Assert.Equal(100, s.Advanced.TvGlow);
        for (int i = 0; i < 40; i++) s = entry.Step(s, -1);
        Assert.Equal(0, s.Advanced.TvGlow);
        Assert.Equal("0%", entry.ValueKey(s));
    }

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_ZuruecksetzenStelltAlleStandardwerteHer()
    {
        var s = new UserSettings { AiLevel = AiLevel.Hard, Advanced = new AdvancedSettings { TvGlow = 10, SwordGlow = 200, TvGlowSoftness = AdvancedSettings.GlowSoftness.Soft } };
        var after = AdvancedMenu.Reset(s);
        Assert.Equal(AdvancedSettings.Default, after.Advanced);
        Assert.Equal(AiLevel.Hard, after.AiLevel);   // nur die erweiterten Optionen
        Assert.DoesNotContain(AdvancedMenu.Entries(s), e => e.LabelKey == "adv.reset");   // OP-16: Knopf statt Eintrag
    }

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_AlleBeschriftungenInBeidenSprachenUndKurzGenugFuerDenKlassischenStil()
    {
        foreach (var language in new[] { "de", "en" })
        {
            var keys = Localizer.KeysOf(language);
            foreach (var entry in AdvancedMenu.Entries(new UserSettings()))
            {
                Assert.Contains(entry.LabelKey, keys);
                Assert.True(BitmapFont.MeasureWidth(new Localizer(language)[entry.LabelKey]) <= 116, entry.LabelKey);
            }
            Assert.Contains("menu.advanced", keys);
        }
    }

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_TvTexturenFolgenDenEinstellungen()
    {
        var wide = TvEffect.ScanlineTexture(10, 0.8f, 2);
        Assert.Equal(20, wide.Length);
        Assert.Equal(0f, TvEffect.AlphaOf(wide[0]));
        Assert.Equal(0.8f, TvEffect.AlphaOf(wide[1]), 2);
        Assert.Equal(0f, TvEffect.VignetteAlpha(0.95f, 0.95f, 0f));
        Assert.True(TvEffect.VignetteAlpha(0.95f, 0.95f, 0.9f) > TvEffect.VignetteAlpha(0.95f, 0.95f, 0.45f));
    }

    [Theory, Trait("Regel", "OP-15")]
    [InlineData(ArenaKind.Original)]
    [InlineData(ArenaKind.SpaceStation)]
    public void OP15_SchwertLeuchtenWirktAufDieDarstellung(ArenaKind arena)
    {
        int Brightness(int swordGlow)
        {
            var settings = new UserSettings { Arena = arena, Advanced = new AdvancedSettings { SwordGlow = swordGlow } };
            var session = new GameSession(settings, twoPlayers: true, seed: 1);
            var canvas = new PixelCanvas(Portrait.Width, Portrait.Height);
            new ModernRenderer(arena).RenderGame(canvas, Portrait, session, new Localizer("de"), 1);
            int sum = 0;
            for (int y = 0; y < canvas.Height; y++)
                for (int x = 0; x < canvas.Width; x++)
                    sum += (int)(canvas[x, y].R + canvas[x, y].G + canvas[x, y].B);
            return sum;
        }
        Assert.True(Brightness(200) > Brightness(0));
    }

    [Theory, Trait("Regel", "SP-2")]
    [InlineData(ArenaKind.Original)]
    [InlineData(ArenaKind.SpaceStation)]
    public void Klingen_SindObenUndUntenGleichBreit(ArenaKind arena)
    {
        var session = new GameSession(new UserSettings { Arena = arena }, twoPlayers: true, seed: 1);
        var canvas = new PixelCanvas(Portrait.Width, Portrait.Height);
        new ModernRenderer(arena).RenderGame(canvas, Portrait, session, new Localizer("de"), 1);
        var map = new FieldMapper(Portrait.Field, session.Simulation.Config);
        int Width(PlayerId id)
        {
            var p = session.Simulation.PlayerFor(id);   // Countdown: Schwert gerade (0 Grad)
            var mid = map.ToScreen(p.Position + p.SwordDirection * (session.Simulation.Config.SwordLength * 0.7f));
            int y = (int)MathF.Round(mid.Y);
            return Enumerable.Range(0, canvas.Width).Count(x => canvas[x, y].R > 200 && canvas[x, y].G > 200 && canvas[x, y].B > 200);
        }
        int top = Width(PlayerId.Two), bottom = Width(PlayerId.One);
        Assert.True(top > 0);
        Assert.Equal(bottom, top);
    }

    [Fact, Trait("Regel", "LG-2")]
    public void LG2_RotesLichtPulsiertImGewitter()
    {
        var config = GameConfig.Default with { StormIntervalMin = 1, StormIntervalMax = 1 };
        var session = new GameSession(new UserSettings(), twoPlayers: true, seed: 2, config);
        for (int i = 0; i < 60 * 30 && session.Simulation.Storm.Phase != LazerArena.Core.StormPhase.Active; i++)
            session.Update(1f / 60f);
        Assert.Equal(LazerArena.Core.StormPhase.Active, session.Simulation.Storm.Phase);
        int Red(int frame)
        {
            var canvas = new PixelCanvas(Portrait.Width, Portrait.Height);
            new ModernRenderer().RenderGame(canvas, Portrait, session, new Localizer("de"), frame);
            var map = new FieldMapper(Portrait.Field, session.Simulation.Config);
            var c = map.ToScreen(session.Simulation.Drone.Position);
            int sum = 0;
            for (int y = (int)c.Y - 25; y <= c.Y + 25; y++)
                for (int x = (int)c.X - 25; x <= c.X + 25; x++)
                    if (x >= 0 && y >= 0 && x < canvas.Width && y < canvas.Height) sum += (int)canvas[x, y].R;
            return sum;
        }
        Assert.NotEqual(Red(0), Red(25));
    }

    [Fact, Trait("Regel", "LG-2"), Trait("Regel", "OP-13")]
    public void LG2_WarnungWirdGesprochenSonstPiept()
    {
        var cue = AudioCue.From(new GameEvent(GameEventType.StormWarning));
        Assert.Equal(VoiceLine.Warning, cue.Voice);
        Assert.Equal(SoundId.StormWarning, cue.Sound);
        Assert.True(cue.VoiceReplacesSound);
    }

    [Fact, Trait("Regel", "OP-15")]
    public void OP15_SanfteStimmeBestaetigtErweiterteOptionenAuchBeiAbgeschalteterStimme()
    {
        var cue = AudioCue.AdvancedOptions;
        Assert.Equal(VoiceLine.Advanced, cue.Voice);
        Assert.Equal(VoiceKind.Soft, cue.ForcedVoice);
        Assert.InRange(AdvancedMenu.HoldSeconds, 0.5f, 2f);
    }

    [Fact, Trait("Regel", "OP-14")]
    public void Optionen_BlitzformStehtDirektNachAngedocktemBlitz()
    {
        foreach (var style in new[] { GraphicsStyle.Classic, GraphicsStyle.Modern })
        {
            var keys = OptionsMenu.Entries(new UserSettings { Style = style }).Select(e => e.LabelKey).ToList();
            Assert.Equal(keys.IndexOf("opt.dockedBolt") + 1, keys.IndexOf("opt.bolts"));
        }
    }
}
