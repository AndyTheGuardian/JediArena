# Lazer Arena – Developer Guide

Version 1.3 · As of 2026-11-16 · German version: [Entwicklerdokumentation.md](Entwicklerdokumentation.md)

This guide explains how Lazer Arena is built, how to build and test it, and where to change things. The game rules themselves are in [REGELWERK.md](../REGELWERK.md) (German). Every rule has an ID such as `SP-3` or `OP-11`; code comments and test traits refer to these IDs.

## 1. Overview

Lazer Arena is an Android game written in C# with MonoGame. Two players (or one player against the computer) defend themselves with light swords against lightning bolts fired by a drone in the middle of the arena. It is an original game inspired by the Atari VCS game "Jedi Arena".

| Item | Value |
|------|-------|
| Language / runtime | C# on .NET 10 |
| Game framework | MonoGame 3.8.6.1 (DesktopGL and Android) |
| Android | `net10.0-android`, minimum Android 10 (API 29), package `de.lazerarena.app` |
| Tests | xUnit, 281 tests (74 game logic, 207 presentation) |
| Graphics styles | Classic (Atari VCS look, 120 px short side) and Modern (SNES look, 270 px short side, 256 colours out of 4096) |
| Orientation | Portrait and landscape |

## 2. Repository layout

```
LazerArena.slnx                 Solution
REGELWERK.md                    Game rules with IDs (German)
SPECS.md                        Original project brief
OFFENE-PUNKTE.md                Open points, assumptions, decisions
src/
  LazerArena.Core/              Game logic, no graphics, no MonoGame
    Ai/AiController.cs          Computer opponent (3 levels)
  LazerArena.Presentation/      Everything between logic and screen, no MonoGame
    Rendering/                  Software renderer for both styles, arenas, font, palettes
    Audio/                      Synthesised sounds and music, voice lines
    Input/                      Touch -> game commands
    Layout/                     Logical screen layout per style and orientation
    Localization/               Strings.de.json, Strings.en.json
    Settings/                   UserSettings, AdvancedSettings, JSON store
    Art/                        Pre-converted pixel images (.lzpx)
    GameSession.cs              One match: simulation plus display settings
  LazerArena.Game/              Shared MonoGame code (linked into Android and Desktop)
    LazerArenaGame.cs           Game loop, scaling, TV effect, input
    Screens.cs                  Title, options, game, victory screens
    SoundPlayer.cs              Plays sounds, voices and music
    Platform.cs                 Platform interface, touch input
  LazerArena.Android/           Android app (Activity, manifest, icons)
  LazerArena.Desktop/           Desktop version for quick tests (mouse = touch)
tests/
  LazerArena.Core.Tests/        Rule tests for the game logic
  LazerArena.Presentation.Tests/ Rendering, layout, audio, options, menus
tools/
  env.sh                        Environment variables for the build
  LazerArena.Snapshots/         Renders screenshots and preview frames as PNG
  voice/make_voice.py           Generates the voice lines (eSpeak NG)
  art/convert_art.py            Converts illustrations to pixel art (.lzpx)
  icon/make_icon.py             Generates the adaptive Android icon
  mockups/                      Scripts that produced the approved mockups
mockups/, snapshots/            Approved designs and current screenshots
sounds/                         Listening copies of voices and sounds
release/                        Built APKs (LazerArena-<version>.apk)
```

## 3. Architecture

The code is split into layers. Each layer only knows the layers below it.

```
LazerArena.Android / LazerArena.Desktop     platform start-up, file paths
        │
LazerArena.Game (shared source)             MonoGame: loop, textures, sound output, screens
        │
LazerArena.Presentation                     drawing into a pixel buffer, audio synthesis,
        │                                   layout, touch mapping, texts, settings
LazerArena.Core                             rules, simulation, AI (pure .NET)
```

Main design decisions:

- **Game logic without graphics.** `LazerArena.Core` has no reference to MonoGame or the renderer. All rules can be tested with plain unit tests.
- **Fixed time step.** The simulation runs at 60 Hz (SO-3) and uses a seeded random generator (SO-4). Tests are deterministic.
- **Software rendering.** Both styles draw into a `PixelCanvas` (an RGB array in logical pixels). MonoGame only uploads the finished image as a texture and scales it by a whole number. This keeps the pixel look exact and makes rendering testable without a GPU.
- **Style only changes the look** (SO-2). `ClassicRenderer` and `ModernRenderer` derive from `StyleRenderer`, which owns the common drawing order. Style, arena and glow settings never change the simulation.
- **Linked shared code.** `LazerArena.Game` is not a project. Android and Desktop include its files with `<Compile Include="..\LazerArena.Game\**\*.cs" />`.

