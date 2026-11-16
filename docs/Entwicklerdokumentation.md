# Lazer Arena – Entwicklerdokumentation

Version 1.3 · Stand 2026-11-16 · English version: [developer-guide.md](developer-guide.md)

Diese Dokumentation beschreibt, wie Lazer Arena aufgebaut ist, wie man es baut und testet und wo man etwas ändert. Die Spielregeln selbst stehen im [REGELWERK.md](../REGELWERK.md). Jede Regel hat eine ID wie `SP-3` oder `OP-11`; Kommentare im Code und Test-Merkmale verweisen auf diese IDs.

## 1. Überblick

Lazer Arena ist ein Android-Spiel in C# mit MonoGame. Zwei Spieler (oder ein Spieler gegen den Computer) wehren mit Lichtschwertern Blitze ab, die eine Drohne in der Mitte der Arena abfeuert. Es ist ein eigenständiges Spiel, angelehnt an „Jedi Arena“ für das Atari VCS.

| Punkt | Wert |
|-------|------|
| Sprache / Laufzeit | C# auf .NET 10 |
| Spiel-Framework | MonoGame 3.8.6.1 (DesktopGL und Android) |
| Android | `net10.0-android`, mindestens Android 10 (API 29), Paket `de.lazerarena.app` |
| Tests | xUnit, 281 Tests (74 Spiellogik, 207 Präsentation) |
| Grafikstile | Klassisch (Atari-VCS-Look, kurze Seite 120 Pixel) und Modern (SNES-Look, kurze Seite 270 Pixel, 256 aus 4096 Farben) |
| Ausrichtung | Hoch- und Querformat |

## 2. Aufbau des Repositorys

```
LazerArena.slnx                 Solution
REGELWERK.md                    Spielregeln mit IDs
SPECS.md                        Ursprünglicher Projektauftrag
OFFENE-PUNKTE.md                Offene Punkte, Annahmen, Entscheidungen
src/
  LazerArena.Core/              Spiellogik, ohne Grafik, ohne MonoGame
    Ai/AiController.cs          Computergegner (3 Stufen)
  LazerArena.Presentation/      Alles zwischen Logik und Bildschirm, ohne MonoGame
    Rendering/                  Software-Renderer beider Stile, Arenen, Schrift, Paletten
    Audio/                      Synthetische Klänge und Musik, Sprachansagen
    Input/                      Berührung -> Spielbefehle
    Layout/                     Logisches Bildschirm-Layout je Stil und Ausrichtung
    Localization/               Strings.de.json, Strings.en.json
    Settings/                   UserSettings, AdvancedSettings, JSON-Speicher
    Art/                        Vorberechnete Pixelbilder (.lzpx)
    GameSession.cs              Eine Partie: Simulation plus Anzeige-Einstellungen
  LazerArena.Game/              Gemeinsamer MonoGame-Code (in Android und Desktop eingebunden)
    LazerArenaGame.cs           Spielschleife, Skalierung, TV-Effekt, Eingabe
    Screens.cs                  Titel, Optionen, Spiel, Siegerbildschirm
    SoundPlayer.cs              Spielt Klänge, Stimmen und Musik ab
    Platform.cs                 Plattform-Schnittstelle, Touch-Eingabe
  LazerArena.Android/           Android-App (Activity, Manifest, Symbole)
  LazerArena.Desktop/           Desktop-Variante zum schnellen Testen (Maus = Touch)
tests/
  LazerArena.Core.Tests/        Regeltests der Spiellogik
  LazerArena.Presentation.Tests/ Darstellung, Layout, Ton, Optionen, Menüs
tools/
  env.sh                        Umgebungsvariablen für den Build
  LazerArena.Snapshots/         Rendert Bildschirmfotos und Vorschaubilder als PNG
  voice/make_voice.py           Erzeugt die Sprachansagen (eSpeak NG)
  art/convert_art.py            Rechnet Illustrationen in Pixelbilder (.lzpx) um
  icon/make_icon.py             Erzeugt das adaptive Android-Symbol
  mockups/                      Skripte, mit denen die abgenommenen Mockups entstanden
mockups/, snapshots/            Abgenommene Entwürfe und aktuelle Bildschirmfotos
sounds/                         Hörproben von Stimmen und Klängen
release/                        Gebaute APKs (LazerArena-<Version>.apk)
```

