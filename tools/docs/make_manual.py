"""Erzeugt das druckbare Benutzerhandbuch als PDF (Deutsch und Englisch) aus der Benutzerdokumentation.

Quelle: docs/Benutzerdokumentation.md bzw. docs/user-guide.md. Dazu kommen Bildschirmfotos in der jeweiligen
Sprache (LazerArena.Snapshots) und ein Titelblatt mit dem Titelbild. Aus Markdown wird druckfertiges HTML,
das LibreOffice (ohne Oberflaeche) in PDF umwandelt.

Aufruf (nach `source tools/env.sh`):  python3 tools/docs/make_manual.py
Ausgabe: docs/Benutzerhandbuch.pdf, docs/user-manual.pdf
Benoetigt: dotnet (fuer die Bildschirmfotos), libreoffice, Python 3 mit Pillow.
"""
import html
import re
import shutil
import subprocess
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
BUILD = ROOT / "tools/docs/build"

LANGS = {
    "de": {
        "source": "docs/Benutzerdokumentation.md",
        "output": "docs/Benutzerhandbuch.pdf",
        "title": "Benutzerhandbuch",
        "subtitle": "Duell im Orbit · Version 1.3",
        "toc": "Inhalt",
        "caption": {
            "titel": "Titelbildschirm im Stil Modern (links) und Klassisch (rechts)",
            "spiel": "Spiel im Hochformat (Modern, links) und Querformat (Klassisch, rechts)",
            "gewitter": "Blitzgewitter in der Arena Raumstation (2 Spieler)",
            "ende": "Sieg und Niederlage im Stil Modern",
            "optionen": "Optionen (links) und Pausenmenü im Querformat (rechts)",
            "erweitert": "Erweiterte Optionen mit dem Knopf „Standard“ nach dem ersten Antippen",
            "anleitung": "Anleitung: Seite „Feuern“ im Stil Modern (links) und Klassisch (rechts)",
        },
    },
    "en": {
        "source": "docs/user-guide.md",
        "output": "docs/user-manual.pdf",
        "title": "User Manual",
        "subtitle": "Duel in orbit · Version 1.3",
        "toc": "Contents",
        "caption": {
            "titel": "Title screen in the Modern style (left) and the Classic style (right)",
            "spiel": "Game in portrait (Modern, left) and landscape (Classic, right)",
            "gewitter": "Lightning storm in the Space Station arena (2 players)",
            "ende": "Victory and defeat in the Modern style",
            "optionen": "Options (left) and the pause menu in landscape (right)",
            "erweitert": "Advanced options with the Defaults button after the first tap",
            "anleitung": "Tutorial: the \"Fire\" page in the Modern style (left) and the Classic style (right)",
        },
    },
}

# Bilder je Abschnitt (Nummer der Ueberschrift): (Beschriftungs-Schluessel, [Bildschirmfotos], Breite je Bild in cm)
FIGURES = {
    "2": ("titel", ["modern-titel-hoch", "klassisch-titel-hoch"], 5.2),
    "4": ("spiel", ["modern-1p-hoch", "klassisch-1p-quer"], None),
    "5": ("gewitter", ["arena-SpaceStation-2p-hoch-gewitter"], 5.2),
    "5b": ("ende", ["modern-sieg-hoch", "modern-niederlage-hoch"], 5.2),
    "2b": ("anleitung", ["anleitung-modern-hoch", "anleitung-klassisch-hoch"], 5.2),
    "6": ("optionen", ["modern-optionen-hoch", "modern-1p-quer-pause"], None),
    "7": ("erweitert", ["modern-erweitert-hoch"], 5.2),
}

FONT = "font-family: 'DejaVu Sans', sans-serif;"
BODY = FONT + " font-size: 10pt;"
CELL = FONT + " font-size: 9pt;"
H2 = FONT + " font-size: 15pt; color: #3a1c71; border-bottom: 1px solid #b8a9e0;"

