"""Erzeugt das App-Symbol aus einer quadratischen Vorlage als adaptives Android-Symbol (ab Android 8):
Das Bild liegt randlos auf der Hintergrundebene (108 dp, xxxhdpi = 432 px); der Launcher schneidet seine Form
(Kreis, Squircle ...) daraus aus, sichtbar ist etwa das innere Drittel bis zwei Drittel. Dadurch entsteht kein
weisser Rand wie bei einem einfachen quadratischen Symbol. Zusaetzlich ein klassisches 192-px-Symbol als Rueckfall.

Aufruf: python3 tools/icon/make_icon.py [vorlage]
"""
from __future__ import annotations

import sys
from pathlib import Path

from PIL import Image, ImageFilter

ROOT = Path(__file__).parents[2]
RES = ROOT / "src/LazerArena.Android/Resources"


def square(img: Image.Image) -> Image.Image:
    side = min(img.size)
    left, top = (img.width - side) // 2, (img.height - side) // 2
    return img.crop((left, top, left + side, top + side))


# Bildausschnitt, der innerhalb der Launcher-Maske sichtbar sein soll (Mitte und Kantenlaenge in Vorlagen-Pixeln
# einer 1248er Vorlage, wird auf andere Groessen umgerechnet): Kopf, Oberkoerper, Schwert und Funkenregen.
FOCUS = (600 / 1248, 560 / 1248, 1060 / 1248)


def full_bleed(src: Image.Image) -> Image.Image:
    """Ebene (108 dp) so aufbauen, dass der Fokus genau die sichtbaren inneren 72 dp fuellt. Was ueber die Vorlage
    hinausragt (nur bei Parallax-Effekten sichtbar), wird mit einer unscharfen Vergroesserung der Vorlage gefuellt."""
    n = src.width
    cx, cy, vis = FOCUS[0] * n, FOCUS[1] * n, FOCUS[2] * n
    size = round(vis * 1.5)
    layer = src.resize((size, size), Image.Resampling.LANCZOS).filter(ImageFilter.GaussianBlur(size / 40))
    layer.paste(src, (round(size / 2 - cx), round(size / 2 - cy)))
    return layer


def main(source: Path):
    img = full_bleed(square(Image.open(source).convert("RGB")))
    (RES / "drawable-xxxhdpi").mkdir(parents=True, exist_ok=True)
    img.resize((432, 432), Image.Resampling.LANCZOS).save(RES / "drawable-xxxhdpi/icon_background.png")
    (RES / "mipmap-xxxhdpi").mkdir(parents=True, exist_ok=True)
    img.resize((192, 192), Image.Resampling.LANCZOS).save(RES / "mipmap-xxxhdpi/icon.png")
    (RES / "mipmap-anydpi-v26").mkdir(parents=True, exist_ok=True)
    (RES / "mipmap-anydpi-v26/icon.xml").write_text(
        '<?xml version="1.0" encoding="utf-8"?>\n'
        '<adaptive-icon xmlns:android="http://schemas.android.com/apk/res/android">\n'
        '  <background android:drawable="@drawable/icon_background" />\n'
        '  <foreground android:drawable="@android:color/transparent" />\n'
        '</adaptive-icon>\n')
    # Vorschau: so etwa zeigt ein Launcher mit runder Maske das Symbol (sichtbar: innere 72 von 108 dp)
    layer = img.resize((432, 432), Image.Resampling.LANCZOS)
    visible = layer.crop((72, 72, 360, 360))
    mask = Image.new("L", visible.size, 0)
    from PIL import ImageDraw
    ImageDraw.Draw(mask).ellipse((0, 0, visible.width - 1, visible.height - 1), fill=255)
    preview = Image.new("RGB", (visible.width * 2 + 30, visible.height + 20), (40, 40, 48))
    preview.paste(visible, (10, 10), mask)
    sq = Image.new("L", visible.size, 0)
    ImageDraw.Draw(sq).rounded_rectangle((0, 0, visible.width - 1, visible.height - 1), radius=70, fill=255)
    preview.paste(visible, (visible.width + 20, 10), sq)
    preview.save(ROOT / "mockups/icon-vorschau.png")
    print("Symbol erzeugt")


if __name__ == "__main__":
    main(Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).parent / "icon-quelle.jpg")
