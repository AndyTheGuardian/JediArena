using LazerArena.Core;
using LazerArena.Presentation;
using LazerArena.Presentation.Audio;
using LazerArena.Presentation.Input;
using LazerArena.Presentation.Layout;
using LazerArena.Presentation.Localization;
using LazerArena.Presentation.Rendering;
using LazerArena.Presentation.Settings;
using static LazerArena.Game.MusicChoice;

namespace LazerArena.Game;

/// <summary>Gemeinsamer Zustand, den alle Bildschirme brauchen.</summary>
public sealed class AppContext(IPlatformServices platform, ISettingsStore store)
{
    private SettingsProfiles _profiles = store.Load();
    public IPlatformServices Platform { get; } = platform;
    public UserSettings Settings => _profiles.Active;
    public Localizer Text { get; private set; } = CreateLocalizer(store.Load().Active);
    public StyleRenderer Renderer { get; private set; } = CreateRenderer(store.Load().Active);
    public int ScreenWidth { get; set; } = 1;
    public int ScreenHeight { get; set; } = 1;

    /// <summary>Speichert geaenderte Optionen; ein Stilwechsel aktiviert den Optionssatz des neuen Stils (OP-19).</summary>
    public void UpdateSettings(UserSettings changed)
    {
        _profiles = _profiles.Apply(changed);
        store.Save(_profiles);
        var settings = Settings;
        Text = CreateLocalizer(settings);
        bool arenaChanged = Renderer is ModernRenderer modern && modern.Arena != settings.Arena;
        if (Renderer.Style != settings.Style || arenaChanged) Renderer = CreateRenderer(settings);
    }

    public ScreenLayout Layout(bool twoPlayers)
    {
        var layout = ScreenLayout.Create(Settings.Style, ScreenWidth, ScreenHeight, GameConfig.Default.FieldAspect, twoPlayers);
        return layout with { SafeTop = ScreenLayout.ToLogical(Platform.Cutout.Overdrawn, ScreenHeight, layout.Height) };
    }

    private static Localizer CreateLocalizer(UserSettings s) =>
        new(LanguageResolver.Resolve(s.Language, System.Globalization.CultureInfo.CurrentUICulture));

    private static StyleRenderer CreateRenderer(UserSettings s) =>
        s.Style == GraphicsStyle.Classic ? new ClassicRenderer() : new ModernRenderer(s.Arena);
}

public static class MusicChoice
{
    /// <summary>OP-10: gewaehlte Spielmusik oder null (Aus).</summary>
    public static SongId? GameMusicSong(UserSettings s) => s.GameMusic switch
    {
        GameTrack.Orbit => SongId.Orbit,
        GameTrack.Nebula => SongId.Nebula,
        GameTrack.Hyperdrive => SongId.Hyperdrive,
        GameTrack.SunsetRun => SongId.SunsetRun,
        GameTrack.LaserNights => SongId.LaserNights,
        GameTrack.PixelPulse => SongId.PixelPulse,
        GameTrack.Starlight => SongId.Starlight,
        _ => null,
    };
}

public interface IScreen
{
    ScreenLayout Layout { get; }
    /// <summary>Liefert den naechsten Bildschirm (oder sich selbst).</summary>
    IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys);
    void Render(PixelCanvas canvas, int frame);
    /// <summary>Seit dem letzten Abruf ausgeloeste Klaenge und Ansagen.</summary>
    IReadOnlyList<AudioCue> DrainCues();
    /// <summary>Was gerade klingen soll: Musikstueck und Dauerklaenge der angedockten Blitze.</summary>
    AudioState Audio { get; }
    void OnBackgrounded() { }
}

/// <summary>Tastatur (Desktop/externe Tastatur): Pfeile/Leertaste fuer P1, A/D/W fuer P2, Esc = zurueck.</summary>
public readonly record struct KeyboardCommands(float P1Turn, bool P1Fire, float P2Turn, bool P2Fire, bool Back);

