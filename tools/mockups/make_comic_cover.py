"""Realistischer Comic-Cover-Versuch der Szene aus Entwurf 10 (nur Illustration, keine Cover-Schrift):
Heldin wehrt links einen Blitz der Drohne ab, der Mann holt rechts zum Schlag aus.

Alles per Programm gezeichnet: anatomische Proportionen (rund 7,5 Kopfhoehen), Profilgesichter, Tuschekonturen,
Cel-Schattierung mit Rasterpunkten im Schatten, Kantenlicht, Strahlenkranz. Ausgabe in 1024 x 896 (Vorschau)
und 256 x 224 mit hoechstens 256 Farben aus 4096 (Spielfassung).

Aufruf: python3 tools/mockups/make_comic_cover.py <ausgabeordner>
"""
from __future__ import annotations

import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

sys.path.insert(0, str(Path(__file__).parent))
from make_snes_mockups import quantize  # noqa: E402

W, H = 1024, 896
K = 2                       # Ueberabtastung: intern doppelt so gross zeichnen, am Ende glatt verkleinern
INK = (10, 6, 18)
DRONE = (560, 430)
CENTER_LIGHT = (560, 400)

HEROINE = {
    "skin": ((246, 204, 172), (198, 132, 112), (255, 232, 210)),
    "suit": ((232, 238, 248), (138, 152, 196), (255, 255, 255)),
    "accent": ((40, 150, 240), (20, 70, 150), (140, 220, 255)),
    "dark": ((40, 50, 96), (16, 18, 44), (90, 110, 170)),
    "hair": ((246, 208, 104), (184, 124, 48), (255, 246, 196)),
    "lips": (196, 82, 96),
    "iris": (40, 130, 200),
    "blade": (70, 225, 255),
    "rim": (255, 150, 220),
}
MAN = {
    "skin": ((226, 176, 138), (150, 92, 76), (250, 210, 176)),
    "suit": ((150, 30, 42), (70, 12, 30), (220, 80, 80)),
    "accent": ((222, 172, 70), (140, 92, 36), (255, 226, 140)),
    "dark": ((62, 58, 70), (26, 24, 34), (110, 106, 124)),
    "hair": ((112, 68, 38), (58, 32, 20), (168, 112, 66)),
    "lips": (150, 84, 76),
    "iris": (120, 70, 30),
    "blade": (255, 60, 170),
    "rim": (110, 200, 255),
}


# ----------------------------------------------------------------------------- Hilfsfunktionen
def s(p):
    return (p[0] * K, p[1] * K)


def lerp(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


def mix(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3))


def unit(v):
    n = math.hypot(*v) or 1
    return (v[0] / n, v[1] / n)


def new_mask():
    return Image.new("L", (W * K, H * K), 0)


def chaikin(pts, rounds=3, closed=True):
    for _ in range(rounds):
        out = []
        n = len(pts)
        for i in range(n if closed else n - 1):
            a, b = pts[i], pts[(i + 1) % n]
            out += [lerp(a, b, 0.25), lerp(a, b, 0.75)]
        if not closed:
            out = [pts[0]] + out + [pts[-1]]
        pts = out
    return pts


def poly_mask(pts, smooth=3):
    m = new_mask()
    ImageDraw.Draw(m).polygon([s(p) for p in chaikin(pts, smooth)], fill=255)
    return m


def limb_points(p0, p1, w0, w1, bulge_l=0.0, bulge_r=0.0, peak=0.4, steps=14):
    """Umriss eines Gliedes: verjuengt sich von w0 nach w1, Muskelbauch je Seite (links/rechts der Laufrichtung)."""
    d = unit((p1[0] - p0[0], p1[1] - p0[1]))
    n = (-d[1], d[0])
    left, right = [], []
    for i in range(steps + 1):
        t = i / steps
        c = lerp(p0, p1, t)
        w = (w0 + (w1 - w0) * t) / 2
        hump = math.sin(math.pi * min(1.0, t / (2 * peak))) if t < 2 * peak else math.sin(math.pi * (0.5 + (t - 2 * peak) / (2 * (1 - peak)) * 0.5 + 0.0)) * 0.6
        hump = max(0.0, hump)
        left.append((c[0] + n[0] * (w + bulge_l * hump), c[1] + n[1] * (w + bulge_l * hump)))
        right.append((c[0] - n[0] * (w + bulge_r * hump), c[1] - n[1] * (w + bulge_r * hump)))
    return left + right[::-1]


def limb_mask(p0, p1, w0, w1, bulge_l=0.0, bulge_r=0.0, peak=0.4):
    m = new_mask()
    d = ImageDraw.Draw(m)
    d.polygon([s(p) for p in limb_points(p0, p1, w0, w1, bulge_l, bulge_r, peak)], fill=255)
    for p, w in ((p0, w0), (p1, w1)):
        r = w / 2 * K
        d.ellipse((p[0] * K - r, p[1] * K - r, p[0] * K + r, p[1] * K + r), fill=255)
    return m


def shifted(m, dx, dy):
    out = Image.new("L", m.size, 0)
    out.paste(m, (int(round(dx)), int(round(dy))))
    return out


def band(m, direction, dist):
    """Teil der Maske, der beim Verschieben um dist entgegen 'direction' herausfaellt: Kante in Richtung direction."""
    sh = shifted(m, -direction[0] * dist, -direction[1] * dist)
    return ImageChops.multiply(m, ImageChops.invert(sh))


_DOTS: Image.Image | None = None


