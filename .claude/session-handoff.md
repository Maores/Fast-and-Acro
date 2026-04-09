# Session Handoff — Car Skins Close Button Bug

## CRITICAL BUG TO FIX

**The car skins screen has NO working exit button.** When the user clicks the SKINS button on the main menu, the skins panel opens and they can browse/select skins, but there is NO way to go back to the main menu. No X button appears, no back button works, Escape key does nothing.

**User instruction:** "Don't stop until you are valid and sure in 100% that the problem was fixed." Connect to Unity MCP and test the full flow live.

---

## Unity MCP Connection

- **MCP server is running** in Unity (verified: green status, Session Active)
- **Server:** `UnityMCP` at `http://127.0.0.1:8080/mcp` (HTTP transport)
- Also registered: `coplay-mcp` at `http://127.0.0.1:8080/sse` (SSE) — may fail, use UnityMCP
- **Tools to use:** `find_gameobjects`, `manage_gameobject`, `manage_components`, `execute_code`, `read_console`, `manage_camera` (for screenshots)
- Run `claude mcp list` to verify connection before starting

---

## Root Cause Analysis

After 5+ failed fix attempts, the suspected root causes are:

### Hypothesis 1: `Show()` is never called
The SKINS button might have a leftover `onClick` listener (from earlier MCP wiring) that directly calls `SetActive(true)` on the CarSkinPanel, bypassing `GameManager.ShowCarSkins()` → `CarSkinUI.Show()`. If `Show()` never runs, the close button is never created.

**How to verify via MCP:**
```
1. find_gameobjects(search_term="SkinsButton", search_method="by_name")
2. Check its onClick listeners via manage_components
3. Check if GameManager._carSkinUI reference is wired (not null)
```

### Hypothesis 2: CarSkinUI component missing or on wrong GameObject
The `CarSkinUI` MonoBehaviour might have been lost from the CarSkinPanel during scene saves, or `_skinPanel` might point to the wrong object.

**How to verify:**
```
1. find_gameobjects(search_term="CarSkinUI", search_method="by_component")
2. Check what _skinPanel, _skinManager, _gameManager references point to
```

### Hypothesis 3: Close button created but invisible
The `CreateCloseButton()` code runs but the button is clipped by a layout group, mask, or rendered behind other UI elements.

---

## What Was Already Tried (all failed)

1. **Scene BackButton wiring** — Added code in `Show()` to find "BackButton" child and wire onClick → didn't work (button probably doesn't exist in scene)
2. **OnBackClicked → ReturnToMenu** — Changed to call `UIController.ShowMainMenu()` instead of scene reload → still can't click anything
3. **Escape key handler** — Added `Update()` checking `Input.GetKeyDown(KeyCode.Escape)` → doesn't fire (likely `_isVisible` is false because `Show()` wasn't called)
4. **CreateCloseButton() in code** — Creates "CloseButton_X" (50x50, top-right, dark background) → never appears visually
5. **FixSkinsButton editor script** — Auto-wires all references on compile → wiring may not persist

## Partial Fix In Progress (NOT YET TESTED)

`CarSkinUI.cs` was partially edited in the dying session. The changes may or may not have been saved to disk. **Read the current file state first.** The intended changes were:

### Change 1: Use `OnEnable()` instead of relying on `Show()`
```csharp
private void OnEnable()
{
    _isVisible = true;
    EnsureCloseButton();
    PopulateButtons();
    UpdateCoinBalance();
}
```
This guarantees the close button is created regardless of HOW the panel was activated.

### Change 2: Parent button to Canvas root, not skin panel
```csharp
Canvas rootCanvas = panelTransform.GetComponentInParent<Canvas>();
Transform buttonParent = rootCanvas != null ? rootCanvas.transform : panelTransform;
```
This avoids layout group / mask clipping issues.

### Change 3: Override sorting order for guaranteed visibility
```csharp
var overlay = go.AddComponent<Canvas>();
overlay.overrideSorting = true;
overlay.sortingOrder = 100;
go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
```

### Change 4: Red 70x70 button (unmissable)
Named "SkinCloseButton" (not "CloseButton_X").

---

## Files Involved

### `Assets/_Project/Scripts/UI/CarSkinUI.cs`
The main buggy file. Key methods:
- `Show()` — activates panel, should create close button
- `Hide()` — deactivates panel
- `OnBackClicked()` — calls Hide() then UIController.ShowMainMenu()
- `CreateCloseButton()` / `EnsureCloseButton()` — programmatic X button
- `PopulateButtons()` — creates skin selection grid
- `Update()` — Escape key handler

### `Assets/_Project/Scripts/UI/UIController.cs`
- Has `_skinsButton` field wired in Awake: `_skinsButton.onClick.AddListener(OnSkinsClicked)`
- `OnSkinsClicked()` calls `_gameManager.ShowCarSkins()`
- `ShowMainMenu()` — the target when closing skins
- `HideAll()` — hides MainMenu, HUD, LevelComplete, GameOver, Pause panels (NOT CarSkinPanel)

### `Assets/_Project/Scripts/Core/GameManager.cs`
- `ShowCarSkins()` calls `_ui.HideAll()` then `_carSkinUI.Show()`
- `_carSkinUI` field ([SerializeField]) — might be null if not wired
- `ReturnToMenu()` — full scene reload fallback

### `Assets/_Project/Scripts/Gameplay/CarSkinManager.cs`
- Manages skin data, unlock state, color application via MaterialPropertyBlock
- 5 presets: Gray, Blue, Red, Green, Yellow (50 coins each to unlock)

### `Assets/_Project/Scripts/Editor/FixSkinsButton.cs`
- `[InitializeOnLoad]` script that runs on every compile
- Fixes PlayButton/SkinsButton anchor positions
- Wires UIController._skinsButton, CarSkinUI references, GameManager._carSkinUI
- Console output: `[FixSkinsButton] Everything already correct — no changes needed.`

---

## Recommended Fix Strategy

### Step 1: Verify via MCP
Connect to Unity MCP and check:
1. Does `CarSkinUI` component exist on a GameObject? Which one?
2. Is `GameManager._carSkinUI` reference wired (not null)?
3. Does the SkinsButton have the correct onClick chain?
4. Take a screenshot of the Game view after clicking SKINS

### Step 2: Apply the robust code fix
If `Show()` is being bypassed, use `OnEnable()` to create the button. If references are null, add self-repair in `OnEnable()` using `FindFirstObjectByType`.

### Step 3: Test via MCP
1. Enter Play mode
2. Click SKINS button (or simulate via execute_code)
3. Verify the X button appears (screenshot)
4. Click the X button
5. Verify return to main menu (screenshot)

### Step 4: Verify Escape key also works

---

## Project Context
- **Unity 6.4** (6000.4.1f1) with URP
- **Game:** "Fast and Acro" — mobile arcade racing
- **CLAUDE.md rule:** MUST use installed skills (octo, superpowers) instead of working manually
- **User feedback style:** Wants visual proof the bug is fixed, not just code changes
