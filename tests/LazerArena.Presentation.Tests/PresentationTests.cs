using System.Globalization;
using LazerArena.Core;
using LazerArena.Presentation;
using LazerArena.Presentation.Input;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Tests;

public class LanguageTests
{
    [Theory, Trait("Regel", "SO-5"), Trait("Regel", "OP-2")]
    [InlineData("de-DE", "de")]
    [InlineData("de-AT", "de")]
    [InlineData("de-CH", "de")]
    [InlineData("en-US", "en")]
    [InlineData("fr-FR", "en")]
    [InlineData("ja-JP", "en")]
    public void SO5_Automatisch_DeutschNurBeiDeutscherSystemsprache_SonstEnglisch(string culture, string expected) =>
        Assert.Equal(expected, LanguageResolver.Resolve(LanguageSetting.Automatic, new CultureInfo(culture)));

    [Theory, Trait("Regel", "OP-2")]
    [InlineData(LanguageSetting.German, "fr-FR", "de")]
    [InlineData(LanguageSetting.English, "de-DE", "en")]
    public void OP2_ManuelleSpracheUebersteuertDieSystemsprache(LanguageSetting setting, string culture, string expected) =>
        Assert.Equal(expected, LanguageResolver.Resolve(setting, new CultureInfo(culture)));

    [Fact, Trait("Regel", "SO-5")]
    public void SO5_BeideSprachdateienHabenDieselbenSchluessel()
    {
        var de = Localizer.KeysOf("de").OrderBy(k => k);
        var en = Localizer.KeysOf("en").OrderBy(k => k);
        Assert.Equal(en, de);
    }

    [Theory, Trait("Regel", "SO-5")]
    [InlineData("de")]
    [InlineData("en")]
    public void SO5_AlleTexteSindMitDerPixelschriftDarstellbar(string language)
    {
        var t = new Localizer(language);
        foreach (var key in Localizer.KeysOf(language))
            Assert.All(t[key], c => Assert.True(BitmapFont.Supports(c), $"'{c}' in {key}"));
    }

    [Fact, Trait("Regel", "SO-5")]
    public void SO5_TexteKommenAusDerRessourcendatei()
    {
        Assert.Equal("1 SPIELER", new Localizer("de")["menu.onePlayer"]);
        Assert.Equal("1 PLAYER", new Localizer("en")["menu.onePlayer"]);
    }
}

public class SettingsTests
{
    [Fact, Trait("Regel", "OP-1")]
    public void Abschnitt10_StandardwerteEntsprechenDemRegelwerk()
    {
        var s = new UserSettings();
        Assert.Equal(GraphicsStyle.Modern, s.Style);
        Assert.Equal(LanguageSetting.Automatic, s.Language);
        Assert.Equal(AiLevel.Medium, s.AiLevel);
        Assert.Equal(SwordTurning.Limited, s.SwordTurning);
        Assert.Equal(DockedBoltBehavior.Fizzle, s.DockedBolt);
        Assert.Equal(ShieldShape.Straight, s.ShieldShape);
        Assert.True(s.Sound);
        Assert.True(s.Music);
        Assert.Equal(WallBehavior.Bounce, s.Walls);
        Assert.Equal(GameTrack.Orbit, s.GameMusic);
    }

    [Fact, Trait("Regel", "SO-2"), Trait("Regel", "OP-1")]
    public void SO2_GrafikstilBeeinflusstDieSpiellogikNicht()
    {
        var modern = new UserSettings { Style = GraphicsStyle.Modern }.ToGameOptions();
        var classic = new UserSettings { Style = GraphicsStyle.Classic }.ToGameOptions();
        Assert.Equal(modern, classic);
    }

    [Fact, Trait("Regel", "OP-8")]
    public void OP8_MusikNurImModernenStil()
    {
        Assert.True(new UserSettings { Style = GraphicsStyle.Modern, Music = true }.MusicActive);
        Assert.False(new UserSettings { Style = GraphicsStyle.Classic, Music = true }.MusicActive);
        Assert.DoesNotContain(OptionsMenu.Entries(new UserSettings { Style = GraphicsStyle.Classic }), e => e.LabelKey == "opt.music");
        Assert.Contains(OptionsMenu.Entries(new UserSettings { Style = GraphicsStyle.Modern }), e => e.LabelKey == "opt.music");
    }

