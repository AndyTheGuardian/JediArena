# Offene Punkte – Projekt „Lazer Arena“

Stand: 2026-11-14

## Offen

| ID | Typ | Beschreibung | Status |
|----|-----|--------------|--------|
| E6 | Hinweis | Abgrenzung zum Original: Mechanik (Andocken, Schwertwinkel = Schusswinkel, wilde Phase) ist angelehnt, Optik bleibt eigenständig (kein ovales Arena-Feld, eigene Figuren, eigene Schildform, keine Star-Wars-Elemente). | zur Kenntnis |
| E2 | Einschränkung | Kein Emulator möglich (Linux läuft in VirtualBox, keine Hardware-Virtualisierung). USB-Durchreichung der VM instabil (USB-1.1-Controller). Kabelloses Debugging lief zunächst, ab 2026-11-13 abends lehnt das Handy die TLS-Verbindung trotz erfolgreicher Kopplung ab („Handshake failed“). Ausweg: APK direkt auf dem Handy installieren; dauerhaft: USB-3.0-Controller (xHCI) in der VM aktivieren. | offen |
| E3 | Hinweis | Farben moderner Stil (geändert auf Wunsch am 2026-11-13): höchstens 256 Farben gleichzeitig, alle aus einer 12-Bit-Grundpalette mit 4096 Farben (16 Stufen je Kanal), wie bei 16-Bit-Heimcomputern. Die 256 Farben wurden aus gerenderten Spielszenen ermittelt (Median-Cut) und liegen als feste Tabelle im Code; Abbildung per geordnetem Dithering. Per Test geprüft. | zur Kenntnis |
| E7 | Hinweis | Ein Release-Build der APK dauert auf diesem Rechner ca. 5–6 Minuten (5 GB RAM). Für schnelle Iteration wird die Spiellogik ohne Android per Unit-Tests entwickelt. | zur Kenntnis |
| E4 | Hinweis | 2 Spieler auf einem Gerät im Hochformat: Der obere Spieler sieht das Gerät „von oben“. Vorschlag: Anzeigen des oberen Spielers werden um 180° gedreht dargestellt. | zur Kenntnis |
| T1 | Hinweis | Stand der Umsetzung: Spiellogik (72 Tests), Präsentation mit Sprache, Optionen, Layout, Touch, beiden Grafikstilen und Ton (75 Tests), Desktop-Variante und Android-App. Alle 209 Tests grün (74 Logik, 135 Präsentation). Ton: klassisch als Nachbildung eines VCS-Tonchips (Rechteck/Rauschen, 16 Lautstärkestufen), modern als synthetische Effekte und eigene Titelmusik (34 s, 112 BPM, nahtlose Schleife). Alles per Software erzeugt (F11). Läuft auf dem Testhandy (Motorola edge 50 pro, Android 15) im Vollbild. | zur Kenntnis |
| E8 | Hinweis | Die APK ist mit dem automatisch erzeugten Debug-Schlüssel signiert (reicht zum Selbstinstallieren). Für spätere Updates muss derselbe Schlüssel verwendet werden, sonst muss die App vorher deinstalliert werden. Vorschlag vor der Abnahme: eigener Release-Schlüssel, sicher aufbewahrt. | offen |
| E9 | Hinweis | Durch die geänderte Option Spielmusik (vorher An/Aus, jetzt Stückauswahl) werden gespeicherte Optionen beim ersten Start der neuen Version einmalig auf die Standardwerte zurückgesetzt. | zur Kenntnis |
| F23 | Frage | Stimme (OP-13): Standard „An“ ist ein Vorschlag. Die Ansagen wurden einmalig mit der Sprachsynthese eSpeak NG (auf dem Rechner vorhanden, quelloffen) erzeugt, mit Computer-Klang bearbeitet und als WAV ins Spiel eingebettet (tools/voice/make_voice.py). Das weicht von F11 (alles im Spiel synthetisiert) ab; die Aufnahmen selbst unterliegen nicht der Lizenz der Software. Einverstanden? | offen |
| E10 | Hinweis | App-Symbol ab V0.7 aus einer vom Anwender gelieferten KI-Illustration (Quelle tools/icon/icon-quelle.jpg, erzeugt mit tools/icon/make_icon.py als adaptives Symbol ohne Rand). Lizenz/Nutzungsrechte des Bildgenerators für die Weitergabe der App bitte prüfen. | zur Kenntnis |
| T2 | Hinweis | Tests bitte gezielt starten (`dotnet test tests/LazerArena.Core.Tests` bzw. `tests/LazerArena.Presentation.Tests`); `dotnet test` auf die ganze Solution würde auch die Android-APK bauen (mehrere Minuten). | zur Kenntnis |

