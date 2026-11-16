# Lazer Arena – Regelwerk (Vorschlag)

Stand: 2026-11-15 · Status: **Version 2.3, abgestimmt**

Jede Regel hat eine ID (z. B. SP-3). Die Unit-Tests der Spiellogik verweisen auf diese IDs, damit die fachliche Korrektheit nachvollziehbar geprüft werden kann. Alle Zahlenwerte sind Startwerte und dürfen nach Spieltests angepasst werden.

Begriffe: **Blitz** = Laserblitz, den die Drohne abfeuert. Winkelangaben: 0° = gerade auf den Gegner zu, negative Werte = nach links, positive Werte = nach rechts, jeweils aus Sicht des Spielers.

## 1. Spielfeld (SF)

- **SF-1** Das Spielfeld ist hochkant. Spieler 1 steht unten, Spieler 2 (oder die KI) steht oben. Im Querformat bleibt das Spielfeld hochkant und steht mittig, links und rechts davon sind Infobereiche.
- **SF-2** In der Mitte schwebt die **Sentinel-Drohne** (in beiden Grafikstilen in Draufsicht dargestellt). Sie bewegt sich selbstständig waagerecht hin und her, wechselt in zufälligen Abständen (1–3 s) Richtung und Geschwindigkeit und prallt an den Seitenrändern ab.
- **SF-3** Vor jedem Spieler liegt ein **Energieschild** aus 2 Reihen mit je 16 Segmenten. Die Anordnung ist in den Optionen wählbar (OP-6):
  - **Gerade:** Die Reihen liegen waagerecht über die ganze Feldbreite.
  - **Gebogen:** Die Reihen liegen auf einem flachen Bogen vor der Figur (angelehnt an das Original). Die Anzahl der Segmente ist gleich, damit beide Varianten gleich schwer sind.
  - Darstellung im Stil Klassisch: alle Blöcke gleich groß im Seitenverhältnis 3:1 (hochkant 6 × 2, quer 3 × 1 Pixel), mit 1 Pixel Fuge gleichmäßig über die Feldbreite verteilt; zwischen den beiden geraden Reihen liegt eine Fuge von einer ganzen Blockhöhe. Das Feld ist dafür 16 Blöcke + 15 Fugen breit. Nur Grafik, die Trefferflächen bleiben lückenlos.
  - In beiden Varianten ragt die Andockzone (AD-1) bei jedem Schwertwinkel bis ±60° vor die vordere Schildreihe, sodass ein auf die Spitze gezielter Blitz immer andocken kann, bevor er das Schild trifft.
- **SF-4** Was an den Seitenrändern mit Blitzen geschieht, regelt BL-3.

## 2. Spieler und Steuerung (SP)

- **SP-1** Die Spielfigur ist von oben zu sehen und steht fest in der Mitte ihrer Grundlinie. Sie bewegt sich nicht.
- **SP-2** Die Figur hält ein **Laserschwert** (Länge: 22 % der Feldbreite), das vom Körper aus in Richtung Gegner zeigt. Waagerechtes Wischen in der eigenen Bildschirmhälfte dreht das Schwert im Bereich von −60° bis +60°. Die Drehgeschwindigkeit ist immer **begrenzt** (höchstens 240°/s, OP-4).
- **SP-3** **Tippen** in der eigenen Hälfte lässt die Drohne einen Blitz auf den Gegner abfeuern. Der Schusswinkel ist gleich dem aktuellen Schwertwinkel des Schützen. Gezielt wird also über den Schwertwinkel und das Timing, weil sich die Drohne bewegt.
- **SP-4** Jeder Spieler hat eine Abklingzeit von 0,8 s zwischen zwei Schüssen und höchstens 2 eigene Blitze gleichzeitig. Ein am gegnerischen Schwert angedockter Blitz zählt mit, bis er erlischt.

## 3. Blitze (BL)

Ein Blitz ist kein fliegendes Geschoss. Er zuckt von der Drohne aus in Schussrichtung auf den Gegner zu und bleibt dabei mit der Drohne verbunden. Maßgeblich für Treffer ist die **Blitzspitze**, also das vordere Ende.