    [Fact, Trait("Regel", "OP-7")]
    public void OP7_TonLaesstSichUmschalten()
    {
        var entry = OptionsMenu.Entries(new UserSettings()).Single(e => e.LabelKey == "opt.sound");
        var off = entry.Step(new UserSettings(), 1);
        Assert.False(off.Sound);
        Assert.Equal("val.off", entry.ValueKey(off));
    }

    [Fact]
    public void Optionen_DurchschaltenLaeuftImKreis()
    {
        var entry = OptionsMenu.Entries(new UserSettings()).Single(e => e.LabelKey == "opt.aiLevel");
        var s = new UserSettings();
        Assert.Equal(AiLevel.Hard, entry.Step(s, 1).AiLevel);
        Assert.Equal(AiLevel.Easy, entry.Step(s, -1).AiLevel);
        Assert.Equal(AiLevel.Medium, entry.Step(entry.Step(entry.Step(s, 1), 1), 1).AiLevel);
    }

    [Fact, Trait("Regel", "OP-1")]
    public void Optionen_WerdenDauerhaftGespeichert()
    {
        string path = Path.Combine(Path.GetTempPath(), $"lazer-{Guid.NewGuid():N}", "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            var changed = new UserSettings { Style = GraphicsStyle.Classic, Walls = WallBehavior.Extinguish, Music = false };
            store.Save(changed);
            Assert.Equal(changed, new JsonSettingsStore(path).Load());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void Optionen_FehlendeOderKaputteDateiLiefertStandardwerte()
    {
        string path = Path.Combine(Path.GetTempPath(), $"lazer-{Guid.NewGuid():N}.json");
        Assert.Equal(new UserSettings(), new JsonSettingsStore(path).Load());
        File.WriteAllText(path, "{ kaputt");
        try { Assert.Equal(new UserSettings(), new JsonSettingsStore(path).Load()); }
        finally { File.Delete(path); }
    }
}

public class LayoutTests
{
    [Theory, Trait("Regel", "SF-1")]
    [InlineData(GraphicsStyle.Classic, 1080, 2340)]
    [InlineData(GraphicsStyle.Modern, 1080, 2400)]
    [InlineData(GraphicsStyle.Modern, 720, 1280)]
    [InlineData(GraphicsStyle.Classic, 2340, 1080)]
    [InlineData(GraphicsStyle.Modern, 2400, 1080)]
    public void SF1_SpielfeldBleibtHochkantImRichtigenSeitenverhaeltnisUndPasstAufDenBildschirm(GraphicsStyle style, int w, int h)
    {
        var layout = ScreenLayout.Create(style, w, h, 0.53f, twoPlayers: false);
        var f = layout.Field;
        Assert.True(f.Height > f.Width);
        Assert.InRange(f.Width / (float)f.Height, 0.51f, 0.55f);
        Assert.True(f.X >= 0 && f.Y >= 0 && f.Right <= layout.Width && f.Bottom <= layout.Height);
        Assert.Equal(w >= h ? ScreenOrientation.Landscape : ScreenOrientation.Portrait, layout.Orientation);
    }

    [Fact, Trait("Regel", "SF-1")]
    public void SF1_ImQuerformatLiegenInfobereicheLinksUndRechts()
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 2340, 1080, 0.53f, false);
        Assert.True(layout.LeftPanel.Right <= layout.Field.X);
        Assert.True(layout.RightPanel.X >= layout.Field.Right);
    }

    [Fact]
    public void Layout_ShortSideJeStil()
    {
        Assert.Equal(120, ScreenLayout.Create(GraphicsStyle.Classic, 1080, 2340, 0.53f, false).Width);
        Assert.Equal(270, ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, 0.53f, false).Width);
        Assert.Equal(120, ScreenLayout.Create(GraphicsStyle.Classic, 2340, 1080, 0.53f, false).Height);
    }
}