public sealed class TitleScreen(AppContext app) : IScreen
{
    private readonly Dictionary<int, PointerEvent> _downs = [];
    private readonly Dictionary<int, double> _downTimes = [];   // OP-15: Optionen-Knopf lange halten
    private readonly List<AudioCue> _sounds = [];
    public ScreenLayout Layout => app.Layout(false);
    public AudioState Audio => new(SongId.Title, false, false);

    public IReadOnlyList<AudioCue> DrainCues() { var s = _sounds.ToList(); _sounds.Clear(); return s; }

    public IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys)
    {
        if (keys.Back) { app.Platform.Quit(); return this; }
        if (keys.P1Fire) return new PlayScreen(app, twoPlayers: false);
        var rects = app.Renderer.TitleItemRects(Layout);
        foreach (var p in pointers)
        {
            if (p.Phase == PointerPhase.Down) { _downs[p.Id] = p; _downTimes[p.Id] = time; continue; }
            if (p.Phase == PointerPhase.Move && _downs.ContainsKey(p.Id) && !rects[(int)TitleAction.Options].Contains(p.X, p.Y))
                _downTimes.Remove(p.Id);   // vom Knopf weggezogen: kein langes Halten mehr
            if (p.Phase != PointerPhase.Up || !_downs.Remove(p.Id, out var down)) continue;
            _downTimes.Remove(p.Id);
            int index = rects.ToList().FindIndex(r => r.Contains(p.X, p.Y) && r.Contains(down.X, down.Y));
            if (index >= 0) _sounds.Add(new AudioCue(SoundId.MenuClick));
            switch ((TitleAction)index)
            {
                case TitleAction.OnePlayer: return new PlayScreen(app, twoPlayers: false);
                case TitleAction.TwoPlayers: return new PlayScreen(app, twoPlayers: true);
                case TitleAction.Tutorial: return new TutorialScreen(app);
                case TitleAction.ToggleStyle:
                    app.UpdateSettings(app.Settings with
                    {
                        Style = app.Settings.Style == GraphicsStyle.Classic ? GraphicsStyle.Modern : GraphicsStyle.Classic,
                    });
                    return this;
                case TitleAction.Options: return new OptionsScreen(app);
            }
        }
        // OP-15: Optionen-Knopf lange gehalten -> versteckte erweiterte Optionen, die sanfte Stimme bestaetigt
        foreach (var (id, since) in _downTimes)
        {
            if (time - since < AdvancedMenu.HoldSeconds || !rects[(int)TitleAction.Options].Contains(_downs[id].X, _downs[id].Y)) continue;
            _downs.Remove(id);
            _downTimes.Remove(id);
            return new OptionsScreen(app, advanced: true, AudioCue.AdvancedOptions);
        }
        return this;
    }

    public void Render(PixelCanvas canvas, int frame) => app.Renderer.RenderTitle(canvas, Layout, app.Settings, app.Text, frame);
}

/// <summary>SO-6: Animierte Anleitung mit sechs Seiten; Knoepfe „Zurueck“ und „Weiter“, Zurueck-Taste zum Titel.</summary>
public sealed class TutorialScreen(AppContext app) : IScreen
{
    private readonly Dictionary<int, PointerEvent> _downs = [];
    private readonly List<AudioCue> _sounds = [];
    private int _page;
    private int? _pageStartFrame;
    public ScreenLayout Layout => app.Layout(false);
    public AudioState Audio => new(SongId.Title, false, false);
    public int Page => _page;

    public IReadOnlyList<AudioCue> DrainCues() { var s = _sounds.ToList(); _sounds.Clear(); return s; }

