# Fast and Acro - Session Handoff Document

> Use this file to instruct the next Claude Code session. Paste it at the start of your conversation or place it in your project's CLAUDE.md.

---

## Project Identity

- **Name:** Fast and Acro
- **Type:** Arcade-style auto-driving racing mobile game
- **Engine:** Unity 6 (6000.0.34f1) with URP
- **Platform:** Android first, iOS later
- **Repo:** https://github.com/Maores/Fast-and-Acro.git
- **Branch:** `master`
- **Project folder:** `C:\Users\maor4\OneDrive\Desktop\Fast-and-Acro-Unity`
- **CoPlay MCP:** Installed (`com.coplaydev.unity-mcp` in Packages/manifest.json) — use it to interact with Unity editor directly

## User Profile

- Unity beginner — needs clear, step-by-step explanations
- Under time pressure — no overengineering
- Prefers production-ready, modular C# code
- Wants to see progress quickly

---

## What Has Been Done (Sessions 1-2)

### Architecture & Planning
- Multi-AI debate (3 Claude agents) reviewed the architecture
- Simplified from 19 scripts to 14 scripts based on debate consensus
- **No singletons** — all wiring via `[SerializeField]` Inspector references
- **Direct method calls** instead of static event bus
- **Lane-based movement** with swipe detection (3 lanes)
- **Transform.Translate** instead of Rigidbody for auto-runner movement

### 14 C# Scripts Written (all in `Assets/_Project/Scripts/`)

| Script | Path | Purpose |
|--------|------|---------|
| `GameConfig.cs` | `Data/` | ScriptableObject — all tunable game values (speeds, HP, lanes, swipe thresholds, frame rate) |
| `LevelData.cs` | `Data/` | ScriptableObject — per-level config (track length, obstacle count, spacing, moving ratio) |
| `GameManager.cs` | `Core/` | Central orchestrator — GameState enum (Menu/Playing/Paused/LevelComplete/GameOver), scoring, save system (PlayerPrefs) |
| `CameraFollow.cs` | `Core/` | LateUpdate smooth follow camera with offset and look-ahead |
| `CarController.cs` | `Gameplay/` | Auto-forward movement + lane-based swipe input, keyboard fallback for editor |
| `CollisionHandler.cs` | `Gameplay/` | HP system with invincibility frames, visual flash via material color toggle |
| `Obstacle.cs` | `Gameplay/` | Static or moving (sine wave) obstacle, pool-compatible Setup/Deactivate |
| `LevelManager.cs` | `Gameplay/` | Spawns obstacles from ObjectPool along track, positions finish line, scales road |
| `FinishLine.cs` | `Gameplay/` | Trigger zone that calls GameManager.CompleteLevel() |
| `FaceDisplay.cs` | `Gameplay/` | Preset face sprites on car with billboard rotation |
| `UIController.cs` | `UI/` | Unified UI for 5 panels (MainMenu, HUD, Pause, LevelComplete, GameOver), HP bar, timer, star display |
| `SafeAreaPanel.cs` | `UI/` | RectTransform anchors to Screen.safeArea for notch handling |
| `AudioManager.cs` | `Audio/` | One-shot SFX via PlayOneShot, separate AudioSource for looping engine |
| `ObjectPool.cs` | `Utils/` | Generic Queue-based pool with auto-expand, ReturnAll() for level reset |

### Unity Assets Created
- `Assets/_Project/Scenes/Level01.unity` — Full scene with 10 root GameObjects, all SerializeField references wired via YAML
- `Assets/_Project/Scenes/GameScene.unity` — Default scene
- `Assets/_Project/Data/DefaultGameConfig.asset` — GameConfig SO (forwardSpeed=15, laneWidth=2.5, laneCount=3, maxHP=3)
- `Assets/_Project/Data/Level01Data.asset` — LevelData SO (trackLength=200, obstacleCount=15, movingRatio=0.3)
- `Assets/_Project/Prefabs/Obstacle.prefab` — Cube with trigger BoxCollider + Obstacle script
- `Assets/_Project/Editor/SceneBuilder.cs` — Editor script for programmatic scene setup
- `Assets/_Project/Editor/TagCreator.cs` — Editor script for creating required tags
- All `.meta` files with stable GUIDs for cross-referencing

### Project Consolidation
- Merged separate git repo (`Fast-and-Acro`) into Unity project folder (`Fast-and-Acro-Unity`)
- Now a single folder that is both the Unity project AND the git repo
- Deleted empty "Fast and Acro" Unity project (was unused)
- `.gitignore` properly excludes Library/, Temp/, Logs/, UserSettings/

### Scene Hierarchy in Level01.unity
```
Main Camera        → CameraFollow (targeting Car)
Directional Light
Car (tag: Player)  → CarController, CollisionHandler
  └── FaceQuad     → FaceDisplay
Road               → Scaled cube
FinishLine         → Trigger collider, FinishLine script
GameManager        → GameManager script (references all managers)
LevelManager       → LevelManager + ObjectPool (prefab→Obstacle)
AudioManager       → AudioManager + 2 AudioSources
GameCanvas         → Canvas + CanvasScaler (1080x1920, match=0.5) + UIController
  └── SafeAreaPanel → SafeAreaPanel script
      ├── MainMenuPanel (active)
      ├── HUDPanel (inactive)
      ├── PausePanel (inactive)
      ├── LevelCompletePanel (inactive)
      └── GameOverPanel (inactive)
EventSystem
```

