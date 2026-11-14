using System.Collections.Concurrent;
using LazerArena.Core;

namespace LazerArena.Presentation.Audio;

/// <summary>Ansagen der weiblichen Computerstimme (OP-13).</summary>
public enum VoiceLine { Three, Two, One, Hit, Victory, Defeat }

/// <summary>Ein Hoerereignis: Klangeffekt und/oder Sprachansage. Ist die Stimme an, ersetzt sie die Countdown-Pieptoene.</summary>
public readonly record struct AudioCue(SoundId? Sound, VoiceLine? Voice = null, bool VoiceReplacesSound = false, float VoiceDelay = 0)
{
    public static AudioCue From(GameEvent e) => e.Type switch
    {
        GameEventType.CountdownStarted => new(SoundId.Countdown, VoiceLine.Three, VoiceReplacesSound: true),
        GameEventType.CountdownTwo => new(SoundId.Countdown, VoiceLine.Two, VoiceReplacesSound: true),
        GameEventType.CountdownOne => new(SoundId.CountdownHigh, VoiceLine.One, VoiceReplacesSound: true),
        GameEventType.PlayerHit => new(SoundId.PlayerHit, VoiceLine.Hit, VoiceDelay: 0.25f),
        _ => new(SoundEvents.For(e)),
    };

    public static AudioCue Victory(bool humanWon) =>
        new(humanWon ? SoundId.MatchWon : SoundId.MatchLost, humanWon ? VoiceLine.Victory : VoiceLine.Defeat, VoiceDelay: 0.6f);
}

/// <summary>Laedt die eingebetteten Sprachaufnahmen (Audio/Voice/&lt;sprache&gt;/&lt;ansage&gt;.wav).</summary>
public static class VoiceBank
{
    private static readonly ConcurrentDictionary<(string, VoiceLine), AudioClip> Cache = new();

    public static AudioClip Get(string language, VoiceLine line) => Cache.GetOrAdd((language, line), key =>
    {
        string resource = $"LazerArena.Presentation.Voice.{key.Item1}.{key.Item2}.wav";
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
