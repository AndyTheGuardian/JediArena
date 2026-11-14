# Lazer Arena – Regelwerk (Vorschlag)

Stand: 2026-11-13 · Status: **Version 1.7, abgestimmt**

Jede Regel hat eine ID (z. B. SP-3). Die Unit-Tests der Spiellogik verweisen auf diese IDs, damit die fachliche Korrektheit nachvollziehbar geprüft werden kann. Alle Zahlenwerte sind Startwerte und dürfen nach Spieltests angepasst werden.

Begriffe: **Blitz** = Laserblitz, den die Drohne abfeuert. Winkelangaben: 0° = gerade auf den Gegner zu, negative Werte = nach links, positive Werte = nach rechts, jeweils aus Sicht des Spielers.

## 1. Spielfeld (SF)

- **SF-1** Das Spielfeld ist hochkant. Spieler 1 steht unten, Spieler 2 (oder die KI) steht oben. Im Querformat bleibt das Spielfeld hochkant und steht mittig, links und rechts davon sind Infobereiche.
- **SF-2** In der Mitte schwebt die **Sentinel-Drohne** (in beiden Grafikstilen in Draufsicht dargestellt). Sie bewegt sich selbstständig waagerecht hin und her, wechselt in zufälligen Abständen (1–3 s) Richtung und Geschwindigkeit und prallt an den Seitenrändern ab.
- **SF-3** Vor jedem Spieler liegt ein **Energieschild** aus 2 Reihen mit je 16 Segmenten. Die Anordnung ist in den Optionen wählbar (OP-6):
  - **Gerade:** Die Reihen liegen waagerecht über die ganze Feldbreite.
  - **Gebogen:** Die Reihen liegen auf einem flachen Bogen vor der Figur (angelehnt an das Original). Die Anzahl der Segmente ist gleich, damit beide Varianten gleich schwer sind.
  - In beiden Varianten ragt die Andockzone (AD-1) bei jedem Schwertwinkel bis ±60° vor die vordere Schildreihe, sodass ein auf die Spitze gezielter Blitz immer andocken kann, bevor er das Schild trifft.
- **SF-4** Was an den Seitenrändern mit Blitzen geschieht, regelt BL-3.

## 2. Spieler und Steuerung (SP)

- **SP-1** Die Spielfigur ist von oben zu sehen und steht fest in der Mitte ihrer Grundlinie. Sie bewegt sich nicht.
- **SP-2** Die Figur hält ein **Laserschwert** (Länge: 22 % der Feldbreite), das vom Körper aus in Richtung Gegner zeigt. Waagerechtes Wischen in der eigenen Bildschirmhälfte dreht das Schwert im Bereich von −60° bis +60°. Die Drehgeschwindigkeit ist in den Optionen wählbar (OP-4): **Sofort** (Schwert folgt dem Finger direkt) oder **Begrenzt** (höchstens 240°/s).
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
- **BL-8** Darstellung: zuckende, unregelmäßige Zickzack-Linie von der Drohne bis zur Blitzspitze, mit ungleich langen Abschnitten, wechselnd starken Ausschlägen, gelegentlichen scharfen Knicken und kurzen Seitenästen. Die Form ändert sich in jedem Bild. Die Trefferlogik nutzt nur die gerade Mittellinie.

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
- **LG-2** Eine Sekunde vorher wird es angekündigt: Die Drohne blinkt, und ein Warnton erklingt.
- **LG-3** Das Gewitter dauert 3 s. In dieser Zeit ist je Richtung (nach oben, nach unten) höchstens ein Gewitter-Blitz unterwegs, insgesamt also höchstens zwei. Ein angedockter Blitz zählt mit, bis er erlischt. Ist in einer Richtung kein Blitz unterwegs, feuert die Drohne nach einer zufälligen kurzen Pause von 0,05–0,35 s den nächsten, in einem zufälligen Winkel zwischen −60° und +60°. Die Blitze folgen so dicht, aber unregelmäßig aufeinander.
- **LG-4** Gewitter-Blitze sind **neutral**. Sie können wie alle anderen Blitze angedockt werden (AD-2).
- **LG-5** Neutrale Blitze zählen nicht zum Blitzlimit der Spieler (SP-4).
- **LG-6** Während des Gewitters (LG-3) können die Spieler nicht schießen, sondern sich nur verteidigen (wie im Original). Die Ankündigungsphase (LG-2) zählt noch nicht dazu.

## 7. Runden und Sieg (RS)