def dot_pattern():
    """Rasterpunkte wie im Vierfarbdruck (45 Grad versetzt)."""
    global _DOTS
    if _DOTS is None:
        _DOTS = new_mask()
        d = ImageDraw.Draw(_DOTS)
        step, r = 5 * K, 1.15 * K
        for row, y in enumerate(range(0, H * K + step, step)):
            off = step / 2 if row % 2 else 0
            for x in range(0, W * K + step, step):
                d.ellipse((x + off - r, y - r, x + off + r, y + r), fill=255)
    return _DOTS


def render_part(canvas, mask, colors, light, rim_color=None, shade=14, ink=3, dots=True, hi=4):
    """Ein Koerperteil: Tuschekontur, Grundfarbe, Schatten (abgewandte Seite) mit Rasterpunkten,
    Glanzkante zum Licht, farbiges Kantenlicht auf der Schattenseite."""
    box = mask.getbbox()
    if box is None:
        return
    pad = int((ink + 4) * K)
    box = (max(0, box[0] - pad), max(0, box[1] - pad), min(W * K, box[2] + pad), min(H * K, box[3] + pad))
    m = mask.crop(box)
    base, dark, light_c = colors
    size = int(round(ink * K)) * 2 + 1
    canvas.paste(INK, box, m.filter(ImageFilter.MaxFilter(size)))
    part = Image.new("RGB", m.size, base)
    away = (-light[0], -light[1])
    shadow = band(m, away, shade * K)
    part.paste(dark, (0, 0), shadow)
    if dots:
        deep = mix(dark, INK, 0.45)
        part.paste(deep, (0, 0), ImageChops.multiply(shadow, dot_pattern().crop(box)))
    if hi:
        part.paste(light_c, (0, 0), band(m, light, hi * K))
    if rim_color:
        rim = band(m, away, 2.2 * K)
        part.paste(rim_color, (0, 0), rim)
    # Terminator-Linie: duenne dunkle Linie zwischen Licht und Schatten fuer den gezeichneten Look
    edge = ImageChops.subtract(shadow.filter(ImageFilter.MaxFilter(3)), shadow)
    part.paste(mix(dark, INK, 0.3), (0, 0), ImageChops.multiply(edge, m))
    canvas.paste(part, box, m)


def ink_line(canvas, pts, width, color=INK, smooth=0):
    if smooth:
        pts = chaikin(pts, smooth, closed=False)
    ImageDraw.Draw(canvas).line([s(p) for p in pts], fill=color, width=max(1, int(width * K)), joint="curve")


def blur_glow(canvas, draw_fn, radius, strength=1.0):
    """Leuchten: in eigene Ebene zeichnen, weichzeichnen und additiv auflegen."""
    glow = Image.new("RGB", canvas.size, (0, 0, 0))
    draw_fn(ImageDraw.Draw(glow))
    glow = glow.filter(ImageFilter.GaussianBlur(radius * K))
    if strength != 1.0:
        glow = glow.point(lambda v: min(255, int(v * strength)))
    canvas.paste(ImageChops.add(canvas, glow))


# ----------------------------------------------------------------------------- Hintergrund
def background(canvas):
    d = ImageDraw.Draw(canvas)
    horizon = 560
    sky = [(0, (10, 6, 30)), (0.45, (46, 18, 78)), (0.8, (150, 40, 110)), (1.0, (250, 120, 90))]
    for y in range(horizon * K):
        t = y / (horizon * K)
        for (t0, c0), (t1, c1) in zip(sky, sky[1:]):
            if t0 <= t <= t1:
                d.line([(0, y), (W * K, y)], fill=mix(c0, c1, (t - t0) / (t1 - t0)))
                break
    rnd = random.Random(4)
    for _ in range(260):
        x, y = rnd.uniform(0, W), rnd.uniform(0, horizon - 120)
        r = rnd.choice((0.6, 0.8, 1.0, 1.4))
        c = mix((255, 255, 255), (150, 160, 255), rnd.random())
        d.ellipse((x * K - r * K, y * K - r * K, x * K + r * K, y * K + r * K), fill=c)

    # Strahlenkranz hinter der Drohne (klassisches Cover-Motiv)
    rays = Image.new("L", canvas.size, 0)
    rd = ImageDraw.Draw(rays)
    cx, cy = CENTER_LIGHT
    for i in range(36):
        a0 = i / 36 * 2 * math.pi
        a1 = a0 + 2 * math.pi / 72
        far = 1600
        rd.polygon([s((cx, cy)), s((cx + math.cos(a0) * far, cy + math.sin(a0) * far)),
                    s((cx + math.cos(a1) * far, cy + math.sin(a1) * far))], fill=60)
    rays = rays.filter(ImageFilter.GaussianBlur(2 * K))
    fade = Image.radial_gradient("L").resize(canvas.size).point(lambda v: max(0, 255 - v))
    canvas.paste((255, 200, 230), (0, 0), ImageChops.multiply(rays, fade))

    # Arenaboden mit Perspektivraster
    for y in range(horizon * K, H * K):
        t = (y - horizon * K) / ((H - horizon) * K)
        d.line([(0, y), (W * K, y)], fill=mix((60, 30, 70), (14, 10, 26), t ** 0.6))
    vx = cx
    for i in range(-14, 15):
        x_far = vx + i * 30
        x_near = vx + i * 150
        d.line([s((x_far, horizon)), s((x_near, H))], fill=(90, 60, 140), width=K)
    y = horizon + 6
    step = 8
    while y < H:
        d.line([s((0, y)), s((W, y))], fill=(90, 60, 140), width=K)
        step *= 1.35
        y += step
    d.line([s((0, horizon)), s((W, horizon))], fill=(255, 190, 170), width=2 * K)

    # Lichtpfuetze der Drohne auf dem Boden
    blur_glow(canvas, lambda g: g.ellipse((*s((cx - 260, 600)), *s((cx + 260, 700))), fill=(120, 60, 120)), 30)

    # Rasterpunkte in den oberen Ecken
    corner = Image.new("L", canvas.size, 0)
    cd = ImageDraw.Draw(corner)
    for (ox, oy) in ((0, 0), (W, 0)):
        for gy in range(0, 260, 9):
            for gx in range(0, 300, 9):
                x = gx if ox == 0 else W - gx
                dist = math.hypot(gx, gy)
                r = max(0.0, 3.2 * (1 - dist / 300))
                if r > 0.3:
                    cd.ellipse((x * K - r * K, gy * K - r * K, x * K + r * K, gy * K + r * K), fill=255)
    canvas.paste((90, 40, 150), (0, 0), corner)


