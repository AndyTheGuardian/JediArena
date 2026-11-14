"""Fuenf Gestaltungsvorschlaege fuer den modernen Stil im Look eines 16-Bit-Super-Nintendo-Spiels (Hochformat).

Gemeinsame SNES-Merkmale in allen Varianten:
  * kein Dithering, stattdessen Farbbaender (wie HDMA-Verlaeufe) und 3-4-stufige Schattierung
  * 1-Pixel-Konturen um Figuren, Drohne und Schild
  * Kachel-Texturen (8x8 / 16x16)
  * Transparenz statt Leuchten: Blitze mit hellem Kern, nach aussen in Stufen transparenter
  * hoechstens 256 Farben aus einer 4096-Farben-Grundpalette (12 Bit)

Aufruf: python3 make_snes_mockups.py <ausgabeordner>
"""
from __future__ import annotations

import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

W, H = 270, 600
SCALE = 4
FIELD = (8, 60, 254, 479)            # x, y, w, h
HUD_TOP = (0, 0, W, 44)
HUD_BOTTOM = (0, H - 44, W, 44)

# ----------------------------------------------------------------------------- Schrift (5x7, wie im Spiel)
GLYPHS = {
    "A": ".###.|#...#|#...#|#####|#...#|#...#|#...#", "C": ".###.|#...#|#....|#....|#....|#...#|.###.",
    "D": "####.|#...#|#...#|#...#|#...#|#...#|####.", "E": "#####|#....|#....|####.|#....|#....|#####",
    "G": ".###.|#...#|#....|#.###|#...#|#...#|.####", "H": "#...#|#...#|#...#|#####|#...#|#...#|#...#",
    "I": ".###.|..#..|..#..|..#..|..#..|..#..|.###.", "L": "#....|#....|#....|#....|#....|#....|#####",
    "M": "#...#|##.##|#.#.#|#.#.#|#...#|#...#|#...#", "N": "#...#|#...#|##..#|#.#.#|#..##|#...#|#...#",
    "O": ".###.|#...#|#...#|#...#|#...#|#...#|.###.", "P": "####.|#...#|#...#|####.|#....|#....|#....",
    "R": "####.|#...#|#...#|####.|#.#..|#..#.|#...#", "S": ".####|#....|#....|.###.|....#|....#|####.",
    "T": "#####|..#..|..#..|..#..|..#..|..#..|..#..", "U": "#...#|#...#|#...#|#...#|#...#|#...#|.###.",
    "0": ".###.|#...#|#..##|#.#.#|##..#|#...#|.###.", "1": "..#..|.##..|..#..|..#..|..#..|..#..|.###.",
    "2": ".###.|#...#|....#|...#.|..#..|.#...|#####", " ": ".....|.....|.....|.....|.....|.....|.....",
    "-": ".....|.....|.....|#####|.....|.....|.....",
}


sys.path.insert(0, str(Path(__file__).parent))
from make_mockups import _GLYPHS  # noqa: E402  (vollstaendiger Zeichensatz)
GLYPHS.update(_GLYPHS)


def text_width(text, scale=1):
    return (len(text) * 6 - 1) * scale


def draw_text(img, text, x, y, color, scale=1, align="left", outline=None, shadow=None):
    if align == "center":
        x -= text_width(text, scale) // 2
    elif align == "right":
        x -= text_width(text, scale)
    px = img.load()
    pts = []
    for i, ch in enumerate(text):
        for gy, row in enumerate(GLYPHS[ch].split("|")):
            for gx, c in enumerate(row):
                if c == "#":
                    for sy in range(scale):
                        for sx in range(scale):
                            pts.append((x + (i * 6 + gx) * scale + sx, y + gy * scale + sy, gy))
    if outline:
        for ox, oy, _ in pts:
            for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (1, 1), (-1, 1), (1, -1)):
                if 0 <= ox + dx < img.width and 0 <= oy + dy < img.height:
                    px[ox + dx, oy + dy] = outline
    if shadow:
        for ox, oy, _ in pts:
            if 0 <= ox + scale < img.width and 0 <= oy + scale < img.height:
                px[ox + scale, oy + scale] = shadow
    for ox, oy, gy in pts:
        if 0 <= ox < img.width and 0 <= oy < img.height:
            px[ox, oy] = color(gy) if callable(color) else color


# ----------------------------------------------------------------------------- Szene (wie im Spiel)
FW, FH = FIELD[2], FIELD[3]


def fpos(nx, ny):
    """Normierte Feldkoordinaten -> Pixel."""
    return FIELD[0] + nx * FW, FIELD[1] + ny * FH


P1 = fpos(0.5, 0.935)
P2 = fpos(0.5, 0.065)
DRONE = fpos(0.63, 0.5)
P1_ANGLE, P2_ANGLE = -22, 12
P1_BROKEN = {(0, 6), (0, 7), (1, 7), (0, 12)}
P2_BROKEN = {(0, 3), (0, 4), (1, 4), (0, 10)}


def sword(player, angle):
    a = math.radians(angle)
    d = (math.sin(a), -math.cos(a)) if player == "p1" else (-math.sin(a), math.cos(a))
    c = P1 if player == "p1" else P2
    hand = (c[0] + d[0] * 0.06 * FW, c[1] + d[1] * 0.06 * FW)
    tip = (c[0] + d[0] * 0.28 * FW, c[1] + d[1] * 0.28 * FW)
    return hand, tip, d


def shield_rects(player):
    row_h = 0.022 * FH
    out = []
    for row, ratio in enumerate((0.83, 0.855)):
        top = ratio * FH if player == "p1" else FH - ratio * FH - row_h
        for i in range(16):
            x0 = FIELD[0] + i * FW / 16
            out.append((row, i, int(x0), int(FIELD[1] + top), int(x0 + FW / 16) - 1, int(FIELD[1] + top + row_h) - 1))
    return out


def zigzag(a, b, step, amp, seed):
    rng = random.Random(seed)
    dx, dy = b[0] - a[0], b[1] - a[1]
    length = math.hypot(dx, dy)
    nx, ny = -dy / length, dx / length
    n = max(2, int(length / step))
    pts = [a]
    for k in range(1, n):
        t = k / n
        off = amp * (1 if k % 2 else -1) * (0.5 + rng.random() * 0.5)
        pts.append((a[0] + dx * t + nx * off, a[1] + dy * t + ny * off))
    pts.append(b)
    return pts


# ----------------------------------------------------------------------------- Bausteine
def rgba(c, a=255):
    return (c[0], c[1], c[2], a)


def mix(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3))


def shade(c, f):
    return tuple(max(0, min(255, int(v * f))) for v in c)


def layer():
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def with_outline(sprite: Image.Image, color=(8, 8, 20)) -> Image.Image:
    """1-Pixel-Kontur um alles Nichttransparente (typisch fuer SNES-Sprites)."""
    alpha = sprite.getchannel("A").point(lambda v: 255 if v > 0 else 0)
    grown = alpha.filter(ImageFilter.MaxFilter(3))
    out = Image.new("RGBA", sprite.size, rgba(color, 0))
    out.putalpha(grown)
    base = Image.new("RGBA", sprite.size, rgba(color))
    base.putalpha(grown)
    base.alpha_composite(sprite)
    return base


