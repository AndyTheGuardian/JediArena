"""Erzeugt die Gestaltungs-Mockups fuer "Lazer Arena".

Zwei Grafikstile:
  * classic - grobe Pixeloptik im Stil des Atari VCS (wenige Farben, flache Flaechen)
  * modern  - 16-Bit-Retro-Pixeloptik, Palette aus 10x10x10 = 1000 Farben (Bayer-Dithering)

Ausgabe (je Stil): Titel (hoch), 1 Spieler hoch/quer, 2 Spieler hoch/quer, plus Uebersicht.
Aufruf:  python3 make_mockups.py <ausgabeordner>
Benoetigt nur Pillow.
"""
from __future__ import annotations

import math
import random
import sys
from dataclasses import dataclass, field
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

Color = tuple[int, int, int]

# --------------------------------------------------------------------------- Bitmap-Font 5x7

_GLYPHS = {
    "A": ".###.|#...#|#...#|#####|#...#|#...#|#...#",
    "B": "####.|#...#|#...#|####.|#...#|#...#|####.",
    "C": ".###.|#...#|#....|#....|#....|#...#|.###.",
    "D": "####.|#...#|#...#|#...#|#...#|#...#|####.",
    "E": "#####|#....|#....|####.|#....|#....|#####",
    "F": "#####|#....|#....|####.|#....|#....|#....",
    "G": ".###.|#...#|#....|#.###|#...#|#...#|.####",
    "H": "#...#|#...#|#...#|#####|#...#|#...#|#...#",
    "I": ".###.|..#..|..#..|..#..|..#..|..#..|.###.",
    "J": "..###|...#.|...#.|...#.|...#.|#..#.|.##..",
    "K": "#...#|#..#.|#.#..|##...|#.#..|#..#.|#...#",
    "L": "#....|#....|#....|#....|#....|#....|#####",
    "M": "#...#|##.##|#.#.#|#.#.#|#...#|#...#|#...#",
    "N": "#...#|#...#|##..#|#.#.#|#..##|#...#|#...#",
    "O": ".###.|#...#|#...#|#...#|#...#|#...#|.###.",
    "P": "####.|#...#|#...#|####.|#....|#....|#....",
    "Q": ".###.|#...#|#...#|#...#|#.#.#|#..#.|.##.#",
    "R": "####.|#...#|#...#|####.|#.#..|#..#.|#...#",
    "S": ".####|#....|#....|.###.|....#|....#|####.",
    "T": "#####|..#..|..#..|..#..|..#..|..#..|..#..",
    "U": "#...#|#...#|#...#|#...#|#...#|#...#|.###.",
    "V": "#...#|#...#|#...#|#...#|#...#|.#.#.|..#..",
    "W": "#...#|#...#|#...#|#.#.#|#.#.#|#.#.#|.#.#.",
    "X": "#...#|#...#|.#.#.|..#..|.#.#.|#...#|#...#",
    "Y": "#...#|#...#|.#.#.|..#..|..#..|..#..|..#..",
    "Z": "#####|....#|...#.|..#..|.#...|#....|#####",
    "0": ".###.|#...#|#..##|#.#.#|##..#|#...#|.###.",
    "1": "..#..|.##..|..#..|..#..|..#..|..#..|.###.",
    "2": ".###.|#...#|....#|...#.|..#..|.#...|#####",
    "3": "#####|...#.|..#..|...#.|....#|#...#|.###.",
    "4": "...#.|..##.|.#.#.|#..#.|#####|...#.|...#.",
    "5": "#####|#....|####.|....#|....#|#...#|.###.",
    "6": "..##.|.#...|#....|####.|#...#|#...#|.###.",
    "7": "#####|....#|...#.|..#..|.#...|.#...|.#...",
    "8": ".###.|#...#|#...#|.###.|#...#|#...#|.###.",
    "9": ".###.|#...#|#...#|.####|....#|...#.|.##..",
    ":": ".....|..#..|..#..|.....|..#..|..#..|.....",
    "!": "..#..|..#..|..#..|..#..|..#..|.....|..#..",
    "-": ".....|.....|.....|#####|.....|.....|.....",
    ".": ".....|.....|.....|.....|.....|.....|..#..",
    "=": ".....|.....|#####|.....|#####|.....|.....",
    "Ä": ".#.#.|.....|.###.|#...#|#####|#...#|#...#",
    "Ö": ".#.#.|.....|.###.|#...#|#...#|#...#|.###.",
    "Ü": ".#.#.|.....|#...#|#...#|#...#|#...#|.###.",
    ">": ".#...|..#..|...#.|....#|...#.|..#..|.#...",
    "<": "...#.|..#..|.#...|#....|.#...|..#..|...#.",
    " ": ".....|.....|.....|.....|.....|.....|.....",
}
GLYPH_W, GLYPH_H = 5, 7


def text_width(text: str, scale: int = 1) -> int:
    return (len(text) * (GLYPH_W + 1) - 1) * scale


def text_pixels(text: str, scale: int = 1):
    """Liefert alle gesetzten Pixel (x, y, zeile) relativ zur linken oberen Ecke."""
    for i, ch in enumerate(text.upper()):
        rows = _GLYPHS[ch].split("|")
        for gy, row in enumerate(rows):
            for gx, cell in enumerate(row):
                if cell != "#":
                    continue
                ox = (i * (GLYPH_W + 1) + gx) * scale
                oy = gy * scale
                for sy in range(scale):
                    for sx in range(scale):
                        yield ox + sx, oy + sy, gy


def draw_text(img: Image.Image, text: str, x: int, y: int, color, scale: int = 1,
              align: str = "left", shadow: Color | None = None) -> None:
    """color: feste Farbe oder Funktion(zeile, y_rel) -> Farbe."""
    if align == "center":
        x -= text_width(text, scale) // 2
    elif align == "right":
        x -= text_width(text, scale)
    px = img.load()
    w, h = img.size
    pixels = list(text_pixels(text, scale))
    if shadow:
        for dx, dy, _ in pixels:
            sx, sy = x + dx + scale, y + dy + scale
            if 0 <= sx < w and 0 <= sy < h:
                px[sx, sy] = shadow
    for dx, dy, row in pixels:
        tx, ty = x + dx, y + dy
        if 0 <= tx < w and 0 <= ty < h:
            px[tx, ty] = color(row, dy) if callable(color) else color


# --------------------------------------------------------------------------- Sprache