## 3. Architektur

Der Code ist in Schichten geteilt. Jede Schicht kennt nur die darunterliegenden.

```
LazerArena.Android / LazerArena.Desktop     Start auf der Plattform, Dateipfade
        │
LazerArena.Game (gemeinsamer Quellcode)     MonoGame: Schleife, Texturen, Tonausgabe, Bildschirme
        │
LazerArena.Presentation                     Zeichnen in einen Pixelpuffer, Tonsynthese,
        │                                   Layout, Touch-Umsetzung, Texte, Einstellungen
LazerArena.Core                             Regeln, Simulation, KI (reines .NET)
```

Wichtige Entwurfsentscheidungen:

- **Spiellogik ohne Grafik.** `LazerArena.Core` kennt weder MonoGame noch den Renderer. Alle Regeln lassen sich mit einfachen Unit-Tests prüfen.
- **Fester Zeitschritt.** Die Simulation läuft mit 60 Hz (SO-3) und nutzt einen Zufallsgenerator mit Startwert (SO-4). Tests sind reproduzierbar.
- **Software-Rendering.** Beide Stile zeichnen in eine `PixelCanvas` (RGB-Feld in logischen Pixeln). MonoGame lädt nur das fertige Bild als Textur hoch und vergrößert es ganzzahlig. So bleibt der Pixel-Look exakt, und die Darstellung ist ohne Grafikkarte testbar.
- **Der Stil ändert nur das Aussehen** (SO-2). `ClassicRenderer` und `ModernRenderer` erben von `StyleRenderer`, der die gemeinsame Zeichenreihenfolge festlegt. Stil, Arena und Leucht-Einstellungen ändern nie die Simulation.
- **Eingebundener gemeinsamer Code.** `LazerArena.Game` ist kein eigenes Projekt. Android und Desktop binden die Dateien mit `<Compile Include="..\LazerArena.Game\**\*.cs" />` ein.

### 3.1 Spiellogik (LazerArena.Core)

| Klasse | Aufgabe | Regeln |
|--------|---------|--------|
| `ArenaSimulation` | Eine Partie: Phasen (Countdown, Spiel, Runde vorbei, Match vorbei), fester Zeitschritt, Ereignisse | RS-*, SO-3 |
| `Player` | Position, Schwertwinkel und Drehung (begrenzt auf 240°/s), Abklingzeit | SP-* |
| `Bolt` | Blitz: Ausbreitung, Seitenrand, Andocken, Treffer | BL-*, AD-* |
| `Drone` | Bewegung der Drohne | SF-2 |
| `Shield`, `ShieldSegment` | 2 × 16 Segmente, gerade oder gebogen, Nachwachsen | SF-3, SC-* |
| `LaserStorm` | Ankündigung, Gewitter, neutrale Blitze | LG-* |
| `GameConfig` | Alle Zahlenwerte (Geschwindigkeiten, Zeiten, Größen) | – |
| `GameOptions` | Vom Spieler gewählte Regel-Optionen | OP-4, OP-5, OP-6, OP-9 |
| `Ai/AiController` | Computergegner nach denselben Regeln wie ein Mensch | KI-* |

Die Simulation erzeugt `GameEvent`s (Treffer, Gewitterwarnung, Countdown usw.). Die Präsentation macht daraus Klänge und Ansagen.

### 3.2 Präsentation (LazerArena.Presentation)

