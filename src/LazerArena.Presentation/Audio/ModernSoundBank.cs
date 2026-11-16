namespace LazerArena.Presentation.Audio;

/// <summary>
/// Moderner Stil (F4, F5): synthetisch erzeugte Soundsamples (Tonhoehenverlaeufe, gefiltertes Rauschen,
/// Echo) und eine eigene Titelmusik. Alles wird beim ersten Gebrauch berechnet (F11: keine Fremdlizenzen).
/// </summary>
public sealed class ModernSoundBank : ISoundBank
{
    private readonly Dictionary<SoundId, AudioClip> _cache = [];
    private readonly Dictionary<SongId, Lazy<AudioClip>> _music =
        Enum.GetValues<SongId>().ToDictionary(id => id, id => new Lazy<AudioClip>(() => MusicComposer.Compose(Songs.For(id))));

    public AudioClip? Music(SongId song) => _music[song].Value;

    public AudioClip Get(SoundId id)
    {
        lock (_cache)   // Vorberechnung laeuft im Hintergrund
        {
            if (!_cache.TryGetValue(id, out var clip))
            {
                var signal = Synthesize(id);
                if (id != SoundId.DockLoop) Dsp.Fade(signal);   // die Schleife wird nahtlos gemacht, nicht ausgeblendet
                _cache[id] = clip = AudioClip.FromFloats(signal, Dsp.SampleRate, peak: Peak(id));
            }
            return clip;
        }
    }

    private static float[] Synthesize(SoundId id) => id switch
    {
        SoundId.Fire => LaserZap(1500, 260, 0.28f, seed: 1),
        SoundId.ThrowBack => LaserZap(280, 1700, 0.3f, seed: 2),
        SoundId.Dock => Snap(0.12f),
        SoundId.DockLoop => CrackleLoop(1.0f),
        SoundId.Fizzle => Fizzle(0.45f),
        SoundId.ShieldHit => Swoosh(0.24f),
        SoundId.ShieldRegrow => ElectricFm(),   // V1.1: kurzes, ansteigendes elektrisches Summen
        SoundId.PlayerHit => Explosion(1.3f),
        SoundId.StormWarning => Alarm(1.0f),
        SoundId.StormStart => Thunder(0.9f),
        SoundId.StormEnd => LaserZap(900, 120, 0.5f, seed: 3, volume: 0.6f),
        SoundId.Countdown => Beep(880, 0.14f),
        SoundId.RoundStart => Beep(1760, 0.32f),   // RS-4: „GO“ eine Oktave hoeher und laenger
        SoundId.MatchWon => Fanfare(),
        SoundId.MatchLost => PowerDown(),
        SoundId.MenuClick => Beep(1320, 0.05f, 0.5f),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Spitzenpegel je Klang; das Nachwachsen des Schilds ist leiser, weil es oft und nebenbei passiert.</summary>
    public static float Peak(SoundId id) => id == SoundId.ShieldRegrow ? RegrowPeak : 0.8f;

    public const float RegrowPeak = 0.4f;

    /// <summary>Laserschuss: abfallende (oder steigende) Tonhoehe, Saege+Rechteck, Rauschanteil, Echo.</summary>
    private static float[] LaserZap(float fromHz, float toHz, float seconds, int seed, float volume = 1f)
    {
        var s = Dsp.Buffer(seconds + 0.25f);
        var noise = new Dsp.Noise(seed);
        var lp = new Dsp.LowPass();
        double phase = 0;
        int n = (int)(seconds * Dsp.SampleRate);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float hz = fromHz * MathF.Pow(toHz / fromHz, t);
            phase += hz / Dsp.SampleRate;
            float tone = 0.6f * Dsp.Saw(phase) + 0.4f * Dsp.Square(phase * 1.005, 0.3);
            float env = MathF.Pow(1 - t, 1.5f);
            s[i] = volume * env * (lp.Process(tone, 2000 + 5000 * (1 - t)) + 0.15f * noise.Next() * (1 - t) * (1 - t));
        }
        Dsp.Echo(s, 0.09f, 0.45f, 1f);
        return s;
    }