### 3.1 Game logic (LazerArena.Core)

| Class | Responsibility | Rules |
|-------|----------------|-------|
| `ArenaSimulation` | One match: phases (countdown, playing, round over, match over), fixed step, events | RS-*, SO-3 |
| `Player` | Position, sword angle and turning (limited to 240°/s), cooldown | SP-* |
| `Bolt` | Lightning bolt: growth, side walls, docking, hits | BL-*, AD-* |
| `Drone` | Movement of the drone | SF-2 |
| `Shield`, `ShieldSegment` | 2 × 16 segments, straight or curved, regrowth | SF-3, SC-* |
| `LaserStorm` | Warning, storm, neutral bolts | LG-* |
| `GameConfig` | All numeric values (speeds, times, sizes) | – |
| `GameOptions` | Rule options chosen by the player | OP-4, OP-5, OP-6, OP-9 |
| `Ai/AiController` | Computer opponent using the same rules as a human | KI-* |

The simulation produces `GameEvent`s (hit, storm warning, countdown and so on). The presentation turns them into sounds and voice lines.

### 3.2 Presentation (LazerArena.Presentation)

- **`GameSession`** joins a simulation with display settings (bolt shape, glow values, AI level).
- **`StyleRenderer`** defines the drawing order of a game frame: background, storm banner, shields, players, bolts, drone, HUD, overlay (countdown, "GO!", pause menu). It also defines the menu screens: title, options, victory.
- **`ClassicRenderer`** draws flat colours in the VCS look.
- **`ModernRenderer`** draws the SNES look:
  - It uses one of seven arenas (`Arenas.cs`, class `SnesArena`).
  - Each arena has its own 256-colour palette (`ArenaPalettes.g.cs`, generated from the approved mockups; the space station adds 42 red tones for its warning lights).
  - The arena "Original" and the menus use the fixed `ModernPalette` (256 colours, 12-bit) with ordered dithering.
- **`PixelArt`** loads the pre-converted images for title, victory and defeat (`Art/*.lzpx`). They are drawn after the colour reduction, so they stay free of dithering. Their colours are already palette colours.
- **Tutorial** (`StyleRenderer.Tutorial.cs`, rule SO-6): six pages, each with a scripted demo scene (no real simulation) built from the style's own drawing blocks, a title and a wrapped text. Destroyed shield blocks in a scene are hidden through the renderer (`Visible`), because the shield state in Core is internal.
- **Classic shield blocks** (`ClassicRenderer.ShieldBlocks`): all blocks 3:1 (6 × 2 px portrait, 3 × 1 px landscape) with 1 px gaps and a row gap of one block height. `ScreenLayout.BlockGridWidth` makes the Classic field 16·k − 1 px wide so the blocks fill it evenly.
- **`BitmapFont`** is a 5 × 7 pixel font with whole and fractional scaling.
- **`TvEffect`** creates scanline and vignette textures. `ScanlineRows` and `CoarseFactor` put the Modern effect on the Classic grid (one line per Classic pixel row, glow 2.25 × coarser), so both styles look the same. The glow is applied in `LazerArenaGame`. The values come from `AdvancedSettings` (OP-15).
- **`Menus.cs`** defines menu contents:
  - `OptionsMenu`, `AdvancedMenu` and `PauseMenu` list the entries.
  - An `OptionEntry` is a label key, a function for the value key, and a step function. Menus are therefore data and can be tested without drawing.
- **`ScreenLayout`** computes the logical size and all areas (field, HUD, side panels, pause button) for style, orientation and player count.
- **`TouchInputMapper`** turns touches into commands:
  - Horizontal swipe turns the sword (180° per screen width).
  - A short tap (under 0.35 s, little movement) fires.
  - With two players, the lower half belongs to player 1 and the upper half to player 2.
- **`Localizer`** reads the JSON string files. A missing key returns the key itself. This is used on purpose for literal values like `48%`.
- **Audio:**
  - `ClassicSoundBank` imitates a VCS sound chip.
  - `ModernSoundBank` synthesises effects, and `MusicComposer` creates the title and game music. Each `Song` names its `Instrumentation`: Synthwave (saw pads, square lead), Chiptune (pulse waves, stepped triangle bass, noise drums) or ElectricPiano (FM electric piano, FM bell lead, sine bass, soft drums).
  - `Voice.cs` maps game events to voice lines.
  - Everything except the voice lines is generated at runtime.

