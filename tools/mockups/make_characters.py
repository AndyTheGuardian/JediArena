"""Fuenf Figuren-Entwuerfe: 16-Bit-Pixel-Heldin im Comic-Stil (wehendes blondes Haar, Lichtschwert mit beiden
Haenden) im Kampf gegen einen Mann mit braunen Haaren. Jede Szene in SNES-Aufloesung 256 x 224, harte Konturen,
2-stufige Cel-Schattierung, Speedlines, hoechstens 256 Farben aus 4096.

Aufruf: python3 tools/mockups/make_characters.py <ausgabeordner>
"""
from __future__ import annotations

import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

W, H = 256, 224
INK = (12, 8, 22)

sys.path.insert(0, str(Path(__file__).parent))
from make_snes_mockups import quantize, draw_text  # noqa: E402

# ----------------------------------------------------------------------------- Farben
HEROINE = {
    "skin": ((252, 214, 182), (222, 164, 132)),
    "suit": ((236, 244, 255), (170, 190, 220)),
    "accent": ((60, 210, 255), (20, 120, 190)),
    "dark": ((40, 50, 90), (20, 24, 50)),
    "hair": ((246, 206, 92), (196, 142, 52), (255, 244, 176)),
    "blade": (60, 220, 255),
}
MAN = {
    "skin": ((236, 186, 150), (196, 136, 104)),
    "suit": ((140, 36, 44), (90, 18, 30)),
    "accent": ((210, 170, 90), (150, 110, 50)),
    "dark": ((70, 66, 76), (40, 36, 46)),
    "hair": ((122, 78, 40), (78, 46, 22), (160, 110, 60)),
    "blade": (255, 70, 190),
}


def layer():
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def rgba(c, a=255):
    return (c[0], c[1], c[2], a)