    public IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys)
    {
        if (keys.Back) return new TitleScreen(app);
        var nav = app.Renderer.TutorialNavRects(Layout);
        foreach (var p in pointers)
        {
            if (p.Phase == PointerPhase.Down) { _downs[p.Id] = p; continue; }
            if (p.Phase != PointerPhase.Up || !_downs.Remove(p.Id, out var down)) continue;
            bool Hit(IntRect r) => r.Contains(p.X, p.Y) && r.Contains(down.X, down.Y);
            if (Hit(nav[0]))
            {
                _sounds.Add(new AudioCue(SoundId.MenuClick));
                if (_page == 0) return new TitleScreen(app);
                _page--;
                _pageStartFrame = null;
            }
            else if (Hit(nav[1]))
            {
                _sounds.Add(new AudioCue(SoundId.MenuClick));
                if (_page == StyleRenderer.TutorialPages - 1) return new TitleScreen(app);
                _page++;
                _pageStartFrame = null;
            }
        }
        if (keys.P1Fire)
        {
            if (_page == StyleRenderer.TutorialPages - 1) return new TitleScreen(app);
            _page++;
            _pageStartFrame = null;
        }
        return this;
    }

    public void Render(PixelCanvas canvas, int frame)
    {
        _pageStartFrame ??= frame;   // jede Seite beginnt ihre Animation von vorn
        app.Renderer.RenderTutorial(canvas, Layout, _page, app.Text, frame - _pageStartFrame.Value);
    }
}

/// <summary>Optionen (Abschnitt 10) oder, mit <paramref name="advanced"/>, die versteckten erweiterten Optionen (OP-15).</summary>
public sealed class OptionsScreen(AppContext app, bool advanced = false, AudioCue? greeting = null) : IScreen
{
    private readonly Dictionary<int, PointerEvent> _downs = [];
    private readonly List<AudioCue> _sounds = greeting is { } g ? [g] : [];
    public const double ResetConfirmSeconds = 2.0;   // OP-16: so lange wartet „Sicher?“ auf das zweite Antippen
    private int _selected = -1;
    private double? _resetArmedAt;
    private SongId? _preview;   // zuletzt gewaehlte Spielmusik wird zur Probe angespielt
    public ScreenLayout Layout => app.Layout(false);
    public AudioState Audio => new(_preview ?? SongId.Title, false, false);

    public IReadOnlyList<AudioCue> DrainCues() { var s = _sounds.ToList(); _sounds.Clear(); return s; }

    public IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys)
    {
        if (_resetArmedAt is { } armed && time - armed > ResetConfirmSeconds) _resetArmedAt = null;
        if (keys.Back) return new TitleScreen(app);
        var entries = Entries(app.Settings);
        var rows = app.Renderer.OptionRowRects(Layout, entries.Count);
        foreach (var p in pointers)
        {
            if (p.Phase == PointerPhase.Down) { _downs[p.Id] = p; continue; }
            if (p.Phase != PointerPhase.Up || !_downs.Remove(p.Id, out var down)) continue;
            if (app.Renderer.OptionsBackRect(Layout).Contains(p.X, p.Y)) return new TitleScreen(app);
            if (app.Renderer.OptionsResetRect(Layout).Contains(p.X, p.Y) && app.Renderer.OptionsResetRect(Layout).Contains(down.X, down.Y))
            {
                _sounds.Add(new AudioCue(SoundId.MenuClick));
                if (_resetArmedAt is null) { _resetArmedAt = time; continue; }   // erstes Antippen: „Sicher?“
                _resetArmedAt = null;
                _selected = -1;
                _preview = null;
                app.UpdateSettings(advanced ? AdvancedMenu.Reset(app.Settings) : OptionsMenu.Reset(app.Settings));
                continue;
            }
            _resetArmedAt = null;
            int index = rows.ToList().FindIndex(r => r.Contains(p.X, p.Y) && r.Contains(down.X, down.Y));
            if (index < 0) continue;
            _sounds.Add(new AudioCue(SoundId.MenuClick));
            _selected = index;
            int direction = p.X < Layout.Width / 2f ? -1 : 1;   // linke Haelfte zurueck, rechte vor
            app.UpdateSettings(entries[index].Step(app.Settings, direction));
            _preview = entries[index].LabelKey == "opt.gameMusic" ? GameMusicSong(app.Settings) : null;
            if (app.Renderer.OptionRowRects(Layout, Entries(app.Settings).Count).Count <= _selected) _selected = -1;
        }
        return this;
    }

    private IReadOnlyList<OptionEntry> Entries(UserSettings s) => advanced ? AdvancedMenu.Entries(s) : OptionsMenu.Entries(s);

    public void Render(PixelCanvas canvas, int frame) =>
        app.Renderer.RenderOptions(canvas, Layout, app.Settings, app.Text, _selected, advanced, _resetArmedAt is not null, frame);
}