public class InputTests
{
    private static readonly ScreenLayout Layout = ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, 0.53f, twoPlayers: true);

    [Fact, Trait("Regel", "SP-2")]
    public void SP2_WaagerechtesWischenDrehtDasSchwert()
    {
        var input = new TouchInputMapper(Layout, twoPlayers: true);
        float y = Layout.Height * 0.8f;
        input.TouchDown(1, 100, y, 0);
        input.TouchMove(1, 100 + Layout.Width / 4f, y);
        input.TouchUp(1, 100 + Layout.Width / 4f, y, 0.5);

        var rotations = input.Drain().Where(c => c.Type == CommandType.RotateSword).ToList();
        Assert.All(rotations, c => Assert.Equal(PlayerId.One, c.Player));
        Assert.Equal(45f, rotations.Sum(c => c.DeltaDegrees), 1);   // Viertel der Breite = 45°
    }

    [Fact, Trait("Regel", "SP-2")]
    public void SP2_Spieler2SitztGegenueber_WischenNachLinksIstSeineRechte()
    {
        var input = new TouchInputMapper(Layout, twoPlayers: true);
        float y = Layout.Height * 0.2f;
        input.TouchDown(7, 200, y, 0);
        input.TouchMove(7, 140, y);
        var rotation = input.Drain().Where(c => c.Type == CommandType.RotateSword).ToList();
        Assert.All(rotation, c => Assert.Equal(PlayerId.Two, c.Player));
        Assert.True(rotation.Sum(c => c.DeltaDegrees) > 0);
    }

    [Fact, Trait("Regel", "SP-3")]
    public void SP3_KurzesTippenFeuert_InDerEigenenHaelfte()
    {
        var input = new TouchInputMapper(Layout, twoPlayers: true);
        input.TouchDown(1, 100, Layout.Height * 0.9f, 0);
        input.TouchUp(1, 101, Layout.Height * 0.9f, 0.1);
        input.TouchDown(2, 100, Layout.Height * 0.3f, 0.2);
        input.TouchUp(2, 100, Layout.Height * 0.3f, 0.3);

        var fires = input.Drain().Where(c => c.Type == CommandType.Fire).Select(c => c.Player).ToList();
        Assert.Equal([PlayerId.One, PlayerId.Two], fires);
    }

    [Fact, Trait("Regel", "SP-3")]
    public void SP3_WischenOderLangesDrueckenFeuertNicht()
    {
        var input = new TouchInputMapper(Layout, twoPlayers: true);
        float y = Layout.Height * 0.9f;
        input.TouchDown(1, 100, y, 0);
        input.TouchUp(1, 160, y, 0.1);       // gewischt
        input.TouchDown(2, 100, y, 1);
        input.TouchUp(2, 100, y, 2);         // zu lange
        Assert.DoesNotContain(input.Drain(), c => c.Type == CommandType.Fire);
    }

    [Fact, Trait("Regel", "SO-1")]
    public void SO1_TippenAufPauseLiefertPausenbefehl()
    {
        var input = new TouchInputMapper(Layout, twoPlayers: true);
        var p = Layout.PauseButton;
        input.TouchDown(1, p.X + p.Width / 2f, p.Y + p.Height / 2f, 0);
        input.TouchUp(1, p.X + p.Width / 2f, p.Y + p.Height / 2f, 0.1);
        Assert.Equal([new InputCommand(CommandType.Pause)], input.Drain());
    }

    [Fact]
    public void A6_EinSpieler_GanzerBildschirmGehoertSpieler1()
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, 0.53f, twoPlayers: false);
        var input = new TouchInputMapper(layout, twoPlayers: false);
        input.TouchDown(1, 100, layout.Height * 0.3f, 0);
        input.TouchUp(1, 100, layout.Height * 0.3f, 0.1);
        Assert.Equal(PlayerId.One, input.Drain().Single().Player);
    }
}

public class SessionTests
{
    [Fact, Trait("Regel", "SO-1")]
    public void SO1_HintergrundPausiert_PauseTasteSchaltetUm()
    {
        var session = new GameSession(new UserSettings(), twoPlayers: false, seed: 1);
        session.OnAppBackgrounded();
        Assert.True(session.Simulation.Paused);
        session.Apply([new InputCommand(CommandType.Pause)]);
        Assert.False(session.Simulation.Paused);
    }

    [Fact, Trait("Regel", "KI-3")]
    public void EinSpieler_BefehleFuerSpieler2WerdenIgnoriert_DieKiSpielt()
    {
        var session = new GameSession(new UserSettings(), twoPlayers: false, seed: 1);
        while (session.Simulation.Phase != MatchPhase.Playing) session.Update(1f / 60);
        int before = session.Simulation.OwnedBoltCount(PlayerId.Two);
        session.Apply([new InputCommand(CommandType.Fire, PlayerId.Two)]);
        Assert.Equal(before, session.Simulation.OwnedBoltCount(PlayerId.Two));
    }