def lerp(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


# ----------------------------------------------------------------------------- Grundformen
def capsule(d, p0, p1, r, color):
    """Gliedmasse: Rechteck mit runden Enden."""
    dx, dy = p1[0] - p0[0], p1[1] - p0[1]
    length = math.hypot(dx, dy) or 1
    nx, ny = -dy / length * r, dx / length * r
    d.polygon([(p0[0] + nx, p0[1] + ny), (p1[0] + nx, p1[1] + ny), (p1[0] - nx, p1[1] - ny), (p0[0] - nx, p0[1] - ny)],
              fill=rgba(color))
    for p in (p0, p1):
        d.ellipse([p[0] - r, p[1] - r, p[0] + r, p[1] + r], fill=rgba(color))


def shaded_capsule(d, p0, p1, r, colors):
    """Cel-Schattierung: dunkle Grundform, helle Form leicht nach links oben versetzt."""
    light, shade = colors
    capsule(d, p0, p1, r, shade)
    off = (-0.9, -0.9)
    capsule(d, (p0[0] + off[0], p0[1] + off[1]), (p1[0] + off[0], p1[1] + off[1]), max(1, r - 1.2), light)


def bezier(p0, p1, p2, steps=14):
    return [((1 - t) ** 2 * p0[0] + 2 * (1 - t) * t * p1[0] + t * t * p2[0],
             (1 - t) ** 2 * p0[1] + 2 * (1 - t) * t * p1[1] + t * t * p2[1]) for t in (i / steps for i in range(steps + 1))]


def strand(d, pts, w0, color):
    """Haarstraehne: verjuengt sich von w0 auf 1."""
    for i in range(len(pts) - 1):
        w = max(1, round(w0 * (1 - i / len(pts))))
        d.line([pts[i], pts[i + 1]], fill=rgba(color), width=w)


def outline(sprite: Image.Image) -> Image.Image:
    alpha = sprite.getchannel("A").point(lambda v: 255 if v > 0 else 0)
    grown = alpha.filter(ImageFilter.MaxFilter(3))
    base = Image.new("RGBA", sprite.size, rgba(INK))
    base.putalpha(grown)
    base.alpha_composite(sprite)
    return base


# ----------------------------------------------------------------------------- Figur
def figure(pose: dict, pal: dict, heroine: bool) -> tuple[Image.Image, tuple]:
    """Zeichnet eine Figur aus Gelenkpunkten. Liefert Sprite und (Griff, Klingenspitze)."""
    back, body, front = layer(), layer(), layer()
    db, dm, df = ImageDraw.Draw(back), ImageDraw.Draw(body), ImageDraw.Draw(front)
    p = pose
    face = p["face"]   # +1 = schaut nach rechts, -1 = nach links

    # hinteres Bein und Arm (dunkler)
    shaded_capsule(db, p["hip"], p["knee_b"], 6.2, pal["dark"])
    shaded_capsule(db, p["knee_b"], p["foot_b"], 5.2, pal["dark"])
    db.ellipse([p["foot_b"][0] - 5, p["foot_b"][1] - 3, p["foot_b"][0] + 5, p["foot_b"][1] + 3], fill=rgba(pal["dark"][1]))
    shaded_capsule(db, p["shoulder_b"], p["elbow_b"], 4.8, pal["suit"])
    shaded_capsule(db, p["elbow_b"], p["hands"], 4.2, pal["suit"])

    # Haarmasse hinter dem Kopf (Heldin: lang und wehend)
    hx, hy = p["head"]
    wind = p.get("wind", (-face, 0.3))
    if heroine:
        rng = random.Random(p.get("seed", 1))
        for k in range(11):
            spread = (k - 5) / 5
            root = (hx - face * 2 + spread * 3, hy - 3 + abs(spread) * 2)
            length = p.get("hair_len", 46) * (0.75 + rng.random() * 0.45)
            end = (root[0] + wind[0] * length + spread * 10, root[1] + wind[1] * length + spread * 14)
            ctrl = (lerp(root, end, 0.5)[0] - wind[1] * 12 * (1 if k % 2 else -1), lerp(root, end, 0.5)[1] + wind[0] * 10 * (1 if k % 2 else -1))
            pts = bezier(root, ctrl, end)
            strand(db, pts, 6, pal["hair"][1])
            strand(db, [(x - 0.8, y - 0.8) for x, y in pts], 4, pal["hair"][0])
            if k % 3 == 0:
                strand(db, [(x - 1.2, y - 1.5) for x, y in pts[:8]], 1, pal["hair"][2])

    # Rumpf
    sf, sb, hip = p["shoulder_f"], p["shoulder_b"], p["hip"]
    sb_w = (sb[0] - face * 3, sb[1]); sf_w = (sf[0] + face * 3, sf[1])
    torso = [sb_w, sf_w, (hip[0] + 8 * face, hip[1] + 2), (hip[0] - 8 * face, hip[1] + 2)]
    dm.polygon(torso, fill=rgba(pal["suit"][1]))
    dm.polygon([(x - 1, y - 1) for x, y in torso[:2]] + [torso[2], (torso[3][0] - 1, torso[3][1] - 1)], fill=rgba(pal["suit"][0]))
    neck = lerp(sf, sb, 0.5)
    dm.line([neck, lerp(neck, hip, 0.85)], fill=rgba(pal["accent"][0]), width=2)      # Mittelstreifen
    dm.line([(hip[0] - 6, hip[1] - 2), (hip[0] + 6, hip[1] - 2)], fill=rgba(pal["accent"][1]), width=3)   # Guertel
    shaded_capsule(dm, sb_w, sf_w, 5.5, pal["accent"])                                 # Schulterpanzer

    # Kopf
    dm.ellipse([hx - 10, hy - 11, hx + 10, hy + 10], fill=rgba(pal["skin"][1]))
    dm.ellipse([hx - 10 + face * 1, hy - 11, hx + 9 + face * 1, hy + 8], fill=rgba(pal["skin"][0]))
    dm.polygon([(hx + face * 6, hy + 4), (hx + face * 9, hy + 7), (hx + face * 3, hy + 9)], fill=rgba(pal["skin"][0]))   # Kinn
    eye = (hx + face * 5, hy - 1)
    dm.rectangle([eye[0] - 1, eye[1] - 2, eye[0] + 1, eye[1] + 1], fill=rgba(INK))
    dm.point((eye[0] - face, eye[1] - 2), fill=(255, 255, 255, 255))
    if p.get("fierce"):   # grimmig: steile, dicke Braue, Zaehne gefletscht
        dm.line([(eye[0] - 4 * face, eye[1] - 6), (eye[0] + 2 * face, eye[1] - 3)], fill=rgba(INK), width=2)
        dm.rectangle([min(hx + face * 3, hx + face * 8), hy + 4, max(hx + face * 3, hx + face * 8), hy + 6], fill=rgba(INK))
        dm.line([(hx + face * 4, hy + 5), (hx + face * 7, hy + 5)], fill=(250, 250, 250, 255))
    else:
        dm.line([(eye[0] - 3, eye[1] - 4), (eye[0] + 2 * face, eye[1] - 5 + (0 if heroine else 1))], fill=rgba(INK))   # Braue (entschlossen)
        dm.line([(hx + face * 3, hy + 5), (hx + face * 7, hy + 5)], fill=rgba(pal["skin"][1] if heroine else INK))   # Mund
    if heroine:   # Pony und Scheitel
        dm.pieslice([hx - 11, hy - 13, hx + 11, hy + 6], 180, 360, fill=rgba(pal["hair"][0]))
        dm.polygon([(hx - 11, hy - 3), (hx + face * 7, hy - 11), (hx + face * 11, hy - 1)], fill=rgba(pal["hair"][0]))
        dm.line([(hx - 5, hy - 10), (hx + face * 4, hy - 11)], fill=rgba(pal["hair"][2]))
    else:   # kurzes, braunes, leicht wildes Haar
        dm.pieslice([hx - 11, hy - 14, hx + 11, hy + 5], 180, 360, fill=rgba(pal["hair"][1]))
        for k in range(7):
            ax = hx - 10 + k * 3
            dm.polygon([(ax, hy - 7), (ax + 2 - face * 4, hy - 17 - (k % 2) * 3), (ax + 4, hy - 7)], fill=rgba(pal["hair"][0]))
        dm.polygon([(hx - 11, hy - 4), (hx - face * 2, hy - 12), (hx + face * 10, hy - 5)], fill=rgba(pal["hair"][0]))
        dm.line([(hx + face * 1, hy + 6), (hx + face * 6, hy + 5)], fill=rgba(pal["hair"][1]))   # Bartschatten

    # vorderes Bein und Arm
    shaded_capsule(df, p["hip"], p["knee_f"], 6.6, pal["dark"])
    shaded_capsule(df, p["knee_f"], p["foot_f"], 5.4, pal["dark"])
    df.ellipse([p["foot_f"][0] - 6, p["foot_f"][1] - 3, p["foot_f"][0] + 6, p["foot_f"][1] + 3], fill=rgba(pal["dark"][1]))
    shaded_capsule(df, sf_w, p["elbow_f"], 5.2, pal["suit"])
    shaded_capsule(df, p["elbow_f"], p["hands"], 4.6, pal["suit"])
    df.ellipse([p["hands"][0] - 5, p["hands"][1] - 5, p["hands"][0] + 5, p["hands"][1] + 5], fill=rgba(pal["dark"][0]))   # Faeuste
    df.ellipse([p["hands"][0] - 4, p["hands"][1] - 5, p["hands"][0] + 3, p["hands"][1] + 2], fill=rgba(pal["dark"][0][:3]))

    if p.get("detail") or figure_detail:
        draw_details(p, pal, heroine, dm, df, db, sf_w, sb_w)

    sprite = layer()
    for part in (back, body, front):
        sprite.alpha_composite(part)
    sprite = outline(sprite)

    # Griff und Klinge
    if "blade_target" in p:   # Klinge so ausrichten, dass sie den Zielpunkt (die andere Klinge) kreuzt
        tx, ty = p["blade_target"]
        a = math.atan2(ty - p["hands"][1], tx - p["hands"][0])
        p["blade_len"] = math.hypot(tx - p["hands"][0], ty - p["hands"][1]) + 12
    else:
        a = math.radians(p["blade_angle"])
    d = (math.cos(a), math.sin(a))
    hilt0 = (p["hands"][0] - d[0] * 7, p["hands"][1] - d[1] * 7)
    hilt1 = (p["hands"][0] + d[0] * 5, p["hands"][1] + d[1] * 5)
    tip = (hilt1[0] + d[0] * p.get("blade_len", 58), hilt1[1] + d[1] * p.get("blade_len", 58))
    hd = ImageDraw.Draw(sprite)
    hd.line([hilt0, hilt1], fill=rgba(INK), width=5)
    hd.line([hilt0, hilt1], fill=(180, 184, 196, 255), width=3)
    return sprite, (hilt1, tip)


def draw_details(p, pal, heroine, dm, df, db, sf_w, sb_w):
    """Ausarbeitung: Gesicht, Haarglanz, Finger am Griff, Stulpen, Falten, Knieschoner, Guertelschnalle, Sohlen."""
    face = p["face"]
    hx, hy = p["head"]
    skin_light, skin_shade = pal["skin"]
    # Gesicht: Augenweiss, Pupille, Glanzpunkt, Nasenschatten, Lippen bzw. Bartschatten
    ex, ey = hx + face * 5, hy - 1
    if heroine:
        dm.rectangle([ex - 1 - (face < 0), ey - 2, ex + 1 + (face > 0), ey + 1], fill=(250, 250, 255, 255))
        dm.rectangle([ex + (face > 0) - (face < 0), ey - 2, ex + (face > 0) - (face < 0), ey + 1], fill=(30, 90, 160, 255))
        dm.point((ex + (face > 0) - (face < 0), ey - 2), fill=(255, 255, 255, 255))
        dm.line([(ex - 2, ey - 3), (ex + 2 * face, ey - 4)], fill=rgba(INK))   # Wimpern/Braue
        dm.line([(hx + face * 8, hy + 1), (hx + face * 8, hy + 3)], fill=rgba(skin_shade))   # Nase
        dm.line([(hx + face * 4, hy + 5), (hx + face * 7, hy + 5)], fill=(220, 110, 120, 255))   # Lippen
        dm.point((hx - face * 2, hy + 3), fill=(255, 190, 180, 255))   # Wangenroete
    else:
        dm.line([(ex - 2, ey), (ex + 2, ey)], fill=(250, 250, 250, 255))   # zusammengekniffenes Auge
        dm.point((ex + face, ey), fill=rgba(INK))
        dm.line([(hx + face * 8, hy), (hx + face * 9, hy + 3)], fill=rgba(skin_shade))   # Nase
        rng = random.Random(7)
        for _ in range(9):   # Bartstoppeln
            dm.point((hx + face * rng.randint(1, 8), hy + rng.randint(4, 9)), fill=rgba(pal["hair"][1]))
    # Haarglanz (Heldin): helle Straehnen auf Scheitel und Pony
    if heroine:
        for k in range(3):
            dm.line([(hx - 7 + k * 3, hy - 9 + k), (hx - 3 + k * 4 * face, hy - 11 + k)], fill=rgba(pal["hair"][2]))
    # Haende: Finger um den Griff (Querlinien), Stulpen am Unterarm
    hdx, hdy = p["hands"]
    a = math.radians(p["blade_angle"]) if "blade_target" not in p else math.atan2(p["blade_target"][1] - hdy, p["blade_target"][0] - hdx)
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    for k in (-2.5, 0, 2.5):
        cx, cy = hdx + ux * k, hdy + uy * k
        df.line([(cx - nx * 4, cy - ny * 4), (cx + nx * 4, cy + ny * 4)], fill=rgba(INK))
    for elbow in (p["elbow_f"],):
        cuff = lerp(elbow, p["hands"], 0.72)
        df.ellipse([cuff[0] - 4, cuff[1] - 4, cuff[0] + 4, cuff[1] + 4], fill=rgba(pal["accent"][0]))
        df.arc([cuff[0] - 4, cuff[1] - 4, cuff[0] + 4, cuff[1] + 4], 20, 160, fill=rgba(pal["accent"][1]))
    # Falten an Ellbogen und Knie
    for joint, layer_draw, col in ((p["elbow_f"], df, pal["suit"][1]), (p["knee_f"], df, pal["dark"][1]), (p["knee_b"], db, pal["dark"][1])):
        layer_draw.arc([joint[0] - 4, joint[1] - 3, joint[0] + 4, joint[1] + 3], 200, 340, fill=rgba(col))
    # Knieschoner
    for knee, d_ in ((p["knee_f"], df), (p["knee_b"], db)):
        d_.ellipse([knee[0] - 4, knee[1] - 4, knee[0] + 4, knee[1] + 3], fill=rgba(pal["accent"][1]))
        d_.ellipse([knee[0] - 3, knee[1] - 4, knee[0] + 2, knee[1] + 1], fill=rgba(pal["accent"][0]))
    # Stiefelsohlen
    for foot, d_ in ((p["foot_f"], df), (p["foot_b"], db)):
        d_.line([(foot[0] - 5, foot[1] + 3), (foot[0] + 5, foot[1] + 3)], fill=(150, 150, 165, 255))
    # Guertelschnalle, Kragen, Schulterglanz
    hip = p["hip"]
    dm.rectangle([hip[0] - 2, hip[1] - 4, hip[0] + 2, hip[1]], fill=(230, 220, 160, 255), outline=rgba(INK))
    neck = lerp(sf_w, sb_w, 0.5)
    dm.polygon([(neck[0] - 5, neck[1] - 1), (neck[0] + 5, neck[1] - 1), (neck[0] + 3, neck[1] + 4), (neck[0] - 3, neck[1] + 4)],
               fill=rgba(pal["accent"][1]))
    dm.line([(sb_w[0], sb_w[1] - 3), (sf_w[0], sf_w[1] - 3)], fill=rgba(pal["accent"][0] if heroine else (230, 200, 130)))


def blade(img, start, tip, color):
    for width, alpha in ((11, 30), (7, 70), (4, 150)):
        lay = layer()
        ImageDraw.Draw(lay).line([start, tip], fill=rgba(color, alpha), width=width)
        img.alpha_composite(lay)
    lay = layer()
    ImageDraw.Draw(lay).line([start, tip], fill=(255, 255, 255, 255), width=2)
    img.alpha_composite(lay)


def crossing(a0, a1, b0, b1):
    """Schnittpunkt zweier Strecken (oder Mittelpunkt der naechsten Enden, falls sie sich nicht schneiden)."""
    x1, y1 = a0; x2, y2 = a1; x3, y3 = b0; x4, y4 = b1
    den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)
    if abs(den) > 1e-6:
        t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den
        u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den
        if 0 <= t <= 1 and 0 <= u <= 1:
            return (x1 + t * (x2 - x1), y1 + t * (y2 - y1))
    return ((x2 + x4) / 2, (y2 + y4) / 2)