def speed_lines(canvas):
    rnd = random.Random(9)
    lines = Image.new("L", canvas.size, 0)
    d = ImageDraw.Draw(lines)
    cx, cy = CENTER_LIGHT
    for _ in range(70):
        a = rnd.uniform(0, 2 * math.pi)
        r0 = rnd.uniform(170, 300)
        r1 = r0 + rnd.uniform(120, 420)
        d.line([s((cx + math.cos(a) * r0, cy + math.sin(a) * r0)), s((cx + math.cos(a) * r1, cy + math.sin(a) * r1))],
               fill=rnd.choice((90, 140, 200)), width=rnd.choice((1, 2)) * K)
    canvas.paste((255, 240, 250), (0, 0), lines)


def floor_shadow(canvas, x0, x1, y):
    sh = new_mask()
    ImageDraw.Draw(sh).ellipse((*s((x0, y - 14)), *s((x1, y + 14))), fill=170)
    canvas.paste((8, 4, 16), (0, 0), sh.filter(ImageFilter.GaussianBlur(6 * K)))


# ----------------------------------------------------------------------------- Kopf im Profil
def profile(points, center, size, facing):
    return [(center[0] + x * size * facing, center[1] + y * size) for x, y in points]


HER_HEAD = [(-0.12, -0.50), (0.12, -0.48), (0.25, -0.39), (0.31, -0.25), (0.32, -0.13), (0.30, -0.06), (0.335, 0.03),
            (0.37, 0.08), (0.355, 0.105), (0.31, 0.115), (0.315, 0.155), (0.33, 0.175), (0.30, 0.20), (0.32, 0.225),
            (0.30, 0.255), (0.275, 0.28), (0.29, 0.33), (0.25, 0.385), (0.14, 0.42), (0.03, 0.37), (-0.06, 0.28),
            (-0.22, 0.14), (-0.40, 0.0), (-0.43, -0.20), (-0.34, -0.40)]
MAN_HEAD = [(-0.12, -0.50), (0.12, -0.49), (0.26, -0.40), (0.31, -0.27), (0.345, -0.15), (0.32, -0.08), (0.36, 0.02),
            (0.40, 0.085), (0.38, 0.115), (0.32, 0.125), (0.33, 0.165), (0.345, 0.185), (0.31, 0.21), (0.335, 0.24),
            (0.31, 0.27), (0.30, 0.30), (0.33, 0.36), (0.30, 0.42), (0.20, 0.46), (0.04, 0.42), (-0.06, 0.30),
            (-0.22, 0.15), (-0.42, 0.02), (-0.45, -0.20), (-0.35, -0.41)]


