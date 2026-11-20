# The Mask Within

It is midnight and someone is moving through your house. You can hear him coming. He can hear you turning the crank.

**[Download the Windows game](https://github.com/deepgotnochill-cmd/the-mask-within/releases/download/v1.1.0/The.Mask.Within.Windows.zip)** · [Release notes](RELEASE.md) · [Original school-fair edition](https://github.com/deepgotnochill-cmd/the-mask-within/releases/tag/v1.0.0)

Extract the whole ZIP, open **The Mask Within**, and run **The Mask Within.exe**. The game runs offline.

## Survive the night

Start by finding the front door. When the route changes, watch the objective in the corner. The stair crank catches at each third of a turn. You can let go at a catch and come back later if footsteps get close.

A wall or corner can buy you enough time to slip under a bed. Try that while the hunter can still see you and he will remember where you went.

| Control | Action |
| --- | --- |
| W A S D / Mouse | Move / look |
| Left Shift / Left Ctrl | Run / crouch |
| E | Interact, take a key or unlock a door |
| Hold E | Turn the crank |
| H beside a bedroom bed | Hide or get out |
| Hold left mouse | Carry an object, release to drop it |
| Esc | Pause or resume |

## About the project

I created The Mask Within during my school years as one of my early experiments with Unity and AI-assisted development. I worked on the concept, game design, direction, and testing. The Windows edition builds on that same house and its escape puzzle.

**Deep Datta Chaudhury**

## Build the Windows edition

The C# files in [Gameplay](Gameplay) run inside the original Unity 2022.3.20f1 Mono player. Its four scenes, art and audio are distributed in the v1.0.0 Windows release. To rebuild on Windows with PowerShell and the .NET 8 SDK or later, download the original release ZIP and run this from the repository:

```powershell
./scripts/Build-Windows.ps1 -BaseZip ./The.Mask.Within.zip -OutputDirectory ./Builds/Windows
```

Choose a new or empty output folder. The script checks the original ZIP against its published SHA-256, extracts the runtime files, and compiles the gameplay against the libraries shipped with the game.