def duel(img, her, man, man_first=True):
    """Beide Figuren zeichnen; die Klinge des Mannes zielt auf einen Punkt der Klinge der Heldin."""
    s1, (h1, t1) = figure(her, HEROINE, True)
    if "blade_target" not in man:
        f = man.pop("meet", 0.72)
        man["blade_target"] = (h1[0] + (t1[0] - h1[0]) * f, h1[1] + (t1[1] - h1[1]) * f)
    s2, (h2, t2) = figure(man, MAN, False)
    order = [(s2, h2, t2, MAN["blade"]), (s1, h1, t1, HEROINE["blade"])]
    if not man_first:
        order.reverse()
    for sprite, h, t, c in order:
        img.alpha_composite(sprite)
        blade(img, h, t, c)
    clash(img, crossing(h1, t1, h2, t2), 16)


def clash(img, at, size=14):
    """Funkenstern am Treffpunkt der Klingen."""
    lay = layer()
    d = ImageDraw.Draw(lay)
    for k in range(12):
        ang = k * math.pi / 6 + 0.2
        ln = size if k % 2 == 0 else size * 0.5
        d.line([at, (at[0] + math.cos(ang) * ln, at[1] + math.sin(ang) * ln)], fill=(255, 250, 200, 230), width=2 if k % 2 == 0 else 1)
    d.ellipse([at[0] - 4, at[1] - 4, at[0] + 4, at[1] + 4], fill=(255, 255, 255, 255))
    img.alpha_composite(lay)


