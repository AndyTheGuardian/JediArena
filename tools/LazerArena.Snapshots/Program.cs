using LazerArena.Core;
using LazerArena.Core.Ai;
using LazerArena.Presentation;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

// Rendert repraesentative Bildschirme beider Stile als PNG (Vergleich mit den abgenommenen Mockups).
// Aufruf: dotnet run --project tools/LazerArena.Snapshots -- <ausgabeordner> [de|en] [--arenen] [--anleitung [--ohne-bildfolge]]
//         [--frames <arena> <anzahl> [jedes-n-te]] [--audio]
string outDir = args.Length > 0 ? args[0] : "snapshots";
if (args.Contains("--audio"))
{
    Directory.CreateDirectory(outDir);
    foreach (var (name, bank) in new (string, LazerArena.Presentation.Audio.ISoundBank)[]
             { ("klassisch", new LazerArena.Presentation.Audio.ClassicSoundBank()), ("modern", new LazerArena.Presentation.Audio.ModernSoundBank()) })
    {
        foreach (var id in Enum.GetValues<LazerArena.Presentation.Audio.SoundId>())
            LazerArena.Presentation.Audio.WavWriter.Write(bank.Get(id), Path.Combine(outDir, $"{name}-{id}.wav"));
        foreach (var song in Enum.GetValues<LazerArena.Presentation.Audio.SongId>())
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (bank.Music(song) is not { } music) continue;
            Console.WriteLine($"Musik {song} berechnet in {sw.ElapsedMilliseconds} ms, {music.DurationSeconds:F1} s");
            LazerArena.Presentation.Audio.WavWriter.Write(music, Path.Combine(outDir, $"{name}-Musik-{song}.wav"));
        }
    }
    Console.WriteLine($"Klaenge in {outDir}");
    return;
}
string language = args.Length > 1 ? args[1] : LanguageResolver.German;
Directory.CreateDirectory(outDir);
var t = new Localizer(language);
const int phoneW = 1080, phoneH = 2340;
if (args.Contains("--frames"))   // Bildfolge fuer eine Animationsvorschau: --frames <arena> <anzahl>
{
    int at = Array.IndexOf(args, "--frames");
    var arena = Enum.Parse<ArenaKind>(args[at + 1]);
    int count = int.Parse(args[at + 2]);
    int every = args.Length > at + 3 && int.TryParse(args[at + 3], out int e) ? e : 1;   // nur jedes n-te Bild speichern
    var renderer = new ModernRenderer(arena);
    var settings = new UserSettings { Style = GraphicsStyle.Modern, Arena = arena };
    var session = new GameSession(settings, true, seed: 4, GameConfig.Default with { StormIntervalMin = 6, StormIntervalMax = 6 });
    var a1 = new AiController(PlayerId.One, AiLevel.Medium, new SeededRandom(99));
    var a2 = new AiController(PlayerId.Two, AiLevel.Medium, new SeededRandom(98));
    int frame = 0;
    void Step() { a1.Update(session.Simulation, 1f / 60); a2.Update(session.Simulation, 1f / 60); session.Update(1f / 60); frame++; }
    while (session.Simulation.Storm.Phase != StormPhase.Warning && frame < 3600) Step();
    // ab der Gewitterwarnung: Leuchten erst aus, dann an
    var layout = ScreenLayout.Create(GraphicsStyle.Modern, phoneW, phoneH, GameConfig.Default.FieldAspect, true);
    for (int i = 0; i < count; i++)
    {
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderGame(canvas, layout, session, t, frame, settings);   // jedes Bild: die Leuchten laufen hoch/aus
        if (i % every == 0) PngWriter.Write(canvas, Path.Combine(outDir, $"frame-{i / every:D4}.png"), scale: 1);
        Step();
    }
    Console.WriteLine($"{count} Bilder in {outDir}");
    return;
}