### 3.3 MonoGame layer (LazerArena.Game)

- **`LazerArenaGame`**:
  - Reads touch and keyboard input and updates the current screen.
  - Uploads the `PixelCanvas` as a texture and scales it by a whole number.
  - Adds the TV effect (glow render target, scanlines, vignette) when it is switched on.
- **Screens (`IScreen`):**
  - `TitleScreen`: holding "Options" for 1 s opens the advanced options.
  - `OptionsScreen`: normal or advanced options, with the "Defaults" button.
  - `TutorialScreen`: the animated tutorial (third title item).
  - `PlayScreen`: the game, including the pause menu.
  - `VictoryScreen`.
  - A screen returns the next screen from `Update`.
- **`SoundPlayer`** plays effects, voices and music. "Sound off" mutes everything (OP-7). The voice can replace the beeps (OP-13).
- **`AppContext`** holds the current settings. Every change is saved immediately via `JsonSettingsStore`.

### 3.4 Settings

`UserSettings` is a C# record with all options. `SettingsProfiles` holds one complete `UserSettings` per graphics style plus the active style (OP-19); switching the style activates the other set. The file is JSON with enums as text. Older files with a single set are loaded into both sets.

| Platform | File |
|----------|------|
| Android | app-private folder, `settings.json` (LocalApplicationData) |
| Desktop (Linux) | `~/.config/LazerArena/settings.json` |
| Desktop (Windows) | `%APPDATA%\LazerArena\settings.json` |

A damaged or incompatible file is ignored. The game then starts with default values.

When you add a setting:

1. Add an `init` property with a default value to `UserSettings`.
2. Add an `OptionEntry` in `Menus.cs`.
3. Add the texts to both JSON files (the labels must fit the Classic style).
4. Write a test and add the rule to REGELWERK.md.

### 3.5 Android full screen

`Activity1.HideSystemBars` hides the status and navigation bar (immersive mode, rule SO-7). On Android 11+ it uses the `WindowInsetsController` with "show transient bars by swipe"; on Android 10 the older `SystemUiFlags`. It runs again on resume and whenever the window gets focus back.

The window may also draw into the camera cutout (`LayoutInDisplayCutoutMode.ShortEdges`). After every layout `AndroidPlatform.UpdateCutout` reads the cutout's top inset and whether it is centred (`Cutout.IsCentered`: all top cutouts within the middle 40 % of the width). A centred cutout is drawn over; `AppContext.Layout` puts its height in logical pixels into `ScreenLayout.SafeTop`, and centred titles use `TopBelowCutout`. A cutout outside the middle is reserved: the picture starts below it (`Cutout.Reserved`). `tools/LazerArena.Snapshots -- <dir> de --aussparung` renders the affected screens with a simulated hole on five screen sizes.

## 4. Setting up the environment

All tools live in the user's home directory. No root rights are needed.

| Tool | Location |
|------|----------|
| .NET 10 SDK | `~/.dotnet` |
| Android SDK | `~/Android/Sdk` (platform android-36, build tools 36) |
| JDK 17 | `~/Android/jdk/jdk-17*` |
| MonoGame | NuGet packages, restored automatically |
| Python 3 with Pillow | only for the tools in `tools/` |
| eSpeak NG (`libespeak-ng`) | only to regenerate voice lines |

Before every build:

```bash
source tools/env.sh
```

## 5. Building

### Desktop (quick testing)

```bash
dotnet run --project src/LazerArena.Desktop
```

```bash
dotnet run --project src/LazerArena.Desktop -- --landscape
```

| Key | Action |
|-----|--------|
| Mouse | Same as touch |
| Left / Right, Space or Up | Player 1: turn sword, fire |
| A / D, W | Player 2: turn sword, fire |
| Esc / Backspace | Back / pause |
| F12 | Save a screenshot |

### Android APK

A release build takes about 6 minutes on the development machine. Use `-m:1` and `UseSharedCompilation=false` to keep memory use low.

```bash
nice -n 10 dotnet build src/LazerArena.Android -c Release -m:1 -p:UseSharedCompilation=false -p:JavaSdkDirectory=$JAVA_HOME -p:AndroidSdkDirectory=$ANDROID_HOME
```

```bash
dotnet build-server shutdown
```

```bash
cp src/LazerArena.Android/bin/Release/net10.0-android/de.lazerarena.app-Signed.apk release/LazerArena-1.3.apk
```