# ----------------------------------------------------------------------------- Hintergruende
def banded(img, top, bottom, bands=10):
    d = ImageDraw.Draw(img)
    for y in range(H):
        t = math.floor(y / H * bands) / (bands - 1)
        d.line([(0, y), (W, y)], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)))


def speed_lines(img, center, color, count=40, seed=1, inner=40):
    rng = random.Random(seed)
    d = ImageDraw.Draw(img)
    for _ in range(count):
        a = rng.random() * math.tau
        r0 = inner + rng.random() * 30
        d.line([(center[0] + math.cos(a) * r0, center[1] + math.sin(a) * r0),
                (center[0] + math.cos(a) * 400, center[1] + math.sin(a) * 400)], fill=color, width=rng.choice([1, 1, 2]))


def halftone(img, corner, color, step=6, radius_max=2.4, extent=90):
    d = ImageDraw.Draw(img)
    cx, cy = corner
    for y in range(0, H, step):
        for x in range(0, W, step):
            dist = math.hypot(x - cx, y - cy)
            if dist > extent:
                continue
            r = radius_max * (1 - dist / extent)
            if r > 0.4:
                d.ellipse([x - r, y - r, x + r, y + r], fill=color)


def floor_grid(img, horizon, color):
    d = ImageDraw.Draw(img)
    for k in range(-12, 13):
        d.line([(W / 2 + k * 8, horizon), (W / 2 + k * 60, H)], fill=color)
    y, step = horizon, 3.0
    while y < H:
        d.line([(0, y), (W, y)], fill=color)
        y += step
        step *= 1.35


def frame(img, caption):
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, W - 1, H - 1], outline=rgba(INK), width=3)
    d.rectangle([3, 3, W - 4, H - 4], outline=(255, 255, 255, 255), width=1)
    if caption:   # Comic-Textkasten
        tw = len(caption) * 6 + 8
        d.rectangle([6, 6, 6 + tw, 18], fill=(255, 240, 150, 255), outline=rgba(INK))
        draw_text(img, caption, 10, 9, INK)


def pose(**kw):
    return kw


# ----------------------------------------------------------------------------- Szenen
def design1():
    """Klingenkreuzen: beide drueckend, Klingen gekreuzt in der Mitte."""
    img = Image.new("RGBA", (W, H))
    banded(img, (40, 16, 70), (240, 110, 80), 12)
    speed_lines(img, (128, 92), (255, 210, 170, 255), 46, 1)
    floor_grid(img, 170, (120, 40, 90, 255))
    her = pose(face=1, head=(70, 70), shoulder_f=(80, 86), shoulder_b=(66, 84), elbow_f=(98, 92), elbow_b=(86, 96),
               hands=(108, 90), hip=(64, 126), knee_f=(84, 148), foot_f=(92, 182), knee_b=(48, 152), foot_b=(30, 180),
               blade_angle=-58, blade_len=54, wind=(-1, 0.25), hair_len=50, seed=3)
    man = pose(face=-1, head=(186, 66), shoulder_f=(176, 84), shoulder_b=(192, 82), elbow_f=(160, 92), elbow_b=(172, 98),
               hands=(150, 90), hip=(194, 126), knee_f=(176, 150), foot_f=(168, 184), knee_b=(210, 152), foot_b=(228, 182),
               blade_angle=-122, blade_len=54)
    duel(img, her, man)
    frame(img, "KLINGENKREUZEN")
    return img


