using LazerArena.Core;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

/// <summary>Aenderungen der Version 1.0: Schilde, Bilder, Optionen, Pausenmenue, Rundumleuchten, Blitze.</summary>
public class Version10Tests
{
    private static ScreenLayout Layout(GraphicsStyle style, bool landscape = false, bool twoPlayers = false) =>
        ScreenLayout.Create(style, landscape ? 2340 : 1080, landscape ? 1080 : 2340, GameConfig.Default.FieldAspect, twoPlayers);

    [Theory, Trait("Regel", "OP-6")]
    [InlineData(false, ShieldShape.Straight)]
    [InlineData(true, ShieldShape.Straight)]
    [InlineData(false, ShieldShape.Curved)]
    public void Klassisch_SchildbloeckeImVerhaeltnis3zu1UndGleichmaessigVerteilt(bool landscape, ShieldShape shape)
    {
        var layout = Layout(GraphicsStyle.Classic, landscape);
        var map = new FieldMapper(layout.Field, GameConfig.Default);
        Assert.Equal(0, (layout.Field.Width + 1) % 16);   // Feldbreite = 16 Bloecke + 15 Fugen von 1 Pixel
        foreach (var id in new[] { PlayerId.One, PlayerId.Two })
        {
            var player = new Player(id, GameConfig.Default);
            var blocks = ClassicRenderer.ShieldBlocks(map, new Shield(id, player.Position, GameConfig.Default, shape));
            var sizes = blocks.Select(b => (W: b.Rect.X1 - b.Rect.X0 + 1, H: b.Rect.Y1 - b.Rect.Y0 + 1)).Distinct().ToList();
            Assert.Single(sizes);
            Assert.Equal(3 * sizes[0].H, sizes[0].W);
            if (shape != ShieldShape.Straight) continue;
            var front = blocks.Where(b => b.Segment.Row == 0).OrderBy(b => b.Rect.X0).ToList();
            Assert.Equal(layout.Field.X, front[0].Rect.X0);                  // buendig an beiden Waenden
            Assert.Equal(layout.Field.Right - 1, front[^1].Rect.X1);
            for (int i = 1; i < front.Count; i++) Assert.Equal(front[i - 1].Rect.X1 + 2, front[i].Rect.X0);   // 1 Pixel Fuge
            int back = blocks.First(b => b.Segment.Row == 1).Rect.Y0;
            Assert.Equal(2 * sizes[0].H, Math.Abs(front[0].Rect.Y0 - back));   // Reihenfuge = ganze Blockhoehe (Entwurf 2)
        }
    }

    [Theory, Trait("Regel", "RS-5")]
    [InlineData(ArtId.Victory)]
    [InlineData(ArtId.Defeat)]
    [InlineData(ArtId.Title)]
    public void Bilder_LadenMitDurchsichtigemRandUndPassenAufDenBildschirm(ArtId id)
    {
        foreach (bool landscape in new[] { false, true })
        {
            var art = PixelArt.Get(id, landscape);
            var layout = Layout(GraphicsStyle.Modern, landscape);
            Assert.True(art.Width < layout.Width && art.Height < layout.Height);
            Assert.True(art.IsTransparent(0, 0) || art.IsTransparent(art.Width - 1, 0));   // Hintergrund bleibt durchsichtig
        }
    }

    [Fact, Trait("Regel", "RS-5")]
    public void Bilder_DefekteDateiWirdAbgelehnt()
    {
        Assert.Throws<InvalidDataException>(() => PixelArt.Parse([1, 2, 3]));
        Assert.Throws<InvalidDataException>(() => PixelArt.Parse([(byte)'L', (byte)'Z', (byte)'P', (byte)'X', 1, 0, 1, 0, 0, 1]));
    }