public sealed class PlayScreen : IScreen
{
    private const float KeyTurnRate = 150f;
    private readonly AppContext _app;
    private readonly GameSession _session;
    private const float MatchOverHold = 1.2f;   // „P1 GEWINNT!“ kurz stehen lassen, dann Siegerbildschirm
    private TouchInputMapper _input;
    private ScreenLayout _layout;
    private float _matchOverTime;
    private readonly List<AudioCue> _pauseSounds = [];

    public PlayScreen(AppContext app, bool twoPlayers)
    {
        _app = app;
        _session = new GameSession(app.Settings, twoPlayers, seed: Environment.TickCount);
        _layout = app.Layout(twoPlayers);
        _input = new TouchInputMapper(_layout, twoPlayers);
    }

    public ScreenLayout Layout => _layout;

    public AudioState Audio
    {
        get
        {
            var sim = _session.Simulation;
            bool live = !sim.Paused && sim.Phase == MatchPhase.Playing;
            bool Docked(PlayerId id) => live && sim.Bolts.Any(b => b.State == BoltState.Docked && b.DockedAt == id);
            return new AudioState(GameMusicSong(_app.Settings), Docked(PlayerId.One), Docked(PlayerId.Two));
        }
    }

    public IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys)
    {
        var current = _app.Layout(_session.TwoPlayers);
        if (current != _layout)   // Drehung des Geraets
        {
            _layout = current;
            _input = new TouchInputMapper(_layout, _session.TwoPlayers);
        }

        var sim = _session.Simulation;
        if (keys.Back)   // Zurueck: erst Pausenmenue, im Pausenmenue zum Hauptmenue
        {
            if (sim.Paused || sim.Phase == MatchPhase.MatchOver) return new TitleScreen(_app);
            _session.TogglePause();
            return this;
        }
        if (sim.Phase == MatchPhase.MatchOver)
        {
            _matchOverTime += dt;
            return _matchOverTime >= MatchOverHold ? new VictoryScreen(_app, _session) : this;
        }
        if (sim.Paused)
            return UpdatePauseMenu(pointers);

        foreach (var p in pointers)
        {
            switch (p.Phase)
            {
                case PointerPhase.Down: _input.TouchDown(p.Id, p.X, p.Y, time); break;
                case PointerPhase.Move: _input.TouchMove(p.Id, p.X, p.Y); break;
                case PointerPhase.Up: _input.TouchUp(p.Id, p.X, p.Y, time); break;
            }
        }
        var commands = _input.Drain().ToList();
        if (keys.P1Turn != 0) commands.Add(new InputCommand(CommandType.RotateSword, PlayerId.One, keys.P1Turn * KeyTurnRate * dt));
        if (keys.P1Fire) commands.Add(new InputCommand(CommandType.Fire, PlayerId.One));
        if (_session.TwoPlayers)
        {
            if (keys.P2Turn != 0) commands.Add(new InputCommand(CommandType.RotateSword, PlayerId.Two, keys.P2Turn * KeyTurnRate * dt));
            if (keys.P2Fire) commands.Add(new InputCommand(CommandType.Fire, PlayerId.Two));
        }
        _session.Apply(commands);
        _session.Update(dt);
        return this;
    }

    /// <summary>SO-1: Pausenmenue mit „Weiter“ und „Hauptmenue“; Tippen auf das Pause-Symbol setzt ebenfalls fort.</summary>
    private IScreen UpdatePauseMenu(IReadOnlyList<PointerEvent> pointers)
    {
        var buttons = _app.Renderer.PauseMenuRects(_layout);
        var entries = PauseMenu.Entries(_app.Settings);
        var rows = _app.Renderer.PauseOptionRects(_layout, entries.Count);
        foreach (var p in pointers.Where(p => p.Phase == PointerPhase.Up))
        {
            int row = rows.ToList().FindIndex(r => r.Contains(p.X, p.Y));
            if (row >= 0)   // OP-17: Spielmusik/Stimme umschalten, linke Haelfte zurueck, rechte vor
            {
                _pauseSounds.Add(new AudioCue(SoundId.MenuClick));
                _app.UpdateSettings(entries[row].Step(_app.Settings, p.X < rows[row].X + rows[row].Width / 2f ? -1 : 1));
                continue;
            }
            if (buttons[1].Contains(p.X, p.Y)) return new TitleScreen(_app);
            if (buttons[0].Contains(p.X, p.Y) || _layout.PauseButton.Contains(p.X, p.Y))
            {
                _session.TogglePause();
                _input = new TouchInputMapper(_layout, _session.TwoPlayers);   // angefangene Gesten verwerfen
                break;
            }
        }
        return this;
    }

    public void Render(PixelCanvas canvas, int frame) => _app.Renderer.RenderGame(canvas, _layout, _session, _app.Text, frame, _app.Settings);

    public IReadOnlyList<AudioCue> DrainCues()
    {
        // Fanfare und Sieg-/Niederlage-Ansage spielt der Siegerbildschirm
        var sounds = _session.PendingEvents.Where(e => e.Type != GameEventType.MatchWon).Select(AudioCue.From).ToList();
        _session.PendingEvents.Clear();
        sounds.AddRange(_pauseSounds);
        _pauseSounds.Clear();
        return sounds;
    }

    public void OnBackgrounded() => _session.OnAppBackgrounded();
}