def head(canvas, center, size, facing, pal, light, heroine):
    pts = HER_HEAD if heroine else MAN_HEAD
    m = poly_mask(profile(pts, center, size, facing), smooth=2)
    render_part(canvas, m, pal["skin"], light, pal["rim"], shade=size * 0.16, ink=2.5, dots=False)
    P = lambda x, y: (center[0] + x * size * facing, center[1] + y * size)  # noqa: E731

    # Ohr
    ear = poly_mask([P(-0.02, -0.04), P(-0.10, -0.06), P(-0.14, 0.02), P(-0.12, 0.12), P(-0.05, 0.14), P(-0.03, 0.06)])
    render_part(canvas, ear, pal["skin"], light, None, shade=size * 0.04, ink=1.5, dots=False)
    ink_line(canvas, [P(-0.05, -0.02), P(-0.10, 0.0), P(-0.10, 0.07), P(-0.07, 0.10)], 1.2, pal["skin"][1], smooth=2)

    # Auge (Profil): Lidfalte, Weiss, Iris, Glanzpunkt
    eye = [P(0.17, -0.07), P(0.24, -0.085), P(0.275, -0.06), P(0.245, -0.04), P(0.19, -0.045)]
    ImageDraw.Draw(canvas).polygon([s(p) for p in eye], fill=(245, 245, 250))
    iris_c = P(0.235, -0.062)
    r = size * 0.024
    ImageDraw.Draw(canvas).ellipse((*s((iris_c[0] - r * 0.8, iris_c[1] - r)), *s((iris_c[0] + r * 0.8, iris_c[1] + r))),
                                   fill=pal["iris"])
    ImageDraw.Draw(canvas).ellipse((*s((iris_c[0] - r * 0.3, iris_c[1] - r * 0.4)), *s((iris_c[0] + r * 0.3, iris_c[1] + r * 0.4))),
                                   fill=INK)
    ImageDraw.Draw(canvas).ellipse((*s((iris_c[0] + r * 0.1 * facing - 1.2, iris_c[1] - r * 0.6 - 1.2)),
                                    *s((iris_c[0] + r * 0.1 * facing + 1.2, iris_c[1] - r * 0.6 + 1.2))), fill=(255, 255, 255))
    ink_line(canvas, [P(0.16, -0.06), P(0.20, -0.085), P(0.25, -0.088), P(0.285, -0.06)], 2.6 if heroine else 2.2, smooth=2)
    ink_line(canvas, [P(0.19, -0.042), P(0.245, -0.038)], 1.0, pal["skin"][1])
    ink_line(canvas, [P(0.17, -0.105), P(0.24, -0.115)], 1.0, pal["skin"][1])         # Lidfalte
    if heroine:
        for lx, ly in ((0.275, -0.065), (0.28, -0.075)):                              # Wimpern
            ink_line(canvas, [P(lx, ly), P(lx + 0.03, ly - 0.02)], 1.6)
        ink_line(canvas, [P(0.15, -0.14), P(0.21, -0.165), P(0.28, -0.15)], 2.2, pal["hair"][1], smooth=2)  # Braue
        # Lippen und Mund
        lips = poly_mask([P(0.305, 0.16), P(0.33, 0.175), P(0.305, 0.20), P(0.322, 0.225), P(0.30, 0.25), P(0.27, 0.22),
                          P(0.27, 0.18)], smooth=1)
        canvas.paste(pal["lips"], (0, 0), lips)
        ink_line(canvas, [P(0.27, 0.20), P(0.30, 0.20)], 1.4)
        ink_line(canvas, [P(0.30, 0.13), P(0.33, 0.12)], 1.0, pal["skin"][1])         # Nasenfluegel
        ink_line(canvas, [P(0.12, 0.10), P(0.20, 0.06)], 1.0, pal["skin"][1])          # Wangenknochen
    else:
        # Zornige Braue: tief, schraeg zur Nasenwurzel
        brow = poly_mask([P(0.13, -0.165), P(0.22, -0.16), P(0.33, -0.11), P(0.31, -0.09), P(0.21, -0.125), P(0.13, -0.135)],
                         smooth=1)
        canvas.paste(pal["hair"][1], (0, 0), brow)
        ink_line(canvas, [P(0.27, -0.13), P(0.30, -0.17)], 1.2, pal["skin"][1])       # Zornesfalte
        # Gefletschte Zaehne
        mouth = poly_mask([P(0.28, 0.185), P(0.335, 0.19), P(0.33, 0.235), P(0.27, 0.235)], smooth=0)
        canvas.paste(INK, (0, 0), mouth)
        teeth = poly_mask([P(0.285, 0.195), P(0.325, 0.198), P(0.322, 0.225), P(0.282, 0.225)], smooth=0)
        canvas.paste((236, 230, 214), (0, 0), teeth)
        ink_line(canvas, [P(0.284, 0.211), P(0.324, 0.211)], 1.0)
        ink_line(canvas, [P(0.24, 0.12), P(0.30, 0.17)], 1.4, pal["skin"][1])         # Nasolabialfalte
        ink_line(canvas, [P(0.10, 0.08), P(0.20, 0.10)], 1.2, pal["skin"][1])          # Wangenknochen
        # Bartschatten
        rnd = random.Random(3)
        stub = new_mask()
        sd = ImageDraw.Draw(stub)
        for _ in range(150):
            x, y = rnd.uniform(0.04, 0.30), rnd.uniform(0.24, 0.44)
            px, py = P(x, y)
            sd.point(s((px, py)), fill=255)
        canvas.paste(mix(pal["skin"][0], pal["hair"][1], 0.45), (0, 0), ImageChops.multiply(stub, m))


def her_hair(canvas, center, size, light, pal):
    """Kopfhaar und lange, wehende Locken als eine zusammenhaengende Masse (keine Naht zwischen Kappe und Locken)."""
    P = lambda x, y: (center[0] + x * size, center[1] + y * size)  # noqa: E731
    m = poly_mask([P(0.24, -0.36), P(0.20, -0.47), P(0.02, -0.57), P(-0.22, -0.55), P(-0.42, -0.42), P(-0.50, -0.20),
                   P(-0.46, 0.10), P(-0.34, 0.30), P(-0.20, 0.30), P(-0.14, 0.10), P(-0.10, -0.08), P(0.02, -0.20),
                   P(0.16, -0.24), P(0.30, -0.17), P(0.33, -0.28)])
    d = ImageDraw.Draw(m)
    rnd = random.Random(12)
    locks = []
    for i in range(9):
        t = i / 8
        root = P(-0.25 - 0.12 * math.sin(t * 3), -0.45 + t * 0.7)
        length = size * (1.5 + 0.8 * math.sin(t * 2.8) + rnd.uniform(-0.1, 0.1))
        phase = rnd.uniform(0, 1.5)
        pts = []
        for k in range(24):
            u = k / 23
            x = root[0] - u * length
            y = root[1] + (u ** 1.4) * size * (0.05 + 0.75 * t) + math.sin(u * 4.6 + phase) * size * 0.13 * u
            pts.append((x, y))
        w0 = size * rnd.uniform(0.26, 0.34)
        locks.append((pts, w0))
        poly_l, poly_r = [], []
        for k, p in enumerate(pts):
            q = pts[min(k + 1, len(pts) - 1)] if k < len(pts) - 1 else p
            o = pts[k - 1] if k else p
            dd = unit((q[0] - o[0], q[1] - o[1]))
            w = w0 * (1 - k / len(pts)) ** 0.9 / 2 + 1
            poly_l.append((p[0] - dd[1] * w, p[1] + dd[0] * w))
            poly_r.append((p[0] + dd[1] * w, p[1] - dd[0] * w))
        tip = (pts[-1][0] - size * 0.08, pts[-1][1] + size * 0.03)
        d.polygon([s(p) for p in poly_l + [tip] + poly_r[::-1]], fill=255)
    render_part(canvas, m, pal["hair"], light, pal["rim"], shade=size * 0.10, ink=2.5, dots=False)
    for pts, w0 in locks:                                                              # Straehnen und Glanz
        for off in (-0.25, 0.0, 0.25):
            line = [(x, y + off * w0 * (1 - k / len(pts))) for k, (x, y) in enumerate(pts)]
            ink_line(canvas, line[1:-3], 1.0, pal["hair"][1], smooth=1)
        ink_line(canvas, [(x, y - w0 * 0.3 * (1 - k / len(pts))) for k, (x, y) in enumerate(pts)][2:10], 1.6,
                 pal["hair"][2], smooth=1)
    for k in range(7):                                                                  # Straehnen am Oberkopf
        t = k / 6
        ink_line(canvas, [P(0.25 - t * 0.05, -0.33 + t * 0.05), P(0.0, -0.45 + t * 0.2), P(-0.25, -0.40 + t * 0.32),
                          P(-0.42, -0.15 + t * 0.3)], 1.0, pal["hair"][1], smooth=2)
    ink_line(canvas, [P(0.18, -0.43), P(-0.05, -0.51), P(-0.28, -0.45)], 2.0, pal["hair"][2], smooth=2)
    # Ohr wieder ueber das Haar
    ear = poly_mask([P(-0.02, -0.04), P(-0.10, -0.06), P(-0.14, 0.02), P(-0.12, 0.12), P(-0.05, 0.14), P(-0.03, 0.06)])
    render_part(canvas, ear, pal["skin"], light, None, shade=size * 0.04, ink=1.5, dots=False)
    ink_line(canvas, [P(-0.05, -0.02), P(-0.10, 0.0), P(-0.10, 0.07), P(-0.07, 0.10)], 1.2, pal["skin"][1], smooth=2)


