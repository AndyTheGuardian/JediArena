"""Erzeugt die Sprachansagen der weiblichen Computerstimmen (Deutsch/Englisch) mit der Sprachsynthese eSpeak NG
(Systembibliothek libespeak-ng). Die WAV-Dateien werden ins Projekt eingebettet; auf dem Handy wird keine
Sprachsynthese benoetigt (OP-13).

- Stimme "Computer": Variante f4, wenig Satzmelodie, Computer-Klang (Ringmodulation, metallischer Kamm, 6 Bit).
- Stimme "Sanft": hauchige Variante f5, tiefer und langsamer, mehr Melodie, weicher Tiefpass, leichter Hall und nur
  ein Hauch Computer-Kamm. eSpeak NG ist eine Formant-Synthese; wirklich verfuehrerisch klingt sie nicht.

Aufruf: python3 tools/voice/make_voice.py [zielordner] [--only=Warning,Advanced] [--profile=soft]
"""
import ctypes
import math
import struct
import sys
import wave
from pathlib import Path

LINES = {
    "en": {"Three": "Three", "Two": "Two", "One": "One", "Go": "Go!", "Hit": "Hit!", "Victory": "Victory!", "Defeat": "Defeat.",
           "Warning": "Warning!", "Advanced": "Advanced options selected."},
    "de": {"Three": "Drei", "Two": "Zwei", "One": "Eins", "Go": "Los!", "Hit": "Treffer!", "Victory": "Sieg!", "Defeat": "Niederlage.",
           "Warning": "Warnung!", "Advanced": "Erweiterte Optionen ausgewählt."},
}
TARGET_RATE = 22050

lib = ctypes.CDLL("libespeak-ng.so.1")
CALLBACK = ctypes.CFUNCTYPE(ctypes.c_int, ctypes.POINTER(ctypes.c_short), ctypes.c_int, ctypes.c_void_p)
samples: list[int] = []


@CALLBACK
def collect(wav, count, events):
    if wav and count > 0:
        samples.extend(wav[i] for i in range(count))
    return 0


AUDIO_OUTPUT_SYNCHRONOUS = 2
rate = lib.espeak_Initialize(AUDIO_OUTPUT_SYNCHRONOUS, 500, None, 0)
lib.espeak_SetSynthCallback(collect)
ESPEAK_RATE, ESPEAK_PITCH, ESPEAK_RANGE = 1, 3, 4


PROFILES = {
    # Name: (eSpeak-Variante, Sprechtempo, Tonhoehe, Melodieumfang)
    "computer": ("f4", 150, 62, 35),   # wenig Satzmelodie -> computerhaft
    "soft": ("f5", 130, 30, 60),       # hauchig, tiefer, melodischer (V1.0: 10 % schneller als 118)
}


def synth(lang: str, text: str, profile: str = "computer") -> list[float]:
    variant, speed, pitch, rng = PROFILES[profile]
    samples.clear()
    lib.espeak_SetVoiceByName(f"{lang}+{variant}".encode())   # weibliche Stimmvariante
    lib.espeak_SetParameter(ESPEAK_RATE, speed, 0)
    lib.espeak_SetParameter(ESPEAK_PITCH, pitch, 0)
    lib.espeak_SetParameter(ESPEAK_RANGE, rng, 0)
    data = text.encode("utf-8")
    lib.espeak_Synth(data, len(data) + 1, 0, 0, 0, 0x0, None, None)
    lib.espeak_Synchronize()
    return [s / 32768 for s in samples]


def trim(x: list[float], tail: int = 800) -> list[float]:
    """Stille an den Raendern entfernen."""
    thr = 0.01
    start = next((i for i, v in enumerate(x) if abs(v) > thr), 0)
    end = len(x) - next((i for i, v in enumerate(reversed(x)) if abs(v) > thr), 0)
    return x[max(0, start - 200):min(len(x), end + tail)]


def soften(x: list[float], sr: int) -> list[float]:
    """Sanfte Stimme: weicher Tiefpass, ein Hauch metallischer Kamm, kurzer warmer Hall, ohne Bitreduktion."""
    x = trim(x, tail=200) + [0.0] * int(0.35 * sr)                          # Platz fuer den Hall
    lp, a = 0.0, 0.55                                                        # einpoliger Tiefpass
    soft = []
    for v in x:
        lp += a * (v - lp)
        soft.append(lp)
    comb = int(0.006 * sr)
    out = []
    for i, v in enumerate(soft):
        out.append(v + (0.12 * out[i - comb] if i >= comb else 0))
    taps = [(int(0.031 * sr), 0.28), (int(0.053 * sr), 0.2), (int(0.089 * sr), 0.14), (int(0.137 * sr), 0.08)]
    wet = [v + sum(g * out[i - d] for d, g in taps if i >= d) for i, v in enumerate(out)]
    peak = max(1e-6, max(abs(v) for v in wet))
    return [v / peak * 0.85 for v in wet]


def computerize(x: list[float], sr: int) -> list[float]:
    x = trim(x)
    out = []
    comb = int(0.0045 * sr)
    for i, v in enumerate(x):
        ring = v * (0.78 + 0.22 * math.sin(2 * math.pi * 55 * i / sr))     # leichte Ringmodulation
        if i >= comb:
            ring += 0.35 * out[i - comb]                                    # metallischer Kammfilter
        out.append(ring)
    peak = max(1e-6, max(abs(v) for v in out))
    levels = 64                                                              # reduzierte Aufloesung (6 Bit)
    return [round(v / peak * 0.85 * levels) / levels for v in out]


def resample(x: list[float], src: int, dst: int) -> list[float]:
    if src == dst:
        return x
    n = int(len(x) * dst / src)
    return [x[min(len(x) - 1, int(i * src / dst))] for i in range(n)]


def main(out: Path, only: set[str] | None = None, profiles: set[str] | None = None):
    """only: nur diese Ansagen erzeugen (z. B. neue), damit abgenommene Aufnahmen unveraendert bleiben."""
    for profile, sub, effect in (("computer", "", computerize), ("soft", "soft", soften)):
        if profiles and profile not in profiles:
            continue
        for lang, lines in LINES.items():
            folder = out / sub / lang if sub else out / lang
            folder.mkdir(parents=True, exist_ok=True)
            for key, text in lines.items():
                if only and key not in only:
                    continue
                audio = resample(effect(synth(lang, text, profile), rate), rate, TARGET_RATE)
                write(folder / f"{key}.wav", audio)
                print(f"{profile} {lang}/{key}.wav  {len(audio) / TARGET_RATE:.2f} s")


def write(path: Path, audio: list[float]):
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(TARGET_RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v)) * 32767)) for v in audio))


if __name__ == "__main__":
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    only = next((set(a[7:].split(",")) for a in sys.argv[1:] if a.startswith("--only=")), None)
    profiles = next((set(a[10:].split(",")) for a in sys.argv[1:] if a.startswith("--profile=")), None)
    main(Path(args[0] if args else "src/LazerArena.Presentation/Audio/Voice"), only, profiles)