---

## What Needs To Be Done Next (Priority Order)

### Phase 1: Get It Running (CRITICAL)
1. **Open Unity and check for compile errors** — Use CoPlay MCP (`check_compile_errors`)
2. **Open Level01 scene** — Use CoPlay MCP (`open_scene`)
3. **Fix any GUID/reference issues** — The YAML-generated scene may have broken references since Unity regenerates GUIDs on import. If so, use CoPlay MCP to re-wire SerializeField references
4. **Add TextMeshPro UI elements to panels** — The 5 UI panels (MainMenu, HUD, Pause, LevelComplete, GameOver) are empty containers. They need:
   - MainMenuPanel: "Fast and Acro" title text, "Play" button
   - HUDPanel: HP bar (Image fill), timer text, pause button
   - PausePanel: "Paused" text, "Resume" button, "Main Menu" button
   - LevelCompletePanel: star display (3 Image objects), score text, "Next Level" button, "Main Menu" button
   - GameOverPanel: "Game Over" text, score text, "Retry" button, "Main Menu" button
5. **Wire UIController button references** — Connect button onClick events to UIController methods
6. **Press Play and test** — Car should auto-drive forward, swipe/arrow keys change lanes, obstacles spawn, HP system works

### Phase 2: Make It Playable
7. **Add proper 3D car model** — Replace the default cube with a simple car mesh (can use Unity primitives: capsule body + cylinder wheels)
8. **Add materials/colors** — Car material, road material, obstacle materials (red for danger)
9. **Add road lane markings** — White dashed lines on the road using stretched quads
10. **Camera angle tuning** — Adjust CameraFollow offset for good third-person racing view
11. **Particle effects** — Collision sparks, level complete celebration
12. **Sound effects** — Engine loop, collision sound, UI click sounds (assign to AudioManager)

### Phase 3: Polish & Content
13. **Multiple levels** — Create Level02Data, Level03Data with increasing difficulty
14. **Level select screen** — Show stars earned, lock/unlock progression
15. **Car face customization** — Multiple face textures player can choose
16. **Difficulty curve** — Gradually increase speed, obstacle density, moving obstacle ratio
17. **Mobile build** — Android APK export, test on device
18. **Touch input testing** — Verify swipe detection thresholds on actual phone

### Phase 4: Release Prep
19. **App icon and splash screen**
20. **Google Play Store assets** — Screenshots, description
21. **Performance optimization** — Profiler check, draw call batching, GC allocation review
22. **Final QA pass**

---

## Key Architecture Decisions (Do NOT Change)

These were debated and agreed upon — stick with them:

| Decision | Rationale |
|----------|-----------|
| No singletons | All wiring via [SerializeField] — beginner-friendly, testable |
| Direct method calls | No event bus — simpler for small game |
| Lane-based movement | 3 discrete lanes via swipe — not free horizontal movement |
| Transform.Translate | No Rigidbody for movement — simpler for auto-runner |
| Object pooling | For obstacles — prevents GC spikes on mobile |
| ScriptableObjects for config | GameConfig + LevelData — tunable without code changes |
| Canvas Scaler 1080x1920, match=0.5 | Standard mobile portrait reference |

---

## CoPlay MCP Usage

The Unity project has CoPlay MCP installed. When Unity is open, you can use these tools:
- `set_unity_project_root` — Point to `C:\Users\maor4\OneDrive\Desktop\Fast-and-Acro-Unity`
- `get_unity_editor_state` — Check editor status
- `check_compile_errors` — Find C# errors
- `list_game_objects_in_hierarchy` — See scene hierarchy
- `create_game_object` — Add new objects
- `add_component` — Add scripts to objects
- `set_property` — Set SerializeField values
- `create_ui_element` — Add UI buttons, text, images
- `set_ui_text` — Set TMP text content
- `play_game` / `stop_game` — Test in editor
- `open_scene` — Open Level01 scene
- `capture_scene_object` — Screenshot objects
- `get_unity_logs` — Read console output

---

## File Locations Quick Reference

```
C:\Users\maor4\OneDrive\Desktop\Fast-and-Acro-Unity\
├── Assets/
│   └── _Project/
│       ├── Scripts/
│       │   ├── Core/       GameManager.cs, CameraFollow.cs
│       │   ├── Data/       GameConfig.cs, LevelData.cs
│       │   ├── Gameplay/   CarController.cs, CollisionHandler.cs, Obstacle.cs,
│       │   │               LevelManager.cs, FinishLine.cs, FaceDisplay.cs
│       │   ├── UI/         UIController.cs, SafeAreaPanel.cs
│       │   ├── Audio/      AudioManager.cs
│       │   └── Utils/      ObjectPool.cs
│       ├── Scenes/         Level01.unity, GameScene.unity
│       ├── Data/           DefaultGameConfig.asset, Level01Data.asset
│       ├── Prefabs/        Obstacle.prefab
│       └── Editor/         SceneBuilder.cs, TagCreator.cs
├── Docs/                   PLAN.md, DEBATE_SYNTHESIS.md, UNITY_SETUP_GUIDE.md
├── ProjectSettings/
├── Packages/
├── CLAUDE.md
└── README.md
```