Before a new release:

- Raise `ApplicationVersion` (integer) and `ApplicationDisplayVersion` in `LazerArena.Android.csproj`.
- Change the version text (`"V1.3"`) in `ClassicRenderer.RenderTitle` and `ModernRenderer.RenderTitle`.

The APK is currently signed with the automatic debug key (open point E8). Updates must be signed with the same key, otherwise the app has to be uninstalled first.

## 6. Tests

Run the test projects one at a time. `dotnet test` on the whole solution would also build the APK.

```bash
dotnet test tests/LazerArena.Core.Tests
```

```bash
dotnet test tests/LazerArena.Presentation.Tests
```

- Tests carry the rule ID as a trait, e.g. `[Fact, Trait("Regel", "RS-4")]`. Run all tests of one rule with `--filter "Regel=RS-4"`.
- Core tests check every rule of REGELWERK.md with fixed seeds.
- Presentation tests render real frames into a `PixelCanvas` and check pixels, for example:
  - colour limits (at most 256 colours)
  - equal blade widths
  - images drawn without dithering
  - layout rectangles inside the screen
  - texts that fit the pixel font
- Some tests write settings through the desktop store. Back up `~/.config/LazerArena/settings.json` first if you need your own settings.

## 7. Tools

| Tool | Purpose | Call |
|------|---------|------|
| Snapshots | Renders all screens of both styles as PNG (2 × size) | `dotnet run --project tools/LazerArena.Snapshots -- <dir> de` |
| Snapshots, arenas | Additionally all arenas | `... -- <dir> de --arenen` |
| Snapshots, animation | Frame sequence of a 2-player game from the storm warning, for GIF previews | `... -- <dir> de --frames SpaceStation 440 2` |
| Snapshots, tutorial | Frame sequences of all tutorial pages (for GIFs) and stills; `--ohne-bildfolge` writes the stills only | `... -- <dir> de --anleitung` |
| Snapshots, audio | Writes all sounds and music as WAV | `... -- <dir> --audio` |
| Voice lines | Generates the WAV files in `Audio/Voice` | `python3 tools/voice/make_voice.py --only=Warning --profile=soft` |
| Pixel art | Converts `tools/art/*-quelle.png` to `Art/*.lzpx` plus previews in `mockups/kunst/` | `python3 tools/art/convert_art.py` |
| User manual | Builds `docs/user-manual.pdf` and `docs/Benutzerhandbuch.pdf` from the user guides, with screenshots in each language (needs LibreOffice; screenshots are cached in `tools/docs/build/`) | `python3 tools/docs/make_manual.py` |
| Icon | Builds the adaptive Android icon from `tools/icon/icon-quelle.jpg` | `python3 tools/icon/make_icon.py` |

After changes to the look, delete `tools/docs/build/` so the manual gets new screenshots.

Important: always use `--only=` and `--profile=` for new voice lines. Regenerating everything overwrites approved recordings.

### LZPX image format

| Bytes | Content |
|-------|---------|
| 0–3 | `LZPX` |
| 4–5 | Width (uint16, little endian) |
| 6–7 | Height (uint16, little endian) |
| 8… | One index per pixel (uint16 LE) into `ModernPalette.Colors`; `0xFFFF` = transparent |

There is one file per orientation (`-hoch` = portrait, `-quer` = landscape), so images are never scaled in the game.

## 8. Typical changes

| Task | Where |
|------|-------|
| Change a rule value (speed, times) | `GameConfig` in Core, then adjust tests and REGELWERK.md |
| New text | Both `Strings.*.json`; only characters supported by `BitmapFont` (a test checks this) |
| New option | `UserSettings`, `Menus.cs`, texts, test, REGELWERK.md |
| New arena | Class derived from `SnesArena` in `Arenas.cs`, palette in `ArenaPalettes.g.cs`, `ArenaKind` value, text `arena.<Name>` |
| New sound | `SoundId`, both sound banks, mapping in `SoundEvents` / `AudioCue.From` |
| New image | Source PNG in `tools/art`, entry in `TARGETS` of `convert_art.py`, value in `ArtId` |

## 9. Known limits

- Voice lines come from eSpeak NG (formant synthesis). A natural voice would need a speaker or a licensed neural voice (open point F23).
- The release signing key is still missing (E8).
- The icon and the title, victory and defeat images come from AI illustrations supplied by the project owner, who confirmed the rights (E10, E12).
- The current list of open points is in [OFFENE-PUNKTE.md](../OFFENE-PUNKTE.md).