- **`GameSession`** verbindet eine Simulation mit den Anzeige-Einstellungen (Blitzform, Leuchtwerte, KI-Stufe).
- **`StyleRenderer`** legt die Zeichenreihenfolge eines Spielbilds fest: Hintergrund, Gewitter-Schriftzug, Schilde, Figuren, Blitze, Drohne, Anzeigeleisten, Einblendungen (Countdown, „LOS!“, Pausenmenü). Er definiert auch die Menübildschirme: Titel, Optionen, Siegerbildschirm.
- **`ClassicRenderer`** zeichnet flache Farben im VCS-Look.
- **`ModernRenderer`** zeichnet den SNES-Look:
  - Er nutzt eine von sieben Arenen (`Arenas.cs`, Klasse `SnesArena`).
  - Jede Arena hat eine eigene Palette mit 256 Farben (`ArenaPalettes.g.cs`, erzeugt aus den abgenommenen Mockups; die Raumstation ergänzt 42 Rottöne für die Warnleuchten).
  - Die Arena „Original“ und die Menüs nutzen die feste `ModernPalette` (256 Farben, 12 Bit) mit geordnetem Dithering.
- **`PixelArt`** lädt die vorberechneten Bilder für Titel, Sieg und Niederlage (`Art/*.lzpx`). Sie werden nach der Farbreduktion gezeichnet und bleiben so frei von Dithering. Ihre Farben sind bereits Palettenfarben.
- **Anleitung** (`StyleRenderer.Tutorial.cs`, Regel SO-6): sechs Seiten, jede mit einer fest ablaufenden Demo-Szene (keine echte Simulation) aus den Zeichenbausteinen des Stils, einer Überschrift und umbrochenem Text. Zerstörte Schildblöcke einer Szene blendet der Renderer aus (`Visible`), weil der Schildzustand im Core intern ist.
- **Klassische Schildblöcke** (`ClassicRenderer.ShieldBlocks`): alle Blöcke 3:1 (hochkant 6 × 2, quer 3 × 1 Pixel) mit 1 Pixel Fuge und einer Reihenfuge von einer Blockhöhe. `ScreenLayout.BlockGridWidth` macht das klassische Feld 16·k − 1 Pixel breit, damit die Blöcke es gleichmäßig füllen.
- **`BitmapFont`** ist eine 5 × 7-Pixelschrift mit ganzzahliger und gebrochener Vergrößerung.
- **`TvEffect`** erzeugt Bildzeilen- und Vignetten-Texturen. `ScanlineRows` und `CoarseFactor` legen den Effekt im Stil Modern auf das klassische Raster (eine Linie je klassischer Pixelzeile, Leuchten 2,25-mal gröber), damit beide Stile gleich aussehen. Das Leuchten wird in `LazerArenaGame` angewendet. Die Werte kommen aus `AdvancedSettings` (OP-15).
- **`Menus.cs`** legt die Menüinhalte fest:
  - `OptionsMenu`, `AdvancedMenu` und `PauseMenu` führen die Einträge.
  - Ein `OptionEntry` besteht aus Textschlüssel, Funktion für den Wertschlüssel und Schrittfunktion. Menüs sind damit Daten und ohne Zeichnen testbar.
- **`ScreenLayout`** berechnet die logische Größe und alle Bereiche (Feld, Anzeigeleisten, Seitentafeln, Pause-Knopf) für Stil, Ausrichtung und Spielerzahl.
- **`TouchInputMapper`** setzt Berührungen in Befehle um:
  - Waagerechtes Wischen dreht das Schwert (180° je Bildschirmbreite).
  - Kurzes Tippen (unter 0,35 s, kaum Bewegung) feuert.
  - Bei zwei Spielern gehört die untere Hälfte Spieler 1, die obere Spieler 2.
- **`Localizer`** liest die JSON-Textdateien. Ein fehlender Schlüssel liefert den Schlüssel selbst. Das wird gezielt für feste Werte wie `48%` genutzt.
- **Ton:**
  - `ClassicSoundBank` ahmt einen VCS-Tonchip nach.
  - `ModernSoundBank` synthetisiert die Effekte, `MusicComposer` erzeugt Titel- und Spielmusik. Jeder `Song` nennt seine `Instrumentation`: Synthwave (Sägezahn-Flächen, Rechteck-Lead), Chiptune (Pulswellen, gestufter Dreieck-Bass, Rausch-Schlagzeug) oder ElectricPiano (FM-E-Piano, FM-Glocken-Lead, Sinus-Bass, weiches Schlagzeug).
  - `Voice.cs` ordnet Spielereignissen die Ansagen zu.
  - Bis auf die Sprachansagen entsteht alles zur Laufzeit.

