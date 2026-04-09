# Session Plan — Phase 3B: Content & Features

**Created:** 2026-04-05
**Goal:** Make Fast and Acro feel like a complete mobile game before Android build

## What You'll End Up With
A polished mobile racing game with: level select screen, coin/score system, power-ups, car skins, background music, and visual variety per level — ready for Android APK export.

## Feature Priorities (Ordered by Impact)

### Tier 1 — Must-Have (Game feels complete)

**1. Level Select Screen**
- Grid of level buttons with star ratings displayed
- Locked/unlocked state (unlock next after completing previous)
- Shows best stars per level from PlayerPrefs
- Replace direct "Play" with level select flow
- Files: new `LevelSelectUI.cs`, modify `UIController.cs`, `GameManager.cs`

**2. Coin/Score System**
- Collectible coins scattered on the track
- Coin counter on HUD
- Persistent total coins in PlayerPrefs
- Coin pickup particle effect + sound
- Files: new `Coin.cs`, `CoinPool` in ObjectPool, modify `LevelManager.cs`, `UIController.cs`

**3. Power-Ups**
- Shield (temporary invincibility) — glowing effect
- Speed Boost — FOV zoom + trail effect
- Magnet (attract coins) — visual pull effect
- Spawn on track like coins, random placement
- Files: new `PowerUp.cs`, `PowerUpEffect.cs`, modify `CarController.cs`, `CollisionHandler.cs`

**4. Background Music**
- 1 menu track + 1 gameplay track (procedural WAV)
- Smooth fade between tracks
- Volume control
- Files: modify `AudioManager.cs`

### Tier 2 — Should-Have (Game feels polished)

**5. Car Skins/Colors**
- 4-5 car color presets (blue, red, green, yellow, purple)
- Unlock with coins
- Selection in main menu or level select
- Files: new `CarSkinManager.cs`, modify `GameManager.cs`

**6. Visual Variety Per Level**
- Level 1 (City): gray road, blue sky — current look
- Level 2 (Japan): darker road, sunset sky, cherry blossom fog color
- Level 3 (Brazil): warm road, tropical sky, green fog
- Change road color, sky color, fog color, ambient light per level
- Files: add color fields to `LevelData.cs`, modify `LevelManager.cs`

**7. Speed Ramp / Difficulty Curve**
- Car accelerates slightly as it progresses through a level
- Later levels start faster
- Add `startSpeed` and `speedIncrement` to LevelData
- Files: modify `LevelData.cs`, `CarController.cs`

### Tier 3 — Nice-to-Have (Extra polish)

**8. Endless/Survival Mode**
- Procedural obstacle spawning
- Score = distance traveled
- Unlocked after completing all 3 levels
- Files: new `EndlessMode.cs`

**9. Simple Tutorial**
- First-time overlay showing swipe controls
- "Swipe left/right to dodge"
- Files: new `TutorialOverlay.cs`

**10. Haptic Feedback**
- Vibrate on collision (mobile only)
- Light vibrate on lane switch
- Files: modify `CollisionHandler.cs`, `CarController.cs`

## Recommended Build Order

```
Step 1: Level Select Screen          (~30 min)
Step 2: Coin System                  (~25 min)
Step 3: Background Music             (~15 min)
Step 4: Visual Variety Per Level     (~20 min)
Step 5: Power-Ups (Shield + Boost)   (~30 min)
Step 6: Car Skins                    (~20 min)
Step 7: Speed Ramp                   (~10 min)
Step 8: Tutorial Overlay             (~10 min)
```

## Phase Weights
- Discover: 5% — We know the codebase well
- Define: 10% — Requirements are clear from this plan
- Develop: 70% — Heavy implementation
- Deliver: 15% — Testing on device

## Success Criteria
- [ ] Level select screen with star display and unlock progression
- [ ] Coins collectible on track with persistent total
- [ ] At least 2 power-up types functional
- [ ] Background music plays during menu and gameplay
- [ ] Visual distinction between levels (sky/road colors)
- [ ] Zero compile errors, smooth 60fps gameplay

## Execution
To execute: `/octo:embrace "Phase 3B features"`
Or build features one at a time with `/octo:develop`
