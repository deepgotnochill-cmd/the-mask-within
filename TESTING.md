# Verification for 1.2

Checks run in an isolated copy of the actual Windows player with the original scenes and baked navigation. Test input substitutes the E key; player placement exercises Unity's real trigger callbacks.

Passed routes:

- Original title button loads the house; pause and resume restore the player correctly.
- Front-door objective, interrupted crank, completed crank, revealed stairs and key.
- Crank and key remain usable when only one of the player's two colliders leaves the zone.
- Collected key unlocks the exit, crossing the exit loads the win scene, and a fresh run resets the puzzle.
- Hunter starts on the baked navigation, patrols, recovers after its attack state, and captures through actual collision.
- Motion blur and grain disabled; light events restore the original fixture intensity.
- One atmosphere controller after restarting; apparition has no collision, navigation, or hunter script.
- No new engine errors or script exceptions in the checked routes.

## Limits

Original cloth assets log `CollisionMeshData` / `Cannot initialize Cloth` diagnostics on scene load. Those exact baseline diagnostics are counted separately. The update removes the failed simulation during play and retains the character's skinned renderer. Eight baseline diagnostics occurred across the two house loads in the scripted check.

Hidden-window tests can skip rendering, so their frame-time measurements must not be used as an FPS claim. Visual inspection uses a visible window. Neither a short measurement nor the scripted route is an exhaustive benchmark or a full manual playthrough.

The visible review produced the hallway screenshot and confirmed the rigged figure appears correctly, but reached the test harness's 140-second timeout before completing its timing sample. Performance remains unverified; the completed gameplay result above comes from the separate scripted run.

## Reproduce

Install a .NET SDK and download the original v1.0 ZIP, then run:

```powershell
./scripts/Test-Windows.ps1 -BaseZip ./The.Mask.Within.zip -OutputDirectory ./qa
```

Choose an empty output directory. The script builds an isolated test player. Release builds include only `Gameplay/*.cs`, never the test harness.
