# Lazer Arena – Benutzerdokumentation

Version 1.3 · English version: [user-guide.md](user-guide.md)

Lazer Arena ist ein Duell im Orbit. Zwischen zwei Kämpfern schwebt eine Drohne und feuert Blitze auf sie. Jeder Kämpfer dreht ein Lichtschwert, um die Blitze abzufangen, und bestimmt, wann die Drohne auf den Gegner schießt. Wer zuerst 3 Treffer landet, gewinnt.

## 1. Installation

Lazer Arena gibt es nicht im Play Store. Die APK-Datei wird selbst installiert („Sideloading“).

1. `LazerArena-1.3.apk` auf das Handy kopieren (USB-Kabel, Cloud-Speicher oder E-Mail).
2. Die Datei auf dem Handy öffnen, zum Beispiel in der App „Dateien“.
3. Wenn Android nachfragt, dieser App (zum Beispiel „Dateien“) erlauben, unbekannte Apps zu installieren.
4. „Installieren“ antippen.

Voraussetzung: Android 10 oder neuer. Hoch- und Querformat werden unterstützt. Das Spiel folgt der Drehung des Handys.

**Aktualisieren:** Die neue APK einfach über die alte installieren. Die Einstellungen bleiben erhalten. Lehnt Android das Update ab („App nicht installiert“), wurde die neue Version mit einem anderen Schlüssel signiert. Dann zuerst die alte Version deinstallieren; die Einstellungen gehen dabei verloren.

## 2. Titelbildschirm

| Knopf | Funktion |
|-------|----------|
| 1 SPIELER | Du gegen den Computer (KI) |
| 2 SPIELER | Zwei Spieler an einem Handy |
| ANLEITUNG | Animierte Anleitung (siehe unten) |
| STIL | Wechselt zwischen KLASSISCH (Retro-Look der 1980er) und MODERN (16-Bit-Look mit Arenen und Musik) |
| OPTIONEN | Einstellungen (siehe Abschnitt 6) |

**Anleitung:** Sechs kurze, animierte Seiten zeigen das Wichtigste: Schwert drehen, Feuern, Blitze fangen, Schild, Blitzgewitter und Punkte. Mit WEITER > und < ZURÜCK blätterst du; FERTIG oder die Zurück-Taste des Handys führt zum Titelbildschirm.

**Verstecktes Menü:** OPTIONEN eine Sekunde gedrückt halten. Eine sanfte Stimme sagt „Erweiterte Optionen ausgewählt“, und die erweiterten Optionen öffnen sich (Abschnitt 7).

## 3. Ziel des Spiels

- Du stehst unten, der Gegner (KI oder Spieler 2) steht oben.
- Vor jedem Kämpfer liegt ein **Energieschild** aus 2 Reihen mit je 16 Blöcken.
- In der Mitte bewegt sich die **Drohne** hin und her.
- Wenn du feuerst, schickt die Drohne einen Blitz zu deinem Gegner. **Der Blitz fliegt in die Richtung, in die dein Schwert zeigt**, und startet dort, wo die Drohne gerade ist.
- Trifft ein Blitz einen Schildblock, wird der Block zerstört, und der Blitz verschwindet. Erreicht ein Blitz den Kämpfer, ist das ein **Treffer**.
- Jeder Treffer gibt 1 Punkt und startet eine neue Runde. **Wer zuerst 3 Punkte hat, gewinnt das Match.**

## 4. Steuerung

| Aktion | Geste |
|--------|-------|
| Schwert drehen | Nach links oder rechts wischen. Das Schwert dreht sich zwischen −60° und +60°. |
| Feuern | Kurz tippen |
| Pause | Pause-Symbol (II) antippen oder die Zurück-Taste des Handys drücken |

- **1 Spieler:** Du kannst den ganzen Bildschirm nutzen.
- **2 Spieler:** Die untere Hälfte gehört Spieler 1, die obere Spieler 2. Die Anzeigen des oberen Spielers stehen auf dem Kopf, damit jeder sie von seiner Seite lesen kann.

Regeln zum Feuern:

- Nach jedem Schuss wartest du 0,8 s bis zum nächsten.
- Jeder Spieler hat höchstens 2 Blitze gleichzeitig in der Arena.
- Während des Countdowns und während eines Blitzgewitters kann niemand feuern.

## 5. Was in der Arena passiert

**Countdown:** Jede Runde beginnt mit „3 … 2 … 1 … LOS!“. Mit eingeschalteter Stimme wird er gesprochen. Sonst hörst du drei gleiche Pieptöne und einen höheren bei LOS.

**Blitze fangen (Andocken):** Erreicht ein Blitz die Spitze deines Schwerts, bleibt er dort 1 bis 2 Sekunden hängen und ist harmlos. Danach:

- **VERPUFFT** (Standard): Er verschwindet.
- **ZURÜCKWERFEN**: Tippst du, solange er hängt, fliegt er von deiner Schwertspitze zum Gegner.

Berührt ein Blitz die Klinge an einer anderen Stelle als der Spitze, fliegt er weiter. Nur die Spitze fängt. Dein Schwert kann immer nur einen Blitz halten.

**Seitenrand:**

- **ABKNICKEN** (Standard): Blitze prallen von den Seiten ab.
- **ERLÖSCHEN**: Blitze verschwinden am Seitenrand.

**Blitzgewitter:**

- Alle 15–30 Sekunden blinkt die Drohne, und ein Warnton erklingt (mit Stimme: „Warnung!“).
- Eine Sekunde später beginnt ein 3 Sekunden langes Gewitter. Die Drohne feuert neutrale Blitze in zufällige Richtungen, und niemand kann feuern.
- Fang die Blitze mit dem Schwert ab.
- Trifft dich ein neutraler Blitz, bekommt dein Gegner den Punkt.
- In der modernen Arena „Raumstation“ drehen sich während des Gewitters vier rote Rundumleuchten in den Ecken.

**Schild wächst nach:** Alle 5 Sekunden wächst ein zerstörter Block nach, zuerst in der vorderen Reihe.

**Es wird schneller:** Blitze werden alle 20 Sekunden einer Runde 10 % schneller, höchstens doppelt so schnell wie am Anfang.

**Ende des Matchs:**

- Der Siegerbildschirm zeigt den Sieger und den Endstand.
- Im Stil Modern hebt die Heldin ihr Schwert. Nach einer Niederlage gegen die KI kniet sie.
- Nach 1,5 Sekunden führt ein Tippen zurück zum Titelbildschirm.

## 6. Optionen

Jede Änderung wird sofort gespeichert. Einen „Speichern“-Knopf gibt es nicht.

**Jeder Grafikstil hat eigene Einstellungen.** Klassisch und Modern merken sich alle Optionen getrennt. Wechselst du den Stil, sind die Einstellungen wieder da, die du zuletzt in diesem Stil benutzt hast.

- Ein Tipp auf die **linke Hälfte** einer Zeile geht zum vorigen Wert zurück.
- Ein Tipp auf die **rechte Hälfte** geht vor.
- **ZURÜCK** führt zum Titelbildschirm.
- **STANDARD** setzt die Optionen des aktuellen Stils zurück. Einmal antippen, der Knopf zeigt „SICHER?“, dann innerhalb von 2 Sekunden noch einmal tippen. Der Grafikstil bleibt erhalten.