def man_hair(canvas, center, size, light, pal):
    P = lambda x, y: (center[0] - x * size, center[1] + y * size)  # noqa: E731  (schaut nach links)
    cap = poly_mask([P(0.27, -0.36), P(0.26, -0.50), P(0.16, -0.60), P(0.06, -0.57), P(-0.04, -0.66), P(-0.12, -0.58),
                     P(-0.26, -0.62), P(-0.32, -0.50), P(-0.46, -0.44), P(-0.48, -0.20), P(-0.42, 0.0), P(-0.30, 0.04),
                     P(-0.14, -0.04), P(-0.02, -0.06), P(0.0, 0.10), P(0.05, 0.10), P(0.06, -0.14), P(0.16, -0.24),
                     P(0.26, -0.26)], smooth=1)
    render_part(canvas, cap, pal["hair"], light, pal["rim"], shade=size * 0.12, ink=2.5, dots=False)
    for k in range(6):
        t = k / 5
        ink_line(canvas, [P(0.24 - t * 0.04, -0.34 + t * 0.04), P(0.05, -0.50 + t * 0.12), P(-0.20, -0.48 + t * 0.2),
                          P(-0.40, -0.30 + t * 0.25)], 1.0, pal["hair"][1], smooth=2)
    ink_line(canvas, [P(0.18, -0.46), P(0.0, -0.54), P(-0.2, -0.50)], 1.8, pal["hair"][2], smooth=2)


# ----------------------------------------------------------------------------- Schwert, Haende, Drohne, Blitz
def sword_blade(canvas, base, tip, color):
    def draw(g):
        g.line([s(base), s(tip)], fill=color, width=26 * K)
    blur_glow(canvas, draw, 10, 1.1)
    d = ImageDraw.Draw(canvas)
    d.line([s(base), s(tip)], fill=mix(color, (255, 255, 255), 0.35), width=11 * K)
    d.line([s(base), s(tip)], fill=(255, 255, 255), width=6 * K)
    r = 3 * K
    d.ellipse((tip[0] * K - r, tip[1] * K - r, tip[0] * K + r, tip[1] * K + r), fill=(255, 255, 255))


def hilt(canvas, p0, p1, light):
    m = limb_mask(p0, p1, 13, 13)
    render_part(canvas, m, ((150, 156, 170), (70, 72, 86), (235, 240, 250)), light, None, shade=5, ink=2, dots=False)
    for k in range(1, 6):
        t = k / 6
        c = lerp(p0, p1, t)
        dd = unit((p1[0] - p0[0], p1[1] - p0[1]))
        n = (-dd[1] * 6, dd[0] * 6)
        ink_line(canvas, [(c[0] + n[0], c[1] + n[1]), (c[0] - n[0], c[1] - n[1])], 1.2, (50, 50, 64))
    em = limb_mask(lerp(p0, p1, 0.92), p1, 17, 17)                                    # Emitter
    render_part(canvas, em, ((90, 94, 110), (40, 40, 52), (200, 206, 220)), light, None, shade=4, ink=2, dots=False)


def fist(canvas, at, direction, size, pal, light):
    """Faust um den Griff: Handruecken plus Fingerreihe."""
    d = unit(direction)
    n = (-d[1], d[0])
    pts = [(at[0] + d[0] * size * a + n[0] * size * b, at[1] + d[1] * size * a + n[1] * size * b)
           for a, b in ((-0.55, -0.6), (0.55, -0.65), (0.6, 0.55), (0.0, 0.75), (-0.6, 0.55))]
    m = poly_mask(pts, smooth=2)
    render_part(canvas, m, pal["dark"], light, pal["rim"], shade=size * 0.3, ink=2.2, dots=False)
    for k in (-0.3, 0.05, 0.4):                                                          # Fingerfugen
        a = (at[0] + d[0] * size * k + n[0] * size * 0.1, at[1] + d[1] * size * k + n[1] * size * 0.1)
        b = (a[0] + n[0] * size * 0.55, a[1] + n[1] * size * 0.55)
        ink_line(canvas, [a, b], 1.2, pal["dark"][1])