    [Fact, Trait("Regel", "SP-2")]
    public void Drehbefehl_AddiertSichZumZielwinkel()
    {
        var session = new GameSession(new UserSettings { SwordTurning = SwordTurning.Instant }, twoPlayers: true, seed: 1);
        session.Apply([new InputCommand(CommandType.RotateSword, PlayerId.One, 20), new InputCommand(CommandType.RotateSword, PlayerId.One, 15)]);
        Assert.Equal(35f, session.Simulation.PlayerFor(PlayerId.One).TargetAngle);
    }
}

public class RenderingTests
{
    [Fact, Trait("Regel", "F4")]
    public void ModernePalette_256FarbenAus4096()
    {
        Assert.Equal(4096, ModernPalette.MasterColorCount);
        Assert.Equal(256, ModernPalette.Colors.Length);
        Assert.Equal(256, ModernPalette.Colors.Distinct().Count());
        Assert.All(ModernPalette.Colors, c =>
        {
            foreach (int shift in new[] { 16, 8, 0 })
                Assert.Equal(0, ((c >> shift) & 0xFF) % 17);   // 4 Bit je Kanal
        });
    }

    private static GameSession PlayedSession(bool twoPlayers)
    {
        var session = new GameSession(new UserSettings(), twoPlayers, seed: 3);
        for (int i = 0; i < 60 * 8; i++) { session.Apply([new InputCommand(CommandType.Fire, PlayerId.One)]); session.Update(1f / 60); }
        return session;
    }

    [Theory, Trait("Regel", "F4")]
    [InlineData(false, 1080, 2340)]
    [InlineData(true, 2340, 1080)]
    public void ModernerStil_HoechstensPalettenfarben_Maximal256(bool twoPlayers, int w, int h)
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, w, h, 0.53f, twoPlayers);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        new ModernRenderer().RenderGame(canvas, layout, PlayedSession(twoPlayers), new Localizer("de"), 5);

        Assert.True(canvas.CountColors() <= ModernPalette.MaxColors);
        for (int y = 0; y < canvas.Height; y += 7)
            for (int x = 0; x < canvas.Width; x += 5)
                Assert.True(ModernPalette.IsPaletteColor(canvas[x, y]));
    }

    [Fact, Trait("Regel", "F4")]
    public void ModernerStil_TitelUndOptionenNurPalettenfarben()
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 1080, 2340, 0.53f, false);
        var renderer = new ModernRenderer();
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderTitle(canvas, layout, new UserSettings(), new Localizer("en"), 0);
        Assert.True(canvas.CountColors() <= ModernPalette.MaxColors);
        renderer.RenderOptions(canvas, layout, new UserSettings(), new Localizer("en"), 0);
        Assert.True(canvas.CountColors() <= ModernPalette.MaxColors);
    }

    [Fact]
    public void KlassischerStil_WenigeFlacheFarben()
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Classic, 1080, 2340, 0.53f, false);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        new ClassicRenderer().RenderGame(canvas, layout, PlayedSession(false), new Localizer("de"), 5);
        Assert.InRange(canvas.CountColors(), 4, 16);
    }

    [Theory, Trait("Regel", "SO-2")]
    [InlineData(GraphicsStyle.Classic)]
    [InlineData(GraphicsStyle.Modern)]
    public void SO2_DarstellungVeraendertDenSpielzustandNicht(GraphicsStyle style)
    {
        var session = PlayedSession(twoPlayers: false);
        string before = Snapshot(session);
        StyleRenderer renderer = style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer();
        var layout = ScreenLayout.Create(style, 1080, 2340, 0.53f, false);
        renderer.RenderGame(new PixelCanvas(layout.Width, layout.Height), layout, session, new Localizer("de"), 1);
        Assert.Equal(before, Snapshot(session));

        static string Snapshot(GameSession s) =>
            $"{s.Simulation.Drone.X}|{s.Simulation.Bolts.Count}|{string.Join(",", s.Simulation.Bolts.Select(b => b.Length))}|{s.Simulation.RoundTime}";
    }

    [Fact]
    public void Png_HatGueltigeSignaturUndGroesse()
    {
        var canvas = new PixelCanvas(3, 2);
        using var stream = new MemoryStream();
        PngWriter.Write(canvas, stream, scale: 2);
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes[..4]);
        Assert.Equal(6, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16)));
        Assert.Equal(4, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20)));
    }
}

