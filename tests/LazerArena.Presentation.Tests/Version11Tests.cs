using LazerArena.Core;
using LazerArena.Presentation.Audio;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

/// <summary>Aenderungen der Version 1.1: Optionen je Stil, TV-Effekt, Anleitung, Klangvorschlaege.</summary>
public class Version11Tests
{
    private static ScreenLayout Layout(GraphicsStyle style, bool landscape = false) =>
        ScreenLayout.Create(style, landscape ? 2340 : 1080, landscape ? 1080 : 2340, GameConfig.Default.FieldAspect, false);

    [Fact, Trait("Regel", "OP-19")]
    public void OptionenJeStil_AenderungBleibtImAktivenSatz()
    {
        var p = new SettingsProfiles().Apply(new UserSettings { AiLevel = AiLevel.Hard });
        Assert.Equal(AiLevel.Hard, p.Modern.AiLevel);
        Assert.Equal(AiLevel.Medium, p.Classic.AiLevel);
    }

    [Fact, Trait("Regel", "OP-19")]
    public void OptionenJeStil_StilwechselAktiviertDenGespeichertenSatz()
    {
        var p = new SettingsProfiles().Apply(new UserSettings { Sound = false });           // Modern: Ton aus
        p = p.Apply(p.Active with { Style = GraphicsStyle.Classic });                      // Wechsel zu Klassisch
        Assert.Equal(GraphicsStyle.Classic, p.Active.Style);
        Assert.True(p.Active.Sound);                                                       // Klassisch hat eigenen Wert
        p = p.Apply(p.Active with { Style = GraphicsStyle.Modern });
        Assert.False(p.Active.Sound);
    }

    [Fact, Trait("Regel", "OP-19")]
    public void OptionenJeStil_AlteDateiWirdFuerBeideStileUebernommen()
    {
        string path = Path.Combine(Path.GetTempPath(), $"lazer-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """{ "Style": "Classic", "AiLevel": "Hard", "Sound": false }""");
            var p = new JsonSettingsStore(path).Load();
            Assert.Equal(GraphicsStyle.Classic, p.Style);
            Assert.Equal(AiLevel.Hard, p.Classic.AiLevel);
            Assert.Equal(AiLevel.Hard, p.Modern.AiLevel);
            Assert.Equal(GraphicsStyle.Modern, p.Modern.Style);
            Assert.False(p.Modern.Sound);
        }
        finally { File.Delete(path); }
    }

    [Theory, Trait("Regel", "OP-12")]
    [InlineData(GraphicsStyle.Classic)]
    [InlineData(GraphicsStyle.Modern)]
    public void TvEffekt_GleichesZeilenrasterInBeidenStilen(GraphicsStyle style)
    {
        var layout = Layout(style);
        Assert.Equal(Layout(GraphicsStyle.Classic).Height, TvEffect.ScanlineRows(layout.Height, style));
    }

    [Theory, Trait("Regel", "SO-6")]
    [InlineData(GraphicsStyle.Classic, false)]
    [InlineData(GraphicsStyle.Classic, true)]
    [InlineData(GraphicsStyle.Modern, false)]
    [InlineData(GraphicsStyle.Modern, true)]
    public void Anleitung_AlleSeitenPassenAufDenBildschirm(GraphicsStyle style, bool landscape)
    {
        StyleRenderer r = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer(ArenaKind.SpaceStation);
        var layout = Layout(style, landscape);
        var (scene, text) = r.TutorialAreas(layout);
        var nav = r.TutorialNavRects(layout);
        Assert.True(scene.Y >= 0 && scene.Bottom <= nav[0].Y && text.Bottom <= nav[0].Y);
        Assert.True(nav[1].Right <= layout.Width && nav[0].Right <= nav[1].X);
        foreach (var language in new[] { "de", "en" })
        {
            var t = new Localizer(language);
            for (int page = 0; page < StyleRenderer.TutorialPages; page++)
            {
                var lines = StyleRenderer.WrapText(t[$"tut.{page + 1}.text"], text.Width);
                Assert.True(lines.All(l => BitmapFont.MeasureWidth(l) <= text.Width), $"{language} {page}");
                Assert.True(lines.Count * (BitmapFont.GlyphHeight + 3) <= text.Height, $"{language} {page}: {lines.Count} Zeilen");
                var canvas = new PixelCanvas(layout.Width, layout.Height);
                for (int f = 0; f < StyleRenderer.TutorialLoopFrames; f += 30) r.RenderTutorial(canvas, layout, page, t, f);
            }
        }
    }

    [Fact, Trait("Regel", "SO-6")]
    public void Anleitung_DemoSzeneBewegtSich()
    {
        var r = new ModernRenderer();
        var layout = Layout(GraphicsStyle.Modern);
        var a = new PixelCanvas(layout.Width, layout.Height);
        var b = new PixelCanvas(layout.Width, layout.Height);
        r.RenderTutorial(a, layout, 0, new Localizer("de"), 0);
        r.RenderTutorial(b, layout, 0, new Localizer("de"), 30);
        var (scene, _) = r.TutorialAreas(layout);
        int diff = 0;
        for (int y = scene.Y; y < scene.Bottom; y++)
            for (int x = scene.X; x < scene.Right; x++)
                if (a[x, y] != b[x, y]) diff++;
        Assert.True(diff > 50);
    }

    [Theory, Trait("Regel", "SO-6")]
    [InlineData(GraphicsStyle.Classic, false)]
    [InlineData(GraphicsStyle.Classic, true)]
    [InlineData(GraphicsStyle.Modern, false)]
    [InlineData(GraphicsStyle.Modern, true)]
    public void Titel_FuenfMenuepunkteUeberlappenNicht(GraphicsStyle style, bool landscape)
    {
        StyleRenderer r = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer();
        var layout = Layout(style, landscape);
        var rects = r.TitleItemRects(layout);
        Assert.Equal(Enum.GetValues<TitleAction>().Length, rects.Count);
        for (int i = 1; i < rects.Count; i++) Assert.True(rects[i].Y >= rects[i - 1].Bottom);
        Assert.True(rects[^1].Bottom <= layout.Height - 8);
    }

    [Fact, Trait("Regel", "F5")]
    public void Klaenge_SchildblockSummtAnsteigendUndLeiser()
    {
        var s = new ModernSoundBank().Get(SoundId.ShieldRegrow).Samples;
        Assert.InRange(s.Length / 22050f, 0.3f, 0.7f);
        Assert.True(s.Max(v => Math.Abs((int)v)) <= ModernSoundBank.RegrowPeak * short.MaxValue + 1);
        Assert.True(ModernSoundBank.RegrowPeak < ModernSoundBank.Peak(SoundId.Fire));
    }

    [Fact, Trait("Regel", "F5")]
    public void Klaenge_AndockenKlassischLeiserBeiGleicherTonhoehe()
    {
        Assert.All(ClassicSoundBank.Script(SoundId.Dock), f => Assert.True(f.Volume <= 7));
        Assert.All(ClassicSoundBank.Script(SoundId.DockLoop), f => Assert.True(f.Volume <= 6 && f.Divider == 1));
        Assert.Equal(2, ClassicSoundBank.Script(SoundId.Dock)[0].Divider);
    }
}
