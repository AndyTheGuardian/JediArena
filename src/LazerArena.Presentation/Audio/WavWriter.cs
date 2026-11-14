namespace LazerArena.Presentation.Audio;

/// <summary>Schreibt einen Klang als WAV-Datei (zum Anhoeren am Rechner).</summary>
public static class WavWriter
{
    public static void Write(AudioClip clip, string path)
    {
        using var w = new BinaryWriter(File.Create(path));
        int dataBytes = clip.Samples.Length * 2;
        w.Write("RIFF"u8); w.Write(36 + dataBytes); w.Write("WAVE"u8);
        w.Write("fmt "u8); w.Write(16); w.Write((short)1); w.Write((short)1);
        w.Write(clip.SampleRate); w.Write(clip.SampleRate * 2); w.Write((short)2); w.Write((short)16);
        w.Write("data"u8); w.Write(dataBytes);
        w.Write(clip.ToBytes());
    }
}
