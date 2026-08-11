# VS Item Tooltips

A [BepInEx (Bleeding Edge)](https://builds.bepinex.dev/projects/bepinex_be) mod for [Vampire Survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/) that adds rich, interactive tooltips to weapon and item icons throughout the game. Supports mouse, keyboard, and controller with full navigation.

If you enjoy this mod, consider supporting me!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/nihilxd)

**[Download the latest release here](https://github.com/NihilXD/VSEvolutionHelper/releases/latest)**

## Supported Screens

Tooltips work on every screen where weapon/item icons appear:

- **Level-up screen** — hover or navigate to weapon/item choices to see what they evolve into before picking
- **Pause screen (inventory)** — browse your equipped weapons and accessories with controller/keyboard equipment selection
- **Collection screen** — explore the full item catalog with tooltips
- **Merchant screen** — see evolution info before purchasing
- **Weapon selection (Arma Dio)** — tooltips on the weapon pick screen

## Features

- **Evolution formulas** — Each tooltip shows all evolution paths: which weapon + passive combines into which evolved weapon
- **Applicable arcanas** — Tooltips display arcanas that affect the item
- **Ownership indicators** — Items you currently own are highlighted with a golden circle in evolution formulas
- **Banned item indicators** — Banished weapons/items show a red X overlay in evolution formulas
- **MAX level indicators** — Passives that require max level to trigger an evolution are labeled
- **Recursive popups** — Click or select icons inside a tooltip to open nested tooltips, letting you explore the full evolution tree
- **"Evolved from" section** — Evolved weapons show which base items they came from
- **Localized descriptions** — Item descriptions use the game's localization system, supporting all languages
- **Full input support** — Mouse hover, keyboard navigation, and controller navigation all fully supported

## Installation

1. Install the latest [BepInEx Bleeding Edge (IL2CPP) build](https://builds.bepinex.dev/projects/bepinex_be) for Vampire Survivors
2. Download `VSEvolutionHelper.dll` from the [latest release](https://github.com/NihilXD/VSEvolutionHelper/releases/latest)
3. Copy the DLL to your Vampire Survivors `BepInEx/plugins` folder:
   ```
   <Steam>/steamapps/common/Vampire Survivors/BepInEx/plugins/VSEvolutionHelper.dll
   ```
4. Launch the game

## Controls

### Mouse
- Hover an icon to show its tooltip
- Hover icons inside the tooltip to open nested tooltips
- Move the mouse away to close

### Controller
- **Y** — Enter tooltip navigation (level-up/collection screens) or equipment selection mode (pause screen); go back one tooltip level when inside a recursive tooltip
- **D-pad** — Navigate between items or formula icons
- **A** — Open a nested tooltip on the selected formula icon
- **B** — Close the tooltip entirely, or exit navigation mode
- Tooltips appear automatically after dwelling on an item for 0.5 seconds

### Keyboard
- **Tab** — Enter tooltip navigation or equipment selection mode
- **Arrow keys** — Navigate between items or formula icons
- **Space/Enter** — Open a nested tooltip on the selected formula icon
- **Backspace** — Go back one tooltip level, or exit navigation mode

## Compatibility

- Vampire Survivors v1.14+
- BepInEx Bleeding Edge (IL2CPP)
- Works with all DLCs

## Known Issues

- Tooltips on "Map Item Pickups" (items found physically sitting on the map rather than from chests or level-ups) may not display reliably due to the game's UI hierarchy layering invisible, full-screen hit-blockers over those specific windows.

## Building from Source

Requires .NET SDK and a local Vampire Survivors installation with BepInEx (for assembly references).

```bash
dotnet build VSEvolutionHelper.csproj
```

Output: `bin/Debug/netstandard2.1/VSEvolutionHelper.dll`
