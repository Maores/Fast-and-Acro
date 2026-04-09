# Gameplay Polish Design Spec — Fast and Acro

## Overview
Add juice, 3D assets, and game-feel improvements to make the game feel polished and fun.

## Items

### Juice (Code)
1. **Screen Shake** — CameraFollow gets shake on collision (decreasing sine wave, 0.3s, configurable intensity)
2. **Coin Collect Particles** — Gold particle burst at coin position on pickup (ParticleSystem prefab, pooled)
3. **Speed Lines Overlay** — Screen-space particle effect when speed > 80% of maxSpeed (streaking white lines)
4. **Damage Flash** — Full-screen red Image overlay, alpha 0→0.4→0 over 0.25s on hit

### 3D Models (Tripo)
5. **3D Coin** — Spinning gold coin model replacing flat disc, continuous Y-axis rotation
6. **New Obstacles** — Traffic cone, jersey barrier, barrel (construction theme, replace/augment existing)
7. **Roadside Props** — Static decoration meshes placed along road edges (cones, barriers, signs)

### Game Feel
8. **Near-Miss Bonus** — Raycast/overlap check when obstacle passes car within 0.5 units without collision; triggers "+10 CLOSE CALL!" floating text and score bonus
9. **Difficulty Ramp** — First 15% of track is obstacle-free; obstacle density increases linearly from 50% to 100% over the remaining track

## Architecture
- All new scripts go in `Assets/_Project/Scripts/Gameplay/` or `Assets/_Project/Scripts/UI/`
- Particle prefabs in `Assets/_Project/Prefabs/VFX/`
- New 3D models from Tripo in `Assets/_Project/Models/`
- Minimal coupling: each feature is self-contained, wired via SerializeField or events

## Parallel Work Packages
- **WP1 (Juice)**: Items 1-4 — screen shake, particles, speed lines, damage flash
- **WP2 (3D Models)**: Items 5-7 — coin model, obstacles, roadside props via Tripo
- **WP3 (Game Feel)**: Items 8-9 — near-miss detection, difficulty ramp tuning