def banded_gradient(img, box, top, bottom, bands):
    """Vertikaler Verlauf in Stufen (HDMA-artig, ohne Dithering)."""
    x0, y0, x1, y1 = box
    d = ImageDraw.Draw(img)
    for y in range(y0, y1):
        t = (y - y0) / max(1, (y1 - y0 - 1))
        tq = math.floor(t * bands) / max(1, bands - 1)
        d.line([(x0, y), (x1 - 1, y)], fill=mix(top, bottom, min(1, tq)))


def posterized_sphere(img, cx, cy, rx, ry, ramp):
    """Kugel/Ellipse mit 4 harten Schattierungsstufen (ramp: dunkel..hell) und Glanzpunkt."""
    px = img.load()
    for y in range(int(cy - ry), int(cy + ry) + 1):
        for x in range(int(cx - rx), int(cx + rx) + 1):
            nx, ny = (x - cx) / rx, (y - cy) / ry
            dd = nx * nx + ny * ny
            if dd > 1:
                continue
            nz = math.sqrt(1 - dd)
            lum = max(0.0, (-0.5 * nx - 0.6 * ny) * 0.8 + nz * 0.6)
            idx = min(len(ramp) - 1, int(lum * len(ramp)))
            if nx < -0.25 and ny < -0.3 and dd < 0.35 and dd > 0.12:
                idx = len(ramp) - 1
            px[x, y] = rgba(ramp[idx]) if img.mode == "RGBA" else ramp[idx]


def translucent_bolt(base: Image.Image, pts, color, core=(255, 255, 255), widths=((11, 28), (7, 60), (4, 120), (2, 200))):
    """Blitz mit hellem Kern, nach aussen in Stufen transparenter (Farbmathematik statt Leuchten)."""
    for width, alpha in widths:
        lay = layer()
        ImageDraw.Draw(lay).line(pts, fill=rgba(color, alpha), width=width, joint="curve")
        base.alpha_composite(lay)
    lay = layer()
    ImageDraw.Draw(lay).line(pts, fill=rgba(mix(color, core, 0.75), 255), width=1)
    base.alpha_composite(lay)


def translucent_blade(base, hand, tip, color, core=(255, 255, 255)):
    for width, alpha in ((7, 40), (5, 90), (3, 170)):
        lay = layer()
        ImageDraw.Draw(lay).line([hand, tip], fill=rgba(color, alpha), width=width)
        base.alpha_composite(lay)
    lay = layer()
    ImageDraw.Draw(lay).line([hand, tip], fill=rgba(mix(color, core, 0.8)), width=1)
    base.alpha_composite(lay)


# ----------------------------------------------------------------------------- Figuren (gemeinsam, mit Kontur)
TEAM = {"p1": (60, 220, 255), "p2": (255, 70, 190)}
ARMOR = {"p1": [(14, 34, 70), (28, 66, 120), (52, 104, 170), (110, 160, 220)],
         "p2": [(50, 10, 40), (100, 26, 80), (160, 50, 130), (220, 120, 190)]}
HELMET = [(70, 76, 96), (130, 138, 160), (190, 196, 214), (240, 244, 255)]
METAL = [(40, 44, 60), (90, 96, 120), (150, 158, 184), (220, 226, 240)]


def draw_player(base, player, angle, team=TEAM, armor=ARMOR, helmet=HELMET, outline=(8, 8, 20)):
    cx, cy = P1 if player == "p1" else P2
    r = FW * 0.045
    hand, tip, d = sword(player, angle)
    sprite = layer()
    posterized_sphere(sprite, cx, cy, r * 2.0, r * 1.05, armor[player])
    posterized_sphere(sprite, cx, cy, r * 0.95, r * 0.95, helmet)
    vy = cy - r * 0.35 if player == "p1" else cy + r * 0.35
    ImageDraw.Draw(sprite).line([(cx - r * 0.5, vy), (cx + r * 0.5, vy)], fill=rgba((20, 20, 40)), width=1)
    hilt = draw_hilt(sprite, hand, d)
    base.alpha_composite(with_outline(sprite, outline))
    translucent_blade(base, hilt, tip, team[player])


HILT_LENGTH = 11


def draw_hilt(sprite, hand, d, length=HILT_LENGTH):
    """Schwertgriff: schlichter Griff mit Rillen, ohne seitliche Teile; liefert den Klingenansatz."""
    dd = ImageDraw.Draw(sprite)
    n = (-d[1], d[0])   # quer zur Klinge
    start = (hand[0] - d[0] * 2, hand[1] - d[1] * 2)   # Knauf etwas hinter der Hand
    end = (hand[0] + d[0] * (length - 2), hand[1] + d[1] * (length - 2))
    dd.line([start, end], fill=rgba((150, 150, 165)), width=3)
    for k in range(1, length - 3, 3):   # Rillen
        p = (start[0] + d[0] * k, start[1] + d[1] * k)
        dd.line([(p[0] - n[0], p[1] - n[1]), (p[0] + n[0], p[1] + n[1])], fill=rgba((70, 70, 85)))
    dd.line([start, (start[0] + d[0] * 1.5, start[1] + d[1] * 1.5)], fill=rgba((205, 205, 220)), width=3)   # Endkappe, so breit wie der Griff
    return (end[0] + d[0], end[1] + d[1])


def draw_drone(base, ramp=METAL, ring=(110, 120, 150), eye=(255, 200, 60), nozzle=(255, 140, 40), outline=(8, 8, 20)):
    cx, cy = DRONE
    r = FW * 0.045
    sprite = layer()
    d = ImageDraw.Draw(sprite)
    d.ellipse([cx - r * 1.6, cy - r * 1.6, cx + r * 1.6, cy + r * 1.6], outline=rgba(ring), width=2)
    for a in (45, 135, 225, 315):
        nx, ny = cx + math.cos(math.radians(a)) * r * 1.55, cy + math.sin(math.radians(a)) * r * 1.55
        d.ellipse([nx - 2, ny - 2, nx + 2, ny + 2], fill=rgba(nozzle))
    posterized_sphere(sprite, cx, cy, r, r, ramp)
    d.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=rgba(eye))
    d.point((cx - 1, cy - 1), fill=rgba((255, 255, 230)))
    base.alpha_composite(with_outline(sprite, outline))