- **RS-1** Ein Treffer (BL-5) beendet die Runde. Der Besitzer des Blitzes erhält 1 Punkt. Ist der Blitz neutral, erhält der Gegner des Getroffenen den Punkt.
- **RS-2** Zu Beginn jeder Runde werden Schilde, Schwertwinkel, Drohnenposition, Blitzgeschwindigkeit und Gewitter-Timer zurückgesetzt. Alle Blitze werden entfernt.
- **RS-3** Wer zuerst 3 Punkte hat, gewinnt das Match.
- **RS-4** Vor jeder Runde läuft ein Countdown von 3 s. Während des Countdowns kann nicht geschossen werden. Akustisch: Piepton bei „3“ und „2“, etwas höherer Piepton bei „1“; bei eingeschalteter Stimme (OP-13) sagt die Computerstimme stattdessen „Drei, Zwei, Eins“ bzw. „Three, Two, One“.
- **RS-5** Nach dem Matchgewinn bleibt die Siegermeldung etwa 1 s stehen. Danach erscheint der Siegerbildschirm mit Pokal, Sieger, Endstand und Sci-Fi-Fanfare. Gewinnt im 1-Spieler-Modus die KI, erscheint stattdessen „Niederlage“ mit grauem Pokal und kurzer absteigender Melodie. Tippen führt zum Hauptmenü, aber erst nach 1,5 s und nur mit einer neuen Berührung. Im 2-Spieler-Hochformat steht der Bildschirm für den Sieger richtig herum.

## 8. KI-Gegner (KI)

- **KI-1** Es gibt drei Stufen: Leicht, Mittel und Schwer.
- **KI-2** Die Stufen unterscheiden sich in der Reaktionszeit (400 / 250 / 120 ms), in der Genauigkeit beim Ausrichten des Schwerts und in der Schussfrequenz.
- **KI-3** Die KI hält sich an dieselben Regeln wie ein menschlicher Spieler (Abklingzeit, Blitzlimit, Drehgeschwindigkeit des Schwerts).

## 9. Sonstiges (SO)

- **SO-1** Pause: Bei Tipp auf das Pause-Symbol, mit der Zurück-Taste oder beim Wechsel der App in den Hintergrund wird das Spiel pausiert. Das Pausenmenü bietet „Weiter“ und „Hauptmenü“. Die Zurück-Taste im Pausenmenü führt ebenfalls zum Hauptmenü.
- **SO-2** Der Grafikstil (Klassisch/Modern) lässt sich im Menü umschalten. Er ändert nur Darstellung und Ton, nie die Spiellogik.
- **SO-3** Die Spiellogik läuft mit festem Zeitschritt (60 Hz), damit sie unabhängig von der Bildrate des Geräts und deterministisch testbar ist.
- **SO-4** Der Zufall (Drohnenbewegung, Gewitter) verwendet einen Zufallsgenerator mit Startwert (Seed), damit Tests reproduzierbar sind.
- **SO-5** **Sprache:** In der Einstellung „Automatisch“ (Standard) wird beim Start die Systemsprache gelesen. Ist sie Deutsch (`de`, alle Regionen), wird Deutsch verwendet, in allen anderen Fällen Englisch. In den Optionen kann die Sprache fest auf Deutsch oder Englisch gestellt werden (OP-2). Alle Texte liegen in Ressourcendateien, nicht im Code.

## 10. Optionen (OP)

Alle Optionen werden dauerhaft auf dem Gerät gespeichert und gelten ab dem nächsten Spiel.

| ID | Option | Werte | Standard |
|----|--------|-------|----------|
| OP-1 | Grafikstil | Klassisch / Modern | Modern |
| OP-2 | Sprache | Automatisch / Deutsch / English | Automatisch |
| OP-3 | KI-Stufe | Leicht / Mittel / Schwer | Mittel |
| OP-4 | Schwertdrehung | Sofort / Begrenzt (240°/s) | Begrenzt |
| OP-5 | Angedockter Blitz | Verpufft / Zurückwerfen | Verpufft (wie Original) |
| OP-6 | Schildform | Gerade / Gebogen | Gerade |
| OP-7 | Ton | An / Aus | An |
| OP-8 | Titelmusik (nur Stil Modern) | An / Aus | An |
| OP-9 | Seitenrand | Abknicken / Erlöschen | Abknicken |
| OP-10 | Spielmusik (nur Stil Modern, leiser als die Titelmusik; beim Umschalten im Optionsmenü wird das Stück angespielt) | Aus / Orbit / Nebula / Hyperdrive / Sunset Run / Laser Nights | Orbit |
| OP-11 | Arena (nur Stil Modern): Original, Raumstation, Sternenflug ✱, Arcade-Chrom, Energiekern ✱, Hologramm ✱, Lava-Schmiede ✱ (✱ = animiert, im Menü markiert). Ändert nur das Aussehen, nie die Spiellogik (SO-2). | 7 Arenen | Original |
| OP-12 | TV-Effekt (beide Stile): Bildzeilen (je logischer Pixelzeile eine dunkle Linie, 50 % Abdunklung), weiches Röhrenleuchten um helle Stellen (48 %), dunklere Ecken. Reine Darstellung, wirkt nicht auf die Spiellogik (SO-2). | An / Aus | Aus |
| OP-13 | Stimme (beide Stile): weibliche Computerstimme sagt den Countdown (ersetzt die Pieptöne), „Treffer“ sowie „Sieg“/„Niederlage“ an; Sprache folgt der Spielsprache. Aus: nur Pieptöne und Effekte. | An / Aus | An (Vorschlag) |
