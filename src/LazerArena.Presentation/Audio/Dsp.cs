namespace LazerArena.Presentation.Audio;

/// <summary>Kleine Bausteine der Klangsynthese (moderner Stil). Alle Funktionen sind deterministisch.</summary>
internal static class Dsp
{
    public const int SampleRate = 22050;

    public static float[] Buffer(float seconds) => new float[(int)(seconds * SampleRate)];

    public static float Saw(double phase) => (float)(2 * (phase - Math.Floor(phase + 0.5)));
    public static float Square(double phase, double duty = 0.5) => phase - Math.Floor(phase) < duty ? 1f : -1f;
    public static float Sine(double phase) => MathF.Sin((float)(phase * 2 * Math.PI));
    public static float Triangle(double phase) => 1 - 4 * MathF.Abs((float)(phase - Math.Floor(phase + 0.5)));

    public static float NoteToHz(int midi) => 440f * MathF.Pow(2, (midi - 69) / 12f);

    /// <summary>Huellkurve: Anstieg, Haltepegel, Ausklingen (Sekunden) relativ zur Notenlaenge.</summary>
    public static float Adsr(float t, float length, float attack, float decay, float sustain, float release)
    {
        if (t < attack) return t / attack;
        if (t < attack + decay) return 1 - (1 - sustain) * (t - attack) / decay;
        if (t < length) return sustain;
        return MathF.Max(0, sustain * (1 - (t - length) / release));
    }

    /// <summary>Einfacher Tiefpass (Einpol-Filter), Grenzfrequenz darf sich je Abtastwert aendern.</summary>
    public sealed class LowPass
    {
        private float _state;
        public float Process(float x, float cutoffHz)
        {
            float a = 1 - MathF.Exp(-2 * MathF.PI * cutoffHz / SampleRate);
            _state += a * (x - _state);
            return _state;
        }
    }

    /// <summary>Rauschen mit festem Startwert.</summary>
    public sealed class Noise(int seed)
    {
        private uint _state = (uint)seed | 1u;
        public float Next()
        {
            _state ^= _state << 13; _state ^= _state >> 17; _state ^= _state << 5;
            return _state / (float)uint.MaxValue * 2 - 1;
        }
    }

    /// <summary>Rueckgekoppeltes Echo, im Puffer angewendet.</summary>
    public static void Echo(float[] signal, float delaySeconds, float feedback, float mix)
    {
        int d = (int)(delaySeconds * SampleRate);
        for (int i = d; i < signal.Length; i++)
            signal[i] += signal[i - d] * feedback * mix;
    }

    /// <summary>Kurzes Aus- und Einblenden gegen Knackser.</summary>
    public static void Fade(float[] signal, float seconds = 0.004f)
    {
        int n = Math.Min(signal.Length / 2, (int)(seconds * SampleRate));
        for (int i = 0; i < n; i++)
        {
            float g = i / (float)n;
            signal[i] *= g;
            signal[^(i + 1)] *= g;
        }
    }
}