public class PauseMenuTests
{
    [Theory, Trait("Regel", "SO-1")]
    [InlineData(LazerArena.Presentation.Settings.GraphicsStyle.Classic, 1080, 2340)]
    [InlineData(LazerArena.Presentation.Settings.GraphicsStyle.Modern, 1220, 2712)]
    [InlineData(LazerArena.Presentation.Settings.GraphicsStyle.Modern, 2712, 1220)]
    public void SO1_PausenmenueHatZweiKnoepfeImSpielfeld(LazerArena.Presentation.Settings.GraphicsStyle style, int w, int h)
    {
        var layout = LazerArena.Presentation.Layout.ScreenLayout.Create(style, w, h, 0.53f, false);
        LazerArena.Presentation.Rendering.StyleRenderer renderer = style == LazerArena.Presentation.Settings.GraphicsStyle.Classic
            ? new LazerArena.Presentation.Rendering.ClassicRenderer() : new LazerArena.Presentation.Rendering.ModernRenderer();
        var rects = renderer.PauseMenuRects(layout);
        Assert.Equal(2, rects.Count);
        Assert.All(rects, r => Assert.True(r.X >= 0 && r.Right <= layout.Width && r.Bottom <= layout.Field.Bottom));
        Assert.True(rects[0].Bottom <= rects[1].Y);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("en")]
    public void Gewitterschriftzug_PasstInsSpielfeld(string language)
    {
        var t = new LazerArena.Presentation.Localization.Localizer(language);
        var modern = LazerArena.Presentation.Layout.ScreenLayout.Create(LazerArena.Presentation.Settings.GraphicsStyle.Modern, 1220, 2712, 0.53f, true);
        var classic = LazerArena.Presentation.Layout.ScreenLayout.Create(LazerArena.Presentation.Settings.GraphicsStyle.Classic, 1220, 2712, 0.53f, true);
        Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["hud.storm"], 2) <= modern.Field.Width);
        Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["hud.stormShort"]) <= classic.Field.Width);
        foreach (var landscape in new[] { modernLandscape(), classicLandscape() })   // kleinste Schrift muss immer passen
            Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["hud.stormShort"]) <= landscape.Field.Width);
        var wideLandscape = modernLandscape();
        Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["hud.storm"]) <= wideLandscape.Field.Width);

        static LazerArena.Presentation.Layout.ScreenLayout modernLandscape() =>
            LazerArena.Presentation.Layout.ScreenLayout.Create(LazerArena.Presentation.Settings.GraphicsStyle.Modern, 2712, 1220, 0.53f, true);
        static LazerArena.Presentation.Layout.ScreenLayout classicLandscape() =>
            LazerArena.Presentation.Layout.ScreenLayout.Create(LazerArena.Presentation.Settings.GraphicsStyle.Classic, 2712, 1220, 0.53f, true);
    }

    [Theory, Trait("Regel", "SO-1")]
    [InlineData("de", LazerArena.Presentation.Settings.GraphicsStyle.Classic, 2712, 1220)]
    [InlineData("en", LazerArena.Presentation.Settings.GraphicsStyle.Classic, 2712, 1220)]
    [InlineData("de", LazerArena.Presentation.Settings.GraphicsStyle.Modern, 2712, 1220)]
    [InlineData("en", LazerArena.Presentation.Settings.GraphicsStyle.Modern, 1220, 2712)]
    [InlineData("de", LazerArena.Presentation.Settings.GraphicsStyle.Classic, 1220, 2712)]
    public void SO1_PausenknoepfeSindBreitGenugFuerIhreBeschriftung(string language, LazerArena.Presentation.Settings.GraphicsStyle style, int w, int h)
    {
        var t = new LazerArena.Presentation.Localization.Localizer(language);
        var layout = LazerArena.Presentation.Layout.ScreenLayout.Create(style, w, h, 0.53f, false);
        LazerArena.Presentation.Rendering.StyleRenderer renderer = style == LazerArena.Presentation.Settings.GraphicsStyle.Classic
            ? new LazerArena.Presentation.Rendering.ClassicRenderer() : new LazerArena.Presentation.Rendering.ModernRenderer();
        var rects = renderer.PauseMenuRects(layout);
        Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["pause.resume"]) <= rects[0].Width - 4);
        Assert.True(LazerArena.Presentation.Rendering.BitmapFont.MeasureWidth(t["pause.menu"]) <= rects[1].Width - 4);
        Assert.All(rects, r => Assert.True(r.X >= 0 && r.Right <= layout.Width));
    }
}