| Option | Werte | Standard | Hinweis |
|--------|-------|----------|---------|
| GRAFIKSTIL | Klassisch / Modern | Modern | Ändert nur Aussehen und Ton, nie die Regeln |
| ARENA | Original, Raumstation, Sternenflug, Arcade-Chrom, Energiekern, Hologramm, Lava-Schmiede | Original | Nur Modern. Animierte Arenen sind mit „ANIMIERT“ markiert. |
| WARNLEUCHTEN | An / Aus | An | Nur Modern, Raumstation: rote Rundumleuchten im Gewitter |
| SPRACHE | Automatisch / Deutsch / English | Automatisch | Automatisch nutzt Deutsch, wenn das Handy auf Deutsch steht, sonst Englisch |
| KI-STUFE | Leicht / Mittel / Schwer | Mittel | Reaktionszeit und Genauigkeit der KI |
| ANGEDOCKTER BLITZ | Verpufft / Zurückwerfen | Verpufft | Siehe Abschnitt 5 |
| BLITZFORM | Gleichmäßig / Zackig | Klassisch: Gleichmäßig, Modern: Zackig | Für jeden Stil getrennt gespeichert |
| SCHILDFORM | Gerade / Gebogen | Gerade | Gebogen: Das Schild bildet einen Bogen vor dem Kämpfer |
| SEITENRAND | Abknicken / Erlöschen | Abknicken | Siehe Abschnitt 5 |
| TV-EFFEKT | An / Aus | Aus | Look eines alten Fernsehers: Bildzeilen, Leuchten, dunklere Ecken |
| TON | An / Aus | An | Aus schaltet auch die Stimme ab |
| STIMME | Aus / Computer / Sanft | Computer | Sagt Countdown, „Warnung!“, „Treffer“, Sieg und Niederlage an |
| TITELMUSIK | An / Aus | An | Nur Modern |
| SPIELMUSIK | Aus / Orbit / Nebula / Hyperdrive / Sunset Run / Laser Nights / Pixel Pulse / Starlight | Orbit | Nur Modern. Beim Auswählen wird das Stück kurz angespielt. |

**Pausenmenü:** WEITER, HAUPTMENÜ sowie diese Einstellungen, die du während eines Matchs ändern kannst:

- Spielmusik (Modern)
- Stimme
- Warnleuchten (Raumstation)

## 7. Erweiterte Optionen (versteckt)

OPTIONEN im Titelbildschirm 1 Sekunde gedrückt halten. Diese Einstellungen verfeinern nur das Aussehen:

| Einstellung | Bereich | Standard |
|-------------|---------|----------|
| TV: ZEILEN-DUNKEL | 0–100 % | 50 % |
| TV: ZEILENBREITE | Schmal / Breit | Schmal |
| TV: LEUCHTEN | 0–100 % | 48 % |
| TV: LEUCHTWEITE | 0–10 % | 2 % |
| TV: WEICHHEIT | Scharf / Mittel / Weich | Mittel |
| TV: DUNKLE ECKEN | 0–100 % | 45 % |
| SCHWERT-LEUCHTEN | 0–200 % | 100 % |
| BLITZ-LEUCHTEN | 0–200 % | 100 % |
| RING-LEUCHTEN | 0–200 % | 100 % |
| AUGEN-LEUCHTEN | 0–200 % | 100 % |

- Die TV-Einstellungen wirken nur, wenn der TV-EFFEKT an ist.
- Die vier Leucht-Einstellungen wirken nur im Stil Modern.
- STANDARD setzt nur diese erweiterten Einstellungen zurück.

## 8. Tipps

- Ziele mit dem Schwertwinkel, aber behalte die Drohne im Blick. Sie bewegt sich ständig, deshalb zählt der Moment, in dem du tippst.
- Halte das Schwert auf die Blitze der Drohne gerichtet. Was du mit der Spitze fängst, schont dein Schild.
- Mit ZURÜCKWERFEN wird ein gefangener Blitz zu einem kostenlosen Gegenangriff.
- Dein Schild wächst nur langsam nach. Vermeide lange Serien von Blitzen auf dieselbe Stelle.

## 9. Hilfe bei Problemen

| Problem | Lösung |
|---------|--------|
| Kein Ton | TON in den Optionen und die Medienlautstärke des Handys prüfen |
| Keine Stimme | STIMME darf nicht Aus sein, und TON muss An sein |
| Keine Musik | Nur im Stil Modern; TITELMUSIK und SPIELMUSIK prüfen |
| Texte in der falschen Sprache | SPRACHE auf Deutsch oder English stellen |
| Das Spiel hat sich selbst pausiert | Es pausiert, wenn du die App verlässt oder ein Anruf kommt. WEITER antippen. |
| Update schlägt fehl | Erst die alte Version deinstallieren (siehe Abschnitt 1) |