def draw_shields(base, style):
    """style: dict mit 'p1'/'p2' -> (licht, mitte, schatten), 'outline'."""
    for player, broken in (("p1", P1_BROKEN), ("p2", P2_BROKEN)):
        light, mid, dark = style[player]
        sprite = layer()
        d = ImageDraw.Draw(sprite)
        for row, i, x0, y0, x1, y1 in shield_rects(player):
            if (row, i) in broken:
                continue
            f = 1.0 if row == 0 else 0.75
            d.rectangle([x0, y0, x1, y1], fill=rgba(shade(mid, f)))
            d.line([(x0, y0), (x1, y0)], fill=rgba(shade(light, f)))
            d.line([(x0, y0), (x0, y1)], fill=rgba(shade(light, f)))
            d.line([(x0, y1), (x1, y1)], fill=rgba(shade(dark, f)))
            d.line([(x1, y0), (x1, y1)], fill=rgba(shade(dark, f)))
        base.alpha_composite(with_outline(sprite, style.get("outline", (8, 8, 20))))


def draw_bolts(base, colors=TEAM, seed=0):
    # Blitz von Spieler 1 (unterwegs nach oben) und angedockter Blitz der KI am Schwert von Spieler 1
    head = fpos(0.45, 0.27)
    translucent_bolt(base, zigzag(DRONE, head, 8, 4, 3 + seed * 31), colors["p1"])
    _, tip, _ = sword("p1", P1_ANGLE)
    translucent_bolt(base, zigzag(DRONE, tip, 9, 5, 5 + seed * 57), colors["p2"])
    spark = layer()
    d = ImageDraw.Draw(spark)
    for a in range(0, 360, 45):
        ln = 6 if a % 90 == 0 else 3
        d.line([tip, (tip[0] + math.cos(math.radians(a)) * ln, tip[1] + math.sin(math.radians(a)) * ln)],
               fill=(255, 245, 200, 220), width=1)
    base.alpha_composite(spark)


def pips(img, x, y, score, color, empty, outline=(8, 8, 20)):
    d = ImageDraw.Draw(img)
    for i in range(3):
        cx = x + i * 13
        pts = [(cx, y - 5), (cx + 5, y), (cx, y + 5), (cx - 5, y)]
        d.polygon(pts, fill=color if i < score else empty, outline=outline)
        if i < score:
            d.point((cx - 1, y - 2), fill=(255, 255, 255))