TEXTS = {
    "de": {"ai": "KI", "level": "MITTEL", "storm_short": "GEWITTER!", "storm": "LASERGEWITTER!",
           "p1_game": "1 SPIELER", "p2_game": "2 SPIELER", "style_classic": "STIL: KLASSISCH",
           "style_modern": "STIL: MODERN", "options": "OPTIONEN", "sound_on": "TON AN",
           "music_on": "MUSIK AN", "tagline": "DUELL IM ORBIT", "swipe": "WISCHEN", "sword": "= SCHWERT",
           "tap": "TIPPEN", "fire": "= FEUER", "overview_style": "STIL", "back": "ZURÜCK",
           "music_label": "MUSIK", "val_classic": "KLASSISCH",
           "opt_items": [("GRAFIKSTIL", "MODERN"), ("SPRACHE", "AUTOMATISCH"), ("KI-STUFE", "MITTEL"),
                         ("SCHWERTDREHUNG", "BEGRENZT"), ("ANGEDOCKTER BLITZ", "VERPUFFT"),
                         ("SCHILDFORM", "GEBOGEN"), ("SEITENRAND", "ABKNICKEN"), ("TON", "AN"),
                         ("MUSIK", "AN")],
           "titles": ["TITEL", "TITEL QUER", "OPTIONEN", "1 SPIELER HOCH", "1 SP. HOCH GEBOGEN", "2 SPIELER HOCH",
                      "1 SPIELER QUER", "2 SPIELER QUER"]},
    "en": {"ai": "CPU", "level": "MEDIUM", "storm_short": "STORM!", "storm": "LASER STORM!",
           "p1_game": "1 PLAYER", "p2_game": "2 PLAYERS", "style_classic": "STYLE: CLASSIC",
           "style_modern": "STYLE: MODERN", "options": "OPTIONS", "sound_on": "SOUND ON",
           "music_on": "MUSIC ON", "tagline": "DUEL IN ORBIT", "swipe": "SWIPE", "sword": "= SWORD",
           "tap": "TAP", "fire": "= FIRE", "overview_style": "STYLE", "back": "BACK",
           "music_label": "MUSIC", "val_classic": "CLASSIC",
           "opt_items": [("GRAPHICS STYLE", "MODERN"), ("LANGUAGE", "AUTOMATIC"), ("CPU LEVEL", "MEDIUM"),
                         ("SWORD TURNING", "LIMITED"), ("DOCKED BOLT", "FIZZLES"),
                         ("SHIELD SHAPE", "CURVED"), ("SIDE WALLS", "BOUNCE"), ("SOUND", "ON"),
                         ("MUSIC", "ON")],
           "titles": ["TITLE", "TITLE LANDSCAPE", "OPTIONS", "1 PLAYER PORTRAIT", "1P PORTRAIT CURVED", "2 PLAYERS PORTRAIT",
                      "1 PLAYER LANDSCAPE", "2 PLAYERS LANDSCAPE"]},
}
_lang = "de"


def set_language(lang: str) -> None:
    global _lang
    _lang = lang


def tr(key: str):
    return TEXTS[_lang][key]


# --------------------------------------------------------------------------- Szenenmodell

@dataclass
class Laser:
    x: float          # Kopf, normiert 0..1 auf Spielfeldbreite
    y: float          # Kopf, normiert 0..1 auf Spielfeldhoehe
    owner: str        # "p1", "p2" oder "n" (neutral, Lasergewitter)


@dataclass
class Scene:
    two_players: bool
    p1_angle: float
    p2_angle: float
    drone_x: float
    score_p1: int
    score_p2: int
    lasers: list[Laser] = field(default_factory=list)
    sparks: list[tuple[float, float]] = field(default_factory=list)
    p1_broken: set[tuple[int, int]] = field(default_factory=set)
    p2_broken: set[tuple[int, int]] = field(default_factory=set)
    p1_regrow: set[tuple[int, int]] = field(default_factory=set)
    p2_regrow: set[tuple[int, int]] = field(default_factory=set)
    storm: bool = False
    p1_docked: str | None = None  # Besitzer des am Schwert angedockten Blitzes
    p2_docked: str | None = None
    shield_shape: str = "straight"  # "straight" oder "curved"


SHIELD_SEGMENTS = 16
FIELD_ASPECT = 0.53  # Breite / Hoehe des Spielfelds

# Layout im Spielfeld (normiert auf Hoehe)
P2_Y, P1_Y = 0.065, 0.935
P2_SHIELD_Y = (0.17, 0.145)  # (vordere Reihe, hintere Reihe)
P1_SHIELD_Y = (0.83, 0.855)
SHIELD_ROW_H = 0.022
DRONE_Y = 0.5


def sword_vector(player: str, angle: float) -> tuple[float, float]:
    """Richtungsvektor in Bildschirmkoordinaten (y nach unten)."""
    a = math.radians(angle)
    if player == "p1":
        return math.sin(a), -math.cos(a)
    return -math.sin(a), math.cos(a)


SWORD_HAND, SWORD_TIP = 0.06, 0.28  # Abstand vom Koerpermittelpunkt, in Feldbreiten
CURVED_ARC_DEG = 30                  # gebogenes Schild: flacher Bogen von -30 bis +30 Grad
CURVED_RADII = (0.90, 0.84)          # vordere, hintere Reihe, in Feldbreiten
CURVED_CENTER_BEHIND = 0.72          # Kreismittelpunkt liegt so weit hinter der Figur (Feldbreiten)


def player_center(rect, p):
    fx, fy, fw, fh = rect
    return fx + fw / 2, fy + (P1_Y if p == "p1" else P2_Y) * fh


def drone_center(rect, s):
    fx, fy, fw, fh = rect
    return fx + s.drone_x * fw, fy + DRONE_Y * fh


def sword_points(rect, p, angle):
    """(Hand, Spitze) des Schwerts in Pixeln."""
    cx, cy = player_center(rect, p)
    vx, vy = sword_vector(p, angle)
    fw = rect[2]
    return (cx + vx * SWORD_HAND * fw, cy + vy * SWORD_HAND * fw), (cx + vx * SWORD_TIP * fw, cy + vy * SWORD_TIP * fw)


def laser_head(rect, l):
    """Blitzspitze in Pixeln."""
    fx, fy, fw, fh = rect
    return fx + l.x * fw, fy + l.y * fh


def zigzag(p0, p1, step, amp, seed):
    """Zickzack-Linie von p0 nach p1 mit wechselndem seitlichem Ausschlag."""
    rng = random.Random(seed)
    (x0, y0), (x1, y1) = p0, p1
    dx, dy = x1 - x0, y1 - y0
    length = math.hypot(dx, dy) or 1.0
    nx, ny = -dy / length, dx / length
    n = max(2, int(length / step))
    pts = [p0]
    for k in range(1, n):
        t = k / n
        off = amp * (1 if k % 2 else -1) * (0.5 + rng.random() * 0.5)
        pts.append((x0 + dx * t + nx * off, y0 + dy * t + ny * off))
    pts.append(p1)
    return pts