- **BL-1** Bei einem Schuss entsteht der Blitz an der Drohne. Seine Spitze breitet sich in Schussrichtung aus, sichtbar mit 1,2 Feldhöhen pro Sekunde. Bis zur gegnerischen Grundlinie dauert das etwa 0,4 s, also genug Zeit zum Reagieren.
- **BL-2** Der Blitz bleibt mit der Drohne verbunden. Bewegt sich die Drohne, wandert der Ursprung des Blitzes mit, die Richtung bleibt gleich.
- **BL-3** Verhalten am Seitenrand, in den Optionen wählbar (OP-9):
  - **Abknicken:** Der Blitz knickt am Rand ab (Einfallswinkel = Ausfallswinkel) und breitet sich weiter aus.
  - **Erlöschen:** Der Blitz erlischt, sobald seine Spitze den Rand erreicht.
- **BL-4** Erreicht die Blitzspitze ein Schildsegment, wird das Segment zerstört, und der Blitz erlischt.
- **BL-5** Erreicht die Blitzspitze die Spielfigur, ist das ein **Treffer** (Wertung siehe RS-1).
- **BL-6** Blitze kreuzen sich ohne Wirkung.
- **BL-7** Die Ausbreitungsgeschwindigkeit steigt alle 20 s einer Runde um 10 %, höchstens auf 200 % des Startwerts.
- **BL-8** Darstellung (Blitzform je Grafikstil wählbar, OP-14): **Zackig:** zuckende, unregelmäßige Zickzack-Linie von der Drohne bis zur Blitzspitze, mit ungleich langen Abschnitten, wechselnd starken Ausschlägen, gelegentlichen scharfen Knicken und (Stil Modern) kurzen Seitenästen. **Gleichmäßig:** regelmäßiges Zickzack, das abwechselnd zu beiden Seiten ausschlägt, ohne Seitenäste. Die Form ändert sich in jedem Bild. Die Mitte von Blitz und Klinge ist deckend, nur das Leuchten außen ist transparent. Im Stil Modern sieht der Blitz aus wie eine Lichtschwert-Klinge (heller, deckender Rand in Teamfarbe mit fast weißem, deckendem Kern), nur schmaler (2 statt 3 Pixel). Die Trefferlogik nutzt nur die gerade Mittellinie.

## 4. Andocken am Schwert (AD)

Ersetzt das Zurückschlagen aus Entwurf v2. Angelehnt an das Prinzip des Originals: Ein abgewehrter Blitz wird festgehalten, statt abzuprallen.

- **AD-1** Um die Schwertspitze liegt eine **Andockzone** mit einem Radius von 8 % der Feldbreite.
- **AD-2** Erreicht die Spitze eines Blitzes, der sich auf den Spieler zubewegt (gegnerisch oder neutral), die Andockzone, dockt sie an der Schwertspitze an. Der Blitz breitet sich nicht weiter aus und kann weder Schild noch Figur treffen. Er bleibt als zuckende Verbindung von der Drohne bis zur Schwertspitze stehen (wie im Original).
- **AD-3** Ein angedockter Blitz bleibt eine zufällige Zeit zwischen 1 und 2 s an der Schwertspitze (wie im Original) und folgt dabei dem Schwert. Was danach geschieht, regelt AD-7.
- **AD-4** Pro Schwert kann höchstens ein Blitz angedockt sein. Solange die Spitze belegt ist, breiten sich weitere Blitze durch die Andockzone hindurch aus.
- **AD-5** Berührt eine Blitzspitze die Klinge außerhalb der Andockzone, hat das keine Wirkung. Der Blitz breitet sich weiter aus.
- **AD-6** Ein angedockter Blitz hindert den Spieler nicht am Schießen (SP-3, SP-4 gelten unverändert), außer im Modus „Zurückwerfen“ (AD-7).
- **AD-7** Verhalten nach dem Andocken, in den Optionen wählbar (OP-5):
  - **Verpufft** (wie im Original): Nach Ablauf der Haltezeit erlischt der Blitz.
  - **Zurückwerfen:** Tippt der Spieler, während ein Blitz angedockt ist, erlischt die Verbindung zur Drohne, und statt eines neuen Schusses zuckt ein Blitz von der Schwertspitze im aktuellen Schwertwinkel auf den Gegner zu. Er gehört diesem Spieler und breitet sich nach BL-1 bis BL-8 aus (Ursprung: Schwertspitze statt Drohne). Er zählt zum Blitzlimit (SP-4). Ohne Tippen erlischt er nach Ablauf der Haltezeit.

## 5. Schild (SC)

