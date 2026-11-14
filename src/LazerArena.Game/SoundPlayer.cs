using LazerArena.Presentation.Audio;
using LazerArena.Presentation.Settings;
using Microsoft.Xna.Framework.Audio;

namespace LazerArena.Game;

/// <summary>Soll-Zustand der Dauerklaenge: Musikstueck (null = keins) und angedockte Blitze je Spieler.</summary>
public readonly record struct AudioState(SongId? Music, bool DockedAtPlayer1, bool DockedAtPlayer2)
{
    public static AudioState Silent => new(null, false, false);
}

/// <summary>
/// Spielt Klaenge, Dauerklaenge und Musik ab (OP-7, OP-8, OP-10). Effekte und Titelmusik werden beim Start im
/// Hintergrund berechnet, die Spielmusiken erst, wenn sie gebraucht werden.
/// </summary>
public sealed class SoundPlayer : IDisposable
{
    private const float MenuMusicVolume = 0.6f;
    private const float GameMusicVolume = 0.35f;   // leiser, damit die Effekte hoerbar bleiben

    private readonly Dictionary<GraphicsStyle, ISoundBank> _banks = new()
    {
        [GraphicsStyle.Classic] = new ClassicSoundBank(),
        [GraphicsStyle.Modern] = new ModernSoundBank(),
    };
    private readonly Dictionary<(GraphicsStyle, SoundId), SoundEffect> _effects = [];
    private readonly Dictionary<SongId, Task<AudioClip?>> _songs = [];
    private readonly SoundEffectInstance?[] _dockLoops = new SoundEffectInstance?[2];
    private GraphicsStyle? _dockLoopStyle;
    private SongId? _currentSong;
    private SoundEffect? _musicEffect;
    private SoundEffectInstance? _music;

    public void Warmup()
    {
        Task.Run(() =>
        {
            foreach (var bank in _banks.Values)
                foreach (var id in Enum.GetValues<SoundId>())
                    bank.Get(id);
        });
        RequestSong(SongId.Title);
    }

    private readonly Dictionary<(string, VoiceLine), SoundEffect> _voices = [];
    private readonly List<(VoiceLine Line, float Remaining)> _pendingVoices = [];

    public void Play(SoundId id, UserSettings settings)
    {
        if (!settings.Sound) return;   // OP-7
        Effect(settings.Style, id).Play(0.8f, 0, 0);
    }

    /// <summary>
    /// OP-13: Mit eingeschalteter Stimme ersetzt die Ansage die Countdown-Pieptoene; bei Treffer, Sieg und
    /// Niederlage kommt sie zusaetzlich (leicht verzoegert). Mit ausgeschalteter Stimme klingen nur die Effekte.
    /// </summary>
    public void Play(AudioCue cue, UserSettings settings, string language)
    {
        bool voice = settings.Voice && cue.Voice is not null;
        if (cue.Sound is { } sound && !(voice && cue.VoiceReplacesSound)) Play(sound, settings);
        if (!voice) return;
        if (cue.VoiceDelay <= 0) PlayVoice(cue.Voice!.Value, language);
        else _pendingVoices.Add((cue.Voice!.Value, cue.VoiceDelay));
    }

    public void TickVoices(float dt, string language)
    {
        for (int i = _pendingVoices.Count - 1; i >= 0; i--)
        {
            var (line, remaining) = _pendingVoices[i];
            if (remaining - dt > 0) { _pendingVoices[i] = (line, remaining - dt); continue; }
            _pendingVoices.RemoveAt(i);
            PlayVoice(line, language);
        }
    }

    private void PlayVoice(VoiceLine line, string language)
    {
        if (!_voices.TryGetValue((language, line), out var effect))
        {
            var clip = VoiceBank.Get(language, line);
            _voices[(language, line)] = effect = new SoundEffect(clip.ToBytes(), clip.SampleRate, AudioChannels.Mono);
        }
        effect.Play(1f, 0, 0);
    }

    /// <summary>Gleicht Musik und Dauerklaenge an den gewuenschten Zustand an (einmal je Bild aufrufen).</summary>
    public void Update(AudioState state, bool appActive, UserSettings settings)
    {
        UpdateDockLoops(state, appActive, settings);
        bool allowed = appActive && settings.Style == GraphicsStyle.Modern && state.Music switch
        {
            SongId.Title => settings.Music,                     // OP-8
            null => false,
            _ => true,                                          // Spielmusik: Auswahl/Aus entscheidet der Bildschirm (OP-10)
        };
        UpdateMusic(allowed ? state.Music : null);
    }

    private void RequestSong(SongId song)
    {
        if (!_songs.ContainsKey(song))
            _songs[song] = Task.Run(() => _banks[GraphicsStyle.Modern].Music(song));
    }

    /// <summary>Knistern eines angedockten Blitzes, solange er an der Schwertspitze haengt.</summary>
    private void UpdateDockLoops(AudioState state, bool appActive, UserSettings settings)
    {
        if (_dockLoopStyle != settings.Style)
        {
            foreach (var loop in _dockLoops) loop?.Dispose();
            Array.Clear(_dockLoops);
            _dockLoopStyle = settings.Style;
        }
        bool[] wanted = [state.DockedAtPlayer1, state.DockedAtPlayer2];
        for (int i = 0; i < 2; i++)
        {
            bool on = wanted[i] && appActive && settings.Sound;
            if (on)
            {
                _dockLoops[i] ??= CreateLoop(Effect(settings.Style, SoundId.DockLoop), 0.55f);
                if (_dockLoops[i]!.State != SoundState.Playing) _dockLoops[i]!.Play();
            }
            else if (_dockLoops[i]?.State == SoundState.Playing)
            {
                _dockLoops[i]!.Stop();
            }
        }
    }

    private void UpdateMusic(SongId? wanted)
    {
        if (wanted != _currentSong)
        {
            _music?.Stop();
            _music?.Dispose();
            _musicEffect?.Dispose();
            _music = null;
            _musicEffect = null;
            _currentSong = null;
        }
        if (wanted is not { } song || _music is not null) return;
        RequestSong(song);
        if (_songs[song] is not { IsCompletedSuccessfully: true } task || task.Result is not { } clip) return;   // wird noch berechnet

        _musicEffect = new SoundEffect(clip.ToBytes(), clip.SampleRate, AudioChannels.Mono);
        _music = CreateLoop(_musicEffect, song == SongId.Title ? MenuMusicVolume : GameMusicVolume);
        _music.Play();
        _currentSong = song;
    }

    private SoundEffect Effect(GraphicsStyle style, SoundId id)
    {
        if (!_effects.TryGetValue((style, id), out var effect))
        {
            var clip = _banks[style].Get(id);
            _effects[(style, id)] = effect = new SoundEffect(clip.ToBytes(), clip.SampleRate, AudioChannels.Mono);
        }
        return effect;
    }

    private static SoundEffectInstance CreateLoop(SoundEffect effect, float volume)
    {
        var instance = effect.CreateInstance();
        instance.IsLooped = true;
        instance.Volume = volume;
        return instance;
    }

    public void Dispose()
    {
        _music?.Dispose();
        _musicEffect?.Dispose();
        foreach (var loop in _dockLoops) loop?.Dispose();
        foreach (var e in _effects.Values) e.Dispose();
        foreach (var v in _voices.Values) v.Dispose();
    }
}
