## The Mask Within 1.2 — Atmosphere update

The original house, menus, and escape route return with clearer visuals and new environmental scares.

- No motion blur, depth-of-field blur, grain, colour fringing, or lens distortion. Lower bloom, cooler colours, and sharper texture filtering.
- Extra footsteps after stopping, knocks behind you, slow light dips, brief masked-figure sightings, and a heartbeat during pursuit.
- Fixed carried-key recognition at the exit, interaction zones with overlapping player colliders, crank animation conflicts, hunter attack recovery, and the door-opening sound.
- Pause freezes gameplay and audio. The v1.1 menu and hiding-system rewrite is removed.

Extract **The.Mask.Within.Windows.zip** completely and run **The Mask Within.exe**. Move with WASD, look with the mouse, run with Shift, crouch with Ctrl, interact with E, hold E at the crank, and pause with Esc.

Verified in the Unity player: front-door objective, interrupted and completed cranking, key collection, exit unlock, win transition, fresh-run reset, pause/resume, hunter patrol and recovery, capture/death, light restoration, and apparition isolation. Scripted collision checks use Unity physics; they are not a full manual playthrough.

Known limitation: the original assets still produce cloth-initialisation diagnostics when a scene loads. The failed cloth simulation is removed during play while the rigged character remains. No new runtime errors or script exceptions were found in the checked routes. This is not a guarantee of bug-free play or a universal frame-rate claim.

The original v1.0 download remains available as a fallback.