- **SC-1** Zerstörte Schildsegmente wachsen nach: alle 5 s ein Segment je Spieler. Zuerst wird die vordere Reihe aufgefüllt, darin das Segment, das der Mitte am nächsten liegt.
- **SC-2** Ein Segment wächst nicht nach, solange ein Blitz durch seine Position verläuft.

## 6. Blitzgewitter (LG)

Anzeige im Spiel: „BLITZGEWITTER!“ bzw. „LIGHTNING STORM!“. Der Schriftzug liegt auf dem Arenaboden, unter Blitzen und Drohne, mit diagonalem Warnstreifen-Rand, und hält Abstand zur Drohne samt Warnring.

- **LG-1** In unregelmäßigen Abständen (zufällig 15–30 s) löst die Drohne ein **Blitzgewitter** aus.
- **LG-2** Eine Sekunde vorher wird es angekündigt: Die Drohne blinkt, und ein Warnton erklingt; bei eingeschalteter Stimme (OP-13) sagt sie stattdessen „Warnung!“ bzw. „Warning!“. Während des Gewitters pulsiert das rote Licht der Drohne (Ring mit dezentem Leuchten und Auge, etwa 1,2-mal je Sekunde; Modern).
- **LG-3** Das Gewitter dauert 3 s. In dieser Zeit ist je Richtung (nach oben, nach unten) höchstens ein Gewitter-Blitz unterwegs, insgesamt also höchstens zwei. Ein angedockter Blitz zählt mit, bis er erlischt. Ist in einer Richtung kein Blitz unterwegs, feuert die Drohne nach einer zufälligen kurzen Pause von 0,05–0,35 s den nächsten, in einem zufälligen Winkel zwischen −60° und +60°. Die Blitze folgen so dicht, aber unregelmäßig aufeinander.
- **LG-4** Gewitter-Blitze sind **neutral**. Sie können wie alle anderen Blitze angedockt werden (AD-2).
- **LG-5** Neutrale Blitze zählen nicht zum Blitzlimit der Spieler (SP-4).
- **LG-6** Während des Gewitters (LG-3) können die Spieler nicht schießen, sondern sich nur verteidigen (wie im Original). Die Ankündigungsphase (LG-2) zählt noch nicht dazu.

## 7. Runden und Sieg (RS)

- **RS-1** Ein Treffer (BL-5) beendet die Runde. Der Besitzer des Blitzes erhält 1 Punkt. Ist der Blitz neutral, erhält der Gegner des Getroffenen den Punkt.
- **RS-2** Zu Beginn jeder Runde werden Schilde, Schwertwinkel, Drohnenposition, Blitzgeschwindigkeit und Gewitter-Timer zurückgesetzt. Alle Blitze werden entfernt.
- **RS-3** Wer zuerst 3 Punkte hat, gewinnt das Match.
- **RS-4** Vor jeder Runde läuft ein Countdown von 3 s. Während des Countdowns kann nicht geschossen werden. Anzeige „3 … 2 … 1 …“, beim Rundenstart erscheint für 0,6 s „LOS!“ bzw. „GO!“, während schon gespielt wird. Akustisch (beide Stile): gleicher Piepton bei „3“, „2“ und „1“, höherer und längerer Piepton bei „LOS!“; bei eingeschalteter Stimme (OP-13) sagt die gewählte Stimme stattdessen „Drei, Zwei, Eins, Los!“ bzw. „Three, Two, One, Go!“.
- **RS-5** Nach dem Matchgewinn bleibt die Siegermeldung etwa 1 s stehen. Danach erscheint der Siegerbildschirm mit Pokal (Klassisch) bzw. der Heldin mit erhobenem Schwert (Modern, Pixelbild vor Sternenflug), Sieger, Endstand und Sci-Fi-Fanfare. Gewinnt im 1-Spieler-Modus die KI, erscheint stattdessen „Niederlage“ mit grauem Pokal (Klassisch) bzw. der knienden Heldin (Modern) und kurzer absteigender Melodie. Tippen führt zum Hauptmenü, aber erst nach 1,5 s und nur mit einer neuen Berührung. Im 2-Spieler-Hochformat steht der Bildschirm für den Sieger richtig herum.

## 8. KI-Gegner (KI)

- **KI-1** Es gibt drei Stufen: Leicht, Mittel und Schwer.
- **KI-2** Die Stufen unterscheiden sich in der Reaktionszeit (400 / 250 / 120 ms), in der Genauigkeit beim Ausrichten des Schwerts und in der Schussfrequenz.
- **KI-3** Die KI hält sich an dieselben Regeln wie ein menschlicher Spieler (Abklingzeit, Blitzlimit, Drehgeschwindigkeit des Schwerts).