public class VictoryTests
{
    private static LazerArena.Presentation.GameSession FinishedMatch(bool twoPlayers, bool p1Wins)
    {
        var session = new LazerArena.Presentation.GameSession(new UserSettings { AiLevel = p1Wins ? AiLevel.Easy : AiLevel.Hard }, twoPlayers, seed: 5);
        var p1 = new LazerArena.Core.Ai.AiController(PlayerId.One, p1Wins ? AiLevel.Hard : AiLevel.Easy, new SeededRandom(7));
        var p2 = twoPlayers ? new LazerArena.Core.Ai.AiController(PlayerId.Two, p1Wins ? AiLevel.Easy : AiLevel.Hard, new SeededRandom(8)) : null;
        for (int i = 0; i < 60 * 900 && session.Simulation.Phase != MatchPhase.MatchOver; i++)
        {
            p1.Update(session.Simulation, 1f / 60);
            p2?.Update(session.Simulation, 1f / 60);
            session.Update(1f / 60);
        }
        Assert.Equal(MatchPhase.MatchOver, session.Simulation.Phase);
        return session;
    }

    [Fact, Trait("Regel", "RS-5")]
    public void RS5_SiegGegenDieKi_ZeigtSieger3PunkteUndPokal()
    {
        var info = VictoryInfo.From(FinishedMatch(twoPlayers: false, p1Wins: true), new Localizer("en"), showContinue: true);
        Assert.Equal("P1", info.WinnerLabel);
        Assert.True(info.HumanWon);
        Assert.Equal(3, info.WinnerScore);
        Assert.InRange(info.LoserScore, 0, 2);
    }

    [Fact, Trait("Regel", "RS-5")]
    public void RS5_NiederlageGegenDieKi_IstKeinSieg()
    {
        var info = VictoryInfo.From(FinishedMatch(twoPlayers: false, p1Wins: false), new Localizer("de"), showContinue: false);
        Assert.Equal("KI", info.WinnerLabel);
        Assert.False(info.HumanWon);
    }

    [Fact, Trait("Regel", "RS-5")]
    public void RS5_ZweiSpieler_SiegerIstImmerEinMensch()
    {
        var info = VictoryInfo.From(FinishedMatch(twoPlayers: true, p1Wins: false), new Localizer("en"), showContinue: true);
        Assert.Equal("P2", info.WinnerLabel);
        Assert.True(info.HumanWon);
    }

    [Theory, Trait("Regel", "F4")]
    [InlineData(1220, 2712)]
    [InlineData(2712, 1220)]
    public void Siegerbildschirm_ModernNurPalettenfarben(int w, int h)
    {
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, w, h, 0.53f, false);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        new ModernRenderer().RenderVictory(canvas, layout, new VictoryInfo("P1", true, 3, 1, true), new Localizer("de"), 10);
        Assert.True(canvas.CountColors() <= ModernPalette.MaxColors);
        Assert.True(ModernPalette.IsPaletteColor(canvas[canvas.Width / 2, canvas.Height / 2]));
    }
}

public class ArenaTests
{
    public static TheoryData<ArenaKind> AllArenas() => new(Enum.GetValues<ArenaKind>());

    private static GameSession Countdown() => new(new UserSettings(), twoPlayers: false, seed: 2);   // noch keine Blitze

    [Fact, Trait("Regel", "OP-11")]
    public void OP11_SiebenArenen_NurImModernenStil_StandardOriginal()
    {
        Assert.Equal(7, Enum.GetValues<ArenaKind>().Length);
        Assert.Equal(ArenaKind.Original, new UserSettings().Arena);
        Assert.Contains(OptionsMenu.Entries(new UserSettings { Style = GraphicsStyle.Modern }), e => e.LabelKey == "opt.arena");
        Assert.DoesNotContain(OptionsMenu.Entries(new UserSettings { Style = GraphicsStyle.Classic }), e => e.LabelKey == "opt.arena");
        var entry = OptionsMenu.Entries(new UserSettings()).Single(e => e.LabelKey == "opt.arena");
        var s = new UserSettings();
        for (int i = 0; i < 7; i++) s = entry.Step(s, 1);
        Assert.Equal(ArenaKind.Original, s.Arena);
    }

