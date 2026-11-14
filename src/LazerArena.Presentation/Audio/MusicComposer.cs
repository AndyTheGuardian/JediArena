namespace LazerArena.Presentation.Audio;

public enum SongId { Title, Orbit, Nebula, Hyperdrive, SunsetRun, LaserNights }

public enum DrumStyle { Standard, FourOnTheFloor, HalfTime }

/// <summary>Bassfigur: Achtel mit Oktavsprung oder rollende Sechzehntel (typisch fuer 80er-Synthwave).</summary>
public enum BassStyle { Eighths, RollingSixteenths }

/// <summary>Daten eines Musikstuecks: Tempo, Akkord je Takt, Melodie je Takt, Arrangement.</summary>
public sealed record Song(
    string Name, float Bpm, Song.Chord[] Progression, Song.Note[][] Melody,
    int ArpFromBar, DrumStyle Drums, float LeadGain, float PadGain = 1f,
    BassStyle Bass = BassStyle.Eighths, bool GatedSnare = false)
{
    public readonly record struct Chord(int Root, bool Minor);
    public readonly record struct Note(float Beat, float Length, int Midi);

    public int Bars => Progression.Length;
    public float BeatSeconds => 60f / Bpm;
    public float LengthSeconds => Bars * 4 * BeatSeconds;
}