def design2():
    """Sprungangriff: Heldin springt von oben links, Mann blockt von unten rechts."""
    img = Image.new("RGBA", (W, H))
    banded(img, (10, 20, 60), (90, 40, 140), 10)
    halftone(img, (240, 210), (60, 200, 255, 255), extent=110)
    speed_lines(img, (60, 40), (190, 210, 255, 255), 36, 2, inner=30)
    her = pose(face=1, head=(72, 40), shoulder_f=(82, 54), shoulder_b=(70, 54), elbow_f=(96, 46), elbow_b=(84, 44),
               hands=(102, 36), hip=(62, 92), knee_f=(84, 104), foot_f=(80, 130), knee_b=(48, 112), foot_b=(36, 96),
               blade_angle=40, blade_len=62, wind=(-0.6, 0.85), hair_len=52, seed=5)
    man = pose(face=-1, head=(176, 112), shoulder_f=(166, 128), shoulder_b=(182, 126), elbow_f=(150, 120), elbow_b=(164, 116),
               hands=(150, 106), hip=(184, 168), knee_f=(166, 188), foot_f=(156, 214), knee_b=(200, 190), foot_b=(214, 214),
               blade_angle=-150, blade_len=56)
    duel(img, her, man)
    frame(img, "SPRUNGANGRIFF")
    return img


def design3():
    """Ausfallschritt: tiefer Stoss der Heldin, Mann pariert nach unten."""
    img = Image.new("RGBA", (W, H))
    banded(img, (255, 170, 90), (120, 30, 60), 12)
    d = ImageDraw.Draw(img)
    d.ellipse([150, 40, 230, 120], fill=(255, 220, 140, 255))   # grosse Sonne
    for y in range(60, 120, 7):
        d.line([(150, y), (230, y)], fill=(255, 170, 90, 255), width=2)
    floor_grid(img, 160, (90, 20, 60, 255))
    her = pose(face=1, head=(62, 104), shoulder_f=(72, 118), shoulder_b=(58, 116), elbow_f=(88, 128), elbow_b=(76, 130),
               hands=(100, 132), hip=(50, 150), knee_f=(80, 160), foot_f=(98, 196), knee_b=(30, 172), foot_b=(14, 196),
               blade_angle=-12, blade_len=60, wind=(-1, -0.2), hair_len=56, seed=7)
    man = pose(face=-1, head=(196, 82), shoulder_f=(186, 98), shoulder_b=(202, 96), elbow_f=(172, 108), elbow_b=(186, 112),
               hands=(168, 120), hip=(200, 140), knee_f=(186, 166), foot_f=(176, 198), knee_b=(214, 168), foot_b=(232, 198),
               blade_angle=110, blade_len=46)
    duel(img, her, man)
    frame(img, "AUSFALL")
    return img


def design4():
    """Drehhieb: Heldin mitten in der Drehung, Haar im Kreisbogen, Klinge waagerecht; Mann weicht zurueck."""
    img = Image.new("RGBA", (W, H))
    banded(img, (14, 10, 40), (40, 90, 120), 10)
    lay = layer()
    d = ImageDraw.Draw(lay)
    for r, a in ((70, 60), (62, 90), (54, 50)):   # Bewegungsbogen der Drehung
        d.arc([100 - r, 104 - r, 100 + r, 104 + r], 200, 340, fill=(120, 230, 255, a), width=3)
    img.alpha_composite(lay)
    halftone(img, (0, 0), (90, 60, 160, 255), extent=120)
    her = pose(face=-1, head=(96, 72), shoulder_f=(88, 86), shoulder_b=(104, 86), elbow_f=(74, 94), elbow_b=(110, 100),
               hands=(70, 104), hip=(100, 128), knee_f=(84, 150), foot_f=(70, 182), knee_b=(118, 154), foot_b=(126, 186),
               blade_angle=-8, blade_len=64, wind=(1, -0.35), hair_len=58, seed=9)
    her.update(face=1, hands=(118, 104), elbow_f=(110, 96), elbow_b=(96, 100), blade_angle=-4, blade_len=62)
    man = pose(face=-1, head=(206, 76), shoulder_f=(198, 92), shoulder_b=(214, 90), elbow_f=(186, 100), elbow_b=(200, 106),
               hands=(184, 112), hip=(214, 134), knee_f=(200, 158), foot_f=(194, 190), knee_b=(228, 156), foot_b=(240, 186),
               blade_angle=-95, blade_len=54)
    duel(img, her, man)
    frame(img, "DREHHIEB")
    return img


def design5():
    """Nahaufnahme: Oberkoerper, Klingen gross gekreuzt, Mann links, Heldin rechts, Haar weht quer ins Bild."""
    img = Image.new("RGBA", (W, H))
    banded(img, (60, 10, 50), (10, 10, 30), 8)
    speed_lines(img, (128, 112), (255, 120, 200, 255), 60, 4, inner=24)
    man = pose(face=1, head=(64, 84), shoulder_f=(76, 104), shoulder_b=(54, 102), elbow_f=(98, 120), elbow_b=(80, 128),
               hands=(112, 118), hip=(58, 190), knee_f=(70, 230), foot_f=(72, 260), knee_b=(46, 230), foot_b=(40, 260),
               blade_angle=-62, blade_len=72)
    her = pose(face=-1, head=(192, 80), shoulder_f=(180, 100), shoulder_b=(202, 98), elbow_f=(158, 116), elbow_b=(176, 124),
               hands=(144, 116), hip=(196, 186), knee_f=(184, 230), foot_f=(182, 260), knee_b=(210, 230), foot_b=(216, 260),
               blade_angle=-118, blade_len=72, wind=(1, 0.1), hair_len=64, seed=11)
    duel(img, her, man)
    frame(img, "AUGE IN AUGE")
    return img