def quantize(img: Image.Image) -> Image.Image:
    """Hoechstens 256 Farben, alle aus 4096 (4 Bit je Kanal), ohne Dithering."""
    rgb = img.convert("RGB").point(lambda v: round(v / 17) * 17)
    q = rgb.quantize(colors=256, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    pal = q.getpalette()[: 256 * 3]
    q.putpalette([round(v / 17) * 17 for v in pal])
    return q.convert("RGB")


# ----------------------------------------------------------------------------- Varianten
def hud_standard(img, style):
    """HUD-Leisten oben/unten; style: dict mit bar, frame, label colors, empty pip etc."""
    for box, label, score, team in ((HUD_TOP, "CPU", 2, TEAM["p2"]), (HUD_BOTTOM, "P1", 1, TEAM["p1"])):
        x, y, w, h = box
        style["bar"](img, (x, y, x + w, y + h))
        draw_text(img, label, 10, y + 15, team, 2, outline=(8, 8, 20))
        pips(img, 10 + text_width(label, 2) + 16, y + h // 2, score, team, style["empty"])
        if box is HUD_TOP:
            draw_text(img, "MEDIUM", W - 40, y + 19, style["dim"], 1, align="right", outline=(8, 8, 20))
            d = ImageDraw.Draw(img)
            d.rectangle([W - 32, y + 11, W - 10, y + 33], fill=style["button"], outline=(8, 8, 20))
            d.rectangle([W - 25, y + 16, W - 23, y + 28], fill=(240, 244, 255))
            d.rectangle([W - 19, y + 16, W - 17, y + 28], fill=(240, 244, 255))


def v1_mode7(seed=1, rotation_deg=28):
    """V1 'Mode-7-Arena': gedrehte Ring-Arena als Boden (Mode-7-Rotation), HDMA-Abendhimmel aussen."""
    img = Image.new("RGBA", (W, H))
    banded_gradient(img, (0, 0, W, H), (40, 20, 80), (255, 140, 90), 14)
    px = img.load()
    fx, fy, fw, fh = FIELD
    cx, cy = fx + fw / 2, fy + fh / 2
    rot = math.radians(rotation_deg)
    for y in range(fy, fy + fh):
        for x in range(fx, fx + fw):
            u, v = x - cx, y - cy
            ru, rv = u * math.cos(rot) - v * math.sin(rot), u * math.sin(rot) + v * math.cos(rot)
            dist = math.hypot(u, v)
            ring = int(dist / 22) % 2
            spoke = int((math.atan2(rv, ru) + math.pi) / (math.pi / 8)) % 2
            tile = (int(ru // 16) + int(rv // 16)) % 2
            base = [(22, 30, 64), (30, 40, 84), (38, 52, 104)][(ring + spoke + tile) % 3]
            if abs(dist - 120) < 2 or abs(dist - 60) < 1.5:
                base = (90, 120, 200)
            px[x, y] = rgba(base)
    d = ImageDraw.Draw(img)
    d.rectangle([fx - 3, fy - 3, fx + fw + 2, fy + fh + 2], outline=(8, 8, 20), width=1)
    d.rectangle([fx - 2, fy - 2, fx + fw + 1, fy + fh + 1], outline=(200, 160, 255), width=2)
    my = int(cy)
    d.line([(fx, my), (fx + fw, my)], fill=(120, 150, 255))
    draw_scene(img, shields={"p1": ((150, 240, 255), (40, 170, 220), (10, 70, 120)),
                             "p2": ((255, 170, 230), (210, 50, 160), (100, 10, 70))})

    def bar(im, box):
        banded_gradient(im, box, (60, 30, 110), (20, 10, 50), 5)
        ImageDraw.Draw(im).line([(box[0], box[3] - 2), (box[2], box[3] - 2)], fill=(200, 160, 255))
    hud_standard(img, {"bar": bar, "empty": (40, 30, 80), "dim": (200, 180, 240), "button": (80, 50, 140)})
    return img


def v2_station(seed=2):
    """V2 'Raumstation': Metallbodenplatten mit Nieten, Warnstreifen-Raender, Stahl-HUD mit Schrauben."""
    img = Image.new("RGBA", (W, H), (20, 22, 30, 255))
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    for ty in range(fy, fy + fh, 16):
        for tx in range(fx, fx + fw, 16):
            plate = (58, 64, 78) if ((tx - fx) // 16 + (ty - fy) // 16) % 2 == 0 else (52, 58, 72)
            d.rectangle([tx, ty, tx + 15, ty + 15], fill=plate)
            d.line([(tx, ty), (tx + 15, ty)], fill=(84, 92, 110))
            d.line([(tx, ty), (tx, ty + 15)], fill=(84, 92, 110))
            d.line([(tx, ty + 15), (tx + 15, ty + 15)], fill=(30, 34, 44))
            d.line([(tx + 15, ty), (tx + 15, ty + 15)], fill=(30, 34, 44))
            for rx, ry in ((tx + 3, ty + 3), (tx + 12, ty + 3), (tx + 3, ty + 12), (tx + 12, ty + 12)):
                d.point((rx, ry), fill=(120, 128, 146))
    # Seitenwaende mit Warnstreifen
    for x0 in (fx - 8, fx + fw):
        for y in range(fy, fy + fh):
            for x in range(x0, x0 + 8):
                d.point((x, y), fill=(230, 180, 30) if ((x + y) // 4) % 2 == 0 else (30, 26, 20))
        d.rectangle([x0, fy, x0 + 7, fy + fh - 1], outline=(8, 8, 20))
    my = fy + fh // 2
    for x in range(fx, fx + fw, 8):
        d.rectangle([x, my - 1, x + 4, my], fill=(90, 200, 120))
    draw_scene(img, shields={"p1": ((160, 240, 255), (50, 160, 210), (16, 70, 110)),
                             "p2": ((255, 180, 220), (200, 60, 150), (90, 20, 70))})

    def bar(im, box):
        x0, y0, x1, y1 = box
        banded_gradient(im, (x0, y0, x1, y1), (110, 118, 136), (50, 56, 70), 6)
        dd = ImageDraw.Draw(im)
        dd.rectangle([x0, y0, x1 - 1, y1 - 1], outline=(8, 8, 20))
        dd.line([(x0 + 1, y0 + 1), (x1 - 2, y0 + 1)], fill=(170, 178, 196))
        for sx in (x0 + 4, x1 - 6):
            for sy in (y0 + 4, y1 - 6):
                dd.ellipse([sx, sy, sx + 2, sy + 2], fill=(200, 206, 220), outline=(40, 44, 56))
    hud_standard(img, {"bar": bar, "empty": (40, 44, 56), "dim": (220, 226, 240), "button": (70, 76, 92)})
    return img


STAR_TILES = (30, 60, 90)   # je Ebene: so weit wandert sie pro Schleife (nah = schneller)


def flight_starfield(img, t, n, seed, count=90):
    """Flug durchs All wie in klassischen Weltraum-Flugspielen: Sterne kommen aus der Bildmitte, fliegen nach
    aussen, werden dabei schneller, heller und ziehen kurze Streifen. Jeder Stern legt pro Schleife genau
    einen Anflug zurueck -> nahtlos."""
    d = ImageDraw.Draw(img)
    cx, cy = FIELD[0] + FIELD[2] / 2, FIELD[1] + FIELD[3] / 2
    rng = random.Random(seed)
    levels = [(70, 70, 110), (120, 120, 170), (180, 180, 225), (255, 255, 255)]
    for _ in range(count):
        ang = rng.random() * 2 * math.pi
        spread = rng.uniform(0.15, 1.0)
        x3, y3 = math.cos(ang) * spread, math.sin(ang) * spread
        phase = rng.random()

        def project(progress):
            z = 1.0 - 0.94 * progress   # 1 (fern) .. 0.06 (nah)
            return cx + x3 / z * 34, cy + y3 / z * 34, z

        p = (phase + t / n) % 1.0
        sx, sy, z = project(p)
        px_, py_, _ = project(max(0.0, p - 0.012))   # Position kurz zuvor -> Streifen
        level = min(3, int((1 - z) * 4.2))
        color = levels[level]
        if level >= 2:
            d.line([(px_, py_), (sx, sy)], fill=levels[level - 1])
        ix, iy = int(sx), int(sy)
        if z < 0.2:
            d.rectangle([ix, iy, ix + 1, iy + 1], fill=color)
        else:
            d.point((ix, iy), fill=color)


def v3_parallax(seed=3, t=0, n=1):
    """V3 'Sternenflug': durchsichtige Arena, darunter ein Flug durchs All (Sterne stroemen aus der Mitte nach aussen)."""
    img = Image.new("RGBA", (W, H))
    banded_gradient(img, (0, 0, W, H), (4, 2, 16), (14, 6, 34), 8)
    flight_starfield(img, t, n, seed)
    # Arena: halbtransparenter Boden mit Gitter, Leuchtschienen
    fx, fy, fw, fh = FIELD
    glass = layer()
    gd = ImageDraw.Draw(glass)
    gd.rectangle([fx, fy, fx + fw - 1, fy + fh - 1], fill=(20, 30, 70, 120))
    for x in range(fx, fx + fw, 24):
        gd.line([(x, fy), (x, fy + fh)], fill=(80, 120, 220, 70))
    for y in range(fy, fy + fh, 24):
        gd.line([(fx, y), (fx + fw, y)], fill=(80, 120, 220, 70))
    img.alpha_composite(glass)
    d = ImageDraw.Draw(img)
    for x0 in (fx - 3, fx + fw):
        d.rectangle([x0, fy, x0 + 2, fy + fh - 1], fill=(120, 200, 255))
        d.line([(x0 + 1, fy), (x0 + 1, fy + fh - 1)], fill=(230, 250, 255))
    draw_scene(img, shields={"p1": ((200, 250, 255), (80, 200, 240), (20, 90, 140)),
                             "p2": ((255, 200, 240), (230, 80, 180), (110, 20, 90))}, bolt_seed=t)

    def bar(im, box):
        x0, y0, x1, y1 = box
        lay = layer()
        ImageDraw.Draw(lay).rectangle([x0, y0, x1 - 1, y1 - 1], fill=(10, 10, 40, 170))
        im.alpha_composite(lay)
        ImageDraw.Draw(im).line([(x0, y1 - 1 if y0 == 0 else y0), (x1, y1 - 1 if y0 == 0 else y0)], fill=(120, 200, 255))
    hud_standard(img, {"bar": bar, "empty": (30, 30, 70), "dim": (170, 190, 240), "button": (30, 40, 90)})
    return img


def energy_pulses(img, t, n, seed):
    """Leuchtimpulse, die auf den 16-px-Gitterlinien geschlossene Rechteckbahnen abfahren (mit Nachleuchten)."""
    fx, fy, fw, fh = FIELD
    rng = random.Random(seed)
    lay = layer()
    d = ImageDraw.Draw(lay)
    for k in range(11):
        gx0 = rng.randrange(0, fw // 16 - 4); gy0 = rng.randrange(0, fh // 16 - 6)
        gw, gh = rng.randint(2, 6), rng.randint(3, 9)
        corners = [(gx0, gy0), (gx0 + gw, gy0), (gx0 + gw, gy0 + gh), (gx0, gy0 + gh)]
        pts = [(fx + cx * 16, fy + cy * 16) for cx, cy in corners]
        segs = [(pts[i], pts[(i + 1) % 4]) for i in range(4)]
        perimeter = sum(abs(b[0] - a[0]) + abs(b[1] - a[1]) for a, b in segs)
        color = rng.choice([(60, 220, 255), (255, 90, 200), (150, 120, 255)])
        laps = rng.choice([1, 2])   # ganzzahlige Runden pro Schleife -> nahtlos
        head = ((t / n) * laps * perimeter + rng.random() * perimeter) % perimeter
        for j in range(22):   # Kopf + Schweif
            pos = (head - j * 2) % perimeter
            for a, b in segs:
                seg_len = abs(b[0] - a[0]) + abs(b[1] - a[1])
                if pos <= seg_len:
                    f = pos / seg_len
                    x, y = a[0] + (b[0] - a[0]) * f, a[1] + (b[1] - a[1]) * f
                    alpha = int(240 * (1 - j / 22))
                    c = mix(color, (255, 255, 255), 0.6) if j < 2 else color
                    w = 2 if j < 3 else 1
                    d.rectangle([x - w, y - w, x + w, y + w], fill=rgba(c, alpha))
                    break
                pos -= seg_len
    img.alpha_composite(lay)


def v4_arcade(seed=4, t=0, n=1):
    """V4 'Arcade-Chrom': Schachbrettboden, Chrom-HUD mit Energieleisten statt Rauten (Kampfspiel-Stil)."""
    img = Image.new("RGBA", (W, H), (10, 10, 30, 255))
    banded_gradient(img, (0, 0, W, H), (10, 20, 60), (60, 10, 50), 10)
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    for ty in range(fy, fy + fh, 16):
        for tx in range(fx, fx + fw, 16):
            even = ((tx - fx) // 16 + (ty - fy) // 16) % 2 == 0
            d.rectangle([tx, ty, tx + 15, ty + 15], fill=(26, 70, 96) if even else (18, 48, 70))
            if even:
                d.line([(tx, ty), (tx + 15, ty)], fill=(40, 96, 124))
    for x0 in (fx - 4, fx + fw):
        banded_gradient(img, (x0, fy, x0 + 4, fy + fh), (240, 240, 255), (90, 100, 140), 6)
    draw_scene(img, shields={"p1": ((210, 250, 255), (70, 190, 240), (20, 80, 130)),
                             "p2": ((255, 210, 240), (230, 70, 170), (100, 20, 80))}, bolt_seed=t)
    # Chrom-HUD mit Energieleiste (3 Segmente = 3 Punkte)
    for box, label, score, team in ((HUD_TOP, "CPU", 2, TEAM["p2"]), (HUD_BOTTOM, "P1", 1, TEAM["p1"])):
        x, y, w, h = box
        banded_gradient(img, (x, y, x + w, y + h), (40, 40, 80), (10, 10, 30), 4)
        chrome = lambda gy: [(255, 255, 255), (230, 236, 250), (200, 210, 235), (150, 160, 200), (230, 200, 120), (250, 220, 140), (255, 240, 180)][gy]
        draw_text(img, label, 8, y + 8, chrome, 2, outline=(8, 8, 20))
        bx, by, bw = 8 + text_width(label, 2) + 12, y + 12, 120
        d.rectangle([bx - 2, by - 2, bx + bw + 1, by + 13], fill=(8, 8, 20), outline=(220, 226, 240))
        for i in range(3):
            sx = bx + i * (bw // 3)
            color = team if i < score else (40, 40, 60)
            d.rectangle([sx + 1, by, sx + bw // 3 - 2, by + 11], fill=color)
            if i < score:
                d.line([(sx + 1, by), (sx + bw // 3 - 2, by)], fill=mix(team, (255, 255, 255), 0.6))
                d.line([(sx + 1, by + 11), (sx + bw // 3 - 2, by + 11)], fill=shade(team, 0.5))
        if box is HUD_TOP:
            draw_text(img, "MEDIUM", W - 40, y + 30, (200, 210, 235), 1, align="right", outline=(8, 8, 20))
            d.rectangle([W - 32, y + 11, W - 10, y + 33], fill=(60, 60, 100), outline=(220, 226, 240))
            d.rectangle([W - 25, y + 16, W - 23, y + 28], fill=(255, 255, 255))
            d.rectangle([W - 19, y + 16, W - 17, y + 28], fill=(255, 255, 255))
    return img


def circuit_paths(seed):
    """Leiterbahnen als Polylinien auf dem 8-px-Raster (rechtwinklige Zufallswege)."""
    fx, fy, fw, fh = FIELD
    rng = random.Random(seed)
    paths = []
    for _ in range(26):
        x = fx + rng.randrange(0, fw // 8) * 8
        y = fy + rng.randrange(0, fh // 8) * 8
        pts = [(x, y)]
        for _ in range(rng.randint(3, 7)):
            nx = min(max(x + rng.choice([0, 8, 16, -8]), fx), fx + fw - 1)
            ny = min(max(y + rng.choice([8, 16, 0, -8]), fy), fy + fh - 1)
            pts += [(nx, y), (nx, ny)]
            x, y = nx, ny
        paths.append(pts)
    return paths


def point_on(path, distance):
    for a, b in zip(path, path[1:]):
        seg = abs(b[0] - a[0]) + abs(b[1] - a[1])
        if distance <= seg:
            f = distance / seg if seg else 0
            return a[0] + (b[0] - a[0]) * f, a[1] + (b[1] - a[1]) * f
        distance -= seg
    return None


def v5_circuit(seed=5, t=0, n=1):
    """V5 'Energiekern': dunkler Boden mit sehr schwachen weissen Leiterbahnen, auf denen weisse Impulse
    entlanglaufen; pulsierender Energiekanal in der Mitte."""
    img = Image.new("RGBA", (W, H), (8, 6, 20, 255))
    banded_gradient(img, (0, 0, W, H), (20, 6, 40), (6, 20, 40), 9)
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    d.rectangle([fx, fy, fx + fw - 1, fy + fh - 1], fill=(16, 14, 34))
    traces = layer()
    td = ImageDraw.Draw(traces)
    paths = circuit_paths(seed)
    for path in paths:   # Leiterbahnen: weiss, sehr schwach
        td.line(path, fill=(255, 255, 255, 26))
        for px_, py_ in path[2::2]:
            td.rectangle([px_ - 1, py_ - 1, px_ + 1, py_ + 1], fill=(255, 255, 255, 40))
    img.alpha_composite(traces)
    pulses = layer()
    pd = ImageDraw.Draw(pulses)
    rng = random.Random(seed + 1)
    for path in paths:   # je Bahn ein Impuls; nach dem Ende kurze Pause, dann von vorn (nahtlose Schleife)
        length = sum(abs(b[0] - a[0]) + abs(b[1] - a[1]) for a, b in zip(path, path[1:]))
        cycle = length + 40
        head = ((t / n) + rng.random()) % 1 * cycle
        for j in range(12):
            pos = head - j * 1.5
            if 0 <= pos <= length and (p := point_on(path, pos)):
                alpha = int(150 * (1 - j / 12))
                pd.point(p, fill=(255, 255, 255, alpha))
                if j == 0:
                    pd.rectangle([p[0] - 1, p[1] - 1, p[0] + 1, p[1] + 1], fill=(255, 255, 255, 90))
    img.alpha_composite(pulses)
    my = fy + fh // 2
    channel = layer()   # Energiekanal: transparente Baender
    cd = ImageDraw.Draw(channel)
    for wdt, a in ((14, 40), (8, 80), (3, 160)):
        cd.rectangle([fx, my - wdt // 2, fx + fw - 1, my + wdt // 2], fill=(120, 60, 255, a))
    img.alpha_composite(channel)
    for x0 in (fx - 3, fx + fw):
        banded_gradient(img, (x0, fy, x0 + 3, fy + fh), (200, 120, 255), (60, 220, 220), 8)
    draw_scene(img, shields={"p1": ((180, 255, 240), (40, 200, 190), (10, 80, 90)),
                             "p2": ((255, 190, 250), (200, 70, 220), (80, 20, 100))}, bolt_seed=t)

    def bar(im, box):
        x0, y0, x1, y1 = box
        banded_gradient(im, box, (40, 14, 70), (14, 6, 30), 4)
        dd = ImageDraw.Draw(im)
        line_y = y1 - 1 if y0 == 0 else y0
        for x in range(x0, x1, 6):
            dd.line([(x, line_y), (x + 3, line_y)], fill=(200, 120, 255))
    hud_standard(img, {"bar": bar, "empty": (40, 20, 60), "dim": (200, 170, 240), "button": (60, 30, 100)})
    return img


def hex_floor(img, base, line, glow, seed, cell=10):
    """Sechseck-Waben als Boden (versetzte Reihen), einige Zellen leuchten."""
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    rng = random.Random(seed)
    h = cell * math.sqrt(3) / 2
    row = 0
    y = fy - h
    while y < fy + fh + h:
        x = fx - cell + (cell * 1.5 if row % 2 else 0)
        while x < fx + fw + cell:
            pts = [(x + cell * math.cos(math.radians(a)), y + cell * math.sin(math.radians(a))) for a in range(0, 360, 60)]
            fill = glow if rng.random() < 0.07 else base
            d.polygon(pts, fill=fill, outline=line)
            x += cell * 3
        y += h
        row += 1


def v6_hive(seed=6):
    """V6 'Alien-Nest': organische Waben in Violett und Giftgruen, Schleim-Leuchten, Bio-HUD."""
    img = Image.new("RGBA", (W, H), (14, 6, 20, 255))
    banded_gradient(img, (0, 0, W, H), (30, 8, 40), (8, 20, 14), 8)
    hex_floor(img, (40, 20, 52), (78, 40, 96), (70, 140, 40), seed)
    fx, fy, fw, fh = FIELD
    goo = layer()   # Schleimpfuetzen, transparent
    gd = ImageDraw.Draw(goo)
    rng = random.Random(seed)
    for _ in range(7):
        x, y = fx + rng.randrange(fw), fy + rng.randrange(fh)
        r = rng.randint(6, 14)
        gd.ellipse([x - r, y - r * 0.6, x + r, y + r * 0.6], fill=(120, 230, 60, 70))
    img.alpha_composite(goo)
    d = ImageDraw.Draw(img)
    for x0 in (fx - 6, fx + fw):   # Waende aus Chitin
        for y in range(fy, fy + fh, 6):
            d.ellipse([x0, y, x0 + 5, y + 7], fill=(70, 30, 80), outline=(8, 8, 20))
    draw_scene(img, shields={"p1": ((190, 255, 160), (90, 200, 80), (30, 90, 30)),
                             "p2": ((240, 170, 255), (170, 60, 200), (70, 20, 90))})

    def bar(im, box):
        banded_gradient(im, box, (50, 20, 60), (20, 8, 26), 4)
        x0, y0, x1, y1 = box
        line_y = y1 - 2 if y0 == 0 else y0 + 1
        dd = ImageDraw.Draw(im)
        for x in range(x0, x1, 10):
            dd.ellipse([x, line_y - 2, x + 6, line_y + 2], fill=(110, 220, 60), outline=(8, 8, 20))
    hud_standard(img, {"bar": bar, "empty": (40, 20, 50), "dim": (190, 240, 150), "button": (60, 30, 80)})
    return img


def v7_mars(seed=7):
    """V7 'Mars-Kolonie': rostrote Felsplatten mit Rissen, staubiger Banderhimmel aussen, Kupfer-HUD."""
    img = Image.new("RGBA", (W, H))
    banded_gradient(img, (0, 0, W, H), (240, 150, 90), (90, 30, 30), 12)
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    rng = random.Random(seed)
    for ty in range(fy, fy + fh, 24):
        offset = 12 if (ty - fy) // 24 % 2 else 0
        for tx in range(fx - offset, fx + fw, 24):
            c = rng.choice([(120, 54, 36), (132, 62, 40), (112, 50, 34)])
            d.rectangle([tx, ty, tx + 23, ty + 23], fill=c)
            d.line([(tx, ty), (tx + 23, ty)], fill=(160, 86, 56))
            d.line([(tx, ty + 23), (tx + 23, ty + 23)], fill=(70, 28, 20))
            d.line([(tx + 23, ty), (tx + 23, ty + 23)], fill=(70, 28, 20))
            if rng.random() < 0.35:   # Riss
                x, y = tx + rng.randrange(4, 20), ty + 2
                for _ in range(4):
                    nx, ny = x + rng.randint(-3, 3), y + rng.randint(3, 6)
                    d.line([(x, y), (nx, ny)], fill=(60, 24, 18))
                    x, y = nx, ny
    d.rectangle([0, 0, fx - 1, H], fill=None)
    for x0 in (fx - 5, fx + fw):
        banded_gradient(img, (x0, fy, x0 + 5, fy + fh), (220, 140, 80), (110, 60, 40), 5)
    dust = layer()
    dd = ImageDraw.Draw(dust)
    for _ in range(60):
        x, y = rng.randrange(W), rng.randrange(H)
        dd.point((x, y), fill=(255, 200, 150, 120))
    img.alpha_composite(dust)
    # Feld ausserhalb abdecken, Himmel nur seitlich sichtbar
    draw_scene(img, shields={"p1": ((170, 240, 255), (60, 170, 220), (20, 70, 110)),
                             "p2": ((255, 190, 230), (220, 70, 160), (100, 20, 70))})

    def bar(im, box):
        banded_gradient(im, box, (170, 100, 60), (70, 34, 20), 5)
        x0, y0, x1, y1 = box
        ImageDraw.Draw(im).rectangle([x0, y0, x1 - 1, y1 - 1], outline=(8, 8, 20))
        ImageDraw.Draw(im).line([(x0 + 1, y0 + 1), (x1 - 2, y0 + 1)], fill=(240, 180, 120))
    hud_standard(img, {"bar": bar, "empty": (60, 30, 20), "dim": (255, 220, 180), "button": (110, 50, 30)})
    return img


def v8_ice(seed=8):
    """V8 'Eismond': Eisplatten mit Spiegelglanz und Spruengen, Frost-Raender, glitzernde Kristalle."""
    img = Image.new("RGBA", (W, H))
    banded_gradient(img, (0, 0, W, H), (10, 20, 60), (90, 140, 200), 10)
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    rng = random.Random(seed)
    for ty in range(fy, fy + fh, 32):
        for tx in range(fx, fx + fw, 32):
            banded_gradient(img, (tx, ty, min(tx + 32, fx + fw), min(ty + 32, fy + fh)), (190, 225, 245), (130, 180, 220), 4)
            d.line([(tx, ty), (tx + 31, ty)], fill=(235, 250, 255))
            d.line([(tx, ty), (tx, ty + 31)], fill=(235, 250, 255))
            d.line([(tx + 2, ty + 29), (tx + 12, ty + 19)], fill=(225, 245, 255))   # Spiegelglanz
            if rng.random() < 0.3:
                x, y = tx + rng.randrange(6, 26), ty + rng.randrange(6, 26)
                for a in range(rng.randint(2, 4)):
                    ang = rng.random() * math.tau
                    d.line([(x, y), (x + math.cos(ang) * 8, y + math.sin(ang) * 8)], fill=(100, 150, 200))
    for x0 in (fx - 5, fx + fw):
        d.rectangle([x0, fy, x0 + 4, fy + fh - 1], fill=(240, 252, 255), outline=(60, 110, 170))
        for y in range(fy, fy + fh, 14):
            d.polygon([(x0, y), (x0 + 4, y), (x0 + 2, y + 6)], fill=(200, 235, 255))
    sparkle = layer()
    sd = ImageDraw.Draw(sparkle)
    for _ in range(18):
        x, y = fx + rng.randrange(fw), fy + rng.randrange(fh)
        sd.line([(x - 2, y), (x + 2, y)], fill=(255, 255, 255, 220)); sd.line([(x, y - 2), (x, y + 2)], fill=(255, 255, 255, 220))
    img.alpha_composite(sparkle)
    # dunklere Teamfarben, damit sie auf hellem Eis tragen
    draw_shields(img, {"p1": ((120, 220, 255), (20, 120, 200), (10, 50, 110)),
                       "p2": ((255, 150, 220), (200, 40, 140), (90, 10, 60))})
    draw_player(img, "p2", P2_ANGLE)
    draw_player(img, "p1", P1_ANGLE)
    draw_bolts(img, colors={"p1": (0, 150, 230), "p2": (230, 30, 160)})
    draw_drone(img)

    def bar(im, box):
        banded_gradient(im, box, (230, 245, 255), (140, 190, 230), 5)
        x0, y0, x1, y1 = box
        ImageDraw.Draw(im).rectangle([x0, y0, x1 - 1, y1 - 1], outline=(40, 80, 140))
    hud_standard(img, {"bar": bar, "empty": (170, 200, 230), "dim": (30, 60, 110), "button": (90, 140, 200)})
    return img


def v9_hologram(seed=9, t=0, n=1):
    """V9 'Hologramm': schwarzer Grund, gruenes Holo-Gitter, das sanft 'atmet' (Helligkeit hebt und senkt sich),
    Roehrenfernseher-Bildzeilen, Holo-HUD."""
    img = Image.new("RGBA", (W, H), (0, 6, 4, 255))
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    breath = 0.5 + 0.5 * math.sin(2 * math.pi * t / n)
    grid = mix((12, 66, 38), (16, 82, 48), breath)   # nur leichter Helligkeitsunterschied
    glow = layer()
    gd = ImageDraw.Draw(glow)
    for x in range(fx, fx + fw, 16):
        gd.line([(x - 1, fy), (x - 1, fy + fh)], fill=rgba(grid, int(18 * breath)))
        gd.line([(x + 1, fy), (x + 1, fy + fh)], fill=rgba(grid, int(18 * breath)))
    img.alpha_composite(glow)
    for x in range(fx, fx + fw, 16):
        d.line([(x, fy), (x, fy + fh)], fill=grid)
    for y in range(fy, fy + fh, 16):
        d.line([(fx, y), (fx + fw, y)], fill=grid)
    my = fy + fh // 2
    d.line([(fx, my), (fx + fw, my)], fill=(60, 220, 120))
    for x0 in (fx - 2, fx + fw):
        d.rectangle([x0, fy, x0 + 1, fy + fh - 1], fill=(80, 255, 150))
    for cx, cy in ((fx, fy), (fx + fw, fy), (fx, fy + fh), (fx + fw, fy + fh)):   # Eckmarken
        d.line([(cx - 6, cy), (cx + 6, cy)], fill=(160, 255, 200)); d.line([(cx, cy - 6), (cx, cy + 6)], fill=(160, 255, 200))
    draw_scene(img, shields={"p1": ((180, 255, 255), (60, 200, 220), (20, 80, 100)),
                             "p2": ((255, 190, 240), (220, 70, 170), (90, 20, 70))}, bolt_seed=t)

    def bar(im, box):
        x0, y0, x1, y1 = box
        dd = ImageDraw.Draw(im)
        dd.rectangle([x0, y0, x1 - 1, y1 - 1], fill=(0, 20, 12))
        dd.rectangle([x0 + 2, y0 + 2, x1 - 3, y1 - 3], outline=(60, 220, 120))
    hud_standard(img, {"bar": bar, "empty": (10, 50, 30), "dim": (120, 255, 170), "button": (10, 60, 30)})
    scan = layer()   # Bildzeilen: jede zweite Zeile abgedunkelt
    sd = ImageDraw.Draw(scan)
    for y in range(0, H, 2):
        sd.line([(0, y), (W, y)], fill=(0, 0, 0, 70))
    img.alpha_composite(scan)
    return img


LAVA_RAMP = [(110, 26, 8), (140, 38, 10), (170, 54, 12), (200, 74, 16), (225, 100, 22)]


def v10_lava(seed=10, t=0, n=1):
    """V10 'Lava-Schmiede': Basaltplatten mit gluehenden Spalten; die Lava pulsiert und fliesst dezent
    (wandernde Helligkeitswellen in 5 Stufen), Eisen-HUD."""
    img = Image.new("RGBA", (W, H))
    banded_gradient(img, (0, 0, W, H), (40, 8, 4), (160, 50, 10), 10)
    d = ImageDraw.Draw(img)
    fx, fy, fw, fh = FIELD
    px = img.load()
    phase = 2 * math.pi * t / n
    for y in range(fy, fy + fh):   # Lava darunter: zwei langsame Wellen, Stufen statt Verlauf
        for x in range(fx, fx + fw):
            v = 0.5 + 0.09 * math.sin(phase + x * 0.045 + y * 0.03) + 0.07 * math.sin(-phase + y * 0.07 - x * 0.02)
            px[x, y] = rgba(LAVA_RAMP[min(4, max(0, int(v * 5)))])
    rng = random.Random(seed)
    for ty in range(fy, fy + fh, 20):
        for tx in range(fx, fx + fw, 20):
            jx, jy = rng.randint(0, 1), rng.randint(0, 1)
            c = rng.choice([(38, 34, 40), (46, 40, 46), (32, 28, 34)])
            d.rectangle([tx + jx, ty + jy, tx + 19, ty + 19], fill=c)   # schmale Spalten bleiben als Lava sichtbar
            d.line([(tx + jx, ty + jy), (tx + 19, ty + jy)], fill=(70, 62, 70))
            d.line([(tx + jx, ty + 19), (tx + 19, ty + 19)], fill=(20, 16, 22))
    heat = layer()
    hd = ImageDraw.Draw(heat)
    for x0 in (fx - 6, fx + fw):
        hd.rectangle([x0, fy, x0 + 5, fy + fh - 1], fill=(60, 50, 56, 255))
        hd.line([(x0, fy), (x0, fy + fh - 1)], fill=(120, 110, 120, 255))
    img.alpha_composite(heat)
    draw_scene(img, shields={"p1": ((170, 240, 255), (50, 160, 220), (20, 70, 110)),
                             "p2": ((255, 190, 230), (220, 60, 160), (100, 20, 70))}, bolt_seed=t)

    def bar(im, box):
        banded_gradient(im, box, (80, 72, 80), (30, 26, 32), 5)
        x0, y0, x1, y1 = box
        dd = ImageDraw.Draw(im)
        dd.rectangle([x0, y0, x1 - 1, y1 - 1], outline=(8, 8, 20))
        line_y = y1 - 2 if y0 == 0 else y0 + 1
        dd.line([(x0, line_y), (x1, line_y)], fill=(255, 140, 30))
    hud_standard(img, {"bar": bar, "empty": (40, 34, 40), "dim": (255, 200, 140), "button": (90, 40, 20)})
    return img


def draw_scene(img, shields, bolt_seed=0, before_sprites=None):
    draw_shields(img, shields)
    draw_player(img, "p2", P2_ANGLE)
    draw_player(img, "p1", P1_ANGLE)
    draw_bolts(img, seed=bolt_seed)
    draw_drone(img)


VARIANTS = [
    ("v1-mode7-arena", "1 MODE-7-ARENA", v1_mode7),
    ("v2-raumstation", "2 RAUMSTATION", v2_station),
    ("v3-sternen-parallaxe", "3 STERNEN-PARALLAXE", v3_parallax),
    ("v4-arcade-chrom", "4 ARCADE-CHROM", v4_arcade),
    ("v5-energiekern", "5 ENERGIEKERN", v5_circuit),
    ("v6-alien-nest", "6 ALIEN-NEST", v6_hive),
    ("v7-mars-kolonie", "7 MARS-KOLONIE", v7_mars),
    ("v8-eismond", "8 EISMOND", v8_ice),
    ("v9-hologramm", "9 HOLOGRAMM", v9_hologram),
    ("v10-lava-schmiede", "10 LAVA-SCHMIEDE", v10_lava),
]


def main(out: Path):
    out.mkdir(parents=True, exist_ok=True)
    thumbs = []
    for key, title, fn in VARIANTS:
        img = quantize(fn())
        colors = len(img.getcolors(1 << 24))
        assert colors <= 256
        img.resize((W * SCALE, H * SCALE), Image.Resampling.NEAREST).save(out / f"{key}.png")
        print(f"{key:24s} {colors:3d} Farben")
        thumbs.append((title, img))
    pad = 14
    for part, chunk in enumerate((thumbs[:5], thumbs[5:]), start=1):
        sheet = Image.new("RGB", (len(chunk) * (W + pad) + pad, H + 40), (24, 24, 28))
        for i, (title, im) in enumerate(chunk):
            x = pad + i * (W + pad)
            draw_text(sheet, title.replace("-", " "), x, 12, (230, 230, 230))
            sheet.paste(im, (x, 32))
        sheet.save(out / ("uebersicht.png" if part == 1 else "uebersicht-2.png"))

    # Mode-7-Vorschau: Boden dreht sich (36 Bilder = Vierteldrehung, nahtlos wiederholbar)
    frames = [quantize(v1_mode7(rotation_deg=28 + k * 90 / 36)).resize((W * 2, H * 2), Image.Resampling.NEAREST)
              for k in range(36)]
    frames[0].save(out / "v1-mode7-rotation.gif", save_all=True, append_images=frames[1:], duration=60, loop=0)
    print("v1-mode7-rotation.gif    36 Bilder")

def selection(out: Path):
    """Vom Anwender gewaehlte Vorschlaege: Standbilder (2, 9) und animierte Vorschauen (3, 4, 10)."""
    out.mkdir(parents=True, exist_ok=True)
    stills = [("v2-raumstation", "2 RAUMSTATION", v2_station), ("v4-arcade-chrom", "4 ARCADE-CHROM", v4_arcade)]
    anims = [("v3-sternenflug", "3 STERNENFLUG", v3_parallax, 120),
             ("v5-energiekern", "5 ENERGIEKERN", v5_circuit, 90),
             ("v9-hologramm", "9 HOLOGRAMM", v9_hologram, 60),
             ("v10-lava-schmiede", "10 LAVA-SCHMIEDE", v10_lava, 60)]
    sheet_items = []
    original = Path(__file__).resolve().parents[2] / "snapshots" / "modern-1p-hoch.png"
    if original.exists():   # bisheriger Spiel-Look (gerendert vom Spielcode), zum Vergleich
        im = Image.open(original).convert("RGB")
        sheet_items.append(("0 ORIGINAL", im.resize((W, int(im.height * W / im.width)), Image.Resampling.NEAREST)))
    for key, title, fn in stills:
        img = quantize(fn())
        img.resize((W * SCALE, H * SCALE), Image.Resampling.NEAREST).save(out / f"{key}.png")
        sheet_items.append((title, img))
        print(f"{key:28s} {len(img.getcolors(1 << 24)):3d} Farben")
    for key, title, fn, frames in anims:
        imgs = [quantize(fn(t=k, n=frames)) for k in range(frames)]
        assert all(len(i.getcolors(1 << 24)) <= 256 for i in imgs)
        big = [i.resize((W * 2, H * 2), Image.Resampling.NEAREST) for i in imgs]
        big[0].save(out / f"{key}.gif", save_all=True, append_images=big[1:], duration=50, loop=0)
        imgs[0].resize((W * SCALE, H * SCALE), Image.Resampling.NEAREST).save(out / f"{key}.png")
        sheet_items.append((title, imgs[0]))
        print(f"{key:28s} {frames} Bilder, max {max(len(i.getcolors(1 << 24)) for i in imgs)} Farben")
    pad = 14
    sheet = Image.new("RGB", (len(sheet_items) * (W + pad) + pad, H + 40), (24, 24, 28))
    for i, (title, im) in enumerate(sheet_items):
        x = pad + i * (W + pad)
        draw_text(sheet, title.replace("-", " "), x, 12, (230, 230, 230))
        sheet.paste(im, (x, 32))
    sheet.save(out / "uebersicht-auswahl.png")


if __name__ == "__main__":
    if len(sys.argv) > 2 and sys.argv[2] == "--auswahl":
        selection(Path(sys.argv[1]))
        sys.exit(0)
    main(Path(sys.argv[1] if len(sys.argv) > 1 else "mockups/snes"))
