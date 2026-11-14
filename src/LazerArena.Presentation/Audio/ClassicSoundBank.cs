namespace LazerArena.Presentation.Audio;

/// <summary>
/// Klassischer Stil (F5): Klaenge wie auf dem Atari VCS. Nachgebildet wird die Arbeitsweise eines einfachen
/// Tonchips: ein Grundtakt von 31,4 kHz wird durch 1..32 geteilt; daraus entstehen Rechteck- oder
/// Schieberegister-Rauschsignale mit nur 16 Lautstaerkestufen. Klaenge sind Folgen von Bildschritten (60 Hz).
/// </summary>
public sealed class ClassicSoundBank : ISoundBank
{
    public const int SampleRate = 22050;
    private const double ChipClock = 31400;
    private readonly Dictionary<SoundId, AudioClip> _cache = [];

    public enum Wave { Square, LowSquare, Buzz4, Buzz5, Noise }

    /// <summary>Ein Bildschritt (1/60 s): Wellenform, Teiler 0..31, Lautstaerke 0..15.</summary>
    public readonly record struct Frame(Wave Wave, int Divider, int Volume);

    public AudioClip? Music(SongId song) => null;

    public AudioClip Get(SoundId id)
    {
        lock (_cache)   // Vorberechnung laeuft im Hintergrund
        {
            if (!_cache.TryGetValue(id, out var clip))
                _cache[id] = clip = Render(Script(id));
            return clip;
        }
    }

    public static IReadOnlyList<Frame> Script(SoundId id) => id switch
    {
        SoundId.Fire => Sweep(Wave.Square, 3, 15, 10, 13, 3),
        SoundId.ThrowBack => Sweep(Wave.Square, 14, 2, 10, 13, 5),
        SoundId.Dock => Sweep(Wave.Square, 2, 8, 6, 12, 8),
        SoundId.DockLoop => Enumerable.Range(0, 30).Select(i => new Frame(Wave.Buzz4, 1, i % 2 == 0 ? 10 : 6)).ToList(),
        SoundId.Fizzle => Sweep(Wave.Noise, 6, 12, 14, 8, 0),
        SoundId.ShieldHit => Sweep(Wave.Noise, 2, 4, 9, 14, 0),
        SoundId.ShieldRegrow => [.. Repeat(new Frame(Wave.Square, 12, 6), 4), .. Repeat(new Frame(Wave.Square, 8, 6), 4)],
        SoundId.PlayerHit => [.. Sweep(Wave.Noise, 14, 31, 40, 15, 0), .. Sweep(Wave.LowSquare, 20, 31, 20, 8, 0)],
        SoundId.StormWarning => Enumerable.Range(0, 56).Select(i => new Frame(Wave.Square, i / 7 % 2 == 0 ? 6 : 11, 12)).ToList(),
        SoundId.StormStart => Sweep(Wave.Buzz5, 2, 6, 30, 13, 2),
        SoundId.StormEnd => Sweep(Wave.Square, 6, 20, 18, 8, 0),
        SoundId.Countdown => Repeat(new Frame(Wave.Square, 11, 12), 8).ToList(),
        SoundId.CountdownHigh => Repeat(new Frame(Wave.Square, 8, 12), 8).ToList(),   // etwas hoeher
        SoundId.RoundStart => Repeat(new Frame(Wave.Square, 5, 13), 14).ToList(),
        // Siegesfanfare: C E G C' – As B – C' (Teiler nach Tonhoehe gewaehlt), Schluss mit Ausklingen
        SoundId.MatchWon => [.. new[] { (29, 6), (23, 6), (19, 6), (14, 18), (18, 10), (16, 10) }
            .SelectMany(n => Repeat(new Frame(Wave.Square, n.Item1, 12), n.Item2)), .. Sweep(Wave.Square, 14, 14, 40, 13, 2)],
        SoundId.MatchLost => [.. new[] { (19, 10), (21, 10), (23, 10) }.SelectMany(n => Repeat(new Frame(Wave.Square, n.Item1, 11), n.Item2)),
            .. Sweep(Wave.Square, 26, 31, 36, 11, 0)],
        SoundId.MenuClick => Repeat(new Frame(Wave.Square, 6, 9), 3).ToList(),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    private static List<Frame> Sweep(Wave wave, int fromDivider, int toDivider, int frames, int fromVolume, int toVolume) =>
        Enumerable.Range(0, frames).Select(i =>
        {
            float t = frames == 1 ? 0 : i / (float)(frames - 1);
            return new Frame(wave, (int)MathF.Round(fromDivider + (toDivider - fromDivider) * t),
                (int)MathF.Round(fromVolume + (toVolume - fromVolume) * t));
        }).ToList();

    private static IEnumerable<Frame> Repeat(Frame f, int n) => Enumerable.Repeat(f, n);

    /// <summary>Erzeugt das Signal: Chip-Takt simulieren, auf 22,05 kHz abtasten.</summary>
    public static AudioClip Render(IReadOnlyList<Frame> frames)
    {
        int samplesPerFrame = SampleRate / 60;
        var samples = new short[frames.Count * samplesPerFrame];
        double chipPerSample = ChipClock / SampleRate, chipTime = 0;
        int dividerCounter = 0, lfsr4 = 0xF, lfsr5 = 0x1F, lfsr9 = 0x1FF, lowCounter = 0;
        bool output = false;

        for (int f = 0; f < frames.Count; f++)
        {
            var frame = frames[f];
            short level = (short)(Math.Clamp(frame.Volume, 0, 15) * 1000);   // 16 Stufen
            for (int s = 0; s < samplesPerFrame; s++)
            {
                chipTime += chipPerSample;
                while (chipTime >= 1)
                {
                    chipTime -= 1;
                    if (++dividerCounter <= Math.Clamp(frame.Divider, 0, 31)) continue;
                    dividerCounter = 0;
                    switch (frame.Wave)
                    {
                        case Wave.Square: output = !output; break;
                        case Wave.LowSquare: if (++lowCounter >= 3) { lowCounter = 0; output = !output; } break;
                        case Wave.Buzz4: lfsr4 = Step(lfsr4, 4, 3); output = (lfsr4 & 1) != 0; break;
                        case Wave.Buzz5: lfsr5 = Step(lfsr5, 5, 3); output = (lfsr5 & 1) != 0; break;
                        case Wave.Noise: lfsr9 = Step(lfsr9, 9, 5); output = (lfsr9 & 1) != 0; break;
                    }
                }
                samples[f * samplesPerFrame + s] = output ? level : (short)-level;
            }
        }
        return new AudioClip(samples, SampleRate);
    }

    /// <summary>Lineares Schieberegister mit Rueckkopplung aus Bit 0 und Bit <paramref name="tap"/>.</summary>
    private static int Step(int register, int bits, int tap)
    {
        int bit = (register ^ (register >> (bits - tap))) & 1;
        return (register >> 1) | (bit << (bits - 1));
    }
}
