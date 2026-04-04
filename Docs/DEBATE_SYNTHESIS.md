# Architecture Debate Synthesis

**Date:** 2026-04-05
**Participants:** Claude Backend Architect (completed), UI/Input Specialist (pending), Performance Engineer (pending)

---

## Top Findings from Backend Architect Review

### CRITICAL — Plan Changes Required

#### 1. Too Many Scripts for MVP (19 → ~12)
**Problem:** 19 scripts is over-engineered for a beginner under time pressure. The wiring overhead (events, singletons, cross-references) will delay getting anything visible on screen.
**Fix:** Merge UI scripts (UIManager + HUD + screens → single UIController). Drop standalone Singleton<T> and GameEvents. Target 10-12 scripts.

#### 2. Drop Singletons — Use Inspector References
**Problem:** 4 singletons introduce initialization order bugs, DontDestroyOnLoad duplication, and global mutable state that's hard to debug. For a single-scene MVP, they add no value.
**Fix:** Put all managers on scene GameObjects. Wire dependencies via [SerializeField] Inspector references. Zero singletons for MVP.

#### 3. Replace Static Events with Direct Calls
**Problem:** Static Action events hide the "who calls what" graph. A beginner can't see subscriptions in the Inspector. Forgotten unsubscribes cause silent bugs on scene reload.
**Fix:** Use direct method calls (CollisionHandler calls ScoreManager.AddCollision() directly). Or use UnityEvents for Inspector-visible wiring. Refactor to events only when script count exceeds 30+.

#### 4. Use Lane-Based Movement Instead of Free Horizontal
**Problem:** Free movement on mobile requires dead zone handling, per-DPI sensitivity, thumb-occlusion avoidance. Subway Surfers, Temple Run, etc. use lanes for good reason.
**Fix:** 3 lanes with swipe/tap to switch. Simplifies input, collision detection, and mobile UX in one decision. Can always change to free movement later.

#### 5. Defer Face Customization
**Problem:** 3 scripts + NativeGallery plugin dependency for a cosmetic feature. Introduces Android permissions, platform build config, and memory management (4K gallery photos → Texture2D). Highest risk, lowest value.
**Fix:** Use preset face sprites for MVP. Add gallery upload only after core loop (drive, dodge, score) is polished.

### IMPORTANT — Missing Systems

#### 6. No Camera System
**Problem:** Plan doesn't mention camera follow behavior.
**Fix:** Use Cinemachine (free, included with Unity). Virtual Camera following car with look-ahead. ~5 minutes of setup.

#### 7. No Audio
**Problem:** Zero sound = broken feeling arcade game.
**Fix:** Add minimal AudioManager with one-shot sound effects (collision, engine hum, UI clicks, level complete). Even placeholder sounds improve feel dramatically.

#### 8. No Object Pooling
**Problem:** Instantiate/Destroy on obstacles causes GC spikes on Android.
**Fix:** Simple pool of 15-20 obstacle instances that reset position when passed.

#### 9. No Initialization Sequence
**Problem:** Unity doesn't guarantee Awake() order across GameObjects. Manager cross-references can null-ref.
**Fix:** Use [DefaultExecutionOrder] attributes, or a single Bootstrapper script that initializes managers in explicit order.

#### 10. Mobile Specifics Missing
- Set `Application.targetFrameRate = 60` explicitly
- Tune `Time.fixedDeltaTime` to 0.04 (reduce physics cost)
- Handle `Screen.safeArea` for notched devices
- Single-scene approach (no scene loading for MVP)

---

## Revised Architecture Recommendation

### Before (Original Plan): 19 scripts, 4 singletons, static events
### After (Debate-Refined): ~12 scripts, 0 singletons, direct references

| Original Script | Verdict | Revised |
|----------------|---------|---------|
| Singleton<T> | DROP | Use Inspector references |
| GameEvents | DROP | Direct method calls |
| GameConfig | KEEP | ScriptableObject, unchanged |
| GameManager | KEEP | No singleton, scene-based |
| CarController | MODIFY | Lane-based movement, 3 lanes |
| CollisionHandler | KEEP | Unchanged |
| Obstacle | KEEP | Add object pooling |
| LevelManager | KEEP | Add obstacle pooling logic |
| LevelData | KEEP | ScriptableObject, unchanged |
| FinishLine | KEEP | Unchanged |
| ScoreManager | MERGE | Merge into GameManager |
| UIManager | MERGE | → UIController (all panels) |
| HUDController | MERGE | → into UIController |
| MainMenuUI | MERGE | → into UIController |
| LevelCompleteUI | MERGE | → into UIController |
| GameOverUI | MERGE | → into UIController |
| FaceCustomizationManager | DEFER | MVP: preset sprites only |
| ImageLoader | DEFER | MVP: not needed |
| FaceBillboard | SIMPLIFY | Just show preset sprite |
| — (NEW) | ADD | CameraFollow.cs |
| — (NEW) | ADD | AudioManager.cs |
| — (NEW) | ADD | ObjectPool.cs |

### Final Script List (~12 scripts):
1. GameConfig.cs (ScriptableObject)
2. LevelData.cs (ScriptableObject)
3. GameManager.cs (state machine + scoring)
4. CarController.cs (lane-based movement)
5. CollisionHandler.cs (HP + damage)
6. Obstacle.cs (behavior)
7. ObjectPool.cs (reusable pool)
8. LevelManager.cs (level flow + pooling)
9. FinishLine.cs (trigger)
10. UIController.cs (all panels + HUD)
11. CameraFollow.cs (or use Cinemachine)
12. AudioManager.cs (one-shot SFX)
13. FaceDisplay.cs (preset sprites, upgradeable later)