### 3.3 MonoGame-Schicht (LazerArena.Game)

- **`LazerArenaGame`**:
  - Liest Touch und Tastatur und aktualisiert den aktuellen Bildschirm.
  - Lädt die `PixelCanvas` als Textur hoch und vergrößert sie ganzzahlig.
  - Ergänzt den TV-Effekt (Leucht-Rendertarget, Bildzeilen, Vignette), wenn er eingeschaltet ist.
- **Bildschirme (`IScreen`):**
  - `TitleScreen`: „Optionen“ 1 s halten öffnet die erweiterten Optionen.
  - `OptionsScreen`: normale oder erweiterte Optionen, mit dem Knopf „Standard“.
  - `TutorialScreen`: die animierte Anleitung (dritter Titel-Menüpunkt).
  - `PlayScreen`: das Spiel, einschließlich Pausenmenü.
  - `VictoryScreen`.
  - Ein Bildschirm liefert aus `Update` den nächsten Bildschirm.
- **`SoundPlayer`** spielt Effekte, Stimmen und Musik. „Ton aus“ schaltet alles stumm (OP-7). Die Stimme kann die Pieptöne ersetzen (OP-13).
- **`AppContext`** hält die aktuellen Einstellungen. Jede Änderung wird sofort über `JsonSettingsStore` gespeichert.

### 3.4 Einstellungen

`UserSettings` ist ein C#-Record mit allen Optionen. `SettingsProfiles` hält je Grafikstil einen vollständigen `UserSettings`-Satz und den aktiven Stil (OP-19); ein Stilwechsel aktiviert den anderen Satz. Die Datei ist JSON, Aufzählungswerte stehen als Text darin. Ältere Dateien mit nur einem Satz werden in beide Sätze übernommen.

| Plattform | Datei |
|-----------|-------|
| Android | privater App-Ordner, `settings.json` (LocalApplicationData) |
| Desktop (Linux) | `~/.config/LazerArena/settings.json` |
| Desktop (Windows) | `%APPDATA%\LazerArena\settings.json` |

Eine beschädigte oder unpassende Datei wird ignoriert. Das Spiel startet dann mit den Standardwerten.

Wenn du eine Einstellung ergänzt:

1. Eine `init`-Eigenschaft mit Standardwert in `UserSettings` anlegen.
2. Einen `OptionEntry` in `Menus.cs` anlegen.
3. Die Texte in beiden JSON-Dateien ergänzen (die Beschriftungen müssen in den klassischen Stil passen).
4. Einen Test schreiben und die Regel im REGELWERK.md ergänzen.

### 3.5 Android-Vollbild

`Activity1.HideSystemBars` blendet Status- und Navigationsleiste aus (immersiver Modus, Regel SO-7). Ab Android 11 über den `WindowInsetsController` mit „Leisten kurz beim Wischen zeigen“, unter Android 10 über die älteren `SystemUiFlags`. Es wird beim Fortsetzen und bei jedem Fokusgewinn des Fensters erneut ausgeführt.

Das Fenster darf auch in die Kamera-Aussparung zeichnen (`LayoutInDisplayCutoutMode.ShortEdges`). Nach jedem Layout liest `AndroidPlatform.UpdateCutout` die Höhe der Aussparung oben und ob sie mittig liegt (`Cutout.IsCentered`: alle Aussparungen oben in den mittleren 40 % der Breite). Eine mittige Aussparung wird überzeichnet; `AppContext.Layout` trägt ihre Höhe in logischen Pixeln in `ScreenLayout.SafeTop` ein, mittige Überschriften nutzen `TopBelowCutout`. Eine Aussparung außerhalb der Mitte bleibt frei: das Bild beginnt darunter (`Cutout.Reserved`). `tools/LazerArena.Snapshots -- <ordner> de --aussparung` rendert die betroffenen Bildschirme mit simulierter Aussparung auf fünf Bildschirmgrößen.

