using LazerArena.Core;
using LazerArena.Presentation.Audio;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

/// <summary>Aenderungen der Version 1.2: Hologramm-Gitter, zwei neue Spielmusiken.</summary>
public class Version12Tests
{
    [Fact, Trait("Regel", "OP-11")]
    public void Hologramm_GitterBlendetWeichAbUndWirdLinieFuerLinieNeuGezeichnet()
    {
        const int H = HologramArena.HoldUntil, F = HologramArena.FadeFrames;
        Assert.Equal(HologramArena.LowLevel, HologramArena.GridLevel(0, 0, 0.5f).Level);           // Anfang: dunkel
        Assert.Equal(HologramArena.FullLevel, HologramArena.GridLevel(H - 1, 10, 1f).Level);       // vor dem Abblenden: hell
        float previous = HologramArena.FullLevel;
        for (int t = H; t <= H + F; t += 5)   // Abblenden: stetig fallend, ohne Spruenge
        {
            float level = HologramArena.GridLevel(t, 0, 0).Level;
            Assert.True(level <= previous && previous - level < 0.05f);
            previous = level;
        }
        // Linie 0 ist schon fertig gezeichnet, wenn Linie 5 erst beginnt; innerhalb einer Linie waechst der helle Teil
        int t0 = HologramArena.DrawStart + 5 * HologramArena.LineDelay;
        Assert.Equal(HologramArena.FullLevel, HologramArena.GridLevel(t0 + HologramArena.DrawFrames / 2, 0, 0.9f).Level);
        Assert.Equal(HologramArena.LowLevel, HologramArena.GridLevel(t0, 5, 0.1f).Level);
        Assert.Equal(HologramArena.FullLevel, HologramArena.GridLevel(t0 + HologramArena.DrawFrames / 2, 5, 0.1f).Level);
        Assert.Equal(HologramArena.LowLevel, HologramArena.GridLevel(t0 + HologramArena.DrawFrames / 2, 5, 0.9f).Level);
    }

    [Fact, Trait("Regel", "OP-11")]
    public void Hologramm_HelligkeitsstufenSindPalettenfarben()
    {
        var arena = (HologramArena)SnesArena.Create(ArenaKind.Hologram)!;
        Assert.True(arena.PaletteColors.Count <= 256);
        Assert.All(HologramArena.GridRamp, c => Assert.Contains(c, arena.PaletteColors));
        Assert.All(HologramArena.GridRamp, c => Assert.True(((c >> 16) & 0xF) == (c >> 20) && ((c >> 8) & 0xF) == ((c >> 12) & 0xF)));   // 12 Bit
    }

    [Theory, Trait("Regel", "OP-10")]
    [InlineData(SongId.PixelPulse, Instrumentation.Chiptune)]
    [InlineData(SongId.Starlight, Instrumentation.ElectricPiano)]
    public void NeueSpielmusik_HatEigeneInstrumentierungUndIstWaehlbar(SongId id, Instrumentation kit)
    {
        var song = Songs.For(id);
        Assert.Equal(kit, song.Kit);
        Assert.Equal(song.Bars, song.Melody.Length);
        var clip = MusicComposer.Compose(song);
        Assert.InRange(clip.DurationSeconds, song.LengthSeconds - 0.1f, song.LengthSeconds + 0.1f);
        Assert.True(clip.Samples.Max(v => Math.Abs((int)v)) > 10000);   // hoerbar
        var options = OptionsMenu.Entries(new UserSettings());
        var music = options.Single(e => e.LabelKey == "opt.gameMusic");
        var s = new UserSettings();
        var seen = new HashSet<string>();
        for (int i = 0; i < Enum.GetValues<GameTrack>().Length; i++) { seen.Add(music.ValueKey(s)); s = music.Step(s, 1); }
        Assert.Contains("track." + char.ToLowerInvariant(id.ToString()[0]) + id.ToString()[1..], seen);
    }

    public static IEnumerable<object[]> Phones() =>
        from style in new[] { GraphicsStyle.Classic, GraphicsStyle.Modern }
        from phone in new[] { (1080, 2340, 96), (1220, 2712, 110), (1440, 3200, 130), (720, 1600, 70), (1080, 1920, 0) }
        select new object[] { style, phone.Item1, phone.Item2, phone.Item3 };

    [Theory, Trait("Regel", "SO-7")]
    [MemberData(nameof(Phones))]
    public void Aussparung_UeberschriftenBeginnenUnterDerKamera(GraphicsStyle style, int w, int h, int hole)
    {
        StyleRenderer r = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer();
        var layout = ScreenLayout.Create(style, w, h, GameConfig.Default.FieldAspect, false);
        layout = layout with { SafeTop = ScreenLayout.ToLogical(hole, h, layout.Height) };
        var t = new Localizer("de");
        foreach (var draw in new Action<PixelCanvas>[]
                 {
                     c => r.RenderOptions(c, layout, new UserSettings { Style = style }, t, -1),
                     c => r.RenderOptions(c, layout, new UserSettings { Style = style }, t, -1, advanced: true),
                     c => r.RenderTutorial(c, layout, 1, t, 100),
                 })
        {
            var canvas = new PixelCanvas(layout.Width, layout.Height);
            draw(canvas);
            // In der Mitte oberhalb der Aussparungsgrenze steht keine Ueberschrift (gelb; weisse Sterne im Hintergrund zaehlen nicht)
            int x0 = layout.Width / 2 - 8, x1 = layout.Width / 2 + 8;
            for (int y = 0; y < layout.SafeTop; y++)
                for (int x = x0; x < x1; x++)
                    Assert.False(canvas[x, y].R > 200 && canvas[x, y].G > 170 && canvas[x, y].B < 170, $"Schrift bei ({x},{y}), SafeTop {layout.SafeTop}");
        }
        Assert.True(r.OptionRowRects(layout, 14)[^1].Bottom <= layout.Height - 18);   // alle Zeilen passen weiter auf den Bildschirm
    }

    [Fact, Trait("Regel", "SO-7")]
    public void Aussparung_UmrechnungInLogischePixel()
    {
        Assert.Equal(0, ScreenLayout.ToLogical(0, 2712, 600));
        Assert.Equal(25, ScreenLayout.ToLogical(110, 2712, 600));   // aufgerundet
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, GameConfig.Default.FieldAspect, false);
        Assert.Equal(18, layout.TopBelowCutout(18));
        Assert.Equal(27, (layout with { SafeTop = 25 }).TopBelowCutout(18));
    }
}