    [Theory, Trait("Regel", "RS-5")]
    [InlineData(true)]
    [InlineData(false)]
    public void Siegerbildschirm_ZeigtDasBildUnverfaelscht(bool humanWon)
    {
        var layout = Layout(GraphicsStyle.Modern);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        var info = new VictoryInfo(humanWon ? "P1" : "KI", humanWon, 3, 1, true);
        new ModernRenderer().RenderVictory(canvas, layout, info, new Localizer("de"), 10);
        Assert.True(canvas.CountColors() <= ModernPalette.MaxColors);
        var art = PixelArt.Get(humanWon ? ArtId.Victory : ArtId.Defeat, false);
        int opaque = 0, match = 0;
        var probe = new PixelCanvas(art.Width, art.Height);
        art.Draw(probe, 0, 0);
        int left = layout.Width / 2 - art.Width / 2, top = (humanWon ? (int)(layout.Height * 0.65f) : (int)(layout.Height * 0.6f)) - art.Height;
        for (int y = 0; y < art.Height; y++)
            for (int x = 0; x < art.Width; x++)
            {
                if (art.IsTransparent(x, y)) continue;
                opaque++;
                if (canvas[left + x, top + y] == probe[x, y]) match++;
            }
        Assert.Equal(opaque, match);   // ohne Dithering darueber gezeichnet
    }

    [Fact, Trait("Regel", "OP-16")]
    public void Optionen_StandardSetztNormaleOptionenZurueckUndBehaeltErweiterte()
    {
        var advanced = new AdvancedSettings { TvGlow = 12 };
        var s = new UserSettings { Style = GraphicsStyle.Classic, AiLevel = AiLevel.Hard, Sound = false, Advanced = advanced };
        var after = OptionsMenu.Reset(s);
        Assert.Equal(new UserSettings { Style = GraphicsStyle.Classic, Advanced = advanced }, after);   // Grafikstil bleibt
    }

    [Theory, Trait("Regel", "OP-16")]
    [InlineData(GraphicsStyle.Classic)]
    [InlineData(GraphicsStyle.Modern)]
    public void Optionen_ZurueckUndStandardKnoepfeUeberlappenNicht(GraphicsStyle style)
    {
        StyleRenderer r = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer();
        var layout = Layout(style);
        var back = r.OptionsBackRect(layout);
        var reset = r.OptionsResetRect(layout);
        Assert.True(back.Right <= reset.X);
        Assert.True(reset.Right <= layout.Width);
        var t = new Localizer("de");
        Assert.True(BitmapFont.MeasureWidth(t["menu.reset"]) <= reset.Width && BitmapFont.MeasureWidth(t["menu.resetConfirm"]) <= reset.Width);
    }

    [Theory, Trait("Regel", "OP-17")]
    [InlineData(GraphicsStyle.Classic, ArenaKind.Original, new[] { "opt.voice" })]
    [InlineData(GraphicsStyle.Modern, ArenaKind.Original, new[] { "opt.gameMusic", "opt.voice" })]
    [InlineData(GraphicsStyle.Modern, ArenaKind.SpaceStation, new[] { "opt.gameMusic", "opt.voice", "opt.stormLights" })]
    public void Pausenmenue_EnthaeltMusikStimmeUndLeuchten(GraphicsStyle style, ArenaKind arena, string[] expected)
    {
        var s = new UserSettings { Style = style, Arena = arena };
        Assert.Equal(expected, PauseMenu.Entries(s).Select(e => e.LabelKey));
    }

    [Theory, Trait("Regel", "OP-17")]
    [InlineData(GraphicsStyle.Classic, false)]
    [InlineData(GraphicsStyle.Modern, false)]
    [InlineData(GraphicsStyle.Modern, true)]
    public void Pausenmenue_ZeilenLiegenUnterDenKnoepfenUndImBild(GraphicsStyle style, bool landscape)
    {
        StyleRenderer r = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer(ArenaKind.SpaceStation);
        var layout = Layout(style, landscape);
        var buttons = r.PauseMenuRects(layout);
        var rows = r.PauseOptionRects(layout, 3);
        Assert.True(rows[0].Y > buttons[1].Bottom || rows[0].X >= buttons[0].Right);
        Assert.True(rows[^1].Bottom < layout.Height && rows[0].Right <= layout.Width);
        for (int i = 1; i < rows.Count; i++) Assert.True(rows[i].Y >= rows[i - 1].Bottom);
    }

