# Fast and Acro — MVP Architecture & Implementation Plan

**Created:** 2026-04-05
**Engine:** Unity 2022 LTS+ | URP | C# .NET Standard 2.1
**Platform:** Android first, iOS later

---

## 1. Game Overview

"Fast and Acro" is an arcade-style auto-driving racing game where:
- The car moves forward automatically
- The player controls left/right movement only (touch input)
- Collisions reduce HP (not instant death)
- Levels end at a finish line, scored 1-3 stars
- Players can upload a face image displayed on the car

---

## 2. MVP Scope (9 Features Only)

| # | Feature | Description |
|---|---------|-------------|
| 1 | One car | Single car with placeholder 3D model |
| 2 | One level | Simple straight track with obstacles |
| 3 | Auto movement | Car moves forward at constant speed |
| 4 | Left/right control | Touch input for horizontal dodging |
| 5 | Collision + HP | HP bar, damage on hit, game over at 0 |
| 6 | Level completion | Finish line trigger ends the level |
| 7 | Basic UI | Main menu, HUD, end screens |
| 8 | Star scoring | 1-3 stars based on time + collisions |
| 9 | Face upload | Gallery image displayed on car |

**OUT OF SCOPE:** Multiple levels, car upgrades, abilities, progression, IAP, ads, localization, analytics.

---

## 3. Architecture Overview

### Game State Machine
```
Menu --> Playing --> LevelComplete
  ^        |              |
  |        v              |
  |     Paused            |
  |        |              |
  |        v              v
  +---- GameOver <--------+
```

States: `Menu | Playing | Paused | LevelComplete | GameOver`

### Communication Pattern
```
ScriptableObjects (GameConfig, LevelData)
        |
        v
Singleton Managers (GameManager, UIManager, ScoreManager)
        | (static C# events via GameEvents.cs)
        v
Scene Components (CarController, CollisionHandler, Obstacle, FinishLine)
        | (SerializeField references)
        v
UI Controllers (HUDController, MainMenuUI, LevelCompleteUI, GameOverUI)
```

### Key Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Input System | Legacy `Input.GetTouch()` | Simpler for MVP, no package dependency |
| Car Physics | Rigidbody + `MovePosition` in FixedUpdate | Smooth movement + collision detection |
| Horizontal Control | Free movement, clamped bounds | More fun than discrete lanes |
| Manager Pattern | Singleton<T> base class | Simple, appropriate for MVP scale |
| Decoupling | Static C# Action events | Zero allocation, no UnityEvent overhead |
| Configuration | ScriptableObjects | Inspector-tunable, no code changes needed |
| Face Upload | NativeGallery plugin | Proven Android/iOS gallery/camera access |
| UI | Unity UI Canvas, Screen Space Overlay | Standard approach, Canvas Scaler for mobile |

---

## 4. Unity Project Folder Structure

```
Assets/
  _Project/
    Scripts/
      Core/
        Singleton.cs                 -- Generic singleton base class
        GameEvents.cs                -- Static event bus (C# Actions)
        GameConfig.cs                -- ScriptableObject: global game settings
        GameManager.cs               -- Game state machine, level flow
      Gameplay/
        CarController.cs             -- Auto-forward + touch left/right input
        CollisionHandler.cs          -- HP tracking, damage, invincibility
        Obstacle.cs                  -- Obstacle behavior (static or moving)
        LevelManager.cs              -- Level setup, obstacle spawning
        LevelData.cs                 -- ScriptableObject: per-level config
        FinishLine.cs                -- Trigger zone for level completion
      UI/
        UIManager.cs                 -- Panel show/hide management
        HUDController.cs             -- HP bar, timer, collision count
        MainMenuUI.cs                -- Start screen with Play button
        LevelCompleteUI.cs           -- Stars display, retry/next buttons
        GameOverUI.cs                -- Failure screen, retry button
      Scoring/
        ScoreManager.cs              -- Star calculation logic
      FaceCustomization/
        FaceCustomizationManager.cs  -- Gallery/camera picker orchestration
        ImageLoader.cs               -- Native image -> Texture2D utility
        FaceBillboard.cs             -- Displays face texture above car
    Prefabs/
      Car.prefab
      Obstacle_Static.prefab
      Obstacle_Moving.prefab
      FinishLine.prefab
    Scenes/
      MainMenu.scene
      Level_01.scene
    Materials/
      Car_Mat.mat
      Road_Mat.mat
      Obstacle_Mat.mat
    ScriptableObjects/
      GameConfig.asset
      Level_01_Data.asset
    UI/
      MainMenuCanvas.prefab
      HUDCanvas.prefab
      LevelCompleteCanvas.prefab
      GameOverCanvas.prefab
```