## Annahmen (zu bestätigen)

| ID | Annahme | Status |
|----|---------|--------|

## Erledigt

| ID | Beschreibung | Entscheidung | Datum |
|----|--------------|--------------|-------|
| R1 | Rechte: kein Remake von „Jedi Arena“ | Eigenständiges Spiel „Lazer Arena“ mit eigenem Sci-Fi-Thema und eigener Grafik | 2026-11-12 |
| F1 | Name und Thema | „Lazer Arena“, Sci-Fi | 2026-11-12 |
| F2 | Spielmodi | 1 Spieler gegen KI und 2 Spieler auf einem Gerät | 2026-11-12 |
| F3 | Steuerung | Wischen in der eigenen Bildschirmhälfte bewegt, Tippen feuert | 2026-11-12 |
| F4 | Moderne Variante | 16-Bit-Retro-Pixeloptik mit max. 1024 Farben, bessere Soundeffekte, Musik im Titelbildschirm | 2026-11-12 |
| F5 | Sound | Klassisch: Klänge wie beim Atari VCS; Modern: Soundsamples und Titelmusik | 2026-11-12 |
| F6 | Vertrieb | Nur APK zum Selbstinstallieren (Sideload) | 2026-11-12 |
| F7 | Regeln | Ich schlage Regeln vor, sie werden gemeinsam besprochen (siehe F12) | 2026-11-12 |
| F8 | Mockups | Als Bilder: Titelbildschirm und je ein Bild pro Modus (siehe F9) | 2026-11-12 |
| F9 | Umfang Mockups | Pro Grafikstil: Titel + 1 Spieler und 2 Spieler, jeweils hoch und quer | 2026-11-12 |
| F10 | Querformat | Spielfeld bleibt hochkant und mittig, Infobereiche links/rechts | 2026-11-12 |
| F11 | Herkunft Sound/Musik | Software-Synthese im Projekt (keine Fremdlizenzen) | 2026-11-12 |
| D1 | Regel: Schild-Regeneration | Ja, Schild wächst nach (SC-1, SC-2) | 2026-11-12 |
| D2 | Regel: Drohne schießt selbst | Lasergewitter in unregelmäßigen Abständen, Drohne feuert in beide Richtungen (LG-1 … LG-6) | 2026-11-12 |
| D3 | Regel: Matchlänge | Wer zuerst 3 Punkte hat, gewinnt (RS-3) | 2026-11-12 |
| D5 | Regel: Abwehr | Blitz dockt an, wenn er in die Nähe der Schwertspitze kommt (statt Zurückschlagen); ersetzt bisheriges LA-1 und den Diskussionspunkt „Schwert-Trefferfläche“ (AD-1 … AD-6) | 2026-11-12 |
| L1 | Sprache | Deutsch und Englisch; Auswahl nach Systemsprache, alles außer Deutsch → Englisch (SO-5) | 2026-11-12 |
| D4 | Regel: Zielen | Figur steht fest (Draufsicht), nur das Schwert dreht sich; Schwertwinkel = Schusswinkel der Drohne (SP-1 … SP-3) | 2026-11-12 |
| D6 | Neutraler Treffer | Punkt für den Gegner des Getroffenen (RS-1) | 2026-11-12 |
| D7 | Drehgeschwindigkeit | In den Optionen wählbar: Sofort / Begrenzt (SP-2, OP-4) | 2026-11-12 |
| D8 | Schießen im Gewitter | Nicht möglich, nur verteidigen, wie im Original (LG-6) | 2026-11-12 |
| D9 | Angedockter Blitz | Haltezeit zufällig 1–2 s wie im Original; Verhalten danach in den Optionen wählbar: Verpufft / Zurückwerfen (AD-3, AD-7, OP-5); Darstellung als Blitzlinie Drohne → Schwertspitze | 2026-11-12 |
| L2 | Sprachwahl | Zusätzlich in den Optionen: Automatisch (Standard) / Deutsch / English (SO-5, OP-2) | 2026-11-12 |
| G1 | Mockup-Änderungen | Drohne in beiden Stilen in Draufsicht; Schildform in den Optionen auf „Gebogen“ (wie Original) umstellbar (SF-3, OP-6); Optionsbildschirm ergänzt | 2026-11-12 |
| E5 | Umlaute im Font | Bitmap-Font um Ä, Ö, Ü erweitert | 2026-11-12 |
| F12 | Standardwerte der Optionen | Bestätigt: Modern, Automatisch, Mittel, Begrenzt, Verpufft, Gerade, Ton an, Musik an | 2026-11-12 |
| F13 | Mockup-Abnahme | Gestaltung abgenommen (Entwurf 3); Titel zusätzlich im Querformat ergänzt (mockups/*/…-titel-quer.png) | 2026-11-12 |
| D10 | Blitz-Prinzip | Blitz fliegt nicht, sondern zuckt von der Drohne in Richtung Gegner und bleibt mit ihr verbunden (BL-1 … BL-8); Mockups entsprechend angepasst | 2026-11-12 |
| F14 | Blitz-Mechanik | (1) Blitz wächst sichtbar (~0,4 s), (2) Ursprung wandert mit der Drohne, (3) Verhalten am Seitenrand in den Optionen wählbar: Abknicken / Erlöschen (BL-1 … BL-3, OP-9); Optionsbildschirm-Mockup um „Seitenrand“ ergänzt | 2026-11-12 |
| F15 | Standardwert Seitenrand | Abknicken (OP-9); Regelwerk damit als Version 1.0 abgestimmt | 2026-11-13 |
| A1 | Technik | C# mit MonoGame; Spiellogik in plattformunabhängiger Bibliothek mit xUnit-Tests | 2026-11-13 |
| E1 | Build-Umgebung | Eingerichtet im Benutzerverzeichnis ohne sudo: .NET SDK 10.0.404 (~/.dotnet) mit Android-Workload, Microsoft OpenJDK 17.0.21 (~/Android/jdk), Android SDK mit platform-tools, android-36, build-tools 36.0.0 (~/Android/Sdk), MonoGame-Templates 3.8.6.1. Pfade: tools/env.sh. Test-APK (net10.0-android, minSdk 29) erfolgreich gebaut. Hinweis: Manifest darf kein uses-sdk enthalten (Fehler XA1036). | 2026-11-13 |
| F16 | Schwertspitze bei steilen Winkeln | Kein Handlungsbedarf: Die Analyse war falsch. Die Andockzone (Radius 0,08) ragt bei allen Winkeln bis ±60° vor die vordere Schildreihe, auch beim gebogenen Schild; per Test belegt (SF-3/AD-1, AD-2). Entscheidung (a) „so lassen“ bleibt damit gültig, ohne Nachteil beim Abwehren. | 2026-11-13 |
| A2 | Annahme bestätigt | Mindestversion Android 10 = API-Level 29 (minSdk 29); Zielplattform net10.0-android. MonoGame 3.8.6.1 (Paket für net8.0-android, kompatibel). | 2026-11-13 |
| A3 | Annahme bestätigt | Drohnentempo (SF-2, im Regelwerk ohne Zahl): zufällig 0,15–0,45 Feldbreiten pro Sekunde. | 2026-11-13 |
| A4 | Annahme bestätigt | Nach einem Treffer bleibt das Bild 2 s stehen (Treffer-Anzeige), dann startet der Countdown der nächsten Runde (RS-2/RS-4). | 2026-11-13 |
| A5 | Annahme bestätigt | Trefferflächen der Schildsegmente schließen lückenlos an; die Fugen zwischen den Segmenten sind nur Grafik (sonst schlüpfen Blitze durch). | 2026-11-13 |
| F17 | Paket-ID | `de.lazerarena.app` bestätigt | 2026-11-13 |
| F18 | Lasergewitter zu zerstörerisch | Höchstens zwei Gewitter-Blitze gleichzeitig, einer je Richtung; Nachschuss nach zufälliger kurzer Pause 0,05–0,35 s (LG-3, Regelwerk 1.1). Auslegung zur Bestätigung siehe F19. | 2026-11-13 |
| R2 | Rückmeldung Handytest: Klänge und Bedienung | Schildtreffer jetzt kurzer Sci-Fi-„Swoosh“ statt metallisch; Andockklang knistert als Dauerton, solange der Blitz angedockt ist; optionale Spielmusik (eigenes Stück, 124 BPM, Option OP-10); Pausenmenü mit „Weiter“/„Hauptmenü“ (SO-1); Gewitter heißt „LIGHTNING STORM!“/„BLITZGEWITTER!“ und liegt auf dem Arenaboden unter Blitzen und Drohne (Regelwerk 1.2) | 2026-11-13 |
| F19 | Auslegung Gewitter-Nachschuss | Bestätigt: kurze zufällige Pausen, oben/unten unabhängig | 2026-11-13 |
| A6 | Annahme bestätigt | Im 1-Spieler-Modus gehört der ganze Bildschirm Spieler 1 (Wischen/Tippen überall); im 2-Spieler-Modus die eigene Hälfte. | 2026-11-13 |
| A7 | Annahme bestätigt | Wischen über die ganze Bildschirmbreite dreht das Schwert um 180°; Tippen = Berührung kürzer als 0,35 s und weniger als 4 % der Breite bewegt. | 2026-11-13 |
| A8 | Annahme bestätigt | Android-Zurück-Taste: im Spiel öffnet sie das Pausenmenü, im Pausenmenü und nach Spielende führt sie zum Hauptmenü, im Titel beendet sie die App. | 2026-11-13 |
| A9 | Annahme bestätigt | Option „Ton“ schaltet die Effekte, „Musik“ die Titelmusik; beide unabhängig voneinander. | 2026-11-13 |
| A10 | Annahme bestätigt | Titelmusik läuft im Titel- und Optionsbildschirm, Spielmusik (leiser) im Spiel inklusive Pause; beide stoppen, wenn die App in den Hintergrund geht. Standard der neuen Option Spielmusik (OP-10): An. | 2026-11-13 |
| R3 | Rückmeldung Handytest (2) | Nach dem Matchgewinn sprang das Spiel sofort ins Menü (Fingerabheben wurde als „Weiter“ gewertet) → neuer Siegerbildschirm mit Pokal und Sci-Fi-Fanfare bzw. Niederlage-Bildschirm, Tippsperre 1,5 s (RS-5); Palette moderner Stil auf 256 aus 4096 Farben; Punktestand oben kleiner und neben der Beschriftung (Kameraaussparung in der Mitte); zwei weitere Spielmusiken „Nebula“ (92 BPM) und „Hyperdrive“ (140 BPM), im Optionsmenü wählbar mit Hörprobe (OP-10); Gewitter-Schriftzug mit diagonalen Warnstreifen und Abstand zur Drohne (Regelwerk 1.3) | 2026-11-13 |
| F20 | Arenen im SNES-Look | Alle sieben wählbar als Option ARENA (OP-11, nur modern): Original, Raumstation, Sternenflug, Arcade-Chrom, Energiekern, Hologramm, Lava-Schmiede; animierte Arenen im Menü mit ✱ markiert. SNES-Arenen: Konturen, 4-stufige Schattierung, transparente Blitze und Klingen, kein Dithering, je Arena eigene Palette (≤ 256 aus 4096). Schwertgriffe in allen modernen Arenen länger und schlicht. Menüs bleiben einheitlich im Original-Look. Regelwerk 1.4. | 2026-11-13 |
| F21 | TV-Effekt | Option TV-EFFEKT (OP-12) für beide Stile: Bildzeilen je logischer Pixelzeile, Röhrenleuchten (verkleinertes, weich gefiltertes Bild additiv darübergelegt), dunklere Ecken; läuft auf der Grafikkarte. Am Desktop geprüft. Regelwerk 1.5. | 2026-11-13 |
| F22a | Standardwert TV-Effekt | Aus (OP-12) | 2026-11-13 |
| F22 | Stärke TV-Effekt | Bildzeilen 50 %, Leuchten 48 % (OP-12) | 2026-11-13 |
| R4 | Rückmeldung (3) | Blitze unregelmäßiger und zappeliger, mit Seitenästen (BL-8); stärkeres Leuchten von Schwertern und Blitzen in allen modernen Arenen; äußerer Drohnenring dreht sich im modernen Stil (eine Umdrehung in 3 s, Markierungen und Düsen drehen mit). Regelwerk 1.6. | 2026-11-13 |
| R5 | Rückmeldung (4) | Original-Leuchten zurück auf Ursprungswerte; SNES-Arenen leuchten wie „Original“ mit 75 % Stärke; Blitze 10 % kleiner; Sternenflug im Titelbild; Countdown: Piep bei 3 und 2, höherer Piep bei 1 (erster Countdown-Ton fehlte vorher in Runde 1 – behoben); Stimme (OP-13); zwei Synthwave-Stücke „Sunset Run“ (118 BPM) und „Laser Nights“ (126 BPM). TV-Leuchten 48 % = Intensität (nicht Größe), daher unverändert. Regelwerk 1.7. | 2026-11-14 |
| F24a | Figuren-Entwurf | Entwurf 10 „Angriff und Abwehr“ (Comic-Doppelpanel, Drohne auf der Trennlinie) | 2026-11-14 |
| F24 | Figuren-Einsatz | Titelbildschirm bleibt im Originalzustand (Demo-Szene). Figuren-Entwürfe 1–10 und der programmierte Comic-Versuch bleiben als Mockups. | 2026-11-14 |
| F25 | Realistisches Comic-Bild | Anwender hat ein KI-Bild geliefert; es wird ausschließlich als App-Symbol verwendet (randlos, adaptiv), nicht im Titel. | 2026-11-14 |