/// <summary>Die eigenen Stuecke des modernen Stils (F4, F11): Titelmusik und drei waehlbare Spielmusiken (OP-10).</summary>
public static class Songs
{
    public static Song For(SongId id) => id switch
    {
        SongId.Title => Title,
        SongId.Orbit => Orbit,
        SongId.Nebula => Nebula,
        SongId.Hyperdrive => Hyperdrive,
        SongId.SunsetRun => SunsetRun,
        SongId.LaserNights => LaserNights,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Titelmusik: Synthwave in a-Moll, 112 BPM, 16 Takte.</summary>
    public static readonly Song Title = new("Titel", 112,
    [
        new(45, true), new(41, false), new(36, false), new(43, false), new(45, true), new(41, false), new(43, false), new(40, false),
        new(41, false), new(43, false), new(40, true), new(45, true), new(38, true), new(43, false), new(40, false), new(40, false),
    ],
    [
        [new(0, 1.5f, 69), new(1.5f, 0.5f, 72), new(2, 1, 76), new(3, 1, 74)],
        [new(0, 1.5f, 72), new(1.5f, 0.5f, 69), new(2, 2, 65)],
        [new(0, 1, 67), new(1, 1, 72), new(2, 1, 76), new(3, 1, 79)],
        [new(0, 3, 74), new(3, 1, 71)],
        [new(0, 1.5f, 69), new(1.5f, 0.5f, 72), new(2, 1, 76), new(3, 1, 81)],
        [new(0, 1, 77), new(1, 1, 76), new(2, 2, 72)],
        [new(0, 1, 74), new(1, 1, 71), new(2, 1, 67), new(3, 1, 71)],
        [new(0, 2, 68), new(2, 2, 71)],
        [new(0, 1, 81), new(1, 0.5f, 79), new(1.5f, 0.5f, 77), new(2, 2, 76)],
        [new(0, 1, 79), new(1, 0.5f, 77), new(1.5f, 0.5f, 76), new(2, 2, 74)],
        [new(0, 1, 76), new(1, 1, 79), new(2, 1, 83), new(3, 1, 79)],
        [new(0, 3, 81), new(3, 1, 76)],
        [new(0, 1, 77), new(1, 1, 74), new(2, 1, 69), new(3, 1, 74)],
        [new(0, 1, 79), new(1, 1, 74), new(2, 1, 71), new(3, 1, 74)],
        [new(0, 2, 76), new(2, 1, 80), new(3, 1, 83)],
        [new(0, 4, 80)],
    ], ArpFromBar: 8, Drums: DrumStyle.Standard, LeadGain: 1f);

    /// <summary>Spielmusik „Orbit“: treibend in d-Moll, 124 BPM, durchgehendes Arpeggio, Melodie leiser (Effekte bleiben hoerbar).</summary>
    public static readonly Song Orbit = new("Orbit", 124,
    [
        new(38, true), new(46, false), new(36, false), new(45, true), new(38, true), new(46, false), new(43, true), new(45, false),
        new(46, false), new(36, false), new(38, true), new(38, true), new(43, true), new(46, false), new(45, false), new(45, false),
    ],
    [
        [new(0, 0.5f, 74), new(0.5f, 0.5f, 77), new(1, 1, 81), new(2, 0.5f, 79), new(2.5f, 0.5f, 77), new(3, 1, 74)],
        [new(0, 1.5f, 77), new(1.5f, 0.5f, 74), new(2, 2, 70)],
        [new(0, 0.5f, 72), new(0.5f, 0.5f, 76), new(1, 1, 79), new(2, 1, 84), new(3, 1, 79)],
        [new(0, 3, 81), new(3, 1, 76)],
        [new(0, 0.5f, 74), new(0.5f, 0.5f, 77), new(1, 1, 81), new(2, 0.5f, 79), new(2.5f, 0.5f, 77), new(3, 1, 74)],
        [new(0, 1, 77), new(1, 1, 74), new(2, 1, 77), new(3, 1, 82)],
        [new(0, 2, 79), new(2, 1, 74), new(3, 1, 70)],
        [new(0, 2, 73), new(2, 2, 76)],
        [new(0, 1, 82), new(1, 1, 81), new(2, 2, 77)],
        [new(0, 1, 79), new(1, 1, 77), new(2, 2, 76)],
        [new(0, 0.5f, 74), new(0.5f, 0.5f, 77), new(1, 0.5f, 81), new(1.5f, 0.5f, 86), new(2, 2, 84)],
        [new(0, 4, 81)],
        [new(0, 1, 79), new(1, 1, 82), new(2, 1, 86), new(3, 1, 82)],
        [new(0, 2, 81), new(2, 2, 77)],
        [new(0, 1, 76), new(1, 1, 79), new(2, 1, 81), new(3, 1, 85)],
        [new(0, 4, 81)],
    ], ArpFromBar: 0, Drums: DrumStyle.FourOnTheFloor, LeadGain: 0.6f);

    /// <summary>Spielmusik „Nebula“: ruhig und schwebend in e-Moll, 92 BPM, Halbzeit-Schlagzeug, breite Flaechen.</summary>
    public static readonly Song Nebula = new("Nebula", 92,
    [
        new(40, true), new(36, false), new(43, false), new(38, false), new(40, true), new(36, false), new(45, true), new(47, false),
        new(36, false), new(43, false), new(38, false), new(40, true), new(45, true), new(36, false), new(47, false), new(47, false),
    ],
    [
        [new(0, 2, 79), new(2, 2, 83)],
        [new(0, 3, 84), new(3, 1, 79)],
        [new(0, 2, 83), new(2, 1, 81), new(3, 1, 79)],
        [new(0, 4, 78)],
        [new(0, 1, 76), new(1, 1, 79), new(2, 2, 83)],
        [new(0, 2, 88), new(2, 2, 84)],
        [new(0, 2, 84), new(2, 1, 83), new(3, 1, 81)],
        [new(0, 4, 78)],
        [new(0, 2, 76), new(2, 2, 79)],
        [new(0, 3, 79), new(3, 1, 83)],
        [new(0, 2, 81), new(2, 2, 78)],
        [new(0, 4, 76)],
        [new(0, 2, 72), new(2, 2, 76)],
        [new(0, 2, 79), new(2, 2, 84)],
        [new(0, 2, 83), new(2, 2, 75)],
        [new(0, 4, 78)],
    ], ArpFromBar: 0, Drums: DrumStyle.HalfTime, LeadGain: 0.7f, PadGain: 1.6f);

    /// <summary>Spielmusik „Hyperdrive“: schnell in g-Moll, 140 BPM, Viertel-Kick, dichte Melodie.</summary>
    public static readonly Song Hyperdrive = new("Hyperdrive", 140,
    [
        new(43, true), new(39, false), new(41, false), new(43, true), new(43, true), new(39, false), new(36, true), new(38, false),
        new(39, false), new(41, false), new(43, true), new(43, true), new(36, true), new(39, false), new(38, false), new(38, false),
    ],
    [
        [new(0, 0.5f, 79), new(0.5f, 0.5f, 82), new(1, 0.5f, 86), new(1.5f, 0.5f, 82), new(2, 1, 79), new(3, 1, 77)],
        [new(0, 0.5f, 79), new(0.5f, 0.5f, 82), new(1, 1, 87), new(2, 2, 86)],
        [new(0, 0.5f, 84), new(0.5f, 0.5f, 81), new(1, 1, 77), new(2, 1, 81), new(3, 1, 84)],
        [new(0, 3, 86), new(3, 1, 82)],
        [new(0, 0.5f, 79), new(0.5f, 0.5f, 82), new(1, 0.5f, 86), new(1.5f, 0.5f, 82), new(2, 1, 79), new(3, 1, 77)],
        [new(0, 1, 87), new(1, 1, 86), new(2, 1, 84), new(3, 1, 82)],
        [new(0, 2, 84), new(2, 1, 87), new(3, 1, 84)],
        [new(0, 2, 81), new(2, 2, 78)],
        [new(0, 1, 82), new(1, 1, 87), new(2, 2, 91)],
        [new(0, 1, 89), new(1, 1, 87), new(2, 2, 84)],
        [new(0, 0.5f, 86), new(0.5f, 0.5f, 87), new(1, 0.5f, 86), new(1.5f, 0.5f, 84), new(2, 2, 82)],
        [new(0, 4, 79)],
        [new(0, 1, 84), new(1, 1, 87), new(2, 1, 91), new(3, 1, 87)],
        [new(0, 2, 87), new(2, 2, 82)],
        [new(0, 1, 81), new(1, 1, 86), new(2, 1, 90), new(3, 1, 86)],
        [new(0, 4, 90)],
    ], ArpFromBar: 0, Drums: DrumStyle.FourOnTheFloor, LeadGain: 0.6f);

    /// <summary>„Sunset Run“: beschwingter 80er-Synthwave, c-Moll mit Es-Dur-Faerbung, 118 BPM, rollender Bass, Gated-Snare.</summary>
    public static readonly Song SunsetRun = new("Sunset Run", 118,
    [
        new(36, true), new(44, false), new(39, false), new(46, false), new(36, true), new(44, false), new(39, false), new(46, false),
        new(44, false), new(46, false), new(36, true), new(36, true), new(44, false), new(46, false), new(43, false), new(43, false),
    ],
    [
        [new(0, 1, 79), new(1, 0.5f, 82), new(1.5f, 0.5f, 84), new(2, 1.5f, 87), new(3.5f, 0.5f, 84)],
        [new(0, 1, 84), new(1, 1, 80), new(2, 2, 77)],
        [new(0, 0.5f, 79), new(0.5f, 0.5f, 82), new(1, 1, 87), new(2, 1, 86), new(3, 1, 82)],
        [new(0, 3, 82), new(3, 1, 77)],
        [new(0, 1, 79), new(1, 0.5f, 82), new(1.5f, 0.5f, 84), new(2, 1.5f, 87), new(3.5f, 0.5f, 84)],
        [new(0, 1, 84), new(1, 1, 87), new(2, 2, 89)],
        [new(0, 1, 87), new(1, 1, 86), new(2, 1, 84), new(3, 1, 82)],
        [new(0, 4, 86)],
        [new(0, 2, 84), new(2, 1, 80), new(3, 1, 84)],
        [new(0, 2, 86), new(2, 1, 82), new(3, 1, 86)],
        [new(0, 1, 87), new(1, 1, 84), new(2, 1, 79), new(3, 1, 84)],
        [new(0, 4, 87)],
        [new(0, 1, 89), new(1, 1, 87), new(2, 2, 84)],
        [new(0, 1, 86), new(1, 1, 84), new(2, 2, 82)],
        [new(0, 2, 83), new(2, 2, 86)],
        [new(0, 4, 91)],
    ], ArpFromBar: 4, Drums: DrumStyle.Standard, LeadGain: 0.65f, PadGain: 1.3f,
       Bass: BassStyle.RollingSixteenths, GatedSnare: true);

    /// <summary>„Laser Nights“: treibender 80er-Synthwave, h-Moll, 126 BPM, Viertel-Kick, rollender Bass, Gated-Snare.</summary>
    public static readonly Song LaserNights = new("Laser Nights", 126,
    [
        new(47, true), new(43, false), new(38, false), new(45, false), new(47, true), new(43, false), new(38, false), new(45, false),
        new(40, true), new(43, false), new(38, false), new(45, false), new(40, true), new(43, false), new(45, false), new(45, false),
    ],
    [
        [new(0, 0.5f, 78), new(0.5f, 0.5f, 81), new(1, 1, 83), new(2, 0.5f, 81), new(2.5f, 0.5f, 78), new(3, 1, 76)],
        [new(0, 1.5f, 79), new(1.5f, 0.5f, 78), new(2, 2, 74)],
        [new(0, 0.5f, 74), new(0.5f, 0.5f, 78), new(1, 1, 81), new(2, 1, 86), new(3, 1, 81)],
        [new(0, 3, 85), new(3, 1, 81)],
        [new(0, 0.5f, 78), new(0.5f, 0.5f, 81), new(1, 1, 83), new(2, 0.5f, 81), new(2.5f, 0.5f, 78), new(3, 1, 76)],
        [new(0, 1, 83), new(1, 1, 86), new(2, 2, 88)],
        [new(0, 1, 86), new(1, 1, 85), new(2, 1, 83), new(3, 1, 81)],
        [new(0, 4, 85)],
        [new(0, 1, 83), new(1, 1, 79), new(2, 2, 76)],
        [new(0, 1, 79), new(1, 1, 83), new(2, 2, 86)],
        [new(0, 0.5f, 86), new(0.5f, 0.5f, 85), new(1, 1, 83), new(2, 2, 81)],
        [new(0, 4, 85)],
        [new(0, 1, 88), new(1, 1, 86), new(2, 1, 83), new(3, 1, 79)],
        [new(0, 1, 83), new(1, 1, 86), new(2, 2, 91)],
        [new(0, 2, 88), new(2, 2, 85)],
        [new(0, 4, 90)],
    ], ArpFromBar: 0, Drums: DrumStyle.FourOnTheFloor, LeadGain: 0.6f, PadGain: 1.2f,
       Bass: BassStyle.RollingSixteenths, GatedSnare: true);
}

/// <summary>
/// Erzeugt ein <see cref="Song"/> synthetisch: Bass, Flaechenakkorde, Melodie mit Echo, Arpeggio, Schlagzeug.
/// Das Ergebnis ist nahtlos wiederholbar (Nachhall wird an den Anfang gefaltet).
/// </summary>
public static class MusicComposer
{
    public static AudioClip Compose(Song song)
    {
        float beat = song.BeatSeconds;
        int length = (int)(song.LengthSeconds * Dsp.SampleRate);
        var mix = new float[length + Dsp.SampleRate * 2];

        for (int bar = 0; bar < song.Bars; bar++)
        {
            float barStart = bar * 4 * beat;
            var chord = song.Progression[bar];
            int[] triad = [chord.Root, chord.Root + (chord.Minor ? 3 : 4), chord.Root + 7];
            bool secondHalf = bar >= song.Bars / 2;

            if (song.Bass == BassStyle.RollingSixteenths)   // rollende Sechzehntel: Grundton, Grundton, Oktave …
                for (int e = 0; e < 16; e++)
                    AddBass(mix, barStart + e * beat / 4, beat / 4 * 0.8f, chord.Root - 12 + (e % 4 == 2 ? 12 : 0));
            else
                for (int e = 0; e < 8; e++)   // Bass: Achtel, jede zweite eine Oktave hoeher
                    AddBass(mix, barStart + e * beat / 2, beat / 2 * 0.9f, chord.Root - 12 + (e % 2 == 1 ? 12 : 0));

            foreach (int n in triad)
                AddPad(mix, barStart, 4 * beat, n + 24, song.PadGain);

            foreach (var note in song.Melody[bar])
                AddLead(mix, barStart + note.Beat * beat, note.Length * beat, note.Midi, song.LeadGain, beat);

            if (bar >= song.ArpFromBar)   // Arpeggio in Sechzehnteln
                for (int s = 0; s < 16; s++)
                    AddArp(mix, barStart + s * beat / 4, beat / 4 * 0.8f, triad[s % 3] + 36 + (s % 6 >= 3 ? 12 : 0));

            for (int b = 0; b < 4; b++)
            {
                float t = barStart + b * beat;
                bool kick = song.Drums switch
                {
                    DrumStyle.FourOnTheFloor => true,
                    DrumStyle.HalfTime => b == 0,
                    _ => b % 2 == 0,
                };
                bool snare = song.Drums == DrumStyle.HalfTime ? b == 2 : b % 2 == 1;
                if (kick) AddKick(mix, t);
                if (snare && song.GatedSnare) AddGatedSnare(mix, t, seed: bar * 4 + b);
                else if (snare) AddSnare(mix, t, seed: bar * 4 + b);
                AddHat(mix, t, open: false, seed: bar * 8 + b * 2);
                if (song.Drums != DrumStyle.HalfTime)
                    AddHat(mix, t + beat / 2, open: secondHalf || song.Drums == DrumStyle.FourOnTheFloor, seed: bar * 8 + b * 2 + 1);
            }
            if (bar % 4 == 3 && song.Drums == DrumStyle.Standard) AddKick(mix, barStart + 3.5f * beat);
        }

        var loop = new float[length];
        Array.Copy(mix, loop, length);
        for (int i = length; i < mix.Length; i++) loop[i - length] += mix[i];
        return AudioClip.FromFloats(loop, Dsp.SampleRate, peak: 0.85f);
    }

