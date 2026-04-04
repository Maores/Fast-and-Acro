# Fast and Acro — Unity Scene Setup Guide

**For Unity beginners.** Follow every step in order. Do not skip ahead.

---

## Table of Contents

1. [Create the Unity Project](#1-create-the-unity-project)
2. [Copy Scripts into the Project](#2-copy-scripts-into-the-project)
3. [Import TextMesh Pro](#3-import-textmesh-pro)
4. [Set Up Tags and Layers](#4-set-up-tags-and-layers)
5. [Create ScriptableObject Assets](#5-create-scriptableobject-assets)
6. [Build the Game Scene — GameObject Hierarchy](#6-build-the-game-scene)
7. [Set Up the UI](#7-set-up-the-ui)
8. [Wire All SerializeField References](#8-wire-all-serializefield-references)
9. [Camera Setup](#9-camera-setup)
10. [Build Settings and Player Settings](#10-build-settings-and-player-settings)
11. [Testing Checklist](#11-testing-checklist)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Create the Unity Project

### Step 1.1: Open Unity Hub
- Launch **Unity Hub** (download from https://unity.com/download if needed)
- Make sure you have **Unity 2022.3 LTS** (or newer 2022 LTS) installed
  - If not: click **Installs** → **Install Editor** → select **2022.3.x LTS** → install

### Step 1.2: Create New Project
1. Click **Projects** → **New project**
2. Select **3D (URP)** template (Universal Render Pipeline)
   - If you don't see it, click **Templates** on the left and download it first
3. Set project name: `FastAndAcro`
4. Choose a save location (e.g., your Desktop)
5. Click **Create project**
6. Wait for Unity to initialize (this takes 2-5 minutes the first time)

### Step 1.3: Verify URP is Working
1. When Unity opens, you should see a sample scene with lighting
2. Go to **Edit → Project Settings → Graphics**
3. Confirm a **URP Asset** is assigned in the **Scriptable Render Pipeline Settings** field
4. If it says "None", something went wrong — delete and recreate with the URP template

---

## 2. Copy Scripts into the Project

### Step 2.1: Locate the Scripts
The scripts are in this repository under:
```
Assets/_Project/Scripts/
```

### Step 2.2: Copy into Unity Project
1. Open your file explorer (Windows Explorer / Finder)
2. Navigate to this repository's `Assets/_Project/` folder
3. **Copy the entire `_Project` folder**
4. Navigate to your Unity project's `Assets/` folder
   - It's at: `<your-save-location>/FastAndAcro/Assets/`
5. **Paste `_Project` here** so the path becomes:
   ```
   FastAndAcro/Assets/_Project/Scripts/...
   ```
6. Switch back to Unity — it will auto-detect the new files and compile
7. Wait for the progress bar at the bottom to finish

### Step 2.3: Verify No Compile Errors
1. Look at the **Console** window (Window → General → Console)
2. You will see **2 expected errors** about `TMPro` — this is normal, we fix it next
3. There should be NO other red errors

---

## 3. Import TextMesh Pro

TextMesh Pro (TMP) is needed for the UI text elements.

### Step 3.1: Import TMP Essentials
1. Go to **Window → TextMeshPro → Import TMP Essential Resources**
2. An import dialog appears — click **Import**
3. Wait for it to finish
4. The 2 compile errors from Step 2.3 should now disappear

### Step 3.2: Verify
1. Check the Console — should be **zero errors** now
2. You should see a `TextMesh Pro` folder in your Assets

---

## 4. Set Up Tags and Layers

### Step 4.1: Add Tags
1. Go to **Edit → Project Settings → Tags and Layers**
2. Expand the **Tags** section
3. Click the **+** button and add these tags (if they don't already exist):
   - `Player`
   - `Obstacle`

### Step 4.2: Add Physics Layers (Optional but Recommended)
In the **Layers** section, add:
- **Layer 6:** `Player`
- **Layer 7:** `Obstacle`
- **Layer 8:** `FinishLine`

### Step 4.3: Configure Layer Collision Matrix
1. Go to **Edit → Project Settings → Physics**
2. Scroll to the **Layer Collision Matrix** at the bottom
3. **Uncheck** all pairs except:
   - Player ↔ Obstacle (checked)
   - Player ↔ FinishLine (checked)
   - Player ↔ Default (checked)
4. This reduces unnecessary physics checks and improves mobile performance

---

## 5. Create ScriptableObject Assets

### Step 5.1: Create GameConfig Asset
1. In the **Project** window, navigate to `Assets/_Project/ScriptableObjects/`
   - If the folder doesn't exist, right-click → Create → Folder → name it `ScriptableObjects`
2. Right-click in the folder → **Create → FastAndAcro → GameConfig**
3. Name it `GameConfig`
4. Click on it and set these values in the **Inspector**:

| Field | Value | What It Controls |
|-------|-------|-----------------|
| Forward Speed | `15` | How fast the car moves forward |
| Lane Switch Speed | `10` | How fast the car slides between lanes |
| Lane Width | `2.5` | Distance between lane centers |
| Lane Count | `3` | Number of lanes (left, center, right) |
| Max HP | `3` | Hits before game over |
| Invincibility Duration | `1` | Seconds of invincibility after hit |
| Three Star Max Collisions | `0` | 0 hits = 3 stars |
| Two Star Max Collisions | `2` | 1-2 hits = 2 stars |
| Three Star Max Time | `30` | Under 30s = 3 stars |
| Two Star Max Time | `45` | Under 45s = 2 stars |
| Swipe Threshold | `50` | Min swipe distance (pixels) |
| Swipe Max Duration | `0.3` | Max swipe time (seconds) |
| Target Frame Rate | `60` | Mobile frame rate target |

### Step 5.2: Create LevelData Asset
1. Right-click in `Assets/_Project/ScriptableObjects/` → **Create → FastAndAcro → LevelData**
2. Name it `Level_01_Data`
3. Set these values:

| Field | Value | What It Controls |
|-------|-------|-----------------|
| Level Name | `Level 01` | Display name |
| Country | `USA` | Theme identifier |
| Track Length | `200` | Total track length in units |
| Obstacle Count | `15` | Number of obstacles to spawn |
| Min Obstacle Spacing | `8` | Minimum gap between obstacles |
| Obstacle Start Offset | `20` | How far from start obstacles begin |
| Moving Obstacle Ratio | `0.3` | 30% of obstacles move side-to-side |
| Moving Obstacle Speed | `2` | Speed of moving obstacles |

---

## 6. Build the Game Scene

Delete the default sample scene contents and build from scratch.

### Step 6.0: Create a New Scene
1. Go to **File → New Scene → Basic (Built-in)**
2. Save it: **File → Save As** → navigate to `Assets/_Project/Scenes/` → name it `Level_01`
3. Delete the default "SampleScene" from `Assets/Scenes/` (if it exists)

You should now have an empty scene with just a **Main Camera** and **Directional Light**.

### Step 6.1: Create the Road

The road is a simple stretched cube that the car drives along.

1. **GameObject → 3D Object → Cube**
2. Rename it to `Road`
3. In the **Inspector**, set:
   - **Position:** X=`0`, Y=`-0.5`, Z=`100`
   - **Scale:** X=`10`, Y=`1`, Z=`200`
4. **Remove the Box Collider** component (click the 3 dots → Remove Component)
   - The road doesn't need collision — obstacles handle that
5. Optionally create a material:
   - Right-click in `Assets/_Project/Materials/` → **Create → Material** → name it `Road_Mat`
   - Set **Albedo** color to dark gray (RGB: 60, 60, 60)
   - Drag the material onto the Road object

### Step 6.2: Create the Car

1. **GameObject → 3D Object → Cube**
2. Rename it to `Car`
3. Set **Tag** to `Player` (dropdown at top of Inspector)
4. Set **Layer** to `Player` (if you created it in Step 4.2)
5. In the Inspector, set:
   - **Position:** X=`0`, Y=`0.5`, Z=`0`
   - **Scale:** X=`1.5`, Y=`0.8`, Z=`2.5` (car-ish proportions)
6. The **Box Collider** should already be there — keep it
   - Set **Is Trigger** = `false` (the car is NOT a trigger — it receives trigger events from obstacles)
7. Optionally create a material:
   - Create → Material → name `Car_Mat`
   - Set color to bright red (RGB: 220, 40, 40)
   - Drag onto Car

### Step 6.3: Add Car Scripts

With the **Car** object selected, click **Add Component** for each:

1. **Add Component → CarController**
2. **Add Component → CollisionHandler**

Do NOT add Rigidbody — we're using Transform.Translate (per debate recommendation).

### Step 6.4: Create the Face Display (Child of Car)

1. Right-click on **Car** in the Hierarchy → **3D Object → Quad**
2. Rename it to `FaceQuad`
3. Set:
   - **Position:** X=`0`, Y=`1.5`, Z=`0` (above the car)
   - **Scale:** X=`1`, Y=`1`, Z=`1`
   - **Rotation:** X=`0`, Y=`0`, Z=`0`
4. **Add Component → FaceDisplay**
5. Remove the **Mesh Collider** from the Quad (it doesn't need one)

### Step 6.5: Create Obstacle Prefab

1. **GameObject → 3D Object → Cube**
2. Rename it to `Obstacle`
3. Set **Tag** to `Obstacle`
4. Set **Layer** to `Obstacle` (if created)
5. Set:
   - **Position:** X=`0`, Y=`0.5`, Z=`10`
   - **Scale:** X=`1.5`, Y=`1`, Z=`1.5`
6. On the **Box Collider**:
   - Set **Is Trigger** = `true` ← CRITICAL: obstacles must be triggers
7. **Add Component → Obstacle**
8. Optionally create a material:
   - Create → Material → name `Obstacle_Mat`
   - Set color to orange (RGB: 230, 140, 30)
   - Drag onto Obstacle
9. **Make it a Prefab:**
   - Drag the `Obstacle` object from the Hierarchy into `Assets/_Project/Prefabs/`
   - A blue cube icon appears in Prefabs — that's your prefab
10. **Delete the Obstacle from the scene** (it will be spawned from the pool)

### Step 6.6: Create the Finish Line

1. **GameObject → 3D Object → Cube**
2. Rename it to `FinishLine`
3. Set **Tag** to `FinishLine` (add this tag in Project Settings if needed, or use `Untagged`)
4. Set **Layer** to `FinishLine` (if created)
5. Set:
   - **Position:** X=`0`, Y=`0.5`, Z=`200` (at end of track — LevelManager will reposition this)
   - **Scale:** X=`10`, Y=`2`, Z=`0.5`
6. On the **Box Collider**:
   - Set **Is Trigger** = `true`
7. **Add Component → FinishLine**
8. Optionally create a material:
   - Create → Material → name `FinishLine_Mat`
   - Set color to green (RGB: 40, 200, 40)
   - Make the material semi-transparent: set **Surface Type** to `Transparent`, reduce **Alpha** to `150`

### Step 6.7: Create Manager GameObjects

These are empty GameObjects that hold manager scripts.

#### GameManager
1. **GameObject → Create Empty**
2. Rename to `GameManager`
3. **Add Component → GameManager**

#### LevelManager
1. **GameObject → Create Empty**
2. Rename to `LevelManager`
3. **Add Component → LevelManager**

#### AudioManager
1. **GameObject → Create Empty**
2. Rename to `AudioManager`
3. **Add Component → AudioManager** (this will auto-add an AudioSource)
4. On the **AudioSource** component:
   - **Play On Awake** = `false`
5. Add a **second AudioSource** for engine sound:
   - **Add Component → Audio Source**
   - **Play On Awake** = `false`
   - **Loop** = `true`

#### ObjectPool
1. **GameObject → Create Empty**
2. Rename to `ObstaclePool`
3. **Add Component → ObjectPool**

### Step 6.8: Final Hierarchy

Your scene hierarchy should look like this:

```
Level_01 (Scene)
├── Directional Light
├── Main Camera
├── Road
├── Car                          [Tag: Player]
│   └── FaceQuad                 [FaceDisplay script]
├── FinishLine                   [FinishLine script, Trigger Collider]
├── GameManager                  [GameManager script]
├── LevelManager                 [LevelManager script]
├── AudioManager                 [AudioManager script, 2x AudioSource]
└── ObstaclePool                 [ObjectPool script]
```

---

## 7. Set Up the UI

### Step 7.1: Create the Main Canvas

1. **GameObject → UI → Canvas**
2. Rename to `GameCanvas`
3. On the **Canvas** component:
   - **Render Mode:** `Screen Space - Overlay`
4. On the **Canvas Scaler** component:
   - **UI Scale Mode:** `Scale With Screen Size`
   - **Reference Resolution:** X=`1080`, Y=`1920`
   - **Screen Match Mode:** `Match Width Or Height`
   - **Match:** `0.5`
5. The **Graphic Raycaster** should already be there — leave it

### Step 7.2: Create the Safe Area Panel

1. Right-click on **GameCanvas** → **UI → Panel**
2. Rename to `SafeAreaPanel`
3. **Delete the Image component** on it (we don't want a visible panel)
4. **Add Component → SafeAreaPanel** (the script)
5. On the **RectTransform**:
   - Anchor: stretch-stretch (hold Alt and click the bottom-right anchor preset)
   - All offsets: `0`

**All UI panels below go INSIDE SafeAreaPanel.**

### Step 7.3: Create Main Menu Panel

1. Right-click on **SafeAreaPanel** → **UI → Panel**
2. Rename to `MainMenuPanel`
3. Set **Image** color to dark blue (RGB: 20, 20, 60, Alpha: 240)

#### Add Title Text
1. Right-click on **MainMenuPanel** → **UI → Text - TextMeshPro**
2. Rename to `TitleText`
3. Set the text to: `Fast and Acro`
4. On the **RectTransform**:
   - **Anchor:** top-center
   - **Pos Y:** `-200`
   - **Width:** `800`, **Height:** `150`
5. On **TextMeshPro** component:
   - **Font Size:** `72`
   - **Alignment:** Center + Middle
   - **Color:** White

#### Add Play Button
1. Right-click on **MainMenuPanel** → **UI → Button - TextMeshPro**
2. Rename to `PlayButton`
3. On **RectTransform**:
   - **Anchor:** middle-center
   - **Pos Y:** `0`
   - **Width:** `400`, **Height:** `100`
4. Expand PlayButton → select the child **Text (TMP)**:
   - Set text to: `PLAY`
   - Font Size: `48`
   - Alignment: Center + Middle
5. On the **Button** component:
   - Set **Normal Color** to green (RGB: 40, 180, 40)
   - **Highlighted Color:** brighter green
   - **Pressed Color:** darker green

#### Add Customize Face Button
1. Duplicate the PlayButton (Ctrl+D)
2. Rename to `CustomizeFaceButton`
3. Move **Pos Y** to `-150`
4. Change child text to: `CHOOSE FACE`
5. Change button color to blue

### Step 7.4: Create HUD Panel

1. Right-click on **SafeAreaPanel** → **UI → Panel**
2. Rename to `HUDPanel`
3. **Delete or make transparent** the Image component (Alpha = 0) — HUD should be overlay, not a solid panel

#### HP Bar
1. Right-click on **HUDPanel** → **UI → Image**
2. Rename to `HPBarBackground`
3. On **RectTransform**:
   - **Anchor:** top-left
   - **Pos X:** `150`, **Pos Y:** `-60`
   - **Width:** `250`, **Height:** `30`
4. Set **Image Color** to dark red (RGB: 80, 20, 20)

5. Right-click on **HPBarBackground** → **UI → Image**
6. Rename to `HPBarFill`
7. On **RectTransform**:
   - **Anchor:** stretch-stretch, all offsets `0`
8. Set **Image Type:** `Filled`
9. Set **Fill Method:** `Horizontal`
10. Set **Fill Origin:** `Left`
11. Set **Fill Amount:** `1` (full)
12. Set **Image Color** to bright red (RGB: 220, 40, 40)

#### Timer Text
1. Right-click on **HUDPanel** → **UI → Text - TextMeshPro**
2. Rename to `TimerText`
3. On **RectTransform**:
   - **Anchor:** top-center
   - **Pos Y:** `-60`
   - **Width:** `200`, **Height:** `50`
4. Set text to: `0:00`
5. Font Size: `36`, Alignment: Center, Color: White

#### Pause Button
1. Right-click on **HUDPanel** → **UI → Button - TextMeshPro**
2. Rename to `PauseButton`
3. On **RectTransform**:
   - **Anchor:** top-right
   - **Pos X:** `-80`, **Pos Y:** `-60`
   - **Width:** `80`, **Height:** `80`
4. Child text: `||` (pause symbol), Font Size: `36`

### Step 7.5: Create Pause Panel

1. Right-click on **SafeAreaPanel** → **UI → Panel**
2. Rename to `PausePanel`
3. Set Image color to semi-transparent black (RGBA: 0, 0, 0, 180)

#### Resume Button
1. Right-click on **PausePanel** → **UI → Button - TextMeshPro**
2. Rename to `ResumeButton`
3. Center it, Width: `400`, Height: `100`
4. Pos Y: `50`
5. Child text: `RESUME`, Font Size: `48`

### Step 7.6: Create Level Complete Panel

1. Right-click on **SafeAreaPanel** → **UI → Panel**
2. Rename to `LevelCompletePanel`
3. Set Image color to dark green (RGB: 20, 60, 20, Alpha: 240)

#### Stars
1. Right-click on **LevelCompletePanel** → **UI → Image**
2. Rename to `Star1`
3. On **RectTransform**:
   - **Anchor:** middle-center
   - **Pos X:** `-100`, **Pos Y:** `150`
   - **Width:** `80`, **Height:** `80`
4. Set color to yellow (RGB: 255, 220, 0)
5. Duplicate twice → rename to `Star2` (Pos X: `0`) and `Star3` (Pos X: `100`)

#### Stats Text
1. Right-click on **LevelCompletePanel** → **UI → Text - TextMeshPro**
2. Rename to `CompletionTimeText`
3. Pos Y: `30`, Width: `600`, Height: `50`
4. Text: `Time: 0:00`, Font Size: `36`, Center

5. Duplicate → rename to `CollisionCountText`
6. Pos Y: `-30`
7. Text: `Hits: 0`

#### Buttons
1. Right-click on **LevelCompletePanel** → **UI → Button - TextMeshPro**
2. Rename to `RetryButtonComplete`
3. Pos X: `-120`, Pos Y: `-150`, Width: `200`, Height: `80`
4. Child text: `RETRY`

5. Duplicate → rename to `MenuButtonComplete`
6. Pos X: `120`
7. Child text: `MENU`

### Step 7.7: Create Game Over Panel

1. Right-click on **SafeAreaPanel** → **UI → Panel**
2. Rename to `GameOverPanel`
3. Set Image color to dark red (RGB: 60, 20, 20, Alpha: 240)

#### Game Over Title
1. Right-click on **GameOverPanel** → **UI → Text - TextMeshPro**
2. Text: `GAME OVER`, Font Size: `64`, Center, Pos Y: `100`, Color: Red

#### Buttons
1. Create `RetryButtonGameOver` button — Pos X: `-120`, Pos Y: `-100`, text: `RETRY`
2. Create `MenuButtonGameOver` button — Pos X: `120`, Pos Y: `-100`, text: `MENU`

### Step 7.8: Add UIController Script

1. Select the **GameCanvas** object
2. **Add Component → UIController**

### Step 7.9: Final UI Hierarchy

```
GameCanvas                         [Canvas, CanvasScaler, UIController]
└── SafeAreaPanel                  [SafeAreaPanel script]
    ├── MainMenuPanel
    │   ├── TitleText              [TMP]
    │   ├── PlayButton             [Button]
    │   └── CustomizeFaceButton    [Button]
    ├── HUDPanel
    │   ├── HPBarBackground
    │   │   └── HPBarFill          [Image, Filled]
    │   ├── TimerText              [TMP]
    │   └── PauseButton            [Button]
    ├── PausePanel
    │   └── ResumeButton           [Button]
    ├── LevelCompletePanel
    │   ├── Star1                  [Image]
    │   ├── Star2                  [Image]
    │   ├── Star3                  [Image]
    │   ├── CompletionTimeText     [TMP]
    │   ├── CollisionCountText     [TMP]
    │   ├── RetryButtonComplete    [Button]
    │   └── MenuButtonComplete     [Button]
    └── GameOverPanel
        ├── GameOverText           [TMP]
        ├── RetryButtonGameOver    [Button]
        └── MenuButtonGameOver     [Button]
```

### Step 7.10: Disable All Panels Except Main Menu

1. Select **HUDPanel** → uncheck the checkbox at the top of the Inspector (disables it)
2. Do the same for: **PausePanel**, **LevelCompletePanel**, **GameOverPanel**
3. Only **MainMenuPanel** should be active (checked) at start

---

## 8. Wire All SerializeField References

This is the most important step. Every `[SerializeField]` field needs to be connected in the Inspector by **dragging the correct GameObject** into the slot.

### 8.1: GameManager (select GameManager object)

| Field | Drag This Object Into the Slot |
|-------|-------------------------------|
| Config | `GameConfig` asset (from ScriptableObjects folder) |
| Ui | `GameCanvas` object (has UIController) |
| Level Manager | `LevelManager` object |
| Car | `Car` object (has CarController) |
| Collision Handler | `Car` object (has CollisionHandler) |
| Audio Manager | `AudioManager` object |
| Camera Follow | `Main Camera` object (has CameraFollow) |

### 8.2: CarController (select Car object → CarController component)

| Field | Drag This |
|-------|-----------|
| Config | `GameConfig` asset |

### 8.3: CollisionHandler (select Car object → CollisionHandler component)

| Field | Drag This |
|-------|-----------|
| Config | `GameConfig` asset |
| Game Manager | `GameManager` object |
| Car Renderer | `Car` object (it will pick up the MeshRenderer) |

### 8.4: FaceDisplay (select FaceQuad → FaceDisplay component)

| Field | Drag This |
|-------|-----------|
| Face Renderer | `FaceQuad` object (its MeshRenderer) |
| Height Above Car | `1.5` (default is fine) |
| Preset Faces | Leave empty for now (add textures later) |

### 8.5: FinishLine (select FinishLine object)

| Field | Drag This |
|-------|-----------|
| Game Manager | `GameManager` object |

### 8.6: LevelManager (select LevelManager object)

| Field | Drag This |
|-------|-----------|
| Level Data | `Level_01_Data` asset (from ScriptableObjects) |
| Game Config | `GameConfig` asset |
| Obstacle Pool | `ObstaclePool` object |
| Finish Line | `FinishLine` object |
| Road | `Road` object |

### 8.7: ObjectPool (select ObstaclePool object)

| Field | Drag This |
|-------|-----------|
| Prefab | `Obstacle` prefab (from Prefabs folder — NOT from the scene) |
| Initial Size | `20` |

### 8.8: UIController (select GameCanvas → UIController component)

| Field | Drag This |
|-------|-----------|
| Game Manager | `GameManager` object |
| Main Menu Panel | `MainMenuPanel` object |
| Hud Panel | `HUDPanel` object |
| Level Complete Panel | `LevelCompletePanel` object |
| Game Over Panel | `GameOverPanel` object |
| Pause Panel | `PausePanel` object |
| Hp Bar Fill | `HPBarFill` object (Image component) |
| Timer Text | `TimerText` object (TMP component) |
| Star Images | Set size to `3`, then drag Star1, Star2, Star3 |
| Completion Time Text | `CompletionTimeText` object |
| Collision Count Text | `CollisionCountText` object |
| Play Button | `PlayButton` object |
| Pause Button | `PauseButton` object |
| Resume Button | `ResumeButton` object |
| Retry Button Complete | `RetryButtonComplete` object |
| Retry Button Game Over | `RetryButtonGameOver` object |
| Menu Button Complete | `MenuButtonComplete` object |
| Menu Button Game Over | `MenuButtonGameOver` object |

### 8.9: CameraFollow (select Main Camera)

| Field | Drag This |
|-------|-----------|
| Target | `Car` object |
| Offset | X=`0`, Y=`8`, Z=`-6` |
| Smooth Speed | `8` |
| Look Ahead Z | `5` |

### 8.10: AudioManager (select AudioManager object)

| Field | Drag This |
|-------|-----------|
| Engine Source | The **second** AudioSource component on AudioManager |
| Sound clips | Leave empty for now (add .wav/.ogg files later) |

The first AudioSource is auto-added by `[RequireComponent]` and used for SFX.

---

## 9. Camera Setup

### Step 9.1: Configure Main Camera
1. Select **Main Camera**
2. **Add Component → CameraFollow** (if not already added)
3. Set the fields as shown in section 8.9
4. Set **Clear Flags:** `Solid Color`
5. Set **Background Color:** sky blue (RGB: 135, 206, 235) or dark (RGB: 30, 30, 50)
6. Set **Clipping Planes:**
   - **Near:** `0.3`
   - **Far:** `500`

### Step 9.2: Position Camera for Preview
For initial testing, set the camera transform to:
- **Position:** X=`0`, Y=`8`, Z=`-6`
- **Rotation:** X=`50`, Y=`0`, Z=`0`

This gives a behind-and-above perspective. The CameraFollow script will take over at runtime.

---

## 10. Build Settings and Player Settings

### Step 10.1: Add Scene to Build
1. Go to **File → Build Settings**
2. Click **Add Open Scenes** — `Level_01` should appear in the list
3. Make sure its index is `0` (first scene loaded)

### Step 10.2: Switch Platform to Android
1. In Build Settings, select **Android** from the platform list
2. Click **Switch Platform** (this takes a minute)
3. If Android module isn't installed: go to Unity Hub → Installs → click the gear on your 2022 LTS → Add Modules → check **Android Build Support** (with SDK & NDK)

### Step 10.3: Player Settings
1. Click **Player Settings** in the Build Settings window
2. Set:
   - **Company Name:** Your name
   - **Product Name:** `Fast and Acro`
   - **Default Orientation:** `Portrait` (or `Landscape Left` depending on your game design — most auto-runners use **Portrait**)

### Step 10.4: Resolution and Presentation
Under **Resolution and Presentation**:
- **Default Orientation:** `Portrait`
- **Allowed Orientations:** only Portrait checked

### Step 10.5: Other Settings
Under **Other Settings**:
- **Color Space:** `Linear` (URP requires this)
- **Target API Level:** `API Level 31` or higher (Google Play requirement)
- **Scripting Backend:** `IL2CPP` (required for release, but `Mono` is fine for testing)
- **Target Architectures:** check `ARM64` (uncheck `ARMv7` if only targeting modern devices)

---

## 11. Testing Checklist

### Before Testing
- [ ] Save the scene (Ctrl+S)
- [ ] Check Console for zero errors
- [ ] All SerializeField slots are filled (no "None" or "Missing")

### Basic Tests (Press Play in Editor)

#### Test 1: Main Menu Appears
- [ ] Press **Play** → Main Menu panel is visible
- [ ] HUD, LevelComplete, GameOver panels are NOT visible
- [ ] "PLAY" button is visible and clickable

#### Test 2: Game Starts
- [ ] Click **PLAY** → Main Menu disappears, HUD appears
- [ ] Car starts moving forward automatically
- [ ] Camera follows the car smoothly
- [ ] Timer counts up in the HUD
- [ ] HP bar shows full (all green/red)

#### Test 3: Lane Switching
- [ ] Press **Left Arrow** → car moves to left lane
- [ ] Press **Right Arrow** → car moves to right lane
- [ ] Car cannot move beyond the leftmost or rightmost lane
- [ ] Movement is smooth (lerp animation between lanes)

#### Test 4: Obstacles Exist
- [ ] Obstacles appear along the track
- [ ] Some obstacles move side-to-side (if movingObstacleRatio > 0)
- [ ] Obstacles have orange/colored material (not white default)

#### Test 5: Collision Works
- [ ] Drive into an obstacle → HP bar decreases
- [ ] Car flashes during invincibility (1 second)
- [ ] During invincibility, hitting another obstacle does NOT reduce HP
- [ ] After invincibility ends, car returns to normal color

#### Test 6: Game Over
- [ ] Get hit 3 times (with maxHP=3) → Game Over screen appears
- [ ] Car stops moving
- [ ] "RETRY" button reloads the level
- [ ] "MENU" button shows the main menu (or reloads scene)

#### Test 7: Level Complete
- [ ] Reach the finish line (green block at Z=200)
- [ ] Level Complete screen appears with stars
- [ ] 0 hits → 3 stars (all yellow)
- [ ] 1-2 hits → 2 stars
- [ ] 3+ hits → 1 star
- [ ] Time and collision count display correctly
- [ ] "RETRY" and "MENU" buttons work

#### Test 8: Pause
- [ ] Press pause button (||) during gameplay → Pause panel appears
- [ ] Game freezes (Time.timeScale = 0)
- [ ] Press "RESUME" → game continues
- [ ] Timer pauses during pause and resumes correctly

#### Test 9: Camera
- [ ] Camera follows the car smoothly (no jitter)
- [ ] Camera shows obstacles ahead of the car (look-ahead)
- [ ] Camera doesn't clip through the road

#### Test 10: Face Display (Optional)
- [ ] If preset face textures are assigned, a quad appears above the car
- [ ] The quad always faces the camera (billboard)
- [ ] "CHOOSE FACE" button exists on main menu (functionality comes later)

### Mobile Testing (Build to Android Device)

1. **File → Build and Run** (with Android device connected via USB)
2. Test:
   - [ ] Touch input works (swipe left/right to change lanes)
   - [ ] Game runs at 60 FPS (check with Unity Profiler or FPS counter)
   - [ ] UI fits within safe area (no elements behind notch)
   - [ ] Text is readable at phone resolution
   - [ ] Sound effects play (if audio clips assigned)

---

## 12. Troubleshooting

### "NullReferenceException" on Play
- **Cause:** A `[SerializeField]` field is not wired in the Inspector
- **Fix:** Read the error message — it tells you which script and line. Go to that script in the Inspector and fill in the missing reference. Check section 8 of this guide.

### Car doesn't move
- Check that `GameManager.StartGame()` is being called (Play button wired to UIController)
- Check that `CarController._config` is assigned
- Check that `CarController.SetMovementEnabled(true)` is called (GameManager does this)

### Obstacles don't appear
- Check that `ObjectPool._prefab` is assigned (drag the Obstacle PREFAB, not a scene object)
- Check that `LevelManager._obstaclePool` is assigned
- Check that `LevelData.obstacleCount` > 0

### Car passes through obstacles without taking damage
- Check that the **Obstacle prefab** has **Is Trigger = true** on its Collider
- Check that the Obstacle is tagged `Obstacle`
- Check that the Car is tagged `Player`
- Check that `CollisionHandler._gameManager` is assigned

### UI panels don't switch
- Check that `UIController._gameManager` is assigned
- Check that all panel fields are assigned in UIController
- Check that panels are children of SafeAreaPanel (inside GameCanvas)

### Camera jitters
- Make sure CameraFollow is on the Main Camera, not on a separate object
- Make sure `_target` points to the Car object
- Try increasing `_smoothSpeed` to 10-12

### Compile error: "TMPro not found"
- Import TextMesh Pro: Window → TextMeshPro → Import TMP Essential Resources

### Stars always show 1 star
- Check `GameConfig` values — `threeStarMaxCollisions` should be `0`, not a high number
- Check that `_collisionCount` is being incremented (add a Debug.Log temporarily)

---

## Quick Reference: Complete Hierarchy

```
Level_01 (Scene)
│
├── Directional Light
│
├── Main Camera                    [CameraFollow]
│     Target → Car
│     Offset → (0, 8, -6)
│
├── Road                           [no scripts, no collider]
│     Scale → (10, 1, 200)
│
├── Car                            [CarController, CollisionHandler]  Tag: Player
│   └── FaceQuad                   [FaceDisplay]
│
├── FinishLine                     [FinishLine]  Trigger Collider
│
├── GameManager                    [GameManager]
│     Config → GameConfig asset
│     All references wired
│
├── LevelManager                   [LevelManager]
│     LevelData → Level_01_Data asset
│
├── AudioManager                   [AudioManager, 2x AudioSource]
│
├── ObstaclePool                   [ObjectPool]
│     Prefab → Obstacle prefab
│     Initial Size → 20
│
└── GameCanvas                     [Canvas, CanvasScaler, UIController]
    └── SafeAreaPanel              [SafeAreaPanel]
        ├── MainMenuPanel          (active at start)
        │   ├── TitleText
        │   ├── PlayButton
        │   └── CustomizeFaceButton
        ├── HUDPanel               (inactive at start)
        │   ├── HPBarBackground
        │   │   └── HPBarFill
        │   ├── TimerText
        │   └── PauseButton
        ├── PausePanel             (inactive at start)
        │   └── ResumeButton
        ├── LevelCompletePanel     (inactive at start)
        │   ├── Star1, Star2, Star3
        │   ├── CompletionTimeText
        │   ├── CollisionCountText
        │   ├── RetryButtonComplete
        │   └── MenuButtonComplete
        └── GameOverPanel          (inactive at start)
            ├── GameOverText
            ├── RetryButtonGameOver
            └── MenuButtonGameOver
```

---

**You're done!** Press Play and test each item in the checklist. If something doesn't work, check the Troubleshooting section. Good luck building Fast and Acro!