    /// <summary>Andocken: kurzer elektrischer Einschlag.</summary>
    private static float[] Snap(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(3);
        var lp = new Dsp.LowPass();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)s.Length;
            s[i] = MathF.Pow(1 - t, 3) * (lp.Process(noise.Next(), 6000) + 0.5f * Dsp.Saw(i * 440.0 / Dsp.SampleRate));
        }
        return s;
    }

    /// <summary>
    /// Angedockter Blitz (Dauerton, solange er haengt): elektrisches Knistern ueber brummendem Grundton.
    /// Alle Periodenlaengen passen ganzzahlig in die Schleife; die Raender werden ueberblendet -> nahtlos.
    /// </summary>
    private static float[] CrackleLoop(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(7);
        var lp = new Dsp.LowPass();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            float wobble = 1 + 0.02f * MathF.Sin(2 * MathF.PI * 4 * t);
            float hum = 0.5f * Dsp.Saw(110 * t * wobble) * (0.6f + 0.4f * Dsp.Square(30 * t));
            float burst = noise.Next() > 0.93f ? 1 : 0.15f;
            s[i] = lp.Process(hum, 1200) + 0.5f * burst * noise.Next();
        }
        int fade = Dsp.SampleRate / 20;
        for (int i = 0; i < fade; i++)   // Ende in den Anfang ueberblenden
        {
            float g = i / (float)fade;
            s[i] = s[i] * g + s[s.Length - fade + i] * (1 - g);
        }
        return s[..^fade];
    }

    private static float[] Fizzle(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(11);
        var lp = new Dsp.LowPass();
        double phase = 0;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)s.Length;
            phase += (900 * MathF.Pow(0.12f, t)) / Dsp.SampleRate;
            s[i] = (1 - t) * (1 - t) * (0.4f * Dsp.Sine(phase) + 0.5f * lp.Process(noise.Next(), 3000 * (1 - t) + 200));
        }
        return s;
    }

    /// <summary>Schildtreffer: kurzer Sci-Fi-„Swoosh“ – Rauschen durch ein von hoch nach tief wanderndes Filter, leiser Abwaertston.</summary>
    private static float[] Swoosh(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(5);
        var low = new Dsp.LowPass();
        var lower = new Dsp.LowPass();
        double phase = 0;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)s.Length;
            float cutoff = 7000 * MathF.Pow(0.06f, t);
            float n = noise.Next();
            float band = low.Process(n, cutoff) - lower.Process(n, cutoff * 0.35f);   // Bandpass
            phase += (1400 * MathF.Pow(0.25f, t)) / Dsp.SampleRate;
            float env = MathF.Min(1, t * 25) * MathF.Pow(1 - t, 2);
            s[i] = env * (1.6f * band + 0.18f * Dsp.Sine(phase));
        }
        return s;
    }

    /// <summary>Sci-Fi-Siegesfanfare: Pauke, Blechblaeser-Synthesizer (C-Dur, As, B, C-Dur), funkelndes Arpeggio, Echo.</summary>
    private static float[] Fanfare()
    {
        var s = Dsp.Buffer(4.2f);
        // (Start, Dauer, Akkordtoene)
        (float Start, float Length, int[] Notes)[] chords =
        [
            (0.00f, 0.12f, [60]), (0.13f, 0.12f, [64]), (0.26f, 0.12f, [67]),
            (0.40f, 1.00f, [60, 64, 67, 72]),
            (1.45f, 0.38f, [56, 60, 63, 68]),
            (1.85f, 0.38f, [58, 62, 65, 70]),
            (2.25f, 1.50f, [60, 64, 67, 72, 76]),
        ];
        foreach (var (start, length, notes) in chords)
            foreach (int n in notes)
                Brass(s, start, length, Dsp.NoteToHz(n), 0.16f);
        foreach (float t in new[] { 0.40f, 2.25f })
            Boom(s, t);
        int[] sparkle = [84, 88, 91, 96, 91, 88, 96, 100];
        for (int i = 0; i < sparkle.Length; i++)
            Sparkle(s, 2.3f + i * 0.09f, Dsp.NoteToHz(sparkle[i]));
        Dsp.Echo(s, 0.18f, 0.35f, 1f);
        return s;
    }

    /// <summary>Niederlage: absteigende Moll-Linie, dann tiefer Akkord, der „herunterfaehrt“.</summary>
    private static float[] PowerDown()
    {
        var s = Dsp.Buffer(2.6f);
        int[] line = [67, 65, 63, 62];
        for (int i = 0; i < line.Length; i++)
            Brass(s, i * 0.2f, 0.18f, Dsp.NoteToHz(line[i]), 0.18f);
        foreach (int n in new[] { 48, 55, 60, 63 })
        {
            int start = (int)(0.85f * Dsp.SampleRate);
            double phase = 0;
            var lp = new Dsp.LowPass();
            for (int i = start; i < s.Length; i++)
            {
                float t = (i - start) / (float)Dsp.SampleRate;
                phase += Dsp.NoteToHz(n) * MathF.Pow(0.5f, t / 1.6f) / Dsp.SampleRate;
                s[i] += 0.14f * MathF.Exp(-t * 1.6f) * lp.Process(Dsp.Saw(phase), 2500 * MathF.Exp(-t * 2) + 150);
            }
        }
        return s;
    }

    /// <summary>Blechblaeser-aehnlicher Synthesizerton: verstimmte Saegezahnwellen, Filter oeffnet beim Anblasen.</summary>
    private static void Brass(float[] s, float start, float length, float hz, float gain)
    {
        var lp = new Dsp.LowPass();
        int s0 = (int)(start * Dsp.SampleRate), n = (int)((length + 0.25f) * Dsp.SampleRate);
        for (int i = 0; i < n && s0 + i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            float vibrato = 1 + (t > 0.3f ? 0.004f * MathF.Sin(2 * MathF.PI * 5.5f * t) : 0);
            float v = Dsp.Saw(hz * vibrato * t) + Dsp.Saw(hz * 1.004f * vibrato * t) + 0.5f * Dsp.Square(hz * 0.5f * t);
            float env = Dsp.Adsr(t, length, 0.03f, 0.15f, 0.75f, 0.25f);
            s[s0 + i] += gain * env * lp.Process(v, 600 + 4500 * MathF.Min(1, t * 6) * env);
        }
    }

    private static void Boom(float[] s, float start)
    {
        var noise = new Dsp.Noise(31);
        int s0 = (int)(start * Dsp.SampleRate);
        double phase = 0;
        for (int i = 0; i < Dsp.SampleRate && s0 + i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            phase += (50 + 60 * MathF.Exp(-t * 20)) / Dsp.SampleRate;
            s[s0 + i] += MathF.Exp(-t * 4) * (0.6f * Dsp.Sine(phase) + 0.15f * noise.Next() * MathF.Exp(-t * 30));
        }
    }

    private static void Sparkle(float[] s, float start, float hz)
    {
        int s0 = (int)(start * Dsp.SampleRate);
        for (int i = 0; i < Dsp.SampleRate / 2 && s0 + i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            s[s0 + i] += 0.12f * MathF.Exp(-t * 9) * (Dsp.Sine(hz * t) + 0.4f * Dsp.Sine(hz * 2.01f * t));
        }
    }

    /// <summary>Schildblock waechst nach: elektrisches FM-Summen, Traeger 120 → 480 Hz mit FM-Obertoenen, am Ende ein kurzes Bereit-Ping.</summary>
    private static float[] ElectricFm()
    {
        const float seconds = 0.34f;
        var s = Dsp.Buffer(seconds + 0.25f);
        double carrier = 0, mod = 0;
        int n = (int)(seconds * Dsp.SampleRate);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float hz = 120 * MathF.Pow(4, t);
            mod += hz * 2.01 / Dsp.SampleRate;
            float index = 3.5f - 2.8f * t;                               // wird zum Ende reiner
            carrier += (hz + index * hz * Dsp.Sine(mod)) / Dsp.SampleRate;
            float env = MathF.Min(1, t / 0.2f) * MathF.Min(1, (1 - t) / 0.06f);
            s[i] = 0.7f * Dsp.Sine(carrier) * env;
        }
        for (int i = 0; i < (int)(0.2f * Dsp.SampleRate) && n + i < s.Length; i++)   // kurzes Ping
        {
            float t = i / (float)Dsp.SampleRate;
            s[n + i] += 0.3f * MathF.Exp(-t * 22) * Dsp.Sine(1440 * t);
        }
        return s;
    }

    /// <summary>Treffer: Rauschen mit sich schliessendem Filter, tiefer Sinus-Schlag.</summary>
    private static float[] Explosion(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(13);
        var lp = new Dsp.LowPass();
        double phase = 0;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)s.Length;
            phase += (90 * MathF.Pow(0.4f, t)) / Dsp.SampleRate;
            float env = MathF.Exp(-t * 4);
            s[i] = env * (lp.Process(noise.Next(), 4000 * MathF.Pow(0.03f, t) + 80) * 1.2f + 0.7f * Dsp.Sine(phase));
        }
        return s;
    }

    /// <summary>LG-2: zweitoeniger Warnton.</summary>
    private static float[] Alarm(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var lp = new Dsp.LowPass();
        double phase = 0;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            float hz = (int)(t * 6) % 2 == 0 ? 880 : 660;
            phase += hz / Dsp.SampleRate;
            s[i] = 0.5f * lp.Process(Dsp.Square(phase), 2500);
        }
        return s;
    }

    private static float[] Thunder(float seconds)
    {
        var s = Dsp.Buffer(seconds);
        var noise = new Dsp.Noise(17);
        var lp = new Dsp.LowPass();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)s.Length;
            float crack = MathF.Exp(-t * 25) * noise.Next();
            float rumble = lp.Process(noise.Next(), 300) * 2.5f * MathF.Exp(-t * 3);
            s[i] = crack + rumble;
        }
        return s;
    }

    private static float[] Beep(float hz, float seconds, float volume = 0.6f)
    {
        var s = Dsp.Buffer(seconds);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Dsp.SampleRate;
            s[i] = volume * Dsp.Sine(hz * t) * MathF.Min(1, (seconds - t) * 40);
        }
        return s;
    }
}
