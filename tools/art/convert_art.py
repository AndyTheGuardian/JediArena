"""Rechnet die KI-Illustrationen (Sieg, Niederlage, Titel) in Pixelgrafiken im SNES-Look um (V1.0).

- Freistellen am Alphakanal, Verkleinern auf die Zielhoehe (Lanczos), Alpha hart (durchsichtig oder deckend).
- Farben etwas kraeftiger und kontrastreicher, dann ohne Dithering auf die 256 Farben der modernen Spielpalette
  (12-Bit-Farben, ModernPalette.Colors) abgebildet: flache Farbflaechen wie auf dem SNES.
- Ausgabe: .lzpx (Kopf "LZPX", Breite, Hoehe als uint16 LE, je Pixel ein Paletteindex als uint16 LE, 0xFFFF = durchsichtig)
  plus PNG-Vorschau in mockups/kunst/.

Aufruf: python3 tools/art/convert_art.py
"""
import re
import struct
from pathlib import Path

from PIL import Image, ImageEnhance

ROOT = Path(__file__).resolve().parents[2]
SRC = Path(__file__).resolve().parent
OUT = ROOT / "src/LazerArena.Presentation/Art"
PREVIEW = ROOT / "mockups/kunst"

# Name: (Quelle, Zielhoehe in logischen Pixeln) – je Ausrichtung eine Groesse, damit nichts im Spiel skaliert wird
TARGETS = {
    "victory-hoch": ("victory-quelle.png", 262),
    "victory-quer": ("victory-quelle.png", 226),
    "defeat-hoch": ("defeat-quelle.png", 172),
    "defeat-quer": ("defeat-quelle.png", 170),
    "title-hoch": ("title-quelle.png", 168),
    "title-quer": ("title-quelle.png", 100),
}
TRANSPARENT = 0xFFFF


def palette() -> list[tuple[int, int, int]]:
    text = (ROOT / "src/LazerArena.Presentation/Rendering/Palettes.cs").read_text()
    block = re.search(r"public static readonly int\[\] Colors =\s*\[(.*?)\];", text, re.S).group(1)
    return [((v >> 16) & 255, (v >> 8) & 255, v & 255) for v in (int(h, 16) for h in re.findall(r"0x([0-9A-Fa-f]{6})", block))]


def nearest(colors, cache, rgb):
    if rgb not in cache:
        r, g, b = rgb
        # gewichteter Abstand (Gruen empfindlicher), damit Hauttoene und Haare stimmig bleiben
        cache[rgb] = min(range(len(colors)), key=lambda i: 2 * (colors[i][0] - r) ** 2 + 4 * (colors[i][1] - g) ** 2 + 3 * (colors[i][2] - b) ** 2)
    return cache[rgb]


def convert(source: Path, height: int, colors):
    im = Image.open(source).convert("RGBA")
    im = im.crop(im.getchannel("A").point(lambda v: 255 if v > 128 else 0).getbbox())
    width = max(1, round(im.width * height / im.height))
    small = im.resize((width, height), Image.LANCZOS)
    rgb = ImageEnhance.Contrast(ImageEnhance.Color(small.convert("RGB")).enhance(1.2)).enhance(1.08)
    alpha = small.getchannel("A")
    cache: dict = {}
    indices = []
    for y in range(height):
        for x in range(width):
            indices.append(TRANSPARENT if alpha.getpixel((x, y)) < 128 else nearest(colors, cache, rgb.getpixel((x, y))))
    return width, height, indices


def main():
    colors = palette()
    OUT.mkdir(parents=True, exist_ok=True)
    PREVIEW.mkdir(parents=True, exist_ok=True)
    for name, (file, height) in TARGETS.items():
        w, h, idx = convert(SRC / file, height, colors)
        (OUT / f"{name}.lzpx").write_bytes(b"LZPX" + struct.pack(f"<HH{len(idx)}H", w, h, *idx))
        prev = Image.new("RGBA", (w, h))
        prev.putdata([(0, 0, 0, 0) if i == TRANSPARENT else (*colors[i], 255) for i in idx])
        prev.resize((w * 2, h * 2), Image.NEAREST).save(PREVIEW / f"{name}.png")
        print(f"{name}: {w}x{h}, {len(set(idx) - {TRANSPARENT})} Farben")


if __name__ == "__main__":
    main()
