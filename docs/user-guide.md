# Lazer Arena – User Guide

Version 1.3 · German version: [Benutzerdokumentation.md](Benutzerdokumentation.md)

Lazer Arena is a duel in orbit. A drone floats between two fighters and fires lightning bolts at them. Each fighter turns a light sword to catch the bolts and decides when the drone fires at the opponent. The first to score 3 hits wins.

## 1. Installing

Lazer Arena is not in the Play Store. You install the APK file yourself ("sideloading").

1. Copy `LazerArena-1.3.apk` to your phone (USB cable, cloud storage or e-mail).
2. Open the file on the phone, for example in the "Files" app.
3. If Android asks, allow this app (for example "Files") to install unknown apps.
4. Tap "Install".

Requirements: Android 10 or newer. Portrait and landscape are supported. The game follows the phone's rotation.

**Updating:** Install the new APK over the old one. Your settings are kept. If Android refuses the update ("App not installed"), the new version was signed with a different key. Uninstall the old version first; your settings are then lost.

## 2. Title screen

| Button | Function |
|--------|----------|
| 1 PLAYER | You against the computer (CPU) |
| 2 PLAYERS | Two players on one phone |
| HOW TO PLAY | Animated tutorial (see below) |
| STYLE | Switches between CLASSIC (retro look of the 1980s) and MODERN (16-bit look with arenas and music) |
| OPTIONS | Settings (see section 6) |

**How to play:** Six short animated pages show the basics: turning the sword, firing, catching bolts, the shield, the lightning storm and scoring. Tap NEXT > and < BACK to turn the pages; DONE or the phone's back button returns to the title screen.

**Hidden menu:** Hold OPTIONS for one second. A soft voice says "Advanced options selected", and the advanced options open (section 7).

## 3. Goal of the game

- You stand at the bottom; the opponent (CPU or player 2) stands at the top.
- In front of each fighter is an **energy shield** made of 2 rows of 16 blocks.
- In the middle, the **drone** moves left and right.
- When you fire, the drone sends a bolt towards your opponent. **The bolt flies in the direction your sword is pointing**, starting from wherever the drone happens to be.
- A bolt that reaches a shield block destroys that block and disappears. A bolt that reaches the fighter is a **hit**.
- Each hit gives 1 point and starts a new round. **The first to reach 3 points wins the match.**

## 4. Controls

| Action | Gesture |
|--------|---------|
| Turn the sword | Swipe left or right. The sword turns between −60° and +60°. |
| Fire | Tap briefly |
| Pause | Tap the pause symbol (II) or press the phone's back button |

- **1 player:** You can use the whole screen.
- **2 players:** The lower half belongs to player 1 and the upper half to player 2. The top player's displays are upside down, so each player can read them from their side.

Rules for firing:

- After each shot you wait 0.8 s before the next one.
- Each player has at most 2 bolts in the arena at the same time.
- You cannot fire during the countdown or during a lightning storm.

## 5. What happens in the arena

**Countdown:** Each round starts with "3 … 2 … 1 … GO!". With the voice on, the game speaks it. Otherwise you hear three equal beeps and a higher one at GO.

**Catching bolts (docking):** If a bolt reaches the tip of your sword, it sticks there for 1 to 2 seconds and is harmless. Then:

- **FIZZLES** (default): it disappears.
- **THROW BACK**: tap while it is attached, and it flies from your sword tip towards the opponent.

A bolt that touches the blade anywhere other than the tip flies on. Only the tip catches. Your sword can hold only one bolt at a time.

**Side walls:**

- **BOUNCE** (default): bolts bounce off the sides.
- **VANISH**: bolts disappear at the side walls.

**Lightning storm:**

- Every 15–30 seconds the drone flashes and a warning sounds ("Warning!" with the voice on).
- One second later, a 3-second storm begins. The drone fires neutral bolts in random directions, and nobody can fire.
- Catch the bolts with your sword.
- If a neutral bolt hits you, your opponent gets the point.
- In the Modern arena "Space Station", four red rotating lights turn in the corners during the storm.

**Shield regrowth:** Every 5 seconds, one destroyed block grows back, front row first.

**Getting faster:** Bolts become 10 % faster every 20 seconds of a round, up to twice the starting speed.

**End of the match:**

