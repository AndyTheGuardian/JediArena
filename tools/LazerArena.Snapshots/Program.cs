using LazerArena.Core;
using LazerArena.Core.Ai;
using LazerArena.Presentation;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;

// Rendert repraesentative Bildschirme beider Stile als PNG (Vergleich mit den abgenommenen Mockups).
// Aufruf: dotnet run --project tools/LazerArena.Snapshots -- <ausgabeordner> [de|en]
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
    renderer.RenderGame(canvas, layout, session, t, (int)(time * 60));
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