    [Fact, Trait("Regel", "OP-18")]
    public void Warnleuchten_NurInDerRaumstationUmschaltbar()
    {
        Assert.Contains(OptionsMenu.Entries(new UserSettings { Arena = ArenaKind.SpaceStation }), e => e.LabelKey == "opt.stormLights");
        Assert.DoesNotContain(OptionsMenu.Entries(new UserSettings { Arena = ArenaKind.Hologram }), e => e.LabelKey == "opt.stormLights");
        var off = OptionsMenu.StormLights.Step(new UserSettings(), 1);
        Assert.False(off.StormLights);
        Assert.True(new UserSettings().StormLights);
    }

    [Fact, Trait("Regel", "OP-18")]
    public void Warnleuchten_LaufenSanftHochUndWiederAus()
    {
        var arena = new SpaceStationArena();
        var layout = Layout(GraphicsStyle.Modern);
        var c = new PixelCanvas(layout.Width, layout.Height);
        int frame = 0;
        void Run(int frames, bool active) { for (int i = 0; i < frames; i++) arena.DrawStormLights(c, layout, frame++, active); }
        Run(2, true);
        Assert.InRange(arena.Spin, 0.001f, 0.1f);                 // beginnt langsam
        Run(SpaceStationArena.SpinUpFrames, true);
        Assert.Equal(1f, arena.Spin);
        Run(SpaceStationArena.SpinDownFrames / 2, false);
        Assert.InRange(arena.Spin, 0.3f, 0.7f);                   // laeuft aus statt abrupt zu stoppen
        Run(SpaceStationArena.SpinDownFrames, false);
        Assert.Equal(0f, arena.Spin);
    }

    [Fact, Trait("Regel", "OP-18")]
    public void Warnleuchten_AusgeschaltetBleibenDunkel()
    {
        var config = GameConfig.Default with { StormIntervalMin = 1, StormIntervalMax = 1 };
        var settings = new UserSettings { Arena = ArenaKind.SpaceStation };
        int RedPixels(UserSettings s)
        {
            var session = new GameSession(s, twoPlayers: false, seed: 2, config);
            var renderer = new ModernRenderer(ArenaKind.SpaceStation);
            var layout = Layout(GraphicsStyle.Modern);
            var canvas = new PixelCanvas(layout.Width, layout.Height);
            int frame = 0;
            while (session.Simulation.Storm.Phase != StormPhase.Active && frame < 3600) { session.Update(1f / 60); frame++; }
            for (int i = 0; i < 90; i++) { renderer.RenderGame(canvas, layout, session, new Localizer("de"), frame++, s); }
            var f = layout.Field;
            int count = 0;
            for (int y = f.Y; y < f.Y + 40; y++)
                for (int x = f.X; x < f.X + 40; x++)
                    if (canvas[x, y].R > canvas[x, y].G + 40 && canvas[x, y].R > canvas[x, y].B + 40) count++;
            return count;
        }
        Assert.True(RedPixels(settings) > RedPixels(settings with { StormLights = false }));
    }

    [Fact, Trait("Regel", "BL-8")]
    public void Blitze_HabenWieDieKlingeEinenDeckendenHellenKern()
    {
        var layout = Layout(GraphicsStyle.Modern);
        foreach (var arena in new[] { ArenaKind.Original, ArenaKind.SpaceStation })
        {
            var session = new GameSession(new UserSettings { Arena = arena, ModernBolts = BoltShape.Regular }, false, seed: 3);
            for (int i = 0; i < 60 * 8 && session.Simulation.Bolts.Count == 0; i++) session.Update(1f / 60);
            var canvas = new PixelCanvas(layout.Width, layout.Height);
            new ModernRenderer(arena).RenderGame(canvas, layout, session, new Localizer("de"), 1);
            int nearWhite = 0;
            var f = layout.Field;
            for (int y = f.Y + f.Height / 3; y < f.Y + f.Height * 2 / 3; y++)
                for (int x = f.X; x < f.Right; x++)
                    if (canvas[x, y].R > 200 && canvas[x, y].G > 200 && canvas[x, y].B > 200) nearWhite++;
            Assert.True(nearWhite > 5, $"{arena}: {nearWhite}");
            Assert.True(ModernRenderer.BoltWidth < ModernRenderer.BladeWidth);
        }
    }
}