def shield_segments(rect, p, shape):
    """Liefert (reihe, index, x0, y0, x1, y1) aller Schildsegmente; Reihe 0 = vordere Reihe."""
    fx, fy, fw, fh = rect
    row_h = max(2, int(SHIELD_ROW_H * fh))
    out = []
    if shape == "straight":
        ys = P1_SHIELD_Y if p == "p1" else P2_SHIELD_Y
        seg_w = fw / SHIELD_SEGMENTS
        for row, ny in enumerate(ys):
            y0 = fy + int(ny * fh)
            for i in range(SHIELD_SEGMENTS):
                out.append((row, i, fx + int(i * seg_w) + 1, y0, fx + int((i + 1) * seg_w) - 1, y0 + row_h - 1))
        return out
    cx, cy = player_center(rect, p)
    sign = -1 if p == "p1" else 1
    cy -= sign * CURVED_CENTER_BEHIND * fw
    for row, rr in enumerate(CURVED_RADII):
        r = rr * fw
        half = max(1, int(r * math.radians(2 * CURVED_ARC_DEG) / SHIELD_SEGMENTS / 2) - 1)
        for i in range(SHIELD_SEGMENTS):
            a = math.radians(-CURVED_ARC_DEG + (i + 0.5) * 2 * CURVED_ARC_DEG / SHIELD_SEGMENTS)
            x, y = cx + r * math.sin(a), cy + sign * r * math.cos(a)
            y0 = int(y - row_h / 2)
            out.append((row, i, int(x - half), y0, int(x + half), y0 + row_h - 1))
    return out


# --------------------------------------------------------------------------- Stile

class Style:
    name: str
    scale: int
    portrait: tuple[int, int]

    def new_image(self, w, h): ...
    def finish(self, img: Image.Image) -> Image.Image: return img