# ----------------------------------------------------------------------------- Drohne, Blitz, Abwehr
def drone(img, c, r=9, glow=True):
    """Drohne in Comic-Optik: Metallkugel mit Ring, Duesen und rotem Auge, schwarze Kontur."""
    sprite = layer()
    d = ImageDraw.Draw(sprite)
    d.ellipse([c[0] - r * 1.7, c[1] - r * 1.7, c[0] + r * 1.7, c[1] + r * 1.7], outline=(120, 128, 156, 255), width=3)
    for a in (40, 140, 220, 320):
        nx, ny = c[0] + math.cos(math.radians(a)) * r * 1.7, c[1] + math.sin(math.radians(a)) * r * 1.7
        d.ellipse([nx - 2.5, ny - 2.5, nx + 2.5, ny + 2.5], fill=(255, 140, 40, 255))
    ramp = [(50, 54, 72), (100, 108, 134), (160, 168, 194), (226, 232, 246)]
    for y in range(int(c[1] - r), int(c[1] + r) + 1):
        for x in range(int(c[0] - r), int(c[0] + r) + 1):
            nx, ny = (x - c[0]) / r, (y - c[1]) / r
            if nx * nx + ny * ny > 1:
                continue
            nz = math.sqrt(max(0.0, 1 - nx * nx - ny * ny))
            lum = max(0.0, (-0.5 * nx - 0.6 * ny) * 0.8 + nz * 0.6)
            d.point((x, y), fill=rgba(ramp[min(3, int(lum * 4))]))
    d.ellipse([c[0] - 2.5, c[1] - 2.5, c[0] + 2.5, c[1] + 2.5], fill=(255, 60, 40, 255))
    d.point((c[0] - 1, c[1] - 1), fill=(255, 230, 220, 255))
    if glow:
        g = layer()
        ImageDraw.Draw(g).ellipse([c[0] - r * 2.6, c[1] - r * 2.6, c[0] + r * 2.6, c[1] + r * 2.6], fill=(255, 80, 60, 40))
        img.alpha_composite(g)
    img.alpha_composite(outline(sprite))


def zigzag_pts(a, b, step, amp, seed):
    rng = random.Random(seed)
    dx, dy = b[0] - a[0], b[1] - a[1]
    length = math.hypot(dx, dy) or 1
    nx, ny = -dy / length, dx / length
    pts, pos, side = [a], 0.0, 1
    while True:
        pos += step * (0.5 + rng.random())
        if pos >= length - step * 0.4:
            break
        side = -side if rng.random() < 0.8 else side
        off = amp * side * (0.3 + rng.random() * 1.1) * (2 if rng.random() < 0.12 else 1)
        pts.append((a[0] + dx * pos / length + nx * off, a[1] + dy * pos / length + ny * off))
    pts.append(b)
    return pts


def bolt(img, a, b, color, seed):
    pts = zigzag_pts(a, b, 7, 5, seed)
    for width, alpha in ((9, 40), (5, 100), (3, 190)):
        lay = layer()
        ImageDraw.Draw(lay).line(pts, fill=rgba(color, alpha), width=width, joint="curve")
        img.alpha_composite(lay)
    lay = layer()
    ImageDraw.Draw(lay).line(pts, fill=(255, 255, 255, 255), width=1)
    img.alpha_composite(lay)


def deflect(img, at, incoming, color, seed):
    """Abprallender Blitz: Funkenstern und drei kurze, klar getrennte Splitter, die seitlich wegspritzen."""
    rng = random.Random(seed)
    dx, dy = at[0] - incoming[0], at[1] - incoming[1]
    back = math.atan2(-dy, -dx)   # zurueck Richtung Drohne
    for k, spread in enumerate((-1.25, 0.0, 1.25)):
        ang = back + spread + (rng.random() - 0.5) * 0.3
        ln = 10 + rng.random() * 6
        start = (at[0] + math.cos(ang) * 4, at[1] + math.sin(ang) * 4)
        end = (at[0] + math.cos(ang) * ln, at[1] + math.sin(ang) * ln)
        lay = layer()
        dd = ImageDraw.Draw(lay)
        dd.line([start, end], fill=rgba(color, 120), width=3)
        dd.line([start, end], fill=(255, 255, 255, 255), width=1)
        img.alpha_composite(lay)
    clash(img, at, 13)


def scene(img, her, man, drone_at, bolt_color=MAN["blade"], seed=1, her_front=True):
    """Beide weit auseinander, Drohne dazwischen; Blitz von der Drohne prallt an der Klinge der Heldin ab."""
    s1, (h1, t1) = figure(her, HEROINE, True)
    s2, (h2, t2) = figure(man, MAN, False)
    hit = (h1[0] + (t1[0] - h1[0]) * her.get("hit_at", 0.62), h1[1] + (t1[1] - h1[1]) * her.get("hit_at", 0.62))
    img.alpha_composite(s2)
    blade(img, h2, t2, MAN["blade"])
    drone(img, drone_at)
    img.alpha_composite(s1)
    blade(img, h1, t1, HEROINE["blade"])
    bolt(img, drone_at, hit, bolt_color, seed)
    deflect(img, hit, drone_at, bolt_color, seed + 10)


def design6():
    """Abwehr bei Sonnenuntergang: Heldin links lenkt den Blitz ab, Mann rechts holt ueber dem Kopf aus."""
    img = Image.new("RGBA", (W, H))
    banded(img, (50, 20, 90), (255, 140, 80), 12)
    floor_grid(img, 172, (120, 40, 90, 255))
    her = pose(face=1, head=(42, 86), shoulder_f=(50, 102), shoulder_b=(38, 100), elbow_f=(64, 106), elbow_b=(54, 112),
               hands=(70, 100), hip=(38, 140), knee_f=(56, 160), foot_f=(64, 192), knee_b=(24, 164), foot_b=(12, 190),
               blade_angle=-55, blade_len=50, wind=(-1, 0.15), hair_len=50, seed=21, hit_at=0.6)
    man = pose(face=-1, head=(214, 82), shoulder_f=(206, 98), shoulder_b=(220, 96), elbow_f=(200, 84), elbow_b=(214, 82),
               hands=(208, 70), hip=(218, 138), knee_f=(198, 160), foot_f=(186, 192), knee_b=(232, 162), foot_b=(246, 190),
               blade_angle=-130, blade_len=52, fierce=True)
    scene(img, her, man, (128, 64), seed=6)
    frame(img, "ABWEHR")
    return img