CSS = """
@page { size: A4; margin: 2cm 2cm 2cm 2cm; }
body { font-family: 'DejaVu Sans', sans-serif; font-size: 10pt; color: #1a1a2e; }
h1 { font-size: 20pt; color: #3a1c71; margin-top: 0; }
h2 { font-size: 15pt; color: #3a1c71; border-bottom: 1px solid #b8a9e0; margin-top: 18pt; }
p, li { line-height: 140%; }
table.data { border-collapse: collapse; width: 100%; margin: 6pt 0 10pt 0; }
table.data th { background: #ece6fb; text-align: left; padding: 3pt 5pt; border: 1px solid #b8a9e0; font-size: 9pt; }
table.data td { padding: 3pt 5pt; border: 1px solid #d4cbef; vertical-align: top; font-size: 9pt; }
.caption { font-size: 8.5pt; color: #555; text-align: center; font-style: italic; }
code { font-family: 'DejaVu Sans Mono', monospace; font-size: 9pt; }
"""


def snapshots(lang: str) -> Path:
    """Bildschirmfotos in der Sprache des Handbuchs (werden bei Bedarf erzeugt)."""
    folder = BUILD / f"snapshots-{lang}"
    if not (folder / "modern-titel-hoch.png").exists():
        subprocess.run(["dotnet", "run", "--project", str(ROOT / "tools/LazerArena.Snapshots"), "--", str(folder), lang, "--arenen"],
                       check=True, cwd=ROOT)
    return folder


def inline(text: str) -> str:
    text = html.escape(text, quote=False)
    text = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", text)            # Links: nur der Text (gedrucktes Handbuch)
    text = re.sub(r"\*\*(.+?)\*\*", r"<b>\1</b>", text)
    text = re.sub(r"\*(.+?)\*", r"<i>\1</i>", text)
    text = re.sub(r"`([^`]+)`", r"<code>\1</code>", text)
    return text


def figure(key: str, names: list[str], width_cm: float | None, shots: Path, captions: dict) -> str:
    cells = []
    for name in names:
        path = shots / f"{name}.png"
        w, h = Image.open(path).size
        cm = width_cm if width_cm else (5.2 if h > w else 9.5)        # Hochformat schmal, Querformat breit
        height = cm * h / w
        cells.append(f'<td align="center" valign="bottom"><img src="{path.as_uri()}" '
                     f'style="width:{cm}cm; height:{height:.2f}cm"></td>')
    return (f'<table align="center" cellpadding="6"><tr>{"".join(cells)}</tr></table>'
            f'<p align="center" style="{FONT} font-size: 8.5pt; color: #555"><i>{html.escape(captions[key])}</i></p>')