**Total: 19 C# scripts**

---

## 5. Script Specifications

### 5.1 Core Layer

#### Singleton<T>.cs
```
Purpose: Generic MonoBehaviour singleton base class
Base: MonoBehaviour
Behavior:
  - Awake(): if instance exists, destroy duplicate; else set instance + DontDestroyOnLoad
  - Expose static Instance property
  - Virtual OnAwake() for subclass initialization
```

#### GameEvents.cs
```
Purpose: Static event bus for decoupled communication
Events:
  - OnGameStateChanged(GameState newState)
  - OnDamageTaken(int remainingHP)
  - OnPlayerDied()
  - OnLevelComplete(float completionTime, int collisionCount)
  - OnScoreCalculated(int starCount)
Pattern:
  - Static Action/Action<T> fields
  - Static Invoke methods with null checks
  - Subscribers: += in OnEnable, -= in OnDisable (CRITICAL for scene reload safety)
```

#### GameConfig.cs (ScriptableObject)
```
Fields:
  - float forwardSpeed = 15f
  - float horizontalSpeed = 10f
  - float roadHalfWidth = 3f
  - int maxHP = 3
  - float invincibilityDuration = 1f
  - float threeStarMaxCollisions = 0
  - float twoStarMaxCollisions = 2
  - float threeStarMaxTime = 30f
  - float twoStarMaxTime = 45f
```

#### GameManager.cs
```
Purpose: Central game state machine
Inherits: Singleton<GameManager>
State: GameState enum (Menu, Playing, Paused, LevelComplete, GameOver)
Methods:
  - StartGame() -> sets Playing, fires event
  - PauseGame() / ResumeGame()
  - CompleteLevel(float time, int collisions) -> sets LevelComplete
  - GameOver() -> sets GameOver
  - RestartLevel() -> reloads scene
  - ReturnToMenu() -> loads MainMenu scene
Listens:
  - GameEvents.OnPlayerDied -> GameOver()
  - GameEvents.OnLevelComplete -> CompleteLevel()
```

### 5.2 Gameplay Layer

#### CarController.cs
```
Purpose: Auto-forward movement + horizontal touch input
Requires: Rigidbody, Collider
Fields:
  [SerializeField] GameConfig _config
Movement:
  - FixedUpdate: Rigidbody.MovePosition(pos + forward * speed * fixedDeltaTime)
  - Update: Read touch input, calculate target X position
  - FixedUpdate: Lerp horizontal position toward target X
  - Clamp X within [-roadHalfWidth, +roadHalfWidth]
Touch Input:
  - If touch active: targetX = (touch.position.x / Screen.width - 0.5f) * 2 * roadHalfWidth
  - Smooth via Mathf.Lerp with horizontalSpeed * deltaTime
  - Only processes first touch (touch index 0)
```

#### CollisionHandler.cs
```
Purpose: HP management and damage handling
On: Car prefab (same GameObject as CarController)
Fields:
  [SerializeField] GameConfig _config
  private int _currentHP
  private bool _isInvincible
  private float _invincibilityTimer
Methods:
  - Initialize(): _currentHP = _config.maxHP
  - OnTriggerEnter(Collider other): if obstacle tag + not invincible -> TakeDamage()
  - TakeDamage(): _currentHP--, start invincibility, fire OnDamageTaken
  - If _currentHP <= 0: fire OnPlayerDied
  - Update: tick invincibility timer, visual flash effect
```

