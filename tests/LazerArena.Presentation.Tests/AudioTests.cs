using LazerArena.Presentation.Settings;
using LazerArena.Core;
using LazerArena.Presentation.Audio;

namespace LazerArena.Presentation.Tests;

public class AudioTests
{
    private static readonly ClassicSoundBank Classic = new();
    private static readonly ModernSoundBank Modern = new();

    public static TheoryData<SoundId> AllSounds() => new(Enum.GetValues<SoundId>());

    [Fact, Trait("Regel", "F5")]
    public void JedesSpielereignisHatEinenKlang()
    {
        foreach (var type in Enum.GetValues<GameEventType>())
            Assert.NotNull(SoundEvents.For(new GameEvent(type)));
    }

    [Fact, Trait("Regel", "RS-4")]
    public void RS4_DreiGleichePieptoeneUndEinHoehererTonBeiGo()
    {
        Assert.Equal(SoundId.Countdown, SoundEvents.For(new GameEvent(GameEventType.CountdownStarted)));
        Assert.Equal(SoundId.Countdown, SoundEvents.For(new GameEvent(GameEventType.CountdownTwo)));
        Assert.Equal(SoundId.Countdown, SoundEvents.For(new GameEvent(GameEventType.CountdownOne)));
        Assert.Equal(SoundId.RoundStart, SoundEvents.For(new GameEvent(GameEventType.RoundStarted)));
        Assert.True(ClassicSoundBank.Script(SoundId.RoundStart)[0].Divider < ClassicSoundBank.Script(SoundId.Countdown)[0].Divider);
        Assert.True(ClassicSoundBank.Script(SoundId.RoundStart).Count > ClassicSoundBank.Script(SoundId.Countdown).Count);
        var modern = new ModernSoundBank();
        Assert.True(ZeroCrossingsPerSample(modern.Get(SoundId.RoundStart).Samples) > 1.5 * ZeroCrossingsPerSample(modern.Get(SoundId.Countdown).Samples));
    }

    private static double ZeroCrossingsPerSample(short[] s)
    {
        int crossings = 0;
        for (int i = 1; i < s.Length; i++) if ((s[i - 1] < 0) != (s[i] < 0)) crossings++;
        return crossings / (double)s.Length;
    }

    [Fact, Trait("Regel", "LG-2")]
    public void LG2_AnkuendigungDesGewittersHatEinenWarnton() =>
        Assert.Equal(SoundId.StormWarning, SoundEvents.For(new GameEvent(GameEventType.StormWarning)));

    [Theory, Trait("Regel", "F5")]
    [MemberData(nameof(AllSounds))]
    public void BeideStile_LiefernHoerbareKurzeKlaenge(SoundId id)
    {
        foreach (var bank in new ISoundBank[] { Classic, Modern })
        {
            var clip = bank.Get(id);
            Assert.InRange(clip.DurationSeconds, 0.04f, 4.5f);
            Assert.True(clip.Samples.Max(s => Math.Abs((int)s)) > 2000, $"{id} zu leise");
        }
    }

    [Theory, Trait("Regel", "F5")]
    [MemberData(nameof(AllSounds))]
    public void Klassisch_NurSechzehnLautstaerkestufenWieAufDemTonchip(SoundId id)
    {
        var levels = Classic.Get(id).Samples.Select(s => Math.Abs((int)s)).Distinct();
        Assert.All(levels, l => Assert.True(l % 1000 == 0 && l <= 15000, $"Pegel {l}"));
    }

    [Fact, Trait("Regel", "F5"), Trait("Regel", "OP-8")]
    public void Titelmusik_NurImModernenStil_EtwaVierunddreissigSekunden()
    {
        Assert.Null(Classic.Music(SongId.Title));
        var music = Modern.Music(SongId.Title)!;
        Assert.InRange(music.DurationSeconds, Songs.Title.LengthSeconds - 0.01f, Songs.Title.LengthSeconds + 0.01f);
        Assert.InRange(Songs.Title.LengthSeconds, 34f, 35f);
        Assert.InRange(music.Samples.Max(s => Math.Abs((int)s)), 20000, 32767 * 0.86);
    }

    [Theory, Trait("Regel", "OP-10")]
    [InlineData(SongId.Orbit)]
    [InlineData(SongId.Nebula)]
    [InlineData(SongId.Hyperdrive)]
    [InlineData(SongId.SunsetRun)]
    [InlineData(SongId.LaserNights)]
    public void Spielmusik_DreiEigeneStuecke_NurImModernenStil(SongId id)
    {
        Assert.Null(Classic.Music(id));
        var song = Songs.For(id);
        var music = Modern.Music(id)!;
        Assert.InRange(music.DurationSeconds, song.LengthSeconds - 0.01f, song.LengthSeconds + 0.01f);
        Assert.Equal(song.Bars, song.Melody.Length);
        Assert.InRange(music.Samples.Max(s => Math.Abs((int)s)), 20000, 32767 * 0.86);
    }