def design7():
    """Sternennacht: Heldin geduckt, Klinge schraeg nach oben; Mann richtet die Klinge drohend auf sie."""
    img = Image.new("RGBA", (W, H))
    banded(img, (6, 6, 26), (30, 16, 60), 8)
    rng = random.Random(7)
    d = ImageDraw.Draw(img)
    for _ in range(70):
        x, y = rng.randrange(W), rng.randrange(150)
        d.point((x, y), fill=rng.choice([(140, 140, 200), (220, 220, 255), (255, 255, 255)]))
    d.ellipse([170, 20, 236, 86], fill=(90, 60, 140))
    d.ellipse([168, 16, 230, 78], fill=(140, 100, 190))
    d.arc([150, 40, 256, 66], 160, 380, fill=(230, 190, 120), width=2)
    floor_grid(img, 176, (60, 70, 150, 255))
    her = pose(face=1, head=(48, 116), shoulder_f=(56, 130), shoulder_b=(44, 128), elbow_f=(70, 128), elbow_b=(58, 138),
               hands=(76, 122), hip=(40, 160), knee_f=(64, 168), foot_f=(70, 196), knee_b=(22, 178), foot_b=(14, 198),
               blade_angle=-40, blade_len=52, wind=(-1, -0.25), hair_len=52, seed=22, hit_at=0.7)
    man = pose(face=-1, head=(212, 86), shoulder_f=(204, 102), shoulder_b=(218, 100), elbow_f=(190, 110), elbow_b=(204, 114),
               hands=(182, 116), hip=(216, 142), knee_f=(200, 166), foot_f=(192, 196), knee_b=(230, 166), foot_b=(240, 196),
               blade_angle=-140, blade_len=48, fierce=True)
    man.update(elbow_f=(196, 92), hands=(190, 86))
    scene(img, her, man, (150, 82), seed=7)
    frame(img, "STERNENNACHT")
    return img


def design8():
    """Seiten getauscht im Hologramm-Raum: Heldin rechts mit senkrechter Klinge, Mann links in Seitenhut."""
    img = Image.new("RGBA", (W, H), (0, 8, 6, 255))
    d = ImageDraw.Draw(img)
    for x in range(0, W, 16):
        d.line([(x, 0), (x, H)], fill=(10, 60, 36))
    for y in range(0, H, 16):
        d.line([(0, y), (W, y)], fill=(10, 60, 36))
    floor_grid(img, 170, (40, 200, 110, 255))
    her = pose(face=-1, head=(212, 88), shoulder_f=(204, 104), shoulder_b=(218, 102), elbow_f=(192, 108), elbow_b=(206, 114),
               hands=(188, 100), hip=(216, 142), knee_f=(200, 164), foot_f=(194, 194), knee_b=(232, 166), foot_b=(244, 194),
               blade_angle=-100, blade_len=52, wind=(1, 0.2), hair_len=52, seed=23, hit_at=0.55)
    man = pose(face=1, head=(44, 84), shoulder_f=(52, 100), shoulder_b=(38, 98), elbow_f=(44, 116), elbow_b=(30, 112),
               hands=(30, 126), hip=(40, 140), knee_f=(60, 162), foot_f=(70, 194), knee_b=(24, 164), foot_b=(12, 194),
               blade_angle=-150, blade_len=48, fierce=True)
    scene(img, her, man, (124, 70), seed=8)
    frame(img, "HOLOGRAMM-DUELL")
    return img