- The victory screen shows the winner and the final score.
- In the Modern style, the heroine raises her sword. After a defeat against the CPU, she kneels.
- After 1.5 seconds, tap to return to the title screen.

## 6. Options

Every change is saved immediately. There is no "Save" button.

**Each graphics style has its own settings.** Classic and Modern remember all options separately. When you switch the style, the settings you last used with that style come back.

- Tap the **left half** of a row to go back to the previous value.
- Tap the **right half** to go forward.
- **BACK** returns to the title screen.
- **DEFAULTS** resets the options of the current style. Tap it once, the button shows "SURE?", then tap again within 2 seconds. The graphics style is kept.

| Option | Values | Default | Notes |
|--------|--------|---------|-------|
| GRAPHICS STYLE | Classic / Modern | Modern | Changes look and sound only, never the rules |
| ARENA | Original, Space Station, Space Flight, Arcade Chrome, Energy Core, Hologram, Lava Forge | Original | Modern only. Animated arenas are marked "ANIMATED". |
| WARNING LIGHTS | On / Off | On | Modern, Space Station only: red rotating lights during the storm |
| LANGUAGE | Automatic / Deutsch / English | Automatic | Automatic uses German if the phone is set to German, otherwise English |
| CPU LEVEL | Easy / Medium / Hard | Medium | Reaction time and accuracy of the CPU |
| DOCKED BOLT | Fizzles / Throw back | Fizzles | See section 5 |
| BOLT SHAPE | Regular / Irregular | Classic: Regular, Modern: Irregular | Saved separately for each style |
| SHIELD SHAPE | Straight / Curved | Straight | Curved: the shield forms an arc in front of the fighter |
| SIDE WALLS | Bounce / Vanish | Bounce | See section 5 |
| TV EFFECT | On / Off | Off | Old-TV look: scanlines, glow, darker corners |
| SOUND | On / Off | On | Off also mutes the voice |
| VOICE | Off / Computer / Soft | Computer | Speaks countdown, "Warning!", "Hit", victory and defeat |
| TITLE MUSIC | On / Off | On | Modern only |
| GAME MUSIC | Off / Orbit / Nebula / Hyperdrive / Sunset Run / Laser Nights / Pixel Pulse / Starlight | Orbit | Modern only. Choosing a track plays a short preview. |

**Pause menu:** RESUME, MAIN MENU, and these settings, which you can change during a match:

- Game music (Modern)
- Voice
- Warning lights (Space Station)

## 7. Advanced options (hidden)

Hold OPTIONS on the title screen for 1 second. These settings fine-tune the look only:

| Setting | Range | Default |
|---------|-------|---------|
| TV: LINE DARKENING | 0–100 % | 50 % |
| TV: LINE WIDTH | Thin / Wide | Thin |
| TV: GLOW | 0–100 % | 48 % |
| TV: GLOW SPREAD | 0–10 % | 2 % |
| TV: GLOW SOFTNESS | Sharp / Medium / Soft | Medium |
| TV: VIGNETTE (darker corners) | 0–100 % | 45 % |
| LIGHT SWORD GLOW | 0–200 % | 100 % |
| BOLT GLOW | 0–200 % | 100 % |
| STORM RING GLOW | 0–200 % | 100 % |
| DRONE EYE GLOW | 0–200 % | 100 % |

- The TV settings only have an effect when TV EFFECT is on.
- The four glow settings only affect the Modern style.
- DEFAULTS resets only these advanced settings.

## 8. Tips

- Aim with your sword angle, but watch the drone. It keeps moving, so the moment you tap matters.
- Keep your sword pointing at the drone's bolts. Catching them at the tip protects your shield.
- With THROW BACK, a caught bolt becomes a free counter-attack.
- Your shield grows back slowly. Avoid long series of bolts that hit the same spot.

## 9. Troubleshooting

| Problem | Solution |
|---------|----------|
| No sound | Check SOUND in the options and the phone's media volume |
| No voice | VOICE must not be Off, and SOUND must be On |
| No music | Modern style only; check TITLE MUSIC and GAME MUSIC |
| Text in the wrong language | Set LANGUAGE to Deutsch or English |
| The game paused by itself | It pauses when you leave the app or a call comes in. Tap RESUME. |
| Update fails | Uninstall the old version first (see section 1) |