#### Obstacle.cs
```
Purpose: Obstacle behavior
Types: Static (just sits there), Moving (moves side to side)
Fields:
  [SerializeField] bool _isMoving = false
  [SerializeField] float _moveSpeed = 2f
  [SerializeField] float _moveRange = 2f
Collider: Trigger collider (IsTrigger = true)
Tag: "Obstacle"
```

#### LevelManager.cs
```
Purpose: Level flow management
Fields:
  [SerializeField] LevelData _levelData
  private float _elapsedTime
  private int _collisionCount
Methods:
  - Start(): setup level from LevelData
  - Update(): track elapsed time while Playing
  - Listen to GameEvents.OnDamageTaken: increment _collisionCount
  - OnLevelComplete: pass time + collisions to GameManager
```

#### LevelData.cs (ScriptableObject)
```
Fields:
  - string levelName = "Level 01"
  - string country = "USA"
  - float trackLength = 200f
  - int obstacleCount = 15
```

#### FinishLine.cs
```
Purpose: Trigger zone at end of track
OnTriggerEnter: if car tag -> fire GameEvents.OnLevelComplete
Tag check: "Player"
```

### 5.3 Scoring

#### ScoreManager.cs
```
Purpose: Calculate 1-3 stars based on performance
Inherits: Singleton<ScoreManager>
Fields:
  [SerializeField] GameConfig _config
Method: int CalculateStars(float time, int collisions)
  - Start at 3 stars
  - If collisions > threeStarMaxCollisions: lose 1 star
  - If collisions > twoStarMaxCollisions: lose 1 more star
  - If time > threeStarMaxTime: lose 1 star
  - If time > twoStarMaxTime: lose 1 more star
  - Clamp result to [1, 3]
  - Fire GameEvents.OnScoreCalculated(stars)
```

### 5.4 UI Layer

#### UIManager.cs
```
Purpose: Manage which UI panel is visible
Inherits: Singleton<UIManager>
Fields:
  [SerializeField] GameObject _mainMenuPanel
  [SerializeField] GameObject _hudPanel
  [SerializeField] GameObject _levelCompletePanel
  [SerializeField] GameObject _gameOverPanel
Methods:
  - ShowPanel(panel): hide all, show target
  - Listen to GameEvents.OnGameStateChanged: switch panels accordingly
```

#### HUDController.cs
```
Purpose: Display in-game info
Fields:
  [SerializeField] Image _hpBarFill       (UI Image, filled type)
  [SerializeField] TextMeshProUGUI _timerText
Updates:
  - Listen GameEvents.OnDamageTaken: update HP bar fill amount
  - Update(): update timer text (format: "00:00")
  - Cache string formatting to avoid GC (use StringBuilder or int-to-string cache)
```

#### MainMenuUI.cs
```
Purpose: Start screen
Fields:
  [SerializeField] Button _playButton
  [SerializeField] Button _faceCustomizeButton
Methods:
  - OnPlayClicked(): GameManager.Instance.StartGame()
  - OnCustomizeClicked(): FaceCustomizationManager.Instance.OpenPicker()
```

#### LevelCompleteUI.cs
```
Purpose: Victory screen with stars
Fields:
  [SerializeField] Image[] _starImages (3 stars)
  [SerializeField] TextMeshProUGUI _timeText
  [SerializeField] TextMeshProUGUI _collisionText
  [SerializeField] Button _retryButton
  [SerializeField] Button _menuButton
Methods:
  - Listen GameEvents.OnScoreCalculated: light up star images
  - Display completion stats
```

#### GameOverUI.cs
```
Purpose: Failure screen
Fields:
  [SerializeField] Button _retryButton
  [SerializeField] Button _menuButton
Methods:
  - OnRetryClicked(): GameManager.Instance.RestartLevel()
  - OnMenuClicked(): GameManager.Instance.ReturnToMenu()
```

