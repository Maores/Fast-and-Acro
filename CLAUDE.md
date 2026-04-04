# Fast and Acro — Project Configuration

## Project Overview
- **Name:** Fast and Acro
- **Type:** Mobile game (Android first, iOS later)
- **Engine:** Unity 2022 LTS or newer
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Language:** C# (.NET Standard 2.1)
- **Model:** claude-opus-4-6 (high performance)

## Primary Skill Sets

This project uses two installed plugin suites as the primary development toolchain:

### octo (Claude Octopus v9.12.0 — nyldn-plugins)
Use for all structured development workflows:
- `/octo:plan` — Architecture and implementation planning
- `/octo:tdd` — Test-driven development with red-green-refactor
- `/octo:debug` — Systematic debugging
- `/octo:review` — Multi-LLM code review
- `/octo:dev` — Dev work mode
- `/octo:design-ui-ux` — UI/UX design systems
- `/octo:brainstorm` — Creative ideation
- `/octo:parallel` — Decompose compound tasks across agents

### superpowers (v5.0.5 — superpowers-dev)
Use for workflow orchestration and execution:
- `/superpowers:writing-plans` — Spec-to-plan conversion
- `/superpowers:executing-plans` — Plan execution with checkpoints
- `/superpowers:dispatching-parallel-agents` — Parallel agent coordination
- `/superpowers:subagent-driven-development` — Implementation via subagents
- `/superpowers:verification-before-completion` — Pre-completion verification
- `/superpowers:brainstorming` — Structured brainstorming before creative work
- `/superpowers:test-driven-development` — TDD workflow
- `/superpowers:systematic-debugging` — Debugging methodology
- `/superpowers:requesting-code-review` — Code review requests

## Unity / C# Conventions

### Folder Structure (inside Unity project)
```
Assets/
  _Project/
    Scripts/
      Core/           # Game managers, singletons, core systems
      Gameplay/       # Player, enemies, obstacles, scoring
      UI/             # UI controllers, views, HUD
      Audio/          # Audio managers, SFX/music controllers
      Utils/          # Helper classes, extensions
      Data/           # ScriptableObjects, data models
    Prefabs/
    Scenes/
    Materials/
    Textures/
    Animations/
    Audio/
      Music/
      SFX/
    UI/
    Fonts/
    Shaders/
    Resources/
    StreamingAssets/
  Plugins/            # Third-party SDKs
  TextMesh Pro/
```

### Naming Conventions
- **Classes/Structs:** PascalCase (`PlayerController`, `ScoreManager`)
- **Methods:** PascalCase (`CalculateScore()`, `OnCollisionEnter()`)
- **Public fields:** camelCase (`public float moveSpeed;`)
- **Private fields:** _camelCase with underscore prefix (`private int _currentScore;`)
- **Constants:** UPPER_SNAKE_CASE (`const float MAX_SPEED = 10f;`)
- **Interfaces:** IPascalCase (`IInteractable`, `IDamageable`)
- **Enums:** PascalCase, singular (`GameState`, not `GameStates`)
- **ScriptableObjects:** PascalCase + "Data" or "Config" suffix (`LevelData`, `GameConfig`)
- **Files:** Match the class name exactly (`PlayerController.cs`)

### Code Guidelines
- One MonoBehaviour per file, filename matches class name
- Use `[SerializeField]` instead of public fields for Inspector exposure
- Prefer composition over inheritance
- Use ScriptableObjects for shared configuration data
- Use events/delegates or UnityEvents for decoupling systems
- Minimize `Update()` usage — prefer event-driven patterns
- Use object pooling for frequently instantiated objects (projectiles, effects)
- All strings that appear in UI must go through a localization system
- Target 60 FPS on mid-range Android devices (optimize draw calls, batching)

### Mobile-Specific Guidelines
- Design for touch input first, support gamepad as secondary
- Use screen-space safe areas for UI (notch/cutout handling)
- Minimize garbage allocation to avoid GC spikes
- Use Addressables for asset management
- Keep APK/AAB size under 150MB (use asset bundles for additional content)
- Test on multiple screen aspect ratios (16:9, 18:9, 19.5:9, 20:9)
- Implement proper app lifecycle handling (pause, resume, background)

### Git Workflow
- Feature branches: `feature/<description>`
- Bug fixes: `fix/<description>`
- Commit messages: imperative mood, concise ("Add player movement system", "Fix score calculation on combo")
- Never commit Library/, Temp/, or build artifacts

## How to Run
1. Open Unity Hub
2. Add project folder
3. Open with Unity 2022 LTS+
4. Ensure URP is configured (Project Settings > Graphics)
5. Open the main scene and press Play
