using System.Collections.Concurrent;
using LazerArena.Core;
using LazerArena.Presentation.Settings;

namespace LazerArena.Presentation.Audio;

/// <summary>Ansagen der weiblichen Computerstimme (OP-13).</summary>
public enum VoiceLine { Three, Two, One, Go, Hit, Victory, Defeat, Warning, Advanced }

/// <summary>
/// Ein Hoerereignis: Klangeffekt und/oder Sprachansage. Ist die Stimme an, ersetzt sie die Countdown- und Warnpieptoene.
/// <paramref name="ForcedVoice"/>: diese Stimme spricht unabhaengig von OP-13 (nur der Ton muss an sein).
/// </summary>
public readonly record struct AudioCue(SoundId? Sound, VoiceLine? Voice = null, bool VoiceReplacesSound = false, float VoiceDelay = 0,
    VoiceKind? ForcedVoice = null)
{
    public static AudioCue From(GameEvent e) => e.Type switch
    {
        GameEventType.CountdownStarted => new(SoundId.Countdown, VoiceLine.Three, VoiceReplacesSound: true),
        GameEventType.CountdownTwo => new(SoundId.Countdown, VoiceLine.Two, VoiceReplacesSound: true),
        GameEventType.CountdownOne => new(SoundId.Countdown, VoiceLine.One, VoiceReplacesSound: true),
        GameEventType.RoundStarted => new(SoundId.RoundStart, VoiceLine.Go, VoiceReplacesSound: true),   // RS-4: „GO!“
        GameEventType.PlayerHit => new(SoundId.PlayerHit, VoiceLine.Hit, VoiceDelay: 0.25f),
        GameEventType.StormWarning => new(SoundId.StormWarning, VoiceLine.Warning, VoiceReplacesSound: true),   // LG-2
        _ => new(SoundEvents.For(e)),
    };

    /// <summary>OP-15: Die sanfte Stimme bestaetigt die versteckten erweiterten Optionen.</summary>
    public static AudioCue AdvancedOptions => new(null, VoiceLine.Advanced, ForcedVoice: VoiceKind.Soft);

    public static AudioCue Victory(bool humanWon) =>
        new(humanWon ? SoundId.MatchWon : SoundId.MatchLost, humanWon ? VoiceLine.Victory : VoiceLine.Defeat, VoiceDelay: 0.6f);
}

/// <summary>
/// Laedt die eingebetteten Sprachaufnahmen: Computerstimme aus Audio/Voice/&lt;sprache&gt;/, sanfte Stimme aus
/// Audio/Voice/soft/&lt;sprache&gt;/ (je &lt;ansage&gt;.wav, OP-13).
/// </summary>
public static class VoiceBank
{
    private static readonly ConcurrentDictionary<(VoiceKind, string, VoiceLine), AudioClip> Cache = new();

    public static AudioClip Get(VoiceKind voice, string language, VoiceLine line) => Cache.GetOrAdd((voice, language, line), key =>
    {
        if (key.Item1 == VoiceKind.Off) throw new ArgumentException("Keine Stimme gewaehlt", nameof(voice));
        string folder = key.Item1 == VoiceKind.Soft ? "soft." : "";
        string resource = $"LazerArena.Presentation.Voice.{folder}{key.Item2}.{key.Item3}.wav";
        using var stream = typeof(VoiceBank).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Sprachaufnahme fehlt: {resource}");
        return ReadWav(stream);
    });

    /// <summary>Liest eine PCM-WAV-Datei (16 Bit, mono).</summary>
    public static AudioClip ReadWav(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        reader.ReadBytes(12);   // RIFF-Kopf
        int sampleRate = 22050;
        while (stream.Position < stream.Length)
        {
            string chunk = new(reader.ReadChars(4));
            int size = reader.ReadInt32();
            if (chunk == "fmt ")
            {
                reader.ReadInt16(); reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadBytes(size - 8);
            }
            else if (chunk == "data")
            {
                var samples = new short[size / 2];
                for (int i = 0; i < samples.Length; i++) samples[i] = reader.ReadInt16();
                return new AudioClip(samples, sampleRate);
            }
            else reader.ReadBytes(size);
        }
        throw new InvalidDataException("WAV ohne Daten");
    }
}