### 5.5 Face Customization

#### ImageLoader.cs (Static Utility)
```
Purpose: Load image from device gallery or camera
Methods:
  - static void PickFromGallery(Action<Texture2D> callback)
    Uses NativeGallery.GetImageFromGallery()
    Loads bytes -> Texture2D.LoadImage()
    Resizes to 256x256 using RenderTexture blit
  - static void PickFromCamera(Action<Texture2D> callback)
    Uses NativeCamera.TakePicture()
    Same load + resize flow
```

#### FaceCustomizationManager.cs
```
Purpose: Orchestrate face image selection
Inherits: Singleton<FaceCustomizationManager>
Fields:
  [SerializeField] FaceBillboard _faceBillboard
Methods:
  - OpenPicker(): show choice (gallery vs camera), then call ImageLoader
  - OnImageLoaded(Texture2D tex): apply to _faceBillboard
  - Save/Load face via PlayerPrefs (store as base64 PNG for persistence)
```

#### FaceBillboard.cs
```
Purpose: Display face texture above car
On: Quad/Plane child of Car prefab, positioned above car
Fields:
  [SerializeField] Renderer _faceRenderer
  [SerializeField] float _billboardHeight = 1.5f
Methods:
  - SetFaceTexture(Texture2D tex): _faceRenderer.material.mainTexture = tex
  - LateUpdate(): billboard rotation to always face camera
```

---

## 6. Implementation Build Order

### Step 1: Unity Project Setup
- [ ] Create Unity project (Unity 2022 LTS, URP template)
- [ ] Configure URP settings for mobile (disable unnecessary features)
- [ ] Create folder structure under Assets/_Project/
- [ ] Import TextMesh Pro essentials
- [ ] Import NativeGallery + NativeCamera plugins

### Step 2: Foundation Scripts (3 scripts)
- [ ] `Singleton<T>.cs` — Generic singleton base
- [ ] `GameEvents.cs` — Static event bus
- [ ] `GameConfig.cs` — ScriptableObject with all tunable values
- [ ] Create GameConfig.asset in ScriptableObjects/

### Step 3: Core Game Loop (1 script)
- [ ] `GameManager.cs` — State machine with all transitions
- [ ] Test: verify state transitions log correctly

### Step 4: Car & Movement (1 script)
- [ ] Create Car prefab (cube placeholder + Rigidbody + Collider)
- [ ] `CarController.cs` — Auto-forward + touch horizontal input
- [ ] Test: car moves forward, responds to touch

### Step 5: Collision & HP (2 scripts)
- [ ] `CollisionHandler.cs` — HP system on Car prefab
- [ ] `Obstacle.cs` — Static + moving obstacle behavior
- [ ] Create Obstacle prefabs with trigger colliders
- [ ] Test: collisions reduce HP, invincibility works

### Step 6: Level Flow (3 scripts)
- [ ] `LevelData.cs` — ScriptableObject for level config
- [ ] `LevelManager.cs` — Time tracking, collision counting
- [ ] `FinishLine.cs` — Level completion trigger
- [ ] Build Level_01 scene: road, obstacles, finish line
- [ ] Test: reaching finish line triggers level complete

### Step 7: Scoring (1 script)
- [ ] `ScoreManager.cs` — Star calculation
- [ ] Test: verify star thresholds work correctly

### Step 8: UI System (5 scripts)
- [ ] `UIManager.cs` — Panel management
- [ ] `HUDController.cs` — HP bar + timer
- [ ] `MainMenuUI.cs` — Play + Customize buttons
- [ ] `LevelCompleteUI.cs` — Stars + stats display
- [ ] `GameOverUI.cs` — Retry + Menu buttons
- [ ] Build UI prefabs with Canvas Scaler (1080x1920 reference, Scale With Screen Size)
- [ ] Test: full game loop through all screens

