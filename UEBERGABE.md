# Übergabe – Lazer Arena

Stand: 2026-11-16 · Version 1.3 (Versionscode 13)

Dieses Dokument enthält alles, um das Projekt mit einem anderen Account oder in einer neuen Sitzung ohne Vorwissen weiterzuführen. Es ergänzt die übrigen Dokumente und verweist auf sie, statt sie zu wiederholen.

| Dokument | Inhalt |
|----------|--------|
| [UEBERGABE.md](UEBERGABE.md) | Diese Übergabe: Stand, Absprachen, Arbeitsweise, Fallstricke |
| [REGELWERK.md](REGELWERK.md) | Spielregeln mit IDs (z. B. SP-3, OP-11); Tests verweisen darauf. Version 2.3 |
| [OFFENE-PUNKTE.md](OFFENE-PUNKTE.md) | Offene Punkte, Annahmen, alle Entscheidungen mit Datum |
| [SPECS.md](SPECS.md) | Ursprünglicher Auftrag (Abnahmekriterien) |
| [docs/Entwicklerdokumentation.md](docs/Entwicklerdokumentation.md) | Architektur, Bauen, Testen, Werkzeuge (auch Englisch: developer-guide.md) |
| [docs/Benutzerdokumentation.md](docs/Benutzerdokumentation.md) | Bedienung für Spieler (auch Englisch: user-guide.md) |
| docs/Benutzerhandbuch.pdf, docs/user-manual.pdf | Druckbares Handbuch, erzeugt aus der Benutzerdokumentation |
| [README.md](README.md) | Kurzüberblick (Englisch mit deutschem Abschnitt) |
| `/home/andy/Claude/CLAUDE.md` | Arbeitsanweisungen des Anwenders für Claude (gelten weiter) |

## 1. Schnellstart für eine neue Sitzung

1. Arbeitsordner: `/home/andy/Claude/JediArena` (Claude-Code-Sitzung im Ordner `/home/andy/Claude` starten, damit `CLAUDE.md` geladen wird).
2. Als erste Nachricht zum Beispiel:

   > Lies bitte UEBERGABE.md, OFFENE-PUNKTE.md und REGELWERK.md im Ordner JediArena. Danach arbeiten wir an Lazer Arena weiter.

3. Umgebung laden und prüfen, ob alles läuft (siehe Abschnitt 4):

```bash
cd /home/andy/Claude/JediArena && source tools/env.sh
```

```bash
dotnet test tests/LazerArena.Core.Tests
```

```bash
dotnet test tests/LazerArena.Presentation.Tests
```

Erwartet: 74 + 207 = 281 Tests grün.

## 2. Projekt in Kürze

- **Was:** Android-Spiel „Lazer Arena“ (Duell im Orbit), eigenständig, angelehnt an „Jedi Arena“ (Atari VCS). Kein Remake: eigene Optik, eigene Figuren, keine Star-Wars-Elemente (R1, E6).
- **Spiel:** Zwei Kämpfer (oder Spieler gegen KI) drehen Lichtschwerter; die Drohne in der Mitte schießt Blitze in Schwertrichtung auf den Gegner. Blitze docken an der Schwertspitze an, zerstören Schildblöcke; Blitzgewitter; wer zuerst 3 Treffer hat, gewinnt.
- **Technik:** C# / .NET 10, MonoGame 3.8.6.1, Android ab 10 (API 29), Paket `de.lazerarena.app`. Spiellogik ohne Grafik in `LazerArena.Core`, Software-Rendering in `LazerArena.Presentation`, MonoGame nur zur Ausgabe. Details: Entwicklerdokumentation.
- **Zwei Grafikstile:** Klassisch (Atari-Look, kurze Seite 120 logische Pixel) und Modern (SNES-Look, kurze Seite 270 Pixel, 256 aus 4096 Farben, 7 Arenen). Hoch- und Querformat.
- **Vertrieb:** nur APK zum Selbstinstallieren, nicht im Play Store.

## 3. Aktueller Stand (Version 1.3)

- APK: `release/LazerArena-1.3.apk` (alle früheren Versionen liegen daneben).
- Alle 281 Tests grün.
- Dokumentation (Regelwerk, Benutzer-/Entwicklerdokumentation, README, PDFs) auf Stand 1.3.