class ClassicStyle(Style):
    """Atari-VCS-Optik: wenige flache Farben, sehr grobe Pixel."""
    name = "klassisch"
    scale = 9
    portrait = (120, 260)

    BLACK = (0, 0, 0)
    WALL = (110, 110, 110)
    P1 = (84, 138, 255)     # Blau
    P1_DARK = (24, 59, 157)
    P2 = (236, 112, 72)     # Orange-Rot
    P2_DARK = (148, 40, 16)
    DRONE = (252, 224, 112)
    NEUTRAL = (236, 236, 236)
    BLADE_P1 = (160, 220, 255)
    BLADE_P2 = (255, 170, 140)
    TEXT = (236, 236, 236)
    GREY = (140, 140, 140)
    HILITE = (252, 224, 112)
    LOGO_BANDS = [(252, 252, 84), (252, 188, 116), (252, 148, 92), (236, 112, 72),
                  (200, 72, 72), (176, 60, 140), (120, 72, 200)]

    def new_image(self, w, h):
        return Image.new("RGB", (w, h), self.BLACK)

    def team(self, p):
        return {"p1": self.P1, "p2": self.P2, "n": self.NEUTRAL}[p]

    # ---- Spielfeld
    def draw_field(self, img, rect, s: Scene):
        fx, fy, fw, fh = rect
        d = ImageDraw.Draw(img)
        d.rectangle([fx - 2, fy, fx - 1, fy + fh - 1], fill=self.WALL)
        d.rectangle([fx + fw, fy, fx + fw + 1, fy + fh - 1], fill=self.WALL)
        # Mittellinie (Drohnenbahn), gestrichelt
        my = fy + int(DRONE_Y * fh)
        for x in range(fx, fx + fw, 4):
            d.point((x, my + 6), fill=(40, 40, 40))
        self._shield(d, rect, "p2", s.shield_shape, s.p2_broken, s.p2_regrow, self.P2, self.P2_DARK)
        self._shield(d, rect, "p1", s.shield_shape, s.p1_broken, s.p1_regrow, self.P1, self.P1_DARK)
        self._player(d, rect, "p2", s.p2_angle)
        self._player(d, rect, "p1", s.p1_angle)
        for laser in s.lasers:
            self._laser(d, rect, laser, drone_center(rect, s))
        self._docked_beams(d, rect, s)
        self._drone(img, rect, s)
        for sx, sy in s.sparks:
            cx, cy = fx + int(sx * fw), fy + int(sy * fh)
            for dx, dy in [(0, -2), (0, 2), (-2, 0), (2, 0), (0, 0)]:
                d.point((cx + dx, cy + dy), fill=self.HILITE)
        if s.storm:
            self._storm_banner(img, rect)

    def _shield(self, d, rect, p, shape, broken, regrow, col, dark):
        for row, i, x0, y0, x1, y1 in shield_segments(rect, p, shape):
            if (row, i) in broken:
                continue
            if (row, i) in regrow:
                d.rectangle([x0, y0, x1, y0], fill=dark)
                continue
            d.rectangle([x0, y0, x1, y1], fill=col if row == 0 else dark)

    def _player(self, d, rect, p, angle):
        cx, cy = (round(v) for v in player_center(rect, p))
        col = self.team(p)
        # Draufsicht: Schultern + Kopf, blockig
        d.rectangle([cx - 4, cy - 1, cx + 4, cy + 1], fill=col)
        d.rectangle([cx - 1, cy - 2, cx + 1, cy + 2], fill=self.TEXT)
        hand, tip = sword_points(rect, p, angle)
        d.line([tuple(round(v) for v in hand), tuple(round(v) for v in tip)],
               fill=self.BLADE_P1 if p == "p1" else self.BLADE_P2, width=1)

    @staticmethod
    def _bolt(d, pts, col):
        d.line([tuple(round(v) for v in pt) for pt in pts], fill=col, width=1)

    def _laser(self, d, rect, l: Laser, origin):
        """Blitz zuckt von der Drohne (origin) bis zur Blitzspitze."""
        self._bolt(d, zigzag(origin, laser_head(rect, l), 4, 1.6, seed=int(l.x * 1000 + l.y * 100)),
                   self.team(l.owner))

    def _docked_beams(self, d, rect, s):
        """Angedockter Blitz: Zickzack-Linie von der Drohne bis zur Schwertspitze."""
        for p, owner, angle in (("p1", s.p1_docked, s.p1_angle), ("p2", s.p2_docked, s.p2_angle)):
            if not owner:
                continue
            _, tip = sword_points(rect, p, angle)
            self._bolt(d, zigzag(drone_center(rect, s), tip, 4, 2, seed=len(p) + int(angle)), self.team(owner))
            tx, ty = round(tip[0]), round(tip[1])
            d.rectangle([tx - 1, ty - 1, tx + 1, ty + 1], fill=self.team(owner))

    # Drohne in Draufsicht: Ring mit Auge und vier Stutzen
    _DRONE_SPRITE = ["....#....", "..#####..", ".##...##.", ".#..#..#.", "##.###.##",
                     ".#..#..#.", ".##...##.", "..#####..", "....#...."]

    def _drone(self, img, rect, s):
        cx, cy = (round(v) for v in drone_center(rect, s))
        col = (236, 72, 72) if s.storm else self.DRONE
        d = ImageDraw.Draw(img)
        for y, row in enumerate(self._DRONE_SPRITE):
            for x, c in enumerate(row):
                if c == "#":
                    d.point((cx - 4 + x, cy - 4 + y), fill=col)

    def _storm_banner(self, img, rect):
        fx, fy, fw, fh = rect
        my = fy + int(DRONE_Y * fh)
        band = Image.new("RGB", (fw, 9), (148, 40, 16))
        draw_text(band, tr("storm_short"), fw // 2, 1, self.TEXT, align="center")
        img.paste(band, (fx, my + 8))
        img.paste(band.transpose(Image.Transpose.ROTATE_180), (fx, my - 16))

    # ---- HUD
    def hud_band(self, w, h, label, score, col, pause=False, extra=""):
        band = Image.new("RGB", (w, h), self.BLACK)
        draw_text(band, label, 3, (h - 7) // 2, col)
        draw_text(band, str(score), w // 2, (h - 14) // 2, col, scale=2, align="center")
        if pause:
            self.pause_icon(band, w - 10, (h - 7) // 2)
        if extra:
            draw_text(band, extra, w - 14, (h - 7) // 2, self.GREY, align="right")
        return band

    def pause_icon(self, img, x, y):
        d = ImageDraw.Draw(img)
        d.rectangle([x, y, x + 1, y + 6], fill=self.GREY)
        d.rectangle([x + 4, y, x + 5, y + 6], fill=self.GREY)

    def side_panel(self, w, h, lines, col):
        panel = Image.new("RGB", (w, h), self.BLACK)
        y = 0
        for text, scale, c in lines:
            if text:
                draw_text(panel, text, w // 2, y, c or col, scale=scale, align="center")
            y += 7 * scale + 4
        return panel

    # ---- Optionen
    def draw_options(self, img, items, selected):
        w, h = img.size
        draw_text(img, tr("options"), w // 2, 10, self.HILITE, scale=2, align="center")
        y = 34
        for i, (label, value) in enumerate(items):
            sel = i == selected
            draw_text(img, label, w // 2, y, self.GREY, align="center")
            col = self.HILITE if sel else self.TEXT
            draw_text(img, f"< {value} >" if sel else value, w // 2, y + 10, col, align="center")
            y += 24
        draw_text(img, tr("back"), w // 2, h - 14, self.GREY, align="center")

    # ---- Titel
    def draw_title(self, img, selected_style_label):
        w, h = img.size
        land = w > h
        bands = self.LOGO_BANDS
        logo_x = 70 if land else w // 2
        logo_y = 14 if land else 26
        draw_text(img, "LAZER", logo_x, logo_y, lambda row, _y: bands[row], scale=3, align="center")
        draw_text(img, "ARENA", logo_x, logo_y + 28, lambda row, _y: bands[6 - row], scale=3, align="center")
        # kleine Szene: zwei Figuren, Drohne, Blitze
        rect = (40, 70, 60, 44) if land else (20, 82, 80, 72)
        demo = Scene(two_players=True, p1_angle=-25, p2_angle=20, drone_x=0.55, score_p1=0, score_p2=0,
                     p2_docked="p1")
        d = ImageDraw.Draw(img)
        self._player(d, rect, "p1", demo.p1_angle)
        self._player(d, rect, "p2", demo.p2_angle)
        self._laser(d, rect, Laser(0.85, 0.85, "p2"), drone_center(rect, demo))
        self._docked_beams(d, rect, demo)
        self._drone(img, rect, demo)
        items = [tr("p1_game"), tr("p2_game"), selected_style_label, tr("options")]
        menu_x, menu_y = (195, 30) if land else (w // 2, 172)
        for i, item in enumerate(items):
            y = menu_y + i * 16
            col = self.HILITE if i == 0 else self.GREY
            draw_text(img, item, menu_x, y, col, align="center")
            if i == 0:
                draw_text(img, ">", menu_x - text_width(item) // 2 - 9, y, col)
        draw_text(img, tr("sound_on"), 4, h - 10, self.GREY)
        draw_text(img, "V0.1", w - 4, h - 10, (70, 70, 70), align="right")

class ModernStyle(Style):
    """16-Bit-Retro-Optik: Verlaeufe, Glow, Schattierung; Palette 10x10x10 mit Bayer-Dithering."""
    name = "modern"
    scale = 4
    portrait = (270, 585)

    P1 = (60, 220, 255)       # Cyan
    P1_ARMOR = (40, 80, 140)
    P2 = (255, 70, 190)       # Magenta
    P2_ARMOR = (120, 30, 90)
    NEUTRAL = (255, 210, 60)
    TEXT = (235, 240, 255)
    DIM = (120, 130, 160)
    LEVELS = 10  # je Kanal -> 1000 Farben

    _BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]

    def __init__(self):
        self.rng = random.Random(7)

    def team(self, p):
        return {"p1": self.P1, "p2": self.P2, "n": self.NEUTRAL}[p]

    # ---- Hintergrund
    def new_image(self, w, h):
        img = Image.new("RGB", (w, h))
        noise_a = Image.effect_noise((max(2, w // 40), max(2, h // 40)), 80).resize((w, h), Image.Resampling.BICUBIC)
        noise_b = Image.effect_noise((max(2, w // 25), max(2, h // 25)), 80).resize((w, h), Image.Resampling.BICUBIC)
        px, pa, pb = img.load(), noise_a.load(), noise_b.load()
        for y in range(h):
            t = y / h
            for x in range(w):
                a = max(0.0, (pa[x, y] - 128) / 127)
                b = max(0.0, (pb[x, y] - 128) / 127)
                r = 8 + 70 * a * a + 10 * t
                g = 8 + 20 * a * a + 60 * b * b
                bl = 22 + 70 * a * a + 70 * b * b + 20 * (1 - t)
                px[x, y] = (int(r), int(g), int(bl))
        rng = random.Random(w * 31 + h)
        for _ in range(w * h // 220):
            x, y = rng.randrange(w), rng.randrange(h)
            v = rng.choice([90, 140, 200, 255])
            px[x, y] = (v, v, min(255, v + 30))
        return img

    def finish(self, img):
        """Auf die 1000-Farben-Palette (10 Stufen je Kanal) mit geordnetem Dithering abbilden."""
        out = img.copy()
        px = out.load()
        w, h = out.size
        steps = self.LEVELS - 1
        for y in range(h):
            row = self._BAYER[y % 4]
            for x in range(w):
                t = (row[x % 4] + 0.5) / 16
                r, g, b = px[x, y]
                px[x, y] = tuple(min(steps, int(c / 255 * steps + t)) * 255 // steps for c in (r, g, b))
        return out

    # ---- Hilfen
    @staticmethod
    def glow(img, draw_fn, radius, strength=1.0):
        layer = Image.new("RGB", img.size, (0, 0, 0))
        draw_fn(ImageDraw.Draw(layer))
        blurred = layer.filter(ImageFilter.GaussianBlur(radius))
        if strength != 1.0:
            blurred = blurred.point(lambda v: min(255, int(v * strength)))
        img.paste(ImageChops.add(img, blurred))

    @staticmethod
    def shaded_ellipse(img, cx, cy, rx, ry, col, light=(-0.5, -0.6)):
        px = img.load()
        w, h = img.size
        for y in range(int(cy - ry), int(cy + ry) + 1):
            for x in range(int(cx - rx), int(cx + rx) + 1):
                nx, ny = (x - cx) / rx, (y - cy) / ry
                dd = nx * nx + ny * ny
                if dd > 1 or not (0 <= x < w and 0 <= y < h):
                    continue
                nz = math.sqrt(1 - dd)
                lum = 0.35 + 0.65 * max(0.0, -(nx * light[0] + ny * light[1]) * 0.8 + nz * 0.6)
                spec = max(0.0, nz - 0.85) * 4 if (nx < -0.1 and ny < -0.1) else 0
                px[x, y] = tuple(min(255, int(c * lum + 255 * spec * 0.5)) for c in col)

    # ---- Spielfeld
    def draw_field(self, img, rect, s: Scene):
        fx, fy, fw, fh = rect
        self._floor(img, rect)
        self._shield(img, rect, "p2", s.shield_shape, s.p2_broken, s.p2_regrow, self.P2)
        self._shield(img, rect, "p1", s.shield_shape, s.p1_broken, s.p1_regrow, self.P1)
        self._player(img, rect, "p2", s.p2_angle, self.P2_ARMOR)
        self._player(img, rect, "p1", s.p1_angle, self.P1_ARMOR)
        for laser in s.lasers:
            self._laser(img, rect, laser, drone_center(rect, s))
        self._docked_beams(img, rect, s)
        self._drone(img, rect, s)
        for sx, sy in s.sparks:
            self._spark(img, fx + sx * fw, fy + sy * fh)
        if s.storm:
            self._storm_banner(img, rect)

    def _floor(self, img, rect):
        fx, fy, fw, fh = rect
        px = img.load()
        tile = 18
        for y in range(fy, fy + fh):
            for x in range(fx, fx + fw):
                lx, ly = (x - fx) % tile, (y - fy) % tile
                base = 26 + int(10 * (1 - abs((y - fy) / fh - 0.5) * 2))
                if lx == 0 or ly == 0:
                    c = (base + 14, base + 18, base + 30)
                elif lx == tile - 1 or ly == tile - 1:
                    c = (base - 10, base - 9, base - 2)
                else:
                    c = (base, base + 3, base + 14)
                px[x, y] = c
        d = ImageDraw.Draw(img)
        d.rectangle([fx - 3, fy, fx - 1, fy + fh - 1], fill=(70, 80, 110))
        d.rectangle([fx + fw, fy, fx + fw + 2, fy + fh - 1], fill=(70, 80, 110))
        d.line([(fx - 2, fy), (fx - 2, fy + fh - 1)], fill=(140, 150, 190))
        d.line([(fx + fw + 1, fy), (fx + fw + 1, fy + fh - 1)], fill=(140, 150, 190))
        my = fy + int(DRONE_Y * fh)
        self.glow(img, lambda g: g.line([(fx, my), (fx + fw, my)], fill=(40, 60, 120), width=2), 3)
        for x in range(fx, fx + fw, 6):
            d.line([(x, my), (x + 2, my)], fill=(80, 110, 180))

    def _shield(self, img, rect, p, shape, broken, regrow, col):
        d = ImageDraw.Draw(img)
        boxes = []
        for row, i, x0, y0, x1, y1 in shield_segments(rect, p, shape):
            if (row, i) in broken:
                continue
            if (row, i) in regrow:
                d.rectangle([x0, y0, x1, y1], outline=tuple(c // 2 for c in col))
                continue
            f = 1.0 if row == 0 else 0.7
            d.rectangle([x0, y0, x1, y1], fill=tuple(int(c * 0.55 * f) for c in col))
            d.line([(x0, y0), (x1, y0)], fill=tuple(min(255, int(c * f + 60)) for c in col))
            d.line([(x0, y1), (x1, y1)], fill=tuple(int(c * 0.3 * f) for c in col))
            boxes.append((x0, y0, x1, y1))
        self.glow(img, lambda g: [g.rectangle(b, fill=tuple(c // 3 for c in col)) for b in boxes], 2)

    def _player(self, img, rect, p, angle, armor):
        cx, cy = player_center(rect, p)
        col = self.team(p)
        r = rect[2] * 0.045
        # Umhang/Schultern, Kopf (Helm) in Draufsicht
        self.shaded_ellipse(img, cx, cy, r * 2.0, r * 1.05, armor)
        self.shaded_ellipse(img, cx, cy, r * 0.95, r * 0.95, (200, 205, 220))
        d = ImageDraw.Draw(img)
        visor_y = cy - r * 0.35 if p == "p1" else cy + r * 0.35
        d.line([(cx - r * 0.5, visor_y), (cx + r * 0.5, visor_y)], fill=(30, 30, 50), width=1)
        hand, tip = sword_points(rect, p, angle)
        vx, vy = sword_vector(p, angle)
        hilt_end = (hand[0] + vx * 4, hand[1] + vy * 4)
        self.glow(img, lambda g: g.line([hilt_end, tip], fill=col, width=4), 3, 1.4)
        d = ImageDraw.Draw(img)
        d.line([hilt_end, tip], fill=tuple(min(255, c + 120) for c in col), width=2)
        d.line([hand, hilt_end], fill=(170, 170, 180), width=2)

    def _bolt(self, img, pts, col, glow_width=3, core_width=1):
        """Zickzack-Blitz: farbiger Schein, heller Kern."""
        self.glow(img, lambda g: g.line(pts, fill=col, width=glow_width, joint="curve"), 3, 1.7)
        ImageDraw.Draw(img).line(pts, fill=tuple(min(255, c + 150) for c in col), width=core_width)

    def _laser(self, img, rect, l: Laser, origin):
        """Blitz zuckt von der Drohne (origin) bis zur Blitzspitze."""
        self._bolt(img, zigzag(origin, laser_head(rect, l), 8, 4, seed=int(l.x * 1000 + l.y * 100)),
                   self.team(l.owner))

    def _docked_beams(self, img, rect, s):
        """Angedockter Blitz: zuckende Verbindung von der Drohne bis zur Schwertspitze."""
        for p, owner, angle in (("p1", s.p1_docked, s.p1_angle), ("p2", s.p2_docked, s.p2_angle)):
            if not owner:
                continue
            _, tip = sword_points(rect, p, angle)
            col = self.team(owner)
            self._bolt(img, zigzag(drone_center(rect, s), tip, 9, 5, seed=len(p) + int(angle)), col, 4, 2)
            self._spark(img, *tip)

    def _drone(self, img, rect, s):
        """Drohne in Draufsicht: Ring mit vier Duesen, Kern mit Auge."""
        cx, cy = drone_center(rect, s)
        r = rect[2] * 0.045
        eye = (255, 60, 40) if s.storm else (255, 200, 60)
        if s.storm:
            self.glow(img, lambda g: g.ellipse([cx - r * 2.4, cy - r * 2.4, cx + r * 2.4, cy + r * 2.4],
                                               outline=(255, 60, 40), width=2), 3, 1.5)
        nozzles = [(cx + math.cos(math.radians(a)) * r * 1.55, cy + math.sin(math.radians(a)) * r * 1.55)
                   for a in (45, 135, 225, 315)]
        self.glow(img, lambda g: [g.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(255, 140, 40)) for x, y in nozzles],
                  2, 1.6)
        d = ImageDraw.Draw(img)
        d.ellipse([cx - r * 1.6, cy - r * 1.6, cx + r * 1.6, cy + r * 1.6], outline=(110, 120, 150), width=2)
        for x, y in nozzles:
            d.ellipse([x - 1.5, y - 1.5, x + 1.5, y + 1.5], fill=(90, 95, 120))
        self.shaded_ellipse(img, cx, cy, r, r, (150, 160, 185))
        self.glow(img, lambda g: g.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=eye), 2.5, 1.8)
        d = ImageDraw.Draw(img)
        d.ellipse([cx - 1.5, cy - 1.5, cx + 1.5, cy + 1.5], fill=(255, 255, 220))

    def _spark(self, img, x, y):
        def fn(g):
            for a in range(0, 360, 45):
                ln = 6 if a % 90 == 0 else 3
                g.line([(x, y), (x + math.cos(math.radians(a)) * ln, y + math.sin(math.radians(a)) * ln)],
                       fill=(255, 240, 180), width=1)
        self.glow(img, fn, 2, 1.5)
        fn(ImageDraw.Draw(img))

    def _storm_banner(self, img, rect):
        fx, fy, fw, fh = rect
        my = fy + int(DRONE_Y * fh)

        def banner():
            b = Image.new("RGB", (fw, 24), (60, 10, 10))
            d = ImageDraw.Draw(b)
            for x in range(-15, fw, 10):
                d.polygon([(x, 0), (x + 5, 0), (x + 10, 2), (x + 5, 2)], fill=(255, 200, 40))
                d.polygon([(x, 21), (x + 5, 21), (x + 10, 23), (x + 5, 23)], fill=(255, 200, 40))
            draw_text(b, tr("storm"), fw // 2, 5, (255, 230, 120), scale=2, align="center", shadow=(0, 0, 0))
            return b
        b = banner()
        img.paste(b, (fx, my + 18))
        img.paste(b.transpose(Image.Transpose.ROTATE_180), (fx, my - 42))

    # ---- HUD
    def _panel(self, w, h):
        p = Image.new("RGB", (w, h), (14, 16, 34))
        d = ImageDraw.Draw(p)
        d.line([(0, 0), (w, 0)], fill=(60, 70, 120))
        d.line([(0, h - 1), (w, h - 1)], fill=(60, 70, 120))
        return p

    def pips(self, img, x, y, score, col, total=3):
        d = ImageDraw.Draw(img)
        for i in range(total):
            cx = x + i * 12
            pts = [(cx, y - 4), (cx + 4, y), (cx, y + 4), (cx - 4, y)]
            if i < score:
                self.glow(img, lambda g: g.polygon(pts, fill=col), 2, 1.2)
                d = ImageDraw.Draw(img)
                d.polygon(pts, fill=col)
            else:
                d.polygon(pts, outline=self.DIM)

    def hud_band(self, w, h, label, score, col, pause=False, extra=""):
        band = self._panel(w, h)
        draw_text(band, label, 8, (h - 14) // 2, col, scale=2, shadow=(0, 0, 0))
        self.pips(band, w // 2 - 12, h // 2, score, col)
        if pause:
            self.pause_icon(band, w - 26, (h - 18) // 2)
        if extra:
            draw_text(band, extra, w - 34, (h - 7) // 2, self.DIM, align="right", shadow=(0, 0, 0))
        return band

    def pause_icon(self, img, x, y):
        d = ImageDraw.Draw(img)
        d.rounded_rectangle([x, y, x + 18, y + 18], radius=4, fill=(40, 45, 80), outline=(110, 120, 180))
        d.rectangle([x + 6, y + 5, x + 7, y + 13], fill=self.TEXT)
        d.rectangle([x + 11, y + 5, x + 12, y + 13], fill=self.TEXT)

    def side_panel(self, w, h, lines, col):
        panel = Image.new("RGB", (w, h), (0, 0, 0))
        panel.paste(self._panel(w, h))
        y = 6
        for text, scale, c in lines:
            if text:
                draw_text(panel, text, w // 2, y, c or col, scale=scale, align="center", shadow=(0, 0, 0))
            y += 7 * scale + 6
        return panel

    # ---- Optionen
    def draw_options(self, img, items, selected):
        w, h = img.size
        d = ImageDraw.Draw(img)
        draw_text(img, tr("options"), w // 2, 22, (255, 220, 120), scale=3, align="center", shadow=(60, 10, 30))
        y = 64
        for i, (label, value) in enumerate(items):
            sel = i == selected
            box = [16, y, w - 16, y + 44]
            if sel:
                self.glow(img, lambda g: g.rounded_rectangle(box, radius=6, outline=self.P1, width=2), 3, 1.4)
                d = ImageDraw.Draw(img)
            d.rounded_rectangle(box, radius=6, fill=(24, 50, 90) if sel else (18, 20, 42),
                                outline=self.P1 if sel else (60, 70, 120))
            draw_text(img, label, 28, y + 6, self.DIM)
            draw_text(img, value, w // 2, y + 21, self.TEXT if sel else (200, 210, 235), scale=2,
                      align="center", shadow=(0, 0, 0))
            arrow_col = self.P1 if sel else (90, 100, 150)
            draw_text(img, "<", 30, y + 21, arrow_col, scale=2)
            draw_text(img, ">", w - 40, y + 21, arrow_col, scale=2)
            y += 50
        bx0, by0 = 70, h - 46
        d.rounded_rectangle([bx0, by0, w - 70, by0 + 28], radius=6, fill=(20, 22, 44), outline=(110, 120, 180))
        draw_text(img, tr("back"), w // 2, by0 + 7, self.TEXT, scale=2, align="center", shadow=(0, 0, 0))

    # ---- Titel
    def draw_title(self, img, selected_style_label):
        w, h = img.size
        land = w > h
        sc = 6 if land else 8
        logo_x = 150 if land else w // 2
        logo_ys = (28, 84) if land else (60, 130)

        def logo_color(_row, ry, top=(255, 250, 170), bottom=(255, 70, 40), size=7 * sc):
            t = min(1.0, ry / size)
            return tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3))

        for word, y in zip(("LAZER", "ARENA"), logo_ys):
            x = logo_x - text_width(word, sc) // 2
            mask = Image.new("L", img.size, 0)
            draw_text(mask, word, x, y, 255, scale=sc)
            outline = mask.filter(ImageFilter.MaxFilter(5))
            self.glow(img, lambda g: g.bitmap((0, 0), outline, fill=(120, 30, 90)), 6, 1.5)
            img.paste((20, 6, 30), (0, 0), outline)
            draw_text(img, word, x, y, logo_color, scale=sc)
        draw_text(img, tr("tagline"), logo_x, 140 if land else 200, (150, 200, 255), scale=1, align="center",
                  shadow=(0, 0, 0))
        # Demo-Szene
        rect = (100, 152, 100, 112) if land else (60, 220, 150, 150)
        demo = Scene(two_players=True, p1_angle=-25, p2_angle=20, drone_x=0.55, score_p1=0, score_p2=0,
                     p2_docked="p1")
        self.glow(img, lambda g: g.ellipse([rect[0], rect[1] + 15, rect[0] + rect[2], rect[1] + rect[3] - 15],
                                           fill=(30, 40, 90)), 12, 1.0)
        self._player(img, rect, "p2", demo.p2_angle, self.P2_ARMOR)
        self._player(img, rect, "p1", demo.p1_angle, self.P1_ARMOR)
        self._laser(img, rect, Laser(0.85, 0.82, "p2"), drone_center(rect, demo))
        self._docked_beams(img, rect, demo)
        self._drone(img, rect, demo)
        items = [tr("p1_game"), tr("p2_game"), selected_style_label, tr("options")]
        bx0, bx1, top, step = (330, w - 30, 50, 42) if land else (45, w - 45, 392, 36)
        for i, item in enumerate(items):
            by0 = top + i * step
            by1 = by0 + 28
            sel = i == 0
            d = ImageDraw.Draw(img)
            if sel:
                self.glow(img, lambda g: g.rounded_rectangle([bx0, by0, bx1, by1], radius=6,
                                                            outline=self.P1, width=2), 3, 1.4)
                d = ImageDraw.Draw(img)
            d.rounded_rectangle([bx0, by0, bx1, by1], radius=6,
                                fill=(24, 50, 90) if sel else (20, 22, 44),
                                outline=self.P1 if sel else (70, 80, 130))
            draw_text(img, item, (bx0 + bx1) // 2, by0 + 7, self.TEXT if sel else self.DIM, scale=2,
                      align="center", shadow=(0, 0, 0))
        self._note_icon(img, 10, h - 22)
        draw_text(img, tr("music_on"), 26, h - 18, self.DIM)
        draw_text(img, "V0.1", w - 8, h - 18, (80, 90, 130), align="right")

    def _note_icon(self, img, x, y):
        d = ImageDraw.Draw(img)
        d.ellipse([x, y + 9, x + 5, y + 13], fill=self.P1)
        d.line([(x + 5, y + 11), (x + 5, y)], fill=self.P1)
        d.line([(x + 5, y), (x + 10, y + 3)], fill=self.P1)


# --------------------------------------------------------------------------- Layouts

def label_p2(scene: Scene) -> str:
    return "P2" if scene.two_players else tr("ai")


def render_portrait(style: Style, scene: Scene) -> Image.Image:
    w, h = style.portrait
    img = style.new_image(w, h)
    hud_h = int(h * 0.075)
    margin = 4 if isinstance(style, ClassicStyle) else 8
    rect = (margin, hud_h, w - 2 * margin, h - 2 * hud_h)
    style.draw_field(img, rect, scene)

    top = style.hud_band(w, hud_h, label_p2(scene), scene.score_p2, style.team("p2"),
                         pause=not scene.two_players,
                         extra="" if scene.two_players else tr("level"))
    if scene.two_players:
        top = top.transpose(Image.Transpose.ROTATE_180)
    img.paste(top, (0, 0))
    bottom = style.hud_band(w, hud_h, "P1", scene.score_p1, style.team("p1"), pause=scene.two_players)
    img.paste(bottom, (0, h - hud_h))
    return style.finish(img)


def render_landscape(style: Style, scene: Scene) -> Image.Image:
    pw, ph = style.portrait
    w, h = ph, pw
    img = style.new_image(w, h)
    margin = 2 if isinstance(style, ClassicStyle) else 6
    fh = h - 2 * margin
    fw = int(fh * FIELD_ASPECT)
    fx = (w - fw) // 2
    rect = (fx, margin, fw, fh)
    style.draw_field(img, rect, scene)

    panel_w = fx - 12
    big = 2
    p2_col, p1_col = style.team("p2"), style.team("p1")
    p2_lines = [(label_p2(scene), big, None), (str(scene.score_p2) if isinstance(style, ClassicStyle) else "", 3, None)]
    if not scene.two_players:
        p2_lines.append((tr("level"), 1, None))
    left = style.side_panel(panel_w, h // 2 - 8, p2_lines, p2_col)
    if isinstance(style, ModernStyle):
        style.pips(left, panel_w // 2 - 12, 7 * big + 22, scene.score_p2, p2_col)
    if scene.two_players:
        left = left.transpose(Image.Transpose.ROTATE_180)
    img.paste(left, (6, 6))

    p1_lines = [("P1", big, None), (str(scene.score_p1) if isinstance(style, ClassicStyle) else "", 3, None)]
    right = style.side_panel(panel_w, h // 2 - 8, p1_lines, p1_col)
    if isinstance(style, ModernStyle):
        style.pips(right, panel_w // 2 - 12, 7 * big + 22, scene.score_p1, p1_col)
    img.paste(right, (fx + fw + 6, h // 2 + 2))

    hint_col = style.DIM if isinstance(style, ModernStyle) else style.GREY
    if not scene.two_players:
        hx = 6 + panel_w // 2
        for i, (text, gap) in enumerate(((tr("swipe"), 0), (tr("sword"), 0), (tr("tap"), 4), (tr("fire"), 4))):
            draw_text(img, text, hx, h // 2 + 8 + i * 10 + gap, hint_col, align="center")
    style.pause_icon(img, w - (22 if isinstance(style, ModernStyle) else 9) - 6, 8)
    return style.finish(img)


def render_title(style: Style, landscape: bool = False) -> Image.Image:
    w, h = style.portrait
    if landscape:
        w, h = h, w
    img = style.new_image(w, h)
    label = tr("style_classic") if isinstance(style, ClassicStyle) else tr("style_modern")
    style.draw_title(img, label)
    return style.finish(img)


def render_options(style: Style) -> Image.Image:
    w, h = style.portrait
    img = style.new_image(w, h)
    items = [(label, value) for label, value in tr("opt_items")]
    if isinstance(style, ClassicStyle):
        items = [it for it in items if it[0] != tr("music_label")]
        items[0] = (items[0][0], tr("val_classic"))
    style.draw_options(img, items, selected=5)
    return style.finish(img)


# --------------------------------------------------------------------------- Szenen

def scene_1p_portrait() -> Scene:
    return Scene(
        two_players=False, p1_angle=-22, p2_angle=12, drone_x=0.63, score_p1=1, score_p2=2,
        lasers=[Laser(0.45, 0.27, "p1")],
        p1_broken={(0, 6), (0, 7), (1, 7), (0, 12)}, p1_regrow={(0, 5)},
        p2_broken={(0, 3), (0, 4), (1, 4), (0, 10)}, p2_regrow={(0, 11)}, p1_docked="p2",
    )


def scene_1p_portrait_curved() -> Scene:
    scene = scene_1p_portrait()
    scene.shield_shape = "curved"
    return scene


def scene_1p_landscape() -> Scene:
    return Scene(
        two_players=False, p1_angle=30, p2_angle=-18, drone_x=0.35, score_p1=2, score_p2=1,
        lasers=[Laser(0.58, 0.29, "p1"), Laser(0.22, 0.70, "p2")],
        p1_broken={(0, 2), (0, 9), (1, 9)}, p1_regrow={(0, 8)},
        p2_broken={(0, 5), (0, 6), (0, 7), (1, 6), (0, 13)},
    )


def scene_2p_portrait() -> Scene:
    return Scene(
        two_players=True, p1_angle=-10, p2_angle=25, drone_x=0.5, score_p1=1, score_p2=1, storm=True,
        lasers=[Laser(0.28, 0.30, "n"), Laser(0.66, 0.22, "n"), Laser(0.50, 0.34, "n"),
                Laser(0.36, 0.71, "n"), Laser(0.70, 0.76, "n"), Laser(0.16, 0.66, "n")],
        p1_broken={(0, 3), (0, 4), (0, 11)}, p2_broken={(0, 7), (0, 8), (1, 8)}, p2_regrow={(0, 9)},
    )


def scene_2p_landscape() -> Scene:
    return Scene(
        two_players=True, p1_angle=18, p2_angle=-30, drone_x=0.7, score_p1=0, score_p2=2,
        lasers=[Laser(0.45, 0.74, "p2")],
        p1_broken={(0, 7), (0, 8), (1, 8), (0, 1)}, p2_broken={(0, 12), (1, 12)}, p1_regrow={(1, 7)},
        p2_docked="p1",
    )


# --------------------------------------------------------------------------- Ausgabe

def upscale(img: Image.Image, factor: int) -> Image.Image:
    return img.resize((img.width * factor, img.height * factor), Image.Resampling.NEAREST)


def build_overview(entries, out: Path) -> None:
    """entries: Liste (stilname, [(titel, bild_logisch)])"""
    pad, label_h = 16, 22
    rows = []
    for style_name, imgs in entries:
        thumbs = [(t, upscale(i, 1 if i.width > 200 else 2)) for t, i in imgs]
        rows.append((style_name, thumbs))
    width = max(sum(t.width for _, t in th) + pad * (len(th) + 1) for _, th in rows)
    height = sum(max(t.height for _, t in th) + label_h * 2 + pad for _, th in rows) + pad
    sheet = Image.new("RGB", (width, height), (24, 24, 28))
    d = ImageDraw.Draw(sheet)
    y = pad
    for style_name, thumbs in rows:
        draw_text(sheet, tr("style_" + ("classic" if style_name == "klassisch" else "modern")).replace(":", ""), pad, y, (230, 230, 230), scale=2)
        y += label_h
        x = pad
        for title, t in thumbs:
            draw_text(sheet, title.upper(), x, y, (170, 170, 180))
            sheet.paste(t, (x, y + 12))
            d.rectangle([x - 1, y + 11, x + t.width, y + 12 + t.height], outline=(70, 70, 80))
            x += t.width + pad
        y += max(t.height for _, t in thumbs) + label_h + pad
    sheet.save(out)


def count_colors(img: Image.Image) -> int:
    return len(img.getcolors(maxcolors=1 << 24))


def main(out_dir: Path) -> None:
    for lang in TEXTS:
        set_language(lang)
        lang_dir = out_dir / lang
        lang_dir.mkdir(parents=True, exist_ok=True)
        overview = []
        for style in (ClassicStyle(), ModernStyle()):
            renders = [
                ("titel-hoch", render_title(style)),
                ("titel-quer", render_title(style, landscape=True)),
                ("optionen-hoch", render_options(style)),
                ("1p-hoch", render_portrait(style, scene_1p_portrait())),
                ("1p-hoch-gebogen", render_portrait(style, scene_1p_portrait_curved())),
                ("2p-hoch", render_portrait(style, scene_2p_portrait())),
                ("1p-quer", render_landscape(style, scene_1p_landscape())),
                ("2p-quer", render_landscape(style, scene_2p_landscape())),
            ]
            for key, img in renders:
                path = lang_dir / f"{style.name}-{key}.png"
                upscale(img, style.scale).save(path)
                print(f"{lang}/{path.name:28s} {img.width}x{img.height} logisch, {count_colors(img):4d} Farben")
            overview.append((style.name, list(zip(tr("titles"), (img for _, img in renders)))))
        build_overview(overview, lang_dir / "uebersicht.png")


if __name__ == "__main__":
    main(Path(sys.argv[1] if len(sys.argv) > 1 else "mockups"))