def drone(canvas):
    cx, cy = DRONE
    r = 44
    blur_glow(canvas, lambda g: g.ellipse((*s((cx - r * 2.2, cy - r * 2.2)), *s((cx + r * 2.2, cy + r * 2.2))),
                                          fill=(160, 70, 120)), 24)
    d = ImageDraw.Draw(canvas)
    d.ellipse((*s((cx - r - 16, cy - r * 0.42 - 6)), *s((cx + r + 16, cy + r * 0.42 + 6))), fill=INK)  # Ring hinten
    d.ellipse((*s((cx - r - 12, cy - r * 0.42 - 2)), *s((cx + r + 12, cy + r * 0.42 + 2))), fill=(90, 92, 110))
    # Kugel: Verlauf zum Licht (oben links) plus Kantenlicht
    d.ellipse((*s((cx - r - 3, cy - r - 3)), *s((cx + r + 3, cy + r + 3))), fill=INK)
    for k in range(40, 0, -1):
        t = k / 40
        rr = r * t
        ox, oy = -r * 0.35 * (1 - t), -r * 0.35 * (1 - t)
        d.ellipse((*s((cx + ox - rr, cy + oy - rr)), *s((cx + ox + rr, cy + oy + rr))),
                  fill=mix((230, 232, 245), (40, 42, 64), t ** 1.6))
    # Ring vorne mit Markierungen
    ring = new_mask()
    ImageDraw.Draw(ring).arc((*s((cx - r - 14, cy - r * 0.42 - 4)), *s((cx + r + 14, cy + r * 0.42 + 4))), 0, 180,
                             fill=255, width=9 * K)
    canvas.paste(INK, (0, 0), ring.filter(ImageFilter.MaxFilter(5)))
    canvas.paste((170, 174, 196), (0, 0), ring)
    for a in (20, 90, 160):
        ax = cx + math.cos(math.radians(a)) * (r + 14)
        ay = cy + math.sin(math.radians(a)) * (r * 0.42 + 4)
        d.ellipse((*s((ax - 7, ay - 5)), *s((ax + 7, ay + 5))), fill=INK)
        d.ellipse((*s((ax - 5, ay - 3)), *s((ax + 5, ay + 3))), fill=(255, 150, 40))
    # Auge
    blur_glow(canvas, lambda g: g.ellipse((*s((cx - 22, cy - 18)), *s((cx + 14, cy + 18))), fill=(255, 40, 80)), 8, 1.4)
    d.ellipse((*s((cx - 13, cy - 11)), *s((cx + 7, cy + 9))), fill=INK)
    d.ellipse((*s((cx - 10, cy - 8)), *s((cx + 4, cy + 6))), fill=(255, 50, 90))
    d.ellipse((*s((cx - 6, cy - 5)), *s((cx - 2, cy - 1))), fill=(255, 230, 240))