    [Fact, Trait("Regel", "OP-11")]
    public void OP11_AnimierteArenenSindImMenueMarkiert()
    {
        foreach (var lang in new[] { "de", "en" })
        {
            var t = new Localizer(lang);
            foreach (var arena in Enum.GetValues<ArenaKind>())
                Assert.Equal(ArenaInfo.IsAnimated(arena), t["arena." + arena].EndsWith('*'));
        }
    }

    [Theory, Trait("Regel", "F4"), Trait("Regel", "OP-11")]
    [MemberData(nameof(AllArenas))]
    public void JedeArena_HoechstensPalettenfarben256Aus4096_HochUndQuer(ArenaKind arena)
    {
        var renderer = new ModernRenderer(arena);
        var session = new GameSession(new UserSettings { Arena = arena }, twoPlayers: false, seed: 3);
        for (int i = 0; i < 60 * 6; i++) { session.Apply([new InputCommand(CommandType.Fire, PlayerId.One)]); session.Update(1f / 60); }
        foreach (var (w, h) in new[] { (1220, 2712), (2712, 1220) })
        {
            var layout = ScreenLayout.Create(GraphicsStyle.Modern, w, h, 0.53f, false);
            var canvas = new PixelCanvas(layout.Width, layout.Height);
            renderer.RenderGame(canvas, layout, session, new Localizer("de"), 17);
            Assert.True(canvas.CountColors() <= 256);
            for (int y = 0; y < canvas.Height; y += 5)
                for (int x = 0; x < canvas.Width; x += 3)
                {
                    var c = canvas[x, y];
                    Assert.True(renderer.IsArenaColor(c), $"{arena}: Farbe {c} bei {x},{y}");
                    Assert.True((int)c.R % 17 == 0 && (int)c.G % 17 == 0 && (int)c.B % 17 == 0, "keine 12-Bit-Farbe");
                }
        }
    }

    [Theory, Trait("Regel", "OP-11")]
    [MemberData(nameof(AllArenas))]
    public void NurAnimierteArenenHabenBewegtenBoden(ArenaKind arena)
    {
        var renderer = new ModernRenderer(arena);
        var session = Countdown();
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 1220, 2712, 0.53f, false);
        var a = new PixelCanvas(layout.Width, layout.Height);
        var b = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderGame(a, layout, session, new Localizer("de"), 0);
        renderer.RenderGame(b, layout, session, new Localizer("de"), 40);
        int changed = 0;
        var f = layout.Field;
        // Die Drohne dreht ihren Ring in jeder Arena; hier zaehlt nur der Boden
        float cx = f.X + f.Width / 2f, cy = f.Y + f.Height / 2f, droneArea = f.Width * 0.045f * 2.6f + 4;
        for (int y = f.Y; y < f.Bottom; y++)
            for (int x = f.X; x < f.Right; x++)
                if (MathF.Abs(x - cx) > droneArea || MathF.Abs(y - cy) > droneArea)
                    if (a[x, y] != b[x, y]) changed++;
        if (ArenaInfo.IsAnimated(arena)) Assert.True(changed > 20, $"{arena}: nur {changed} Pixel bewegt");
        else Assert.Equal(0, changed);
    }

    [Fact]
    public void SnesArenen_ohneDithering_FlaechenBleibenEinfarbig()
    {
        // Eine gleichfarbige Flaeche darf nach dem Abbilden nicht in ein Rastermuster zerfallen.
        var renderer = new ModernRenderer(ArenaKind.ArcadeChrome);
        var layout = ScreenLayout.Create(GraphicsStyle.Modern, 1220, 2712, 0.53f, false);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderGame(canvas, layout, Countdown(), new Localizer("de"), 0);
        var f = layout.Field;
        var tile = Enumerable.Range(2, 10).SelectMany(dy => Enumerable.Range(2, 10).Select(dx => canvas[f.X + dx, f.Y + dy])).Distinct().Count();
        Assert.Equal(1, tile);
    }
}