Offen (Details in OFFENE-PUNKTE.md):

| ID | Punkt |
|----|-------|
| E14 | Kamera-Aussparung (V1.3): Spiel zeichnet bis an den oberen Rand, Überschriften rücken unter das Kameraloch. Auf dem Handy des Anwenders noch zu prüfen. |
| E2 | Kein Emulator, kein ADB: Der Anwender installiert jede APK selbst auf dem Handy und meldet Ergebnisse (oft mit Bildschirmfoto). |
| – | Laut SPECS.md fehlt noch die **abschließende Abnahme mit Protokoll**. Danach: Dokumentation ist bereits erstellt (Entwickler-, Anwenderdokumentation, Benutzerhandbuch). |

Testgerät des Anwenders: Motorola edge 50 pro, Android 15, 1220 × 2712 Pixel, Kameraloch oben mittig.

### Versionsverlauf (Auszug)

| Version | Wichtigste Änderungen |
|---------|-----------------------|
| 0.1–0.7 | Grundspiel, beide Stile, Arenen, Ton, Musik, Stimme, App-Symbol |
| 0.8 | Schwertdrehung immer begrenzt, Blitzform je Stil wählbar, „GO!“, sanfte Stimme, Original-Drohne in allen Arenen |
| 0.9 | „Warnung!“-Ansage, pulsierender Gewitterring, versteckte erweiterte Optionen (Optionen-Knopf 1 s halten) |
| 1.0 | Pixelbilder für Titel/Sieg/Niederlage, Sternenflug in Menüs, Musik/Stimme im Pausenmenü, „Standard“-Knopf, Rundumleuchten in der Raumstation |
| 1.1 | Optionen je Stil getrennt, TV-Effekt in Modern wie in Klassisch, animierte Anleitung, Optionsliste ohne Kästen, Schildblöcke 3:1, neue Klänge |
| 1.2 | Navigationsleiste ausgeblendet (immersiv), neue Hologramm-Animation, Spielmusiken „Pixel Pulse“ und „Starlight“ |
| 1.3 | Bild reicht bis in die Kamera-Aussparung, Überschriften darunter |

## 4. Entwicklungsumgebung

Alles liegt im Benutzerverzeichnis, ohne root-Rechte. Linux Mint in VirtualBox, etwa 5 GB RAM.

| Werkzeug | Ort / Hinweis |
|----------|---------------|
| .NET SDK 10 | `~/.dotnet` |
| Android SDK | `~/Android/Sdk` (Plattform android-36, Build-Tools 36) |
| JDK 17 | `~/Android/jdk/jdk-17*` |
| Umgebungsvariablen | `source tools/env.sh` vor jedem Build |
| Python 3 mit Pillow | System-Python; **kein pip** vorhanden |
| eSpeak NG | Systembibliothek `libespeak-ng.so.1` (nur für Sprachansagen) |
| LibreOffice | `/usr/bin/libreoffice` (nur für die Handbuch-PDFs) |
| Debug-Signierschlüssel | `~/.local/share/Xamarin/Mono for Android/debug.keystore`, gültig bis 2056 |

## 5. Wichtige Befehle

Release-APK bauen (dauert ca. 6 Minuten, am besten im Hintergrund; danach den Build-Server beenden, sonst bleibt viel Speicher belegt):

```bash
nice -n 10 dotnet build src/LazerArena.Android -c Release -m:1 -p:UseSharedCompilation=false -p:JavaSdkDirectory=$JAVA_HOME -p:AndroidSdkDirectory=$ANDROID_HOME
```

```bash
dotnet build-server shutdown
```

```bash
cp src/LazerArena.Android/bin/Release/net10.0-android/de.lazerarena.app-Signed.apk release/LazerArena-1.3.apk
```

Desktop-Variante zum schnellen Ausprobieren (Maus = Touch, F12 = Bildschirmfoto):

```bash
dotnet run --project src/LazerArena.Desktop
```

Bildschirmfotos aller Bildschirme (zum Prüfen von Gestaltung):

```bash
dotnet run --project tools/LazerArena.Snapshots -- snapshots de --arenen --anleitung --ohne-bildfolge
```