    [Fact, Trait("Regel", "OP-10")]
    public void Spielmusik_AlleStueckeUnterscheidenSich()
    {
        var songs = Enum.GetValues<SongId>().Select(Songs.For).ToList();
        Assert.Equal(songs.Count, songs.Select(s => s.Bpm).Distinct().Count());
        Assert.Equal(songs.Count, songs.Select(s => s.Name).Distinct().Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Andockklang_IstEineNahtloseSchleife(bool modern)
    {
        var clip = (modern ? (ISoundBank)Modern : Classic).Get(SoundId.DockLoop);
        Assert.True(clip.DurationSeconds >= 0.4f);
        if (modern)   // kein Sprung zwischen Ende und Anfang
            Assert.True(Math.Abs(clip.Samples[^1] - clip.Samples[0]) < 6000);
    }

    [Fact]
    public void Klangerzeugung_IstDeterministisch()
    {
        Assert.Equal(new ModernSoundBank().Get(SoundId.Fire).Samples, new ModernSoundBank().Get(SoundId.Fire).Samples);
        Assert.Equal(ClassicSoundBank.Render(ClassicSoundBank.Script(SoundId.PlayerHit)).Samples,
            ClassicSoundBank.Render(ClassicSoundBank.Script(SoundId.PlayerHit)).Samples);
    }

    [Fact]
    public void Wav_HatKorrektenKopf()
    {
        string path = Path.Combine(Path.GetTempPath(), $"lazer-{Guid.NewGuid():N}.wav");
        try
        {
            var clip = Classic.Get(SoundId.MenuClick);
            WavWriter.Write(clip, path);
            var bytes = File.ReadAllBytes(path);
            Assert.Equal("RIFF"u8.ToArray(), bytes[..4]);
            Assert.Equal(44 + clip.Samples.Length * 2, bytes.Length);
            Assert.Equal(22050, BitConverter.ToInt32(bytes, 24));
        }
        finally { File.Delete(path); }
    }
}

public class VoiceTests
{
    [Theory, Trait("Regel", "OP-13")]
    [InlineData(VoiceKind.Computer, "de")]
    [InlineData(VoiceKind.Computer, "en")]
    [InlineData(VoiceKind.Soft, "de")]
    [InlineData(VoiceKind.Soft, "en")]
    public void OP13_AlleAnsagenSindJeStimmeInBeidenSprachenVorhanden(VoiceKind voice, string language)
    {
        foreach (var line in Enum.GetValues<VoiceLine>())
        {
            var clip = VoiceBank.Get(voice, language, line);
            Assert.InRange(clip.DurationSeconds, 0.2f, line == VoiceLine.Advanced ? 3.5f : 1.5f);
            Assert.True(clip.Samples.Max(s => Math.Abs((int)s)) > 10000, $"{language}/{line} zu leise");
        }
    }

    [Fact, Trait("Regel", "OP-13")]
    public void OP13_StimmeErsetztCountdownPieptoene_BeiTrefferUndSiegZusaetzlich()
    {
        var three = AudioCue.From(new GameEvent(GameEventType.CountdownStarted));
        Assert.Equal(VoiceLine.Three, three.Voice);
        Assert.True(three.VoiceReplacesSound);
        Assert.Equal(VoiceLine.One, AudioCue.From(new GameEvent(GameEventType.CountdownOne)).Voice);
        var go = AudioCue.From(new GameEvent(GameEventType.RoundStarted));
        Assert.Equal(VoiceLine.Go, go.Voice);
        Assert.True(go.VoiceReplacesSound);
        var hit = AudioCue.From(new GameEvent(GameEventType.PlayerHit));
        Assert.Equal(VoiceLine.Hit, hit.Voice);
        Assert.False(hit.VoiceReplacesSound);
        Assert.Equal(VoiceLine.Victory, AudioCue.Victory(humanWon: true).Voice);
        Assert.Equal(VoiceLine.Defeat, AudioCue.Victory(humanWon: false).Voice);
        Assert.Null(AudioCue.From(new GameEvent(GameEventType.BoltFired)).Voice);
    }

    [Fact, Trait("Regel", "OP-13")]
    public void OP13_StimmeIstWaehlbarUndAbschaltbar_InBeidenStilen()
    {
        Assert.Equal(VoiceKind.Computer, new LazerArena.Presentation.Settings.UserSettings().VoiceStyle);
        Assert.False(new LazerArena.Presentation.Settings.UserSettings { VoiceStyle = VoiceKind.Off }.Voice);
        var entry = LazerArena.Presentation.Rendering.OptionsMenu.Entries(new LazerArena.Presentation.Settings.UserSettings())
            .Single(e => e.LabelKey == "opt.voice");
        var s = new LazerArena.Presentation.Settings.UserSettings();
        var keys = new List<string>();
        for (int i = 0; i < 3; i++) { keys.Add(entry.ValueKey(s)); s = entry.Step(s, 1); }
        Assert.Equal(["val.voiceComputer", "val.voiceSoft", "val.off"], keys);
        foreach (var style in new[] { LazerArena.Presentation.Settings.GraphicsStyle.Classic, LazerArena.Presentation.Settings.GraphicsStyle.Modern })
            Assert.Contains(LazerArena.Presentation.Rendering.OptionsMenu.Entries(new LazerArena.Presentation.Settings.UserSettings { Style = style }),
                e => e.LabelKey == "opt.voice");
    }
}
