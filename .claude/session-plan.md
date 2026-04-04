# Session Plan — Fast and Acro MVP

**Created:** 2026-04-05
**Intent Contract:** See .claude/session-intent.md

---

## What You'll End Up With

A complete, buildable Unity MVP for "Fast and Acro" — an arcade auto-driving racing game with:
- All C# scripts for the 9 MVP features
- Clean folder structure ready to drop into Unity 2022 LTS+ with URP
- Touch-based left/right controls, auto-forward movement
- HP-based collision system, star scoring, basic UI flow
- Face customization (gallery image on car)
- Step-by-step explanations for each component

---

## How We'll Get There

### Phase Weights
- **Discover: 15%** — Validate architecture patterns for Unity mobile
- **Define: 20%** — Lock folder structure, script interfaces, data flow
- **Develop: 45%** — Implement all 9 MVP features as C# scripts
- **Deliver: 20%** — Review quality, mobile optimization, integration

---

## Phase 1: DISCOVER (15%) — Architecture Validation

**Goal:** Confirm the right patterns for a Unity mobile arcade game.

### Key Decisions to Lock Down:
1. **Game State Machine** — Enum-based state machine in GameManager (Menu → Playing → Paused → LevelComplete → GameOver)
2. **Input System** — Unity's new Input System vs. legacy `Input.GetTouch()` → Use legacy for MVP simplicity
3. **Car Movement** — Rigidbody-based physics vs. Transform-based → Rigidbody for collision detection
4. **Lane System** — Free horizontal movement vs. discrete lanes → Free movement with clamped bounds (more fun, simpler)
5. **UI Framework** — Unity UI (Canvas) with screen-space overlay for HUD
6. **Face Customization** — NativeGallery plugin for gallery/camera access on mobile

### Architecture Pattern:
```
Singleton Managers (GameManager, UIManager, ScoreManager)
    ↓ Events
MonoBehaviour Components (CarController, CollisionHandler, Obstacle)
    ↓ ScriptableObjects
Data/Config (GameConfig, LevelData)
```

---

## Phase 2: DEFINE (20%) — Architecture & Structure

**Goal:** Define every script, its responsibility, and how they connect.

### Unity Project Folder Structure:
```
Assets/
  _Project/
    Scripts/
      Core/
        GameManager.cs          — Game state machine, level flow
        GameConfig.cs           — ScriptableObject: global settings
        GameEvents.cs           — Static event bus (Actions)
      Gameplay/
        CarController.cs        — Auto-forward + left/right input
        CollisionHandler.cs     — HP tracking, damage, invincibility frames
        Obstacle.cs             — Obstacle behavior (static/moving)
        LevelManager.cs         — Level setup, finish line detection
        LevelData.cs            — ScriptableObject: level config
        FinishLine.cs           — Trigger zone for level completion
      UI/
        UIManager.cs            — Screen management (show/hide panels)
        HUDController.cs        — HP bar, score, timer display
        MainMenuUI.cs           — Start screen
        LevelCompleteUI.cs      — End screen with stars
        GameOverUI.cs           — Failure screen
      Scoring/
        ScoreManager.cs         — Star calculation logic
        ScoreData.cs            — Per-level score tracking
      FaceCustomization/
        FaceCustomizationManager.cs — Gallery/camera picker flow
        ImageLoader.cs          — Native image → Texture2D utility
        FaceBillboard.cs        — Display texture on car, billboard behavior
      Utils/
        Singleton.cs            — Generic singleton base class
    Prefabs/
      Car.prefab
      Obstacle_Static.prefab
      Obstacle_Moving.prefab
      FinishLine.prefab
    Scenes/
      MainMenu.scene
      Level_01.scene
    Materials/
      Car_Material.mat
      Road_Material.mat
      Obstacle_Material.mat
    ScriptableObjects/
      GameConfig.asset
      Level_01_Data.asset
    UI/
      MainMenuCanvas.prefab
      HUDCanvas.prefab
      LevelCompleteCanvas.prefab
      GameOverCanvas.prefab
```