public class TvEffectTests
{
    [Fact, Trait("Regel", "OP-12")]
    public void OP12_TvEffektIstInBeidenStilenWaehlbar_StandardAus()
    {
        Assert.False(new UserSettings().TvEffect);
        foreach (var style in new[] { GraphicsStyle.Classic, GraphicsStyle.Modern })
        {
            var entry = OptionsMenu.Entries(new UserSettings { Style = style }).Single(e => e.LabelKey == "opt.tvEffect");
            Assert.True(entry.Step(new UserSettings(), 1).TvEffect);
        }
    }

    [Theory, Trait("Regel", "OP-12")]
    [InlineData(260)]
    [InlineData(600)]
    public void OP12_EineDunkleBildzeileJeLogischerPixelzeile(int rows)
    {
        var data = TvEffect.ScanlineTexture(rows);
        Assert.Equal(rows * TvEffect.RowParts, data.Length);
        for (int row = 0; row < rows; row++)
        {
            Assert.Equal(0f, TvEffect.AlphaOf(data[row * TvEffect.RowParts]));
            Assert.Equal(TvEffect.ScanlineDarkness, TvEffect.AlphaOf(data[row * TvEffect.RowParts + TvEffect.RowParts - 1]), 2);
        }
    }

    [Fact, Trait("Regel", "OP-12")]
    public void OP12_VignetteMitteUnveraendert_EckenDunkler()
    {
        Assert.Equal(0f, TvEffect.VignetteAlpha(0, 0));
        Assert.True(TvEffect.VignetteAlpha(0.95f, 0.95f) > 0.2f);
        Assert.True(TvEffect.VignetteAlpha(0.95f, 0.95f) <= TvEffect.VignetteDarkness);
        var texture = TvEffect.VignetteTexture(64);
        Assert.True(TvEffect.AlphaOf(texture[0]) > TvEffect.AlphaOf(texture[32 * 64 + 32]));
    }

    [Fact]
    public void Vormultiplizierte_Transparenz_Schwarz()
    {
        uint c = TvEffect.Premultiplied(0, 0, 0, 0.5f);
        Assert.Equal(0u, c & 0x00FFFFFF);
        Assert.Equal(0.5f, TvEffect.AlphaOf(c), 2);
    }
}

public class BoltShapeTests
{
    [Fact, Trait("Regel", "BL-8")]
    public void BL8_UnregelmaessigerBlitz_EndpunkteStimmen_FormWechseltMitStartwert()
    {
        var a = new System.Numerics.Vector2(10, 300);
        var b = new System.Numerics.Vector2(120, 40);
        var one = Zigzag.Irregular(a, b, 8, 6, seed: 1);
        var two = Zigzag.Irregular(a, b, 8, 6, seed: 2);
        Assert.Equal(a, one[0]);
        Assert.Equal(b, one[^1]);
        Assert.NotEqual(one, two);                                  // zuckt von Bild zu Bild
        Assert.Equal(one, Zigzag.Irregular(a, b, 8, 6, seed: 1));   // aber deterministisch je Startwert
    }

    [Fact, Trait("Regel", "BL-8")]
    public void BL8_AbschnitteUndAusschlaegeSindUngleichmaessig()
    {
        var a = new System.Numerics.Vector2(0, 0);
        var b = new System.Numerics.Vector2(0, 400);
        var pts = Zigzag.Irregular(a, b, 8, 6, seed: 7);
        var steps = pts.Zip(pts.Skip(1), (p, q) => MathF.Round(q.Y - p.Y)).Distinct().Count();
        var offsets = pts.Select(p => MathF.Round(MathF.Abs(p.X))).Distinct().Count();
        Assert.True(steps > 5, "Abschnittslaengen zu gleichmaessig");
        Assert.True(offsets > 5, "Ausschlaege zu gleichmaessig");
        Assert.All(pts, p => Assert.True(MathF.Abs(p.X) <= 6 * 1.4f * 2 + 0.01f));
    }

    [Fact, Trait("Regel", "BL-8")]
    public void BL8_SeitenaesteZweigenVomBlitzAb()
    {
        var bolt = Zigzag.Irregular(new(0, 0), new(0, 300), 8, 6, seed: 3);
        var branches = Zigzag.Branches(bolt, 3, 14, seed: 3);
        Assert.Equal(3, branches.Count);
        Assert.All(branches, br => Assert.Contains(br[0], bolt));
    }
}
