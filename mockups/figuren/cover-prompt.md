# Bildprompt: Titelillustration „Lazer Arena“ (realistischer Comic-Cover-Stil)

Für einen externen KI-Bildgenerator (z. B. Midjourney, DALL·E, Stable Diffusion, Firefly) oder als Briefing für einen Zeichner.
Nur Illustration, **keine Schrift** im Bild. Das Spiel blendet das Logo selbst ein.

Vor der Nutzung im Spiel bitte die Lizenzbedingungen des Generators prüfen. Kommerzielle Nutzung und Weitergabe des APK müssen erlaubt sein.

## Hauptprompt (Englisch, die meisten Generatoren verstehen Englisch am besten)

```
Dramatic realistic American comic book cover illustration, no text, no logo, no lettering.
A sci-fi arena duel at dusk. On the left foreground: a young athletic woman with long flowing
golden-blonde hair streaming behind her, determined expression, seen in near-profile facing right,
wearing a fitted white and light-blue sci-fi bodysuit with blue accent seams, dark navy leggings,
gloves and armored boots. She stands in a deep lunge and holds a glowing cyan energy sword with
both hands, angled upward to block and deflect a jagged magenta lightning bolt; a bright white
impact flash with sparks bursts where the bolt hits her blade.
In the center, between the two fighters and slightly behind them, floats a small spherical metal
training drone (chrome sphere, horizontal ring with orange lights, one glowing red eye) that fires
the lightning bolt.
On the right, a few steps further back: a fierce, muscular man with short swept-back brown hair,
stubble and a furrowed brow, gritted teeth, in a dark red tunic with gold trim and charcoal trousers,
both arms raised high, gripping a glowing magenta energy sword overhead, poised to strike.
Background: radial light burst behind the drone, purple-to-orange synthwave sky with stars,
reflective dark arena floor with a glowing perspective grid fading to the horizon, speed lines.
Style: bold black ink outlines with varied line weight, realistic anatomy and proportions,
cel-shaded rendering with dramatic rim lighting from the glowing blades, subtle halftone dots in
the shadows, rich saturated colors, 1980s/1990s comic cover art meets synthwave, dynamic
composition, cinematic lighting, highly detailed faces and hands.
Aspect ratio 8:7 (landscape), both figures fully visible head to toe.
```

## Negativprompt (falls der Generator einen hat)

```
text, letters, logo, watermark, signature, speech bubbles, panel borders, extra fingers,
deformed hands, extra limbs, blurry, photo, 3D render, anime chibi, cropped feet, cropped heads
```

## Parameter-Hinweise

- **Midjourney:** `--ar 8:7 --style raw --v 7`. Danach ggf. „Vary (Subtle)“ für Hände/Gesichter.
- **Stable Diffusion/SDXL:** 1024 × 896, CFG 6–7, 30–40 Schritte; ggf. ControlNet „OpenPose“ mit der Posen-Vorlage `cover-realistisch.png`, damit die Haltungen stimmen.
- **DALL·E / ChatGPT:** Format „Querformat“ wählen; den Satz „no text in the image“ wiederholen.

## Vorgaben für den Einbau ins Spiel

- Liefergröße mindestens 1024 × 896 px (PNG). Ich rechne das Bild auf 256 × 224 Pixel und höchstens 256 Farben aus 4096 um (Spielpalette).
- Wichtige Bildteile (Gesichter, Klingen, Drohne) nicht in die äußeren 5 % legen. Im Querformat überlagert links oben das Logo.
- Farben der Figuren beibehalten (Heldin weiß/blau mit Cyan-Klinge, Gegner rot/gold mit Magenta-Klinge). Sie passen zu den Spielerfarben im Spiel.