### Script Dependency Map:
```
GameManager (Singleton)
  ├── Listens: GameEvents.OnLevelComplete, GameEvents.OnPlayerDied
  ├── Controls: UIManager, ScoreManager, LevelManager
  └── State: Menu → Playing → Paused → LevelComplete → GameOver

CarController (on Car prefab)
  ├── Reads: GameConfig (speed, lane bounds)
  ├── Input: Touch position → horizontal movement
  └── Physics: Rigidbody forward velocity + horizontal lerp

CollisionHandler (on Car prefab)
  ├── Reads: GameConfig (max HP, invincibility duration)
  ├── Fires: GameEvents.OnDamageTaken, GameEvents.OnPlayerDied
  └── Manages: HP, invincibility frames, damage feedback

LevelManager (in scene)
  ├── Reads: LevelData (obstacles, length, time targets)
  └── Fires: GameEvents.OnLevelComplete

ScoreManager (Singleton)
  ├── Listens: GameEvents.OnDamageTaken, GameEvents.OnLevelComplete
  └── Calculates: time, collisions → 1-3 stars

UIManager (Singleton)
  ├── Manages: Panel visibility (Menu, HUD, LevelComplete, GameOver)
  └── Listens: GameEvents (state changes)

FaceCustomizationManager (Singleton)
  ├── Uses: ImageLoader (NativeGallery)
  └── Applies: Texture2D → FaceBillboard on car
```

### Event Bus (GameEvents.cs):
```csharp
public static event Action OnGameStart;
public static event Action OnGamePause;
public static event Action OnGameResume;
public static event Action<int> OnDamageTaken;      // remaining HP
public static event Action OnPlayerDied;
public static event Action<float> OnLevelComplete;   // completion time
public static event Action<int> OnScoreCalculated;   // star count
```

---

## Phase 3: DEVELOP (45%) — Implementation

**Goal:** Write all C# scripts, organized in build order.

### Build Order (13 Steps):

#### Step 1: Foundation Layer
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 1.1 | `Singleton<T>.cs` | Generic singleton base class | P0 |
| 1.2 | `GameEvents.cs` | Static event bus | P0 |
| 1.3 | `GameConfig.cs` | ScriptableObject with all game settings | P0 |

#### Step 2: Core Game Loop
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 2.1 | `GameManager.cs` | State machine, level flow orchestration | P0 |
| 2.2 | `LevelData.cs` | ScriptableObject for level configuration | P0 |

#### Step 3: Car & Movement
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 3.1 | `CarController.cs` | Auto-forward + touch left/right | P0 |

#### Step 4: Collision & HP
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 4.1 | `CollisionHandler.cs` | HP system, damage, invincibility | P0 |
| 4.2 | `Obstacle.cs` | Obstacle behavior | P0 |

#### Step 5: Level Flow
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 5.1 | `LevelManager.cs` | Level setup and flow | P0 |
| 5.2 | `FinishLine.cs` | Level completion trigger | P0 |

#### Step 6: Scoring
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 6.1 | `ScoreManager.cs` | Star calculation | P0 |

#### Step 7: UI System
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 7.1 | `UIManager.cs` | Panel management | P0 |
| 7.2 | `HUDController.cs` | In-game HUD | P0 |
| 7.3 | `MainMenuUI.cs` | Start screen | P0 |
| 7.4 | `LevelCompleteUI.cs` | End screen with stars | P0 |
| 7.5 | `GameOverUI.cs` | Failure screen | P0 |

#### Step 8: Face Customization
| # | Script | Purpose | Priority |
|---|--------|---------|----------|
| 8.1 | `ImageLoader.cs` | Gallery/camera → Texture2D | P1 |
| 8.2 | `FaceCustomizationManager.cs` | Image picker flow | P1 |
| 8.3 | `FaceBillboard.cs` | Display face on car | P1 |