### Step 9: Face Customization (3 scripts)
- [ ] `ImageLoader.cs` — Gallery/camera to Texture2D utility
- [ ] `FaceCustomizationManager.cs` — Picker flow + persistence
- [ ] `FaceBillboard.cs` — Display on car with billboard rotation
- [ ] Test: pick image, see it on car, persists after restart

### Step 10: Polish & Mobile Optimization
- [ ] Configure physics layers (Car, Obstacle, FinishLine)
- [ ] Enable Rigidbody interpolation
- [ ] Set Canvas Scaler reference resolution (1080x1920)
- [ ] Add safe area handling for notched devices
- [ ] Remove all Debug.Log calls
- [ ] Test on Android device or emulator
- [ ] Verify 60 FPS on mid-range device

---

## 7. Scene Setup Guide

### MainMenu.scene
```
Hierarchy:
  - MainMenuCanvas (Canvas, Screen Space Overlay)
    - Background (Image, full screen)
    - Title (TextMeshPro: "Fast and Acro")
    - PlayButton (Button)
    - CustomizeFaceButton (Button)
  - GameManager (Singleton, DontDestroyOnLoad)
  - UIManager (Singleton, DontDestroyOnLoad)
  - ScoreManager (Singleton, DontDestroyOnLoad)
  - FaceCustomizationManager (Singleton, DontDestroyOnLoad)
```

### Level_01.scene
```
Hierarchy:
  - Directional Light
  - Main Camera (follow car on Z axis)
  - Road (stretched cube or plane, long on Z)
  - Car (prefab: cube + Rigidbody + CarController + CollisionHandler)
    - FaceQuad (child: Quad + FaceBillboard)
  - Obstacles (parent)
    - Obstacle_01..15 (prefabs placed along road)
  - FinishLine (prefab at end of road)
  - LevelManager (reads LevelData SO)
  - HUDCanvas (Canvas)
    - HPBar (Slider or filled Image)
    - Timer (TextMeshPro)
  - LevelCompleteCanvas (Canvas, hidden by default)
  - GameOverCanvas (Canvas, hidden by default)
```

---

## 8. Key Technical Notes

### Event Safety Pattern
```csharp
// ALWAYS: subscribe in OnEnable, unsubscribe in OnDisable
void OnEnable() => GameEvents.OnDamageTaken += HandleDamage;
void OnDisable() => GameEvents.OnDamageTaken -= HandleDamage;
```

### Touch Input Pattern
```csharp
// In Update():
if (Input.touchCount > 0)
{
    Touch touch = Input.GetTouch(0);
    float normalizedX = touch.position.x / Screen.width;  // 0 to 1
    _targetX = (normalizedX - 0.5f) * 2f * _config.roadHalfWidth;  // -width to +width
}
```

### Rigidbody Movement (Jitter-Free)
```csharp
// In FixedUpdate():
Vector3 forward = transform.position + Vector3.forward * _config.forwardSpeed * Time.fixedDeltaTime;
float smoothX = Mathf.Lerp(transform.position.x, _targetX, _config.horizontalSpeed * Time.fixedDeltaTime);
_rb.MovePosition(new Vector3(smoothX, transform.position.y, forward.z));
```

### Singleton Base (Safe for Scene Reload)
```csharp
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = (T)(MonoBehaviour)this;
        DontDestroyOnLoad(gameObject);
    }
}
```

---

## 9. Dependencies

| Package/Plugin | Purpose | Source |
|---------------|---------|--------|
| TextMesh Pro | UI text rendering | Unity Registry (built-in) |
| NativeGallery | Android/iOS gallery access | GitHub: yasirkula/UnityNativeGallery |
| NativeCamera | Android/iOS camera access | GitHub: yasirkula/UnityNativeCamera |

---

## 10. Next Steps

1. **Review this plan** — adjust anything before implementation
2. **Create Unity project** via Unity Hub (manual step)
3. **Execute implementation** — follow Steps 1-10 above
4. **Incorporate debate feedback** — refinements from multi-AI review (pending)

To start implementation, run:
```
/octo:embrace "Build Fast and Acro MVP"
```

Or build scripts step-by-step following the build order above.