    private static void ForEachSample(float[] mix, float start, float seconds, Action<int, float> write)
    {
        int s0 = (int)(start * Dsp.SampleRate), n = (int)(seconds * Dsp.SampleRate);
        for (int i = 0; i < n && s0 + i < mix.Length; i++) write(s0 + i, i / (float)Dsp.SampleRate);
    }

    private static void AddBass(float[] mix, float start, float length, int midi)
    {
        float hz = Dsp.NoteToHz(midi);
        var lp = new Dsp.LowPass();
        ForEachSample(mix, start, length + 0.05f, (i, t) =>
        {
            float env = Dsp.Adsr(t, length, 0.005f, 0.12f, 0.6f, 0.05f);
            mix[i] += 0.32f * env * lp.Process(Dsp.Saw(hz * t) + 0.5f * Dsp.Square(hz * t * 0.5), 300 + 1500 * MathF.Exp(-t * 18));
        });
    }

    private static void AddPad(float[] mix, float start, float length, int midi, float gain)
    {
        float hz = Dsp.NoteToHz(midi);
        var lp = new Dsp.LowPass();
        ForEachSample(mix, start, length + 0.4f, (i, t) =>
        {
            float env = Dsp.Adsr(t, length, 0.35f, 0.5f, 0.8f, 0.4f);
            float v = Dsp.Saw(hz * t) + Dsp.Saw(hz * 1.006f * t) + Dsp.Saw(hz * 0.994f * t);
            mix[i] += 0.05f * gain * env * lp.Process(v, 1400);
        });
    }