Weitere Schalter des Snapshot-Werkzeugs: `--frames <Arena> <Anzahl> [jedes-n-te]` (Bildfolge für GIF-Vorschauen), `--anleitung` (Bildfolgen der Anleitung), `--aussparung` (simulierte Kameralöcher auf fünf Bildschirmgrößen), `--audio` (alle Klänge und Musikstücke als WAV).

Handbuch-PDFs neu erzeugen:

```bash
python3 tools/docs/make_manual.py
```

## 6. Arbeitsweise und Absprachen mit dem Anwender

Diese Punkte haben sich im Projekt bewährt und sind ausdrücklich gewünscht:

- **Sprache:** Der Anwender schreibt meist Englisch, manchmal Deutsch; Projektdateien (Regelwerk, Offene Punkte) sind Deutsch.
- **Nachfragen statt annehmen:** Bei unklaren Wünschen nachfragen (CLAUDE.md). Annahmen als „A…“ in OFFENE-PUNKTE.md eintragen und im Chat bestätigen lassen.
- **OFFENE-PUNKTE.md sofort pflegen:** IDs (E = Hinweis/Einschränkung, F = Frage, A = Annahme, T = Technik, D = Regelentscheidung, R = Rückmeldung), Status, Stand-Datum. Erledigtes mit Datum in „Erledigt“ verschieben. Nächste freie IDs: E15, F32, A20, T5.
- **Erst Vorschläge, dann umsetzen:** Bei Gestaltungs- oder Klangwünschen mehrere Entwürfe als Bildschirmfotos, GIFs oder WAV-Dateien zeigen und auswählen lassen. Der Anwender will Klänge vor dem APK-Bau selbst anhören.
- **Jede Ausgabe ist eine neue Version:** `ApplicationVersion` (+1) und `ApplicationDisplayVersion` in `src/LazerArena.Android/LazerArena.Android.csproj`, Versionstext `"V1.x"` in `ClassicRenderer.RenderTitle` und `ModernRenderer.RenderTitle`, APK als `release/LazerArena-<Version>.apk`.
- **Dokumentation mitziehen:** Bei jeder Version Regelwerk (neue Regeln mit ID), Benutzer- und Entwicklerdokumentation (DE und EN), README, Testanzahl, PDFs und diese UEBERGABE.md aktualisieren.
- **Tests:** Neue Funktionen bekommen Tests mit `Trait("Regel", "<ID>")`. Tests nur einzeln je Projekt starten (die ganze Solution würde die APK bauen).
- **Keine Passwörter eingeben**, keine Zugangsdaten verwenden. Der Anwender installiert und testet selbst.
- **Git:** Das Projekt ist ein Git-Repository (Zweig `main`, entfernt: `origin` = https://github.com/AndyTheGuardian/JediArena.git). Nur auf ausdrücklichen Wunsch committen; pushen nur nach Rückfrage. Neue Arbeit auf einem eigenen Zweig. Build-Ausgaben (`bin/`, `obj/`) und der Handbuch-Zwischenspeicher stehen in `.gitignore`.
- **Einstellungsdatei:** Die Desktop-Variante speichert in `~/.config/LazerArena/settings.json` (derzeit nicht vorhanden). Falls vorhanden, vor Tests und Probeläufen sichern und danach zurückkopieren, damit eigene Einstellungen erhalten bleiben.
- **Bildrechte:** App-Symbol und Bilder für Titel/Sieg/Niederlage stammen aus KI-Illustrationen des Anwenders; er hat die Nutzungsrechte bestätigt (E10, E12).

## 7. Wichtige Entwurfsentscheidungen

Ausführlich im Regelwerk und in „Erledigt“ von OFFENE-PUNKTE.md. Die wichtigsten auf einen Blick:

- **Spiellogik:** fester Zeitschritt 60 Hz, Zufall mit Startwert, Stil ändert nie die Regeln (SO-2, SO-3, SO-4).
- **Blitze:** zucken von der Drohne in Schwertrichtung, docken an der Schwertspitze an (Haltezeit 1–2 s), dann „verpufft“ oder „zurückwerfen“ (AD-*).
- **Farben Modern:** jede Arena hat eine feste Palette mit höchstens 256 Farben aus 4096 (12 Bit); SNES-Look ohne Dithering. Raumstation und Hologramm haben zusätzliche Farbstufen (Rot für Warnleuchten, Grün für das Gitter).
- **Optionen:** sofort gespeichert, je Grafikstil ein eigener vollständiger Satz (OP-19). „Standard“-Knopf mit Rückfrage (OP-16). Versteckte erweiterte Optionen: Optionen-Knopf 1 s halten (OP-15).
- **Klassische Schildblöcke:** 3:1 (6 × 2 Pixel hochkant, 3 × 1 quer), 1 Pixel Fuge, Reihenfuge eine Blockhöhe; Feldbreite 16·k − 1 Pixel.
- **Optionsbildschirm Modern:** Liste ohne Kästen (Entwurf B), Werte in doppelter Schrift.
- **Klänge:** alles zur Laufzeit synthetisiert, außer den Sprachansagen (eSpeak NG, eingebettete WAVs, Stimmen „Computer“ und „Sanft“).
- **Android:** immersives Vollbild; Kamera-Aussparung wird bei mittiger Lage überzeichnet, sonst bleibt sie frei (SO-7).

## 8. Fallstricke

- **Sprachansagen nie komplett neu erzeugen:** Das überschreibt abgenommene Aufnahmen. Immer mit `--only=<Zeile>` und `--profile=soft|computer` arbeiten.
- **Handbuch-PDFs:** `make_manual.py` nutzt zwischengespeicherte Bildschirmfotos in `tools/docs/build/`. Nach Änderungen am Aussehen den Ordner löschen.
- **Zeichensatz:** Die Pixelschrift kennt nur A–Z, ÄÖÜ, Ziffern und `: ! ? - % . = > < * ,` sowie Leerzeichen; kein ß (stattdessen „SS“). Ein Test prüft alle Texte.
- **Beschriftungen:** müssen in den klassischen Stil passen (120 Pixel breit, höchstens 18 Zeichen bei kleiner Schrift).
- **Lange Musikberechnung:** Jedes Musikstück wird beim ersten Gebrauch im Hintergrund berechnet (auf dem PC 1–9 s).
- **Warnung CA1422** beim Android-Build (`SetDecorFitsSystemWindows` ist ab Android 15 veraltet): harmlos, der neuere Aufruf wirkt.
- **Updates auf dem Handy** funktionieren nur mit demselben Signierschlüssel. Die Datei `debug.keystore` sichern; nach einer Neuinstallation des Rechners vor dem nächsten Build zurückkopieren.
- **Speicher:** Android-Build immer mit `-m:1` und `UseSharedCompilation=false`; nicht gleichzeitig mit anderen großen `dotnet`-Läufen.
- **Löschbefehle** mit Variablen (`rm -rf $X/*`) werden vom Sicherheitscheck blockiert; feste Pfade verwenden.

## 9. Was gesichert werden sollte

| Was | Wo |
|-----|----|
| Projektordner (Quellcode, Dokumente, Werkzeuge, Bildquellen, APKs) | `/home/andy/Claude/JediArena` (Git; nach einem Push auch auf GitHub) |
| Signierschlüssel | `~/.local/share/Xamarin/Mono for Android/debug.keystore` |
| Arbeitsanweisungen | `/home/andy/Claude/CLAUDE.md` |
| Bildquellen (bereits im Projekt) | `tools/art/*-quelle.png`, `tools/icon/icon-quelle.jpg` |
| Verlauf der bisherigen Sitzung (optional, zum Nachlesen) | `~/.claude/projects/-home-andy-Claude/dcff130d-bf16-4f02-9c4e-d6fc4fbd5753.jsonl` |

## 10. Mögliche nächste Schritte

1. E14 auf dem Handy prüfen (Bild bis oben, Überschriften unter dem Kameraloch).
2. Abschließende Abnahme durchführen und ein Abnahmeprotokoll erstellen (SPECS.md).
3. Commits nach GitHub pushen (`git push`), damit eine Kopie außerhalb dieses Rechners liegt. Der Signierschlüssel gehört nicht ins Repository.