## 9. Sonstiges (SO)

- **SO-1** Pause: Bei Tipp auf das Pause-Symbol, mit der Zurück-Taste oder beim Wechsel der App in den Hintergrund wird das Spiel pausiert. Das Pausenmenü bietet „Weiter“ und „Hauptmenü“ sowie Umschaltzeilen für Spielmusik (Modern), Stimme und in der Raumstation die Warnleuchten (OP-17). Die Zurück-Taste im Pausenmenü führt ebenfalls zum Hauptmenü.
- **SO-2** Der Grafikstil (Klassisch/Modern) lässt sich im Menü umschalten. Er ändert nur Darstellung und Ton, nie die Spiellogik.
- **SO-3** Die Spiellogik läuft mit festem Zeitschritt (60 Hz), damit sie unabhängig von der Bildrate des Geräts und deterministisch testbar ist.
- **SO-4** Der Zufall (Drohnenbewegung, Gewitter) verwendet einen Zufallsgenerator mit Startwert (Seed), damit Tests reproduzierbar sind.
- **SO-7** **Vollbild:** Auf Android werden Status- und Navigationsleiste ausgeblendet; sie erscheinen nur kurz beim Wischen vom Bildschirmrand und verdecken keine Knöpfe. Liegt die Kamera-Aussparung oben in der Mitte, zeichnet das Spiel bis an den oberen Rand; mittig oben stehende Überschriften beginnen erst unter der Aussparung, und Anzeigen der oberen Leiste reichen nicht in die Mitte (sonst steht die KI-Stufe in der unteren Leiste). Liegt die Aussparung nicht in der Mitte, beginnt das Bild unter ihr.
- **SO-6** **Anleitung:** Menüpunkt „Anleitung“ im Titel. Sechs Seiten mit kleiner Animation und kurzem Text: Schwert drehen, Feuern, Blitze fangen, Schild, Blitzgewitter, Punkte. „Weiter“ / „Zurück“ blättern, die Zurück-Taste führt zum Titel.
- **SO-5** **Sprache:** In der Einstellung „Automatisch“ (Standard) wird beim Start die Systemsprache gelesen. Ist sie Deutsch (`de`, alle Regionen), wird Deutsch verwendet, in allen anderen Fällen Englisch. In den Optionen kann die Sprache fest auf Deutsch oder Englisch gestellt werden (OP-2). Alle Texte liegen in Ressourcendateien, nicht im Code.

## 10. Optionen (OP)

Jede Änderung wird sofort automatisch dauerhaft auf dem Gerät gespeichert (kein Speichern-Knopf) und gilt ab dem nächsten Spiel. Im Stil Modern laufen im Hintergrund der Optionen, wie im Titel, Sterne vorbei (Sternenflug).