    private static void AddLead(float[] mix, float start, float length, int midi, float gain0, float beat)
    {
        float hz = Dsp.NoteToHz(midi);
        var lp = new Dsp.LowPass();
        float echo = beat * 0.75f;   // punktierte Achtel
        for (int tap = 0; tap < 3; tap++)
        {
            float gain = gain0 * MathF.Pow(0.4f, tap);
            double phase = 0;
            ForEachSample(mix, start + tap * echo, length + 0.08f, (i, t) =>
            {
                float vibrato = t > 0.15f ? 1 + 0.006f * MathF.Sin(2 * MathF.PI * 5.5f * t) : 1;
                phase += hz * vibrato / Dsp.SampleRate;
                float env = Dsp.Adsr(t, length, 0.01f, 0.1f, 0.7f, 0.08f);
                mix[i] += 0.17f * gain * env * lp.Process(0.6f * Dsp.Square(phase) + 0.4f * Dsp.Saw(phase), 3200);
            });
        }
    }

    private static void AddArp(float[] mix, float start, float length, int midi)
    {
        float hz = Dsp.NoteToHz(midi);
        ForEachSample(mix, start, length, (i, t) =>
            mix[i] += 0.05f * MathF.Exp(-t * 22) * Dsp.Square(hz * t, 0.25));
    }