def design9():
    """Lava-Schmiede, weite Einstellung: grosse Drohne in der Mitte, kleine Figuren weit aussen."""
    img = Image.new("RGBA", (W, H))
    banded(img, (40, 8, 4), (180, 60, 12), 10)
    d = ImageDraw.Draw(img)
    rng = random.Random(9)
    for ty in range(150, H, 14):   # Basaltboden mit Lavaspalten
        for tx in range(-7 if ty // 14 % 2 else 0, W, 14):
            d.rectangle([tx + 1, ty + 1, tx + 13, ty + 13], fill=rng.choice([(40, 34, 40), (48, 42, 48), (34, 30, 36)]))
    her = pose(face=1, head=(34, 98), shoulder_f=(42, 114), shoulder_b=(30, 112), elbow_f=(54, 112), elbow_b=(44, 120),
               hands=(60, 108), hip=(30, 150), knee_f=(46, 170), foot_f=(54, 198), knee_b=(16, 172), foot_b=(8, 198),
               blade_angle=-20, blade_len=48, wind=(-1, 0.1), hair_len=46, seed=24, hit_at=0.75)
    man = pose(face=-1, head=(222, 92), shoulder_f=(214, 108), shoulder_b=(228, 106), elbow_f=(212, 92), elbow_b=(226, 90),
               hands=(218, 78), hip=(226, 146), knee_f=(208, 168), foot_f=(198, 198), knee_b=(240, 168), foot_b=(250, 198),
               blade_angle=-105, blade_len=50, fierce=True)
    s1, (h1, t1) = figure(her, HEROINE, True)
    s2, (h2, t2) = figure(man, MAN, False)
    img.alpha_composite(s2); blade(img, h2, t2, MAN["blade"])
    img.alpha_composite(s1); blade(img, h1, t1, HEROINE["blade"])
    hit = (h1[0] + (t1[0] - h1[0]) * 0.6, h1[1] + (t1[1] - h1[1]) * 0.6)
    drone(img, (146, 80), r=15)
    bolt(img, (146, 80), hit, MAN["blade"], 9)
    deflect(img, hit, (146, 80), MAN["blade"], 19)
    frame(img, "LAVA-SCHMIEDE")
    return img


def design10(caption=True):
    """Comic-Doppelpanel: diagonal geteilt, Mann oben rechts im Angriff, Heldin unten links in der Abwehr, Drohne auf der Trennlinie."""
    img = Image.new("RGBA", (W, H))
    banded(img, (20, 30, 80), (60, 20, 70), 8)
    lower = Image.new("RGBA", (W, H))
    banded(lower, (255, 200, 110), (240, 90, 70), 10)
    mask = Image.new("L", (W, H), 0)
    ImageDraw.Draw(mask).polygon([(0, 60), (W, 170), (W, H), (0, H)], fill=255)
    img.paste(lower, (0, 0), mask)
    speed_lines(img, (128, 116), (255, 255, 255, 120), 30, 10, inner=30)
    halftone(img, (W, 0), (120, 90, 200, 255), extent=100)
    d = ImageDraw.Draw(img)
    d.line([(0, 60), (W, 170)], fill=rgba(INK), width=5)
    d.line([(0, 60), (W, 170)], fill=(255, 255, 255, 255), width=1)
    her = pose(face=1, head=(44, 126), shoulder_f=(52, 142), shoulder_b=(40, 140), elbow_f=(66, 144), elbow_b=(54, 152),
               hands=(72, 138), hip=(40, 176), knee_f=(58, 194), foot_f=(66, 218), knee_b=(24, 198), foot_b=(14, 218),
               blade_angle=-62, blade_len=52, wind=(-1, 0.3), hair_len=50, seed=25, hit_at=0.65)
    # Arme hinter dem Kopf hochgerissen, damit das grimmige Gesicht frei bleibt
    man = pose(face=-1, head=(204, 44), shoulder_f=(198, 60), shoulder_b=(212, 58), elbow_f=(216, 46), elbow_b=(226, 48),
               hands=(222, 28), hip=(210, 98), knee_f=(192, 120), foot_f=(182, 148), knee_b=(226, 120), foot_b=(238, 146),
               blade_angle=-150, blade_len=50, fierce=True)
    scene(img, her, man, (128, 112), seed=10)
    frame(img, "ANGRIFF UND ABWEHR" if caption else "")
    return img


DESIGNS2 = [("figuren-6-abwehr", design6), ("figuren-7-sternennacht", design7), ("figuren-8-hologramm-duell", design8),
            ("figuren-9-lava-schmiede", design9), ("figuren-10-angriff-und-abwehr", design10)]

DESIGNS = [("figuren-1-klingenkreuzen", design1), ("figuren-2-sprungangriff", design2), ("figuren-3-ausfall", design3),
           ("figuren-4-drehhieb", design4), ("figuren-5-auge-in-auge", design5)]


def main(out: Path, designs=None, sheet_name="uebersicht-figuren.png"):
    out.mkdir(parents=True, exist_ok=True)
    imgs = []
    for key, fn in designs or DESIGNS:
        img = quantize(fn())
        colors = len(img.getcolors(1 << 24))
        assert colors <= 256
        img.resize((W * 4, H * 4), Image.Resampling.NEAREST).save(out / f"{key}.png")
        imgs.append(img)
        print(f"{key:28s} {colors} Farben")
    pad = 10
    sheet = Image.new("RGB", (3 * (W * 2 + pad) + pad, 2 * (H * 2 + pad) + pad), (24, 24, 28))
    for i, im in enumerate(imgs):
        x, y = pad + (i % 3) * (W * 2 + pad), pad + (i // 3) * (H * 2 + pad)
        sheet.paste(im.resize((W * 2, H * 2), Image.Resampling.NEAREST), (x, y))
    sheet.save(out / sheet_name)


def design10_detail(caption=True):
    """Entwurf 10, ausgearbeitet: gleiche Szene, Figuren mit Gesichtern, Fingern, Falten und Ausruestungsdetails."""
    global figure_detail
    figure_detail = True
    try:
        return design10(caption)
    finally:
        figure_detail = False


figure_detail = False


def export_title_panel(path: Path):
    """Entwurf 10 ohne Bildunterschrift als kompaktes Palettenbild fuer das Titelbild (LZPX-Format):
    'LZPX', Breite/Hoehe (je 2 Byte), Farbanzahl (1 Byte), Palette (RGB), dann je Pixel ein Farbindex."""
    img = quantize(design10(caption=False))
    colors = [c for _, c in img.getcolors(256)]
    index = {c: i for i, c in enumerate(colors)}
    data = bytearray(b"LZPX") + W.to_bytes(2, "little") + H.to_bytes(2, "little") + bytes([len(colors) - 1])
    for c in colors:
        data += bytes(c)
    px = img.load()
    data += bytes(index[px[x, y]] for y in range(H) for x in range(W))
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)
    print(f"{path}: {len(colors)} Farben, {len(data)} Byte")


if __name__ == "__main__":
    if "--detail" in sys.argv:
        target = Path(sys.argv[1])
        target.mkdir(parents=True, exist_ok=True)
        before = quantize(design10())
        after = quantize(design10_detail())
        print(f"Farben: vorher {len(before.getcolors(1 << 24))}, nachher {len(after.getcolors(1 << 24))}")
        after.resize((W * 4, H * 4), Image.Resampling.NEAREST).save(target / "figuren-10-ausgearbeitet.png")
        sheet = Image.new("RGB", (W * 4 + 30, H * 2 + 20), (24, 24, 28))
        sheet.paste(before.resize((W * 2, H * 2), Image.Resampling.NEAREST), (10, 10))
        sheet.paste(after.resize((W * 2, H * 2), Image.Resampling.NEAREST), (W * 2 + 20, 10))
        sheet.save(target / "figuren-10-vergleich.png")
        sys.exit(0)
    if "--titelbild" in sys.argv:
        export_title_panel(Path(sys.argv[1]))
        sys.exit(0)
    target = Path(sys.argv[1] if len(sys.argv) > 1 else "mockups/figuren")
    if "--zweite-runde" in sys.argv:
        main(target, DESIGNS2, "uebersicht-figuren-2.png")
    else:
        main(target)