| ID | Option | Werte | Standard |
|----|--------|-------|----------|
| OP-1 | Grafikstil | Klassisch / Modern | Modern |
| OP-2 | Sprache | Automatisch / Deutsch / English | Automatisch |
| OP-3 | KI-Stufe | Leicht / Mittel / Schwer | Mittel |
| OP-4 | Schwertdrehung: keine Option mehr (seit Version 1.8), immer begrenzt | – | Begrenzt (240°/s) |
| OP-5 | Angedockter Blitz | Verpufft / Zurückwerfen | Verpufft (wie Original) |
| OP-6 | Schildform | Gerade / Gebogen | Gerade |
| OP-7 | Ton (Aus schaltet auch die Stimme ab) | An / Aus | An |
| OP-8 | Titelmusik (nur Stil Modern) | An / Aus | An |
| OP-9 | Seitenrand | Abknicken / Erlöschen | Abknicken |
| OP-10 | Spielmusik (nur Stil Modern, leiser als die Titelmusik; beim Umschalten im Optionsmenü wird das Stück angespielt). Pixel Pulse klingt wie eine Spielkonsole (Pulswellen, Dreieck-Bass, Rausch-Schlagzeug), Starlight ist ein ruhiges E-Piano-Stück (FM-E-Piano, Glocken-Melodie, Sinus-Bass). | Aus / Orbit / Nebula / Hyperdrive / Sunset Run / Laser Nights / Pixel Pulse / Starlight | Orbit |
| OP-11 | Arena (nur Stil Modern): Original, Raumstation, Sternenflug, Arcade-Chrom, Energiekern, Hologramm, Lava-Schmiede. Animierte Arenen (Sternenflug, Energiekern, Hologramm, Lava-Schmiede) sind im Menü mit dem Wort „ANIMIERT“ markiert. Alle Arenen zeigen die Drohne der Arena „Original“. Im Hologramm blendet das Gitter weich ab und wird dann Linie für Linie neu gezeichnet (senkrecht und waagerecht gleichzeitig, jede Linie etwas verzögert; 7,5 s je Durchlauf). Die Raumstation hat in jeder Ecke eine rote Rundumleuchte (Draufsicht), die sich während des Gewitters dreht (1,6 s je Umdrehung, 1 s Hochlaufen, 1,5 s Auslaufen) und zwei Lichtkegel über den Boden wirft (OP-18). Ändert nur das Aussehen, nie die Spiellogik (SO-2). | 7 Arenen | Original |
| OP-12 | TV-Effekt (beide Stile): Bildzeilen (eine dunkle Linie je Pixelzeile des klassischen Stils, auch im Stil Modern, damit beide gleich aussehen; 50 % Abdunklung), weiches Röhrenleuchten um helle Stellen (48 %), dunklere Ecken. Reine Darstellung, wirkt nicht auf die Spiellogik (SO-2). | An / Aus | Aus |
| OP-13 | Stimme (beide Stile): weibliche Stimme sagt den Countdown mit „Los!“ (ersetzt die Pieptöne), „Warnung!“ vor dem Gewitter (ersetzt den Warnton), „Treffer“ sowie „Sieg“/„Niederlage“ an; Sprache folgt der Spielsprache. Computer: klare Computerstimme. Sanft: tiefere, hauchige Stimme mit leichtem Hall (seit V1.0 10 % schneller). Aus: nur Pieptöne und Effekte. | Aus / Computer / Sanft | Computer |
| OP-14 | Blitzform, je Grafikstil getrennt gespeichert (BL-8); im Menü direkt nach „Angedockter Blitz“, zeigt die Einstellung des aktuellen Stils. Reine Darstellung (SO-2). | Gleichmäßig / Zackig | Klassisch: Gleichmäßig, Modern: Zackig |
| OP-15 | **Erweiterte Optionen (versteckt):** Den Knopf „Optionen“ im Titelbildschirm 1 s gedrückt halten. Die sanfte Stimme sagt „Erweiterte Optionen ausgewählt“ (auch wenn OP-13 aus ist; nur bei Ton an). Einstellungen: TV-Zeilen-Abdunklung 0–100 % (5er-Schritte), Zeilenbreite schmal (⅓ Zeile) / breit (½ Zeile), TV-Leuchten 0–100 % (4er-Schritte), Leuchtweite 0–10 %, Leucht-Weichheit scharf / mittel / weich, dunkle Ecken 0–100 % (5er-Schritte); Leuchten von Laserschwert, Blitz, Gewitterring und Drohnenauge je 0–200 % des Standards (10er-Schritte; nur Stil Modern); Der Knopf „Standard“ (OP-16) setzt sie zurück. Reine Darstellung (SO-2). | siehe links | Zeilen 50 %, schmal, Leuchten 48 %, Weite 2 %, mittel, Ecken 45 %, Leuchten je 100 % |
| OP-16 | Knopf **„Standard“** neben „Zurück“ in den Optionen und den erweiterten Optionen: erstes Antippen zeigt „Sicher?“, ein zweites Antippen innerhalb von 2 s setzt die Einstellungen dieses Bildschirms auf die Standardwerte (normale Optionen: alle außer Grafikstil und erweiterten Optionen; erweiterte Optionen: nur diese). | – | – |
| OP-17 | Im **Pausenmenü** umschaltbar: Spielmusik (nur Modern), Stimme und in der Raumstation die Warnleuchten. Linke Hälfte der Zeile zurück, rechte vor; wirkt sofort und wird gespeichert. Im Querformat stehen die Zeilen rechts neben den Knöpfen. | – | – |
| OP-19 | **Optionen je Stil:** Klassisch und Modern haben jeweils einen eigenen, vollständigen Satz Optionen. Ein Stilwechsel aktiviert den gespeicherten Satz des neuen Stils. | – | – |
| OP-18 | **Warnleuchten** der Raumstation (nur Modern, Arena Raumstation; im Menü direkt nach „Arena“). Aus: Die Leuchten bleiben dunkel. | An / Aus | An |
