using LazerArena.Core;

namespace LazerArena.Presentation.Audio;

public enum SoundId
{
    Fire, ThrowBack, Dock, DockLoop, Fizzle, ShieldHit, ShieldRegrow, PlayerHit,
    StormWarning, StormStart, StormEnd, Countdown, CountdownHigh, RoundStart, MatchWon, MatchLost, MenuClick,
}

/// <summary>Mono-PCM, 16 Bit.</summary>
public sealed class AudioClip(short[] samples, int sampleRate)
{
    public short[] Samples { get; } = samples;
    public int SampleRate { get; } = sampleRate;
    public float DurationSeconds => Samples.Length / (float)SampleRate;

    /// <summary>Little-Endian-Bytes, wie sie MonoGame fuer SoundEffect erwartet.</summary>
    public byte[] ToBytes()
    {
        var bytes = new byte[Samples.Length * 2];
        Buffer.BlockCopy(Samples, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>Gleitkomma-Signal (-1..1) in 16 Bit; bei Bedarf auf Spitzenpegel <paramref name="peak"/> normiert.</summary>
    public static AudioClip FromFloats(float[] signal, int sampleRate, float peak = 0.9f)
    {
        float max = signal.Length == 0 ? 0 : signal.Max(MathF.Abs);
        float gain = max > peak ? peak / max : 1f;
        return new AudioClip(signal.Select(s => (short)MathF.Round(Math.Clamp(s * gain, -1f, 1f) * short.MaxValue)).ToArray(), sampleRate);
    }
}

/// <summary>Ordnet Spielereignisse den Klaengen zu.</summary>
public static class SoundEvents
{
    public static SoundId? For(GameEvent e) => e.Type switch
    {
        GameEventType.BoltFired => SoundId.Fire,
        GameEventType.BoltThrownBack => SoundId.ThrowBack,
        GameEventType.BoltDocked => SoundId.Dock,
        GameEventType.BoltFizzled => SoundId.Fizzle,
        GameEventType.ShieldSegmentDestroyed => SoundId.ShieldHit,
        GameEventType.ShieldSegmentRegrown => SoundId.ShieldRegrow,
        GameEventType.PlayerHit => SoundId.PlayerHit,
        GameEventType.StormWarning => SoundId.StormWarning,   // LG-2: Warnton
        GameEventType.StormStarted => SoundId.StormStart,
        GameEventType.StormEnded => SoundId.StormEnd,
        GameEventType.CountdownStarted => SoundId.Countdown,      // „3“
        GameEventType.CountdownTwo => SoundId.Countdown,          // „2“
        GameEventType.CountdownOne => SoundId.CountdownHigh,      // „1“: etwas hoeher
        GameEventType.RoundStarted => null,                       // Start ohne eigenen Ton (die drei Pieptoene genuegen)
        GameEventType.MatchWon => SoundId.MatchWon,
        _ => null,
    };
}

public interface ISoundBank
{
    AudioClip Get(SoundId id);
    /// <summary>Musikstueck (nur moderner Stil, OP-8, OP-10); null, wenn der Stil keine Musik hat. Wird beim ersten Abruf berechnet.</summary>
    AudioClip? Music(SongId song);
}