def lightning(canvas, a, b, color, seed, amp=16, width=4):
    rnd = random.Random(seed)
    n = 14
    pts = [a]
    d = unit((b[0] - a[0], b[1] - a[1]))
    nrm = (-d[1], d[0])
    for i in range(1, n):
        t = i / n
        c = lerp(a, b, t)
        o = rnd.uniform(-amp, amp) * math.sin(math.pi * t) ** 0.5
        pts.append((c[0] + nrm[0] * o, c[1] + nrm[1] * o))
    pts.append(b)
    branches = []
    for i in (4, 8, 11):
        p = pts[i]
        ang = math.atan2(d[1], d[0]) + rnd.choice((-1, 1)) * rnd.uniform(0.5, 0.9)
        q = (p[0] + math.cos(ang) * 40, p[1] + math.sin(ang) * 40)
        branches.append([p, lerp(p, q, 0.5), q])

    def draw(g):
        g.line([s(p) for p in pts], fill=color, width=18 * K)
        for br in branches:
            g.line([s(p) for p in br], fill=color, width=8 * K)
    blur_glow(canvas, draw, 9, 1.2)
    dd = ImageDraw.Draw(canvas)
    dd.line([s(p) for p in pts], fill=mix(color, (255, 255, 255), 0.4), width=width * 2 * K, joint="curve")
    dd.line([s(p) for p in pts], fill=(255, 255, 255), width=width * K, joint="curve")
    for br in branches:
        dd.line([s(p) for p in br], fill=(255, 255, 255), width=max(1, width // 2) * K)


def impact(canvas, at, color):
    cx, cy = at
    blur_glow(canvas, lambda g: g.ellipse((*s((cx - 34, cy - 34)), *s((cx + 34, cy + 34))), fill=color), 12, 1.4)
    star = []
    for i in range(16):
        a = i / 16 * 2 * math.pi
        r = 24 if i % 2 == 0 else 6
        if i % 4 == 0:
            r = 40
        star.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    ImageDraw.Draw(canvas).polygon([s(p) for p in star], fill=(255, 255, 255))
    rnd = random.Random(5)
    for _ in range(14):                                                                  # Funken
        a = rnd.uniform(-2.2, 0.6)
        r0, r1 = rnd.uniform(18, 30), rnd.uniform(45, 85)
        ink_line(canvas, [(cx + math.cos(a) * r0, cy + math.sin(a) * r0), (cx + math.cos(a) * r1, cy + math.sin(a) * r1)],
                 rnd.choice((1.5, 2.5)), (255, 255, 230))


# ----------------------------------------------------------------------------- Figuren
def heroine(canvas):
    pal, L = HEROINE, unit((0.85, -0.5))
    hc, hs = (270, 254), 100
    floor_shadow(canvas, 60, 470, 776)

    # hinteres Bein (rechtes Bein, Ausfallschritt nach hinten)
    render_part(canvas, limb_mask((248, 505), (186, 618), 58, 40, 0, 6), pal["dark"], L, pal["rim"], shade=17)
    render_part(canvas, limb_mask((186, 618), (124, 736), 40, 26, 0, 9, 0.3), pal["dark"], L, pal["rim"], shade=13)
    render_part(canvas, poly_mask([(110, 724), (140, 726), (170, 768), (160, 780), (120, 778), (100, 748)], 2),
                pal["dark"], L, pal["rim"], shade=8)
    ink_line(canvas, [(140, 664), (178, 682)], 5, pal["accent"][0])  # Stiefelrand

    # hinterer Arm
    render_part(canvas, limb_mask((242, 334), (296, 420), 34, 28, 0, 5), pal["suit"], L, pal["rim"], shade=10)
    render_part(canvas, limb_mask((296, 420), (360, 402), 28, 22, 3, 0), pal["dark"], L, pal["rim"], shade=8)

    # Rumpf
    torso = [(234, 318), (276, 312), (312, 326), (326, 356), (322, 392), (306, 428), (300, 456), (312, 500), (296, 524),
             (240, 524), (220, 500), (232, 456), (226, 410), (220, 362)]
    render_part(canvas, poly_mask(torso), pal["suit"], L, pal["rim"], shade=22)
    ink_line(canvas, [(316, 362), (300, 386), (276, 392)], 1.4, pal["suit"][1], smooth=2)         # Brustfalte
    ink_line(canvas, [(272, 316), (282, 360), (290, 420), (300, 470), (304, 520)], 5, pal["accent"][0], smooth=2)
    ink_line(canvas, [(272, 316), (282, 360), (290, 420), (300, 470), (304, 520)], 1.2, INK, smooth=2)
    render_part(canvas, poly_mask([(226, 462), (302, 470), (306, 488), (224, 482)], 0), pal["dark"], L, None, shade=4,
                dots=False)                                                                         # Guertel
    render_part(canvas, poly_mask([(282, 466), (298, 468), (298, 486), (282, 484)], 0), pal["accent"], L, None, shade=3,
                dots=False)

    # vorderes Bein
    render_part(canvas, limb_mask((286, 502), (360, 612), 62, 42, 6, 0), pal["dark"], L, pal["rim"], shade=18)
    render_part(canvas, limb_mask((360, 612), (402, 748), 42, 26, 0, 10, 0.3), pal["dark"], L, pal["rim"], shade=13)
    render_part(canvas, poly_mask([(388, 738), (414, 738), (450, 766), (452, 780), (386, 780)], 2), pal["dark"], L,
                pal["rim"], shade=8)
    ink_line(canvas, [(362, 686), (400, 678)], 5, pal["accent"][0])
    render_part(canvas, poly_mask([(346, 596), (376, 600), (378, 626), (348, 626)], 2), pal["accent"], L, None, shade=6,
                dots=False)                                                                         # Knieschoner

    # Hals und Kopf
    render_part(canvas, limb_mask((264, 296), (270, 324), 32, 36), pal["skin"], L, pal["rim"], shade=10, dots=False)
    render_part(canvas, poly_mask([(244, 318), (276, 308), (300, 322), (276, 330)], 1), pal["suit"], L, None, shade=6,
                dots=False)                                                                         # Kragen
    head(canvas, hc, hs, 1, pal, L, True)
    her_hair(canvas, hc, hs, L, pal)

    # Schwert: Klinge zur Abwehr schraeg nach oben vorn
    g0, g1 = (352, 418), (392, 362)
    tip = (528, 172)
    sword_blade(canvas, g1, tip, pal["blade"])
    hilt(canvas, g0, g1, L)

    # vorderer Arm
    render_part(canvas, limb_mask((304, 336), (336, 416), 36, 30, 6, 0), pal["suit"], L, pal["rim"], shade=11)
    render_part(canvas, poly_mask([(296, 326), (318, 330), (322, 356), (300, 358)], 2), pal["accent"], L, None, shade=6,
                dots=False)                                                                         # Schulterstueck
    render_part(canvas, limb_mask((336, 416), (378, 380), 30, 22, 0, 4), pal["dark"], L, pal["rim"], shade=8)
    fist(canvas, (364, 400), (g1[0] - g0[0], g1[1] - g0[1]), 17, pal, L)
    fist(canvas, (382, 376), (g1[0] - g0[0], g1[1] - g0[1]), 17, pal, L)
    return lerp(g1, tip, 0.42)


def man(canvas):
    pal, L = MAN, unit((-0.85, -0.5))
    hc, hs = (814, 238), 96
    floor_shadow(canvas, 620, 990, 756)

    # hinterer Arm (erhoben, hinter dem Kopf)
    render_part(canvas, limb_mask((856, 312), (896, 222), 38, 32, 0, 6), pal["suit"], L, pal["rim"], shade=12)
    render_part(canvas, limb_mask((896, 222), (800, 128), 32, 24, 5, 0), pal["suit"], L, pal["rim"], shade=10)

    # hinteres Bein
    render_part(canvas, limb_mask((842, 498), (882, 610), 62, 44, 0, 6), pal["dark"], L, pal["rim"], shade=18)
    render_part(canvas, limb_mask((882, 610), (926, 728), 44, 28, 10, 0, 0.3), pal["dark"], L, pal["rim"], shade=13)
    render_part(canvas, poly_mask([(906, 716), (944, 718), (950, 756), (884, 760), (888, 740)], 2),
                ((40, 30, 30), (16, 12, 14), (90, 80, 76)), L, pal["rim"], shade=8)

    # Rumpf (breite Schultern)
    torso = [(764, 306), (792, 292), (830, 288), (868, 300), (880, 340), (870, 392), (852, 440), (858, 492), (842, 514),
             (790, 514), (776, 492), (782, 440), (766, 396), (758, 346)]
    render_part(canvas, poly_mask(torso), pal["suit"], L, pal["rim"], shade=24)
    ink_line(canvas, [(812, 300), (808, 360), (812, 420), (818, 500)], 4, pal["accent"][0], smooth=2)
    ink_line(canvas, [(812, 300), (808, 360), (812, 420), (818, 500)], 1.2, INK, smooth=2)
    ink_line(canvas, [(770, 380), (796, 392), (826, 388)], 1.4, pal["suit"][1], smooth=2)
    render_part(canvas, poly_mask([(778, 462), (856, 466), (856, 486), (776, 482)], 0), ((50, 36, 30), (24, 16, 14),
                (110, 86, 70)), L, None, shade=4, dots=False)
    render_part(canvas, poly_mask([(806, 462), (826, 463), (826, 486), (806, 485)], 0), pal["accent"], L, None, shade=3,
                dots=False)

    # vorderes Bein
    render_part(canvas, limb_mask((794, 496), (726, 600), 64, 44, 0, 6), pal["dark"], L, pal["rim"], shade=18)
    render_part(canvas, limb_mask((726, 600), (690, 734), 44, 28, 0, 10, 0.3), pal["dark"], L, pal["rim"], shade=13)
    render_part(canvas, poly_mask([(676, 722), (708, 722), (712, 760), (640, 762), (642, 746)], 2),
                ((40, 30, 30), (16, 12, 14), (90, 80, 76)), L, pal["rim"], shade=8)

    # Hals und Kopf
    render_part(canvas, limb_mask((818, 282), (814, 308), 40, 46), pal["skin"], L, pal["rim"], shade=12, dots=False)
    render_part(canvas, poly_mask([(784, 300), (814, 290), (850, 300), (816, 312)], 1), pal["accent"], L, None, shade=6,
                dots=False)
    head(canvas, hc, hs, -1, pal, L, False)
    man_hair(canvas, hc, hs, L, pal)

    # Schwert hoch erhoben, Klinge nach hinten oben: Ausholen zum Schlag
    g0, g1 = (758, 156), (804, 120)
    tip = (978, 6)
    sword_blade(canvas, g1, tip, pal["blade"])
    hilt(canvas, g0, g1, L)
    fist(canvas, (790, 130), (g1[0] - g0[0], g1[1] - g0[1]), 18, pal, L)

    # vorderer Arm (Ellbogen weit vorn, Gesicht bleibt frei)
    render_part(canvas, limb_mask((774, 318), (708, 242), 40, 32, 0, 7), pal["suit"], L, pal["rim"], shade=12)
    render_part(canvas, poly_mask([(764, 304), (786, 302), (788, 324), (768, 328)], 2), pal["accent"], L, None, shade=6,
                dots=False)
    render_part(canvas, limb_mask((708, 242), (766, 150), 32, 24, 0, 5), pal["suit"], L, pal["rim"], shade=10)
    render_part(canvas, limb_mask((752, 172), (760, 160), 30, 30), pal["accent"], L, None, shade=6, dots=False)  # Manschette
    fist(canvas, (768, 148), (g1[0] - g0[0], g1[1] - g0[1]), 18, pal, L)


# ----------------------------------------------------------------------------- Szene
def cover() -> Image.Image:
    canvas = Image.new("RGB", (W * K, H * K), (0, 0, 0))
    background(canvas)
    speed_lines(canvas)
    drone(canvas)
    man(canvas)
    contact = heroine_contact()
    lightning(canvas, (DRONE[0] - 26, DRONE[1] - 10), contact, MAN["blade"], seed=3)
    heroine(canvas)
    impact(canvas, contact, HEROINE["blade"])
    # abgelenkte Splitter
    rnd = random.Random(8)
    for k in range(3):
        ang = math.radians(-100 - k * 28 + rnd.uniform(-6, 6))
        far = (contact[0] + math.cos(ang) * 150, contact[1] + math.sin(ang) * 150)
        lightning(canvas, contact, far, MAN["blade"], seed=20 + k, amp=8, width=2)
    # Rahmen
    ImageDraw.Draw(canvas).rectangle((0, 0, W * K - 1, H * K - 1), outline=INK, width=6 * K)
    return canvas.resize((W, H), Image.Resampling.LANCZOS)


def heroine_contact():
    g1, tip = (392, 362), (528, 172)
    return lerp(g1, tip, 0.42)


def main(out: Path):
    out.mkdir(parents=True, exist_ok=True)
    img = cover()
    img.save(out / "cover-realistisch.png")
    game = quantize(img.resize((256, 224), Image.Resampling.LANCZOS))
    print(f"Spielfassung: {len(game.getcolors(1 << 24))} Farben")
    game.resize((1024, 896), Image.Resampling.NEAREST).save(out / "cover-realistisch-spiel.png")


if __name__ == "__main__":
    main(Path(sys.argv[1] if len(sys.argv) > 1 else "mockups/figuren"))