def convert(markdown: str, shots: Path, captions: dict) -> str:
    out: list[str] = []
    lines = markdown.splitlines()
    i = 0
    section = None
    pending_end = None    # Bild am Ende eines Abschnitts („2b“: Anleitung, „5b“: Sieg/Niederlage)

    def flush_section_end():
        nonlocal pending_end
        if pending_end:
            out.append(figure(*FIGURES[pending_end], shots, captions))
            pending_end = None

    while i < len(lines):
        line = lines[i]
        if line.startswith("# "):
            i += 1
            continue                                                   # Titel steht auf dem Titelblatt
        if line.startswith("## "):
            flush_section_end()
            heading = line[3:]
            section = heading.split(".")[0]
            brk = "page-break-before: always; " if section == "6" else ""
            out.append(f'<h2 style="{brk}{H2}">{inline(heading)}</h2>')
            if section in FIGURES:
                out.append(figure(*FIGURES[section], shots, captions))
            pending_end = section + "b" if section + "b" in FIGURES else None
            i += 1
            continue
        if line.startswith("|"):
            rows = []
            while i < len(lines) and lines[i].startswith("|"):
                cells = [c.strip() for c in lines[i].strip().strip("|").split("|")]
                if not all(re.fullmatch(r"-+", c) for c in cells):
                    rows.append(cells)
                i += 1
            head = "".join(f'<th bgcolor="#ece6fb" align="left" style="{CELL}">{inline(c)}</th>' for c in rows[0])
            body = "".join("<tr>" + "".join(f'<td valign="top" style="{CELL}">{inline(c)}</td>' for c in r) + "</tr>" for r in rows[1:])
            out.append(f'<table width="100%" border="1" bordercolor="#b8a9e0" cellspacing="0" cellpadding="4">'
                       f"<thead><tr>{head}</tr></thead><tbody>{body}</tbody></table>")
            continue
        if re.match(r"^(\d+\.|-) ", line):
            ordered = line[0].isdigit()
            items = []
            while i < len(lines) and re.match(r"^(\d+\.|-) ", lines[i]):
                items.append(re.sub(r"^(\d+\.|-) ", "", lines[i]))
                i += 1
            tag = "ol" if ordered else "ul"
            out.append(f"<{tag}>" + "".join(f'<li style="{BODY}">{inline(t)}</li>' for t in items) + f"</{tag}>")
            continue
        if line.strip():
            para = [line]
            i += 1
            while i < len(lines) and lines[i].strip() and not re.match(r"^(#|\||\d+\. |- )", lines[i]):
                para.append(lines[i])
                i += 1
            text = " ".join(para)
            if text.startswith("Version 1.3 ·"):
                continue                                               # Verweis auf die andere Sprache entfaellt
            first = not any(o.startswith("<p") for o in out)
            m = re.fullmatch(r"\*\*(.+?):\*\*", text.strip())
            if m:   # Zwischentitel wie „**Blitzgewitter:**“: als Ueberschrift, die LibreOffice beim folgenden Absatz haelt
                out.append(f'<h3 style="{FONT} font-size: 10.5pt; color: #3a1c71; margin-bottom: 2pt">{inline(m.group(1))}</h3>')
                continue
            out.append(f'<p style="{"page-break-before: always; " if first else ""}{BODY}">{inline(text)}</p>')
            continue
        i += 1
    flush_section_end()
    return "\n".join(out)


def toc(markdown: str, title: str) -> str:
    items = [inline(l[3:]) for l in markdown.splitlines() if l.startswith("## ")]
    return (f'<h2 style="page-break-before: always; {H2}">{title}</h2>'
            f'<p style="{BODY} font-size: 12pt; line-height: 180%">' + "<br>".join(items) + "</p>")


def build(lang: str, cfg: dict):
    shots = snapshots(lang)
    markdown = (ROOT / cfg["source"]).read_text(encoding="utf-8")
    cover_art = ROOT / "mockups/kunst/title-hoch.png"
    w, h = Image.open(cover_art).size
    cover = (f'<p style="margin-top:2cm; {FONT}" align="center"><span style="font-size:34pt; color:#e8552c"><b>LAZER ARENA</b></span><br>'
             f'<span style="font-size:18pt; color:#3a1c71">{cfg["title"]}</span><br>'
             f'<span style="font-size:11pt; color:#555">{cfg["subtitle"]}</span></p>'
             f'<p align="center" style="margin-top:1.5cm; {FONT}"><img src="{cover_art.as_uri()}" style="width:9cm; height:{9 * h / w:.2f}cm"></p>')
    page = (f'<!DOCTYPE html><html lang="{lang}"><head><meta charset="utf-8"><title>Lazer Arena – {cfg["title"]}</title>'
            f"<style>{CSS}</style></head><body>{cover}"
            f'{toc(markdown, cfg["toc"])}{convert(markdown, shots, cfg["caption"])}</body></html>')
    source = BUILD / f"handbuch-{lang}.html"
    source.write_text(page, encoding="utf-8")
    subprocess.run(["libreoffice", "--headless", "--convert-to", "pdf:writer_web_pdf_Export", "--outdir", str(BUILD), str(source)],
                   check=True, cwd=BUILD, stdout=subprocess.DEVNULL)
    target = ROOT / cfg["output"]
    shutil.move(BUILD / f"handbuch-{lang}.pdf", target)
    print(f"{target.relative_to(ROOT)}")


def main():
    BUILD.mkdir(parents=True, exist_ok=True)
    for lang in (sys.argv[1:] or LANGS):
        build(lang, LANGS[lang])


if __name__ == "__main__":
    main()