foreach (StyleRenderer renderer in new StyleRenderer[] { new ClassicRenderer(), new ModernRenderer() })
{
    var settings = new UserSettings { Style = renderer.Style };
    string prefix = renderer.Style == GraphicsStyle.Classic ? "klassisch" : "modern";

    foreach (bool landscape in new[] { false, true })
    {
        var layout = Layout(renderer.Style, landscape, twoPlayers: false);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderTitle(canvas, layout, settings, t, 0);
        Save(canvas, layout, $"{prefix}-titel-{(landscape ? "quer" : "hoch")}");
    }

    {
        var layout = Layout(renderer.Style, false, false);
        var canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderOptions(canvas, layout, settings with { ShieldShape = ShieldShape.Curved, Arena = ArenaKind.SpaceFlight }, t, 1);
        Save(canvas, layout, $"{prefix}-optionen-hoch");
        canvas = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderOptions(canvas, layout, settings, t, 2, advanced: true, resetArmed: true);
        Save(canvas, layout, $"{prefix}-erweitert-hoch");
    }

    Game(renderer, settings, twoPlayers: false, landscape: false, seconds: 7.3f, name: $"{prefix}-1p-hoch");
    Game(renderer, settings with { ShieldShape = ShieldShape.Curved }, false, false, 7.3f, $"{prefix}-1p-hoch-gebogen");
    Game(renderer, settings, false, true, 9.1f, $"{prefix}-1p-quer");
    Game(renderer, settings, true, false, 0, $"{prefix}-2p-hoch", stormFrame: true);
    Game(renderer, settings, true, true, 11.4f, $"{prefix}-2p-quer");
    Game(renderer, settings, true, true, 0, $"{prefix}-2p-quer-gewitter", stormFrame: true);
    Game(renderer, settings, false, true, 6f, $"{prefix}-1p-quer-pause", paused: true);
    Victory(renderer, settings, humanWins: true, landscape: false, $"{prefix}-sieg-hoch");
    Victory(renderer, settings, humanWins: true, landscape: true, $"{prefix}-sieg-quer");
    Victory(renderer, settings, humanWins: false, landscape: false, $"{prefix}-niederlage-hoch");
}
if (args.Contains("--arenen"))
{
    foreach (var arena in Enum.GetValues<ArenaKind>())
    {
        var renderer = new ModernRenderer(arena);
        var settings = new UserSettings { Style = GraphicsStyle.Modern, Arena = arena };
        Game(renderer, settings, false, false, 7.3f, $"arena-{arena}-1p-hoch");
        Game(renderer, settings, false, true, 9.1f, $"arena-{arena}-1p-quer");
        Game(renderer, settings, true, false, 0, $"arena-{arena}-2p-hoch-gewitter", stormFrame: true);
        if (arena == ArenaKind.SpaceStation) Game(renderer, settings, false, false, 6f, $"arena-{arena}-1p-hoch-pause", paused: true);
    }
}
if (args.Contains("--anleitung"))   // Anleitung: Bildfolge je Stil und Seite fuer GIF-Vorschauen
{
    foreach (StyleRenderer renderer in new StyleRenderer[] { new ClassicRenderer(), new ModernRenderer() })
    {
        string prefix = renderer.Style == GraphicsStyle.Classic ? "klassisch" : "modern";
        var layout = Layout(renderer.Style, false, false);
        Directory.CreateDirectory(Path.Combine(outDir, "anleitung"));
        for (int page = 0; page < StyleRenderer.TutorialPages && !args.Contains("--ohne-bildfolge"); page++)
            for (int f = 0; f < StyleRenderer.TutorialLoopFrames; f += 4)
            {
                var canvas = new PixelCanvas(layout.Width, layout.Height);
                renderer.RenderTutorial(canvas, layout, page, t, f);
                PngWriter.Write(canvas, Path.Combine(outDir, "anleitung", $"{prefix}-{page}-{f:D3}.png"), scale: renderer.Style == GraphicsStyle.Classic ? 2 : 1);
            }
        var still = new PixelCanvas(layout.Width, layout.Height);
        renderer.RenderTutorial(still, layout, 1, t, 100);
        Save(still, layout, $"anleitung-{prefix}-hoch");
        var land = Layout(renderer.Style, true, false);
        var c2 = new PixelCanvas(land.Width, land.Height);
        renderer.RenderTutorial(c2, land, 2, t, 120);
        Save(c2, land, $"anleitung-{prefix}-quer");
    }
}
if (args.Contains("--aussparung"))   // SO-7: Bildschirme mit simulierter mittiger Kamera-Aussparung auf mehreren Bildschirmgroessen
{
    var phones = new (string Name, int W, int H, int Hole)[]
    {
        ("1080x2340", 1080, 2340, 96), ("1220x2712", 1220, 2712, 110), ("1440x3200", 1440, 3200, 130),
        ("720x1600", 720, 1600, 70), ("1080x1920", 1080, 1920, 0),
    };
    foreach (StyleRenderer renderer in new StyleRenderer[] { new ClassicRenderer(), new ModernRenderer() })
    {
        string prefix = renderer.Style == GraphicsStyle.Classic ? "klassisch" : "modern";
        var settings = new UserSettings { Style = renderer.Style };
        foreach (var (name, pw, ph, hole) in phones)
        {
            var layout = ScreenLayout.Create(renderer.Style, pw, ph, GameConfig.Default.FieldAspect, false);
            layout = layout with { SafeTop = ScreenLayout.ToLogical(hole, ph, layout.Height) };
            void Hole(PixelCanvas c)   // Kamera-Loch als schwarzer Kreis
            {
                if (hole == 0) return;
                float r = layout.SafeTop * 0.42f;
                c.FillEllipse(layout.Width / 2f, layout.SafeTop / 2f, r, r, Rgb.FromBytes(0, 0, 0));
            }
            var shots = new (string Shot, Action<PixelCanvas> Draw)[]
            {
                ("titel", c => renderer.RenderTitle(c, layout, settings, t, 0)),
                ("optionen", c => renderer.RenderOptions(c, layout, settings, t, 1)),
                ("erweitert", c => renderer.RenderOptions(c, layout, settings, t, 1, advanced: true)),
                ("anleitung", c => renderer.RenderTutorial(c, layout, 1, t, 100)),
                ("spiel", c =>
                {
                    var session = new GameSession(settings, false, seed: 4);
                    for (int i = 0; i < 300; i++) session.Update(1f / 60);
                    renderer.RenderGame(c, layout, session, t, 300, settings);
                }),
            };
            foreach (var (shot, draw) in shots)
            {
                var canvas = new PixelCanvas(layout.Width, layout.Height);
                draw(canvas);
                Hole(canvas);
                PngWriter.Write(canvas, Path.Combine(outDir, $"loch-{prefix}-{name}-{shot}.png"), scale: renderer.Style == GraphicsStyle.Classic ? 2 : 1);
            }
        }
    }
}
Console.WriteLine($"Fertig: {outDir}");