    private static void AddKick(float[] mix, float start)
    {
        double phase = 0;
        ForEachSample(mix, start, 0.3f, (i, t) =>
        {
            phase += (45 + 110 * MathF.Exp(-t * 35)) / Dsp.SampleRate;
            mix[i] += 0.55f * MathF.Exp(-t * 9) * Dsp.Sine(phase);
        });
    }

    private static void AddSnare(float[] mix, float start, int seed)
    {
        var noise = new Dsp.Noise(seed + 101);
        var lp = new Dsp.LowPass();
        ForEachSample(mix, start, 0.22f, (i, t) =>
            mix[i] += MathF.Exp(-t * 18) * (0.22f * lp.Process(noise.Next(), 5000) + 0.12f * Dsp.Sine(190 * t)));
    }

    /// <summary>80er-„Gated Reverb“-Snare: kraeftiger Rauschschlag mit Hall, der nach kurzer Zeit hart abgeschnitten wird.</summary>
    private static void AddGatedSnare(float[] mix, float start, int seed)
    {
        var noise = new Dsp.Noise(seed + 303);
        var lp = new Dsp.LowPass();
        const float gate = 0.16f;
        ForEachSample(mix, start, gate + 0.01f, (i, t) =>
        {
            float body = 0.16f * MathF.Exp(-t * 25) * Dsp.Sine(180 * t);
            float verb = 0.2f * (0.55f + 0.45f * MathF.Exp(-t * 10)) * lp.Process(noise.Next(), 6500);
            float cut = t < gate ? 1 : MathF.Max(0, 1 - (t - gate) / 0.01f);
            mix[i] += (body + verb) * cut;
        });
    }

    private static void AddHat(float[] mix, float start, bool open, int seed)
    {
        var noise = new Dsp.Noise(seed + 977);
        float last = 0;
        ForEachSample(mix, start, open ? 0.18f : 0.05f, (i, t) =>
        {
            float n = noise.Next();
            float high = n - last;   // einfacher Hochpass
            last = n;
            mix[i] += 0.06f * MathF.Exp(-t * (open ? 14 : 60)) * high;
        });
    }
}