## 4. Entwicklungsumgebung einrichten

Alle Werkzeuge liegen im Benutzerverzeichnis. Root-Rechte sind nicht nötig.

| Werkzeug | Ort |
|----------|-----|
| .NET 10 SDK | `~/.dotnet` |
| Android SDK | `~/Android/Sdk` (Plattform android-36, Build-Tools 36) |
| JDK 17 | `~/Android/jdk/jdk-17*` |
| MonoGame | NuGet-Pakete, werden automatisch geladen |
| Python 3 mit Pillow | nur für die Werkzeuge in `tools/` |
| eSpeak NG (`libespeak-ng`) | nur zum Neuerzeugen der Ansagen |

Vor jedem Build:

```bash
source tools/env.sh
```

## 5. Bauen

### Desktop (schnelles Testen)

```bash
dotnet run --project src/LazerArena.Desktop
```

```bash
dotnet run --project src/LazerArena.Desktop -- --landscape
```

| Taste | Wirkung |
|-------|---------|
| Maus | Wie Touch |
| Links / Rechts, Leertaste oder Hoch | Spieler 1: Schwert drehen, feuern |
| A / D, W | Spieler 2: Schwert drehen, feuern |
| Esc / Rücktaste | Zurück / Pause |
| F12 | Bildschirmfoto speichern |

### Android-APK

Ein Release-Build dauert auf dem Entwicklungsrechner etwa 6 Minuten. `-m:1` und `UseSharedCompilation=false` halten den Speicherbedarf niedrig.

```bash
nice -n 10 dotnet build src/LazerArena.Android -c Release -m:1 -p:UseSharedCompilation=false -p:JavaSdkDirectory=$JAVA_HOME -p:AndroidSdkDirectory=$ANDROID_HOME
```

```bash
dotnet build-server shutdown
```

```bash
cp src/LazerArena.Android/bin/Release/net10.0-android/de.lazerarena.app-Signed.apk release/LazerArena-1.3.apk
```

Vor einer neuen Ausgabe:

- `ApplicationVersion` (ganze Zahl) und `ApplicationDisplayVersion` in `LazerArena.Android.csproj` erhöhen.
- Den Versionstext (`"V1.3"`) in `ClassicRenderer.RenderTitle` und `ModernRenderer.RenderTitle` ändern.

Die APK ist derzeit mit dem automatischen Debug-Schlüssel signiert (offener Punkt E8). Updates müssen mit demselben Schlüssel signiert sein, sonst muss die App vorher deinstalliert werden.

## 6. Tests

Die Testprojekte einzeln starten. `dotnet test` auf die ganze Solution würde auch die APK bauen.

```bash
dotnet test tests/LazerArena.Core.Tests
```

```bash
dotnet test tests/LazerArena.Presentation.Tests
```

- Tests tragen die Regel-ID als Merkmal, z. B. `[Fact, Trait("Regel", "RS-4")]`. Alle Tests einer Regel startet man mit `--filter "Regel=RS-4"`.
- Die Core-Tests prüfen jede Regel des REGELWERK.md mit festen Startwerten.
- Die Präsentationstests rendern echte Bilder in eine `PixelCanvas` und prüfen Pixel, zum Beispiel:
  - Farbgrenzen (höchstens 256 Farben)
  - gleich breite Klingen
  - Bilder ohne Dithering gezeichnet
  - Layout-Rechtecke innerhalb des Bildschirms
  - Texte, die in die Pixelschrift passen
- Einige Tests schreiben Einstellungen über den Desktop-Speicher. Wer eigene Einstellungen braucht, sichert vorher `~/.config/LazerArena/settings.json`.

## 7. Werkzeuge

