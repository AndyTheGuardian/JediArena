"""Erzeugt die Sprachansagen der weiblichen Computerstimme (Deutsch/Englisch) mit der Sprachsynthese eSpeak NG
(Systembibliothek libespeak-ng) und gibt ihnen einen Computer-Klang (leichte Ringmodulation, metallischer Kamm,
reduzierte Aufloesung). Die WAV-Dateien werden ins Projekt eingebettet; auf dem Handy wird keine Sprachsynthese benoetigt.

Aufruf: python3 tools/voice/make_voice.py <zielordner>
"""
import ctypes
import math
import struct
import sys
import wave
from pathlib import Path

LINES = {
    "en": {"Three": "Three", "Two": "Two", "One": "One", "Hit": "Hit!", "Victory": "Victory!", "Defeat": "Defeat."},
    "de": {"Three": "Drei", "Two": "Zwei", "One": "Eins", "Hit": "Treffer!", "Victory": "Sieg!", "Defeat": "Niederlage."},
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


def synth(lang: str, text: str) -> list[float]:
    samples.clear()
    lib.espeak_SetVoiceByName(f"{lang}+f4".encode())   # weibliche Stimmvariante
    lib.espeak_SetParameter(ESPEAK_RATE, 150, 0)
    lib.espeak_SetParameter(ESPEAK_PITCH, 62, 0)
    lib.espeak_SetParameter(ESPEAK_RANGE, 35, 0)       # wenig Satzmelodie -> computerhaft
    data = text.encode("utf-8")
    lib.espeak_Synth(data, len(data) + 1, 0, 0, 0, 0x0, None, None)
    lib.espeak_Synchronize()
    return [s / 32768 for s in samples]


def computerize(x: list[float], sr: int) -> list[float]:
    # Stille an den Raendern entfernen
    thr = 0.01
    start = next((i for i, v in enumerate(x) if abs(v) > thr), 0)
    end = len(x) - next((i for i, v in enumerate(reversed(x)) if abs(v) > thr), 0)
    x = x[max(0, start - 200):min(len(x), end + 800)]
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


def main(out: Path):
    for lang, lines in LINES.items():
        folder = out / lang
        folder.mkdir(parents=True, exist_ok=True)
        for key, text in lines.items():
            audio = resample(computerize(synth(lang, text), rate), rate, TARGET_RATE)
            with wave.open(str(folder / f"{key}.wav"), "wb") as w:
                w.setnchannels(1)
                w.setsampwidth(2)
                w.setframerate(TARGET_RATE)
                w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v)) * 32767)) for v in audio))
            print(f"{lang}/{key}.wav  {len(audio) / TARGET_RATE:.2f} s")


if __name__ == "__main__":
    main(Path(sys.argv[1] if len(sys.argv) > 1 else "src/LazerArena.Presentation/Audio/Voice"))