ScreenLayout Layout(GraphicsStyle style, bool landscape, bool twoPlayers) =>
    ScreenLayout.Create(style, landscape ? phoneH : phoneW, landscape ? phoneW : phoneH, GameConfig.Default.FieldAspect, twoPlayers);

void Game(StyleRenderer renderer, UserSettings settings, bool twoPlayers, bool landscape, float seconds, string name, bool stormFrame = false, bool paused = false)
{
    var config = stormFrame ? GameConfig.Default with { StormIntervalMin = 6, StormIntervalMax = 6 } : GameConfig.Default;
    var session = new GameSession(settings, twoPlayers, seed: 4, config);
    var p1 = new AiController(PlayerId.One, AiLevel.Medium, new SeededRandom(99));
    var p2 = twoPlayers ? new AiController(PlayerId.Two, AiLevel.Medium, new SeededRandom(98)) : null;
    float dt = 1f / 60f, time = 0;
    while (true)
    {
        p1.Update(session.Simulation, dt);
        p2?.Update(session.Simulation, dt);
        session.Update(dt);
        time += dt;
        if (stormFrame ? session.Simulation.Storm.Phase == StormPhase.Active && session.Simulation.Bolts.Count >= 5 : time >= seconds) break;
        if (time > 60) break;
    }
    if (paused) session.Simulation.Pause();
    var layout = ScreenLayout.Create(renderer.Style, landscape ? phoneH : phoneW, landscape ? phoneW : phoneH,
        session.Simulation.Config.FieldAspect, twoPlayers);
    var canvas = new PixelCanvas(layout.Width, layout.Height);
    renderer.RenderGame(canvas, layout, session, t, (int)(time * 60), settings);
    Save(canvas, layout, name);
}

void Victory(StyleRenderer renderer, UserSettings settings, bool humanWins, bool landscape, string name)
{
    var session = new GameSession(settings with { AiLevel = humanWins ? AiLevel.Easy : AiLevel.Hard }, false, seed: 5);
    var p1 = new AiController(PlayerId.One, humanWins ? AiLevel.Hard : AiLevel.Easy, new SeededRandom(7));
    for (int i = 0; i < 60 * 900 && session.Simulation.Phase != MatchPhase.MatchOver; i++)
    {
        p1.Update(session.Simulation, 1f / 60);
        session.Update(1f / 60);
    }
    var layout = Layout(renderer.Style, landscape, false);
    var canvas = new PixelCanvas(layout.Width, layout.Height);
    renderer.RenderVictory(canvas, layout, VictoryInfo.From(session, t, showContinue: true), t, 40);
    Save(canvas, layout, name);
}

void Save(PixelCanvas canvas, ScreenLayout layout, string name)
{
    string path = Path.Combine(outDir, name + ".png");
    PngWriter.Write(canvas, path, scale: 2);
    Console.WriteLine($"{name,-28} {canvas.Width}x{canvas.Height}  {canvas.CountColors(),4} Farben");
}