| Werkzeug | Zweck | Aufruf |
|----------|-------|--------|
| Snapshots | Rendert alle Bildschirme beider Stile als PNG (doppelte Größe) | `dotnet run --project tools/LazerArena.Snapshots -- <ordner> de` |
| Snapshots, Arenen | Zusätzlich alle Arenen | `... -- <ordner> de --arenen` |
| Snapshots, Animation | Bildfolge einer 2-Spieler-Partie ab der Gewitterwarnung, für GIF-Vorschauen | `... -- <ordner> de --frames SpaceStation 440 2` |
| Snapshots, Anleitung | Bildfolgen aller Anleitungsseiten (für GIFs) und Standbilder; `--ohne-bildfolge` schreibt nur die Standbilder | `... -- <ordner> de --anleitung` |
| Snapshots, Ton | Schreibt alle Klänge und Musikstücke als WAV | `... -- <ordner> --audio` |
| Sprachansagen | Erzeugt die WAV-Dateien in `Audio/Voice` | `python3 tools/voice/make_voice.py --only=Warning --profile=soft` |
| Pixelbilder | Rechnet `tools/art/*-quelle.png` in `Art/*.lzpx` um, Vorschau in `mockups/kunst/` | `python3 tools/art/convert_art.py` |
| Benutzerhandbuch | Baut `docs/Benutzerhandbuch.pdf` und `docs/user-manual.pdf` aus der Benutzerdokumentation, mit Bildschirmfotos in der jeweiligen Sprache (braucht LibreOffice; Bildschirmfotos werden in `tools/docs/build/` zwischengespeichert) | `python3 tools/docs/make_manual.py` |
| Symbol | Baut das adaptive Android-Symbol aus `tools/icon/icon-quelle.jpg` | `python3 tools/icon/make_icon.py` |

Nach Änderungen am Aussehen `tools/docs/build/` löschen, damit das Handbuch neue Bildschirmfotos bekommt.

Wichtig: Für neue Ansagen immer `--only=` und `--profile=` verwenden. Ein Neuerzeugen aller Ansagen überschreibt abgenommene Aufnahmen.

### Bildformat LZPX

| Bytes | Inhalt |
|-------|--------|
| 0–3 | `LZPX` |
| 4–5 | Breite (uint16, Little Endian) |
| 6–7 | Höhe (uint16, Little Endian) |
| 8… | Je Pixel ein Index (uint16 LE) in `ModernPalette.Colors`; `0xFFFF` = durchsichtig |

Je Ausrichtung gibt es eine eigene Datei (`-hoch`, `-quer`), damit im Spiel nichts skaliert wird.

## 8. Typische Änderungen

| Aufgabe | Wo |
|---------|----|
| Regelwert ändern (Geschwindigkeit, Zeiten) | `GameConfig` im Core, danach Tests und REGELWERK.md anpassen |
| Neuer Text | Beide `Strings.*.json`; nur Zeichen, die `BitmapFont` kennt (ein Test prüft das) |
| Neue Option | `UserSettings`, `Menus.cs`, Texte, Test, REGELWERK.md |
| Neue Arena | Von `SnesArena` abgeleitete Klasse in `Arenas.cs`, Palette in `ArenaPalettes.g.cs`, Wert in `ArenaKind`, Text `arena.<Name>` |
| Neuer Klang | `SoundId`, beide Klangbänke, Zuordnung in `SoundEvents` / `AudioCue.From` |
| Neues Bild | Quell-PNG in `tools/art`, Eintrag in `TARGETS` von `convert_art.py`, Wert in `ArtId` |

## 9. Bekannte Einschränkungen

- Die Ansagen stammen von eSpeak NG (Formantsynthese). Für eine natürliche Stimme bräuchte es eine Sprecherin oder eine lizenzierte neuronale Stimme (offener Punkt F23).
- Der Release-Signierschlüssel fehlt noch (E8).
- Das Symbol sowie die Bilder für Titel, Sieg und Niederlage stammen aus KI-Illustrationen des Auftraggebers, der die Rechte bestätigt hat (E10, E12).
- Die aktuelle Liste offener Punkte steht in [OFFENE-PUNKTE.md](../OFFENE-PUNKTE.md).