### Implementation Details Per Step:

**Step 1 — Foundation:**
- `Singleton<T>` using `DontDestroyOnLoad`, lazy initialization
- `GameEvents` with static C# events (no UnityEvents for performance)
- `GameConfig` ScriptableObject with fields: moveSpeed, laneWidth, maxHP, invincibilityDuration, star thresholds

**Step 3 — Car Movement:**
- Rigidbody with constant forward velocity (`rb.linearVelocity`)
- Touch input: horizontal screen position mapped to lane position
- Clamped within road bounds from GameConfig
- Smooth horizontal movement via `Mathf.Lerp`

**Step 4 — Collision:**
- `OnCollisionEnter` or `OnTriggerEnter` with obstacle tag check
- HP decrement + invincibility window (0.5-1s)
- Visual feedback: car flashes during invincibility
- Fire `GameEvents.OnDamageTaken(remainingHP)` and `OnPlayerDied` at HP=0

**Step 6 — Scoring:**
- Formula: `stars = 3 - collisionPenalty - timePenalty`
- Collision penalty: 0 hits = 0, 1-2 hits = 0.5, 3+ hits = 1
- Time penalty: under target = 0, over by <20% = 0.5, over by >20% = 1
- Clamp to 1-3 stars

**Step 8 — Face Customization:**
- Use `NativeGallery` plugin for Android/iOS gallery access
- `NativeCamera` for camera capture
- Load image as `Texture2D`, resize to 256x256 for performance
- Apply to a Quad/Plane positioned above car with billboard script

---

## Phase 4: DELIVER (20%) — Quality & Optimization

**Goal:** Ensure mobile-ready, clean, documented code.

### Quality Checklist:
- [ ] All scripts compile without errors
- [ ] Naming follows CLAUDE.md conventions (PascalCase methods, _camelCase privates)
- [ ] [SerializeField] used instead of public fields
- [ ] No Update() abuse — event-driven where possible
- [ ] Object references via [SerializeField], not Find/GetComponent in Update
- [ ] Mobile touch input tested for edge cases (multi-touch, screen edges)
- [ ] HP system handles edge cases (double damage during invincibility)
- [ ] UI scales properly (Canvas Scaler set to Scale With Screen Size)
- [ ] GC-friendly: no allocations in hot paths (Update, FixedUpdate)
- [ ] Comments only where logic isn't self-evident

### Mobile Optimization:
- [ ] Rigidbody interpolation enabled for smooth visuals
- [ ] Physics layers configured (car vs obstacles only)
- [ ] UI uses sprite atlas for batching
- [ ] Face texture capped at 256x256
- [ ] No unnecessary `Debug.Log` in production

---

## Execution Commands

To execute this plan, run:
```
/octo:embrace "Build Fast and Acro MVP"
```

Or execute phases individually:
- `/octo:discover` — Validate architecture patterns
- `/octo:define` — Lock structure and interfaces
- `/octo:develop` — Implement all scripts
- `/octo:deliver` — Quality review and optimization

Or skip straight to implementation:
- Start building scripts in order (Steps 1-8) following the plan above

## Provider Availability
- 🔴 Codex CLI: Available ✓
- 🟡 Gemini CLI: Available ✓
- 🟤 OpenCode: Available ✓
- 🟣 Copilot: Available ✓
- 🟠 Qwen: Available ✓
- 🔵 Claude: Available ✓

## Success Criteria
1. All 19 C# scripts compile in Unity 2022 LTS+
2. Game loop works: Menu → Play → Dodge → Complete/Die → Score → Restart
3. Touch input responsive on Android
4. Face upload works from gallery
5. Code is clean, modular, and follows CLAUDE.md conventions

## Next Steps
1. Review this plan
2. Adjust if needed (re-run /octo:plan)
3. Execute with /octo:embrace when ready, or start building step-by-step