/// <summary>
/// Siegerbildschirm (RS-5): Pokal, Sieger, Endstand und Fanfare. Tippen fuehrt zum Hauptmenue, aber erst nach
/// einer kurzen Sperrzeit und nur mit einer neuen Beruehrung – so wird er nicht versehentlich uebersprungen.
/// </summary>
public sealed class VictoryScreen(AppContext app, GameSession session) : IScreen
{
    public const float InputLockSeconds = 1.5f;
    private readonly HashSet<int> _freshTouches = [];
    private float _elapsed;
    private bool _fanfarePlayed;

    public ScreenLayout Layout => app.Layout(false);
    public AudioState Audio => AudioState.Silent;

    public IReadOnlyList<AudioCue> DrainCues()
    {
        if (_fanfarePlayed) return [];
        _fanfarePlayed = true;
        bool humanWon = session.TwoPlayers || session.Simulation.Winner == PlayerId.One;
        return [AudioCue.Victory(humanWon)];
    }

    public IScreen Update(float dt, IReadOnlyList<PointerEvent> pointers, double time, KeyboardCommands keys)
    {
        _elapsed += dt;
        if (keys.Back) return new TitleScreen(app);
        foreach (var p in pointers)
        {
            if (p.Phase == PointerPhase.Down && _elapsed >= InputLockSeconds) _freshTouches.Add(p.Id);
            if (p.Phase == PointerPhase.Up && _freshTouches.Remove(p.Id)) return new TitleScreen(app);
        }
        if (keys.P1Fire && _elapsed >= InputLockSeconds) return new TitleScreen(app);
        return this;
    }

    public void Render(PixelCanvas canvas, int frame)
    {
        var info = VictoryInfo.From(session, app.Text, showContinue: _elapsed >= InputLockSeconds);
        // 2 Spieler hochkant: hat Spieler 2 (oben) gewonnen, steht der Bildschirm fuer ihn richtig herum
        bool rotate = session.TwoPlayers && session.Simulation.Winner == PlayerId.Two && Layout.Orientation == ScreenOrientation.Portrait;
        if (!rotate)
        {
            app.Renderer.RenderVictory(canvas, Layout, info, app.Text, frame);
            return;
        }
        var temp = new PixelCanvas(canvas.Width, canvas.Height);
        app.Renderer.RenderVictory(temp, Layout, info, app.Text, frame);
        canvas.Blit(temp, 0, 0, rotate180: true);
    }
}

