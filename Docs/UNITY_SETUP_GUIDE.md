# Fast and Acro — Quick Setup Guide

Everything is pre-built. Just 4 steps.

---

## Step 1: Create a Unity Project

1. Open **Unity Hub**
2. Click **New project**
3. Select **3D (URP)** template
4. Unity version: **2022.3 LTS** (any 2022 LTS patch)
5. Name it `FastAndAcro`, click **Create project**

## Step 2: Copy Assets

1. Delete everything inside the new project's `Assets/` folder EXCEPT:
   - `Settings/` (URP settings — keep this)
   - `TextMesh Pro/` (if present — keep this)
2. Copy this repo's entire `Assets/_Project/` folder into the project's `Assets/`
3. Final structure should be:
   ```
   Assets/
     _Project/           <-- from this repo
       Scripts/
       Data/
       Prefabs/
       Scenes/
     Settings/            <-- from URP template
   ```
4. Switch to Unity — wait for compilation (watch bottom progress bar)

## Step 3: Import TextMesh Pro (if not already present)

1. Go to **Window > TextMeshPro > Import TMP Essential Resources**
2. Click **Import** in the dialog

## Step 4: Open the Scene and Play

1. In the Project window, navigate to `Assets/_Project/Scenes/`
2. Double-click **Level01**
3. Press **Play**

---

## What's Pre-Built

The scene comes with everything wired:

| GameObject | Scripts | Purpose |
|-----------|---------|---------|
| Main Camera | CameraFollow | Follows car with look-ahead |
| Directional Light | — | Scene lighting |
| Car (+ FaceQuad child) | CarController, CollisionHandler, FaceDisplay | Player car |
| Road | — | Track surface |
| FinishLine | FinishLine | Level completion trigger |
| GameManager | GameManager | State machine + scoring |
| LevelManager | LevelManager, ObjectPool | Level setup + obstacle pooling |
| AudioManager | AudioManager + 2x AudioSource | Sound effects |
| GameCanvas | Canvas, CanvasScaler, UIController | All UI |
| SafeAreaPanel | SafeAreaPanel | Notch handling |
| 5 UI Panels | — | Menu, HUD, Pause, Complete, GameOver |

## Pre-Built Assets

| Asset | Type | Location |
|-------|------|----------|
| DefaultGameConfig | GameConfig (ScriptableObject) | `_Project/Data/` |
| Level01Data | LevelData (ScriptableObject) | `_Project/Data/` |
| Obstacle | Prefab | `_Project/Prefabs/` |

## Controls

- **Arrow keys / A,D**: Switch lanes (editor)
- **Swipe left/right**: Switch lanes (mobile)
- Car moves forward automatically

## What You Still Need To Do

The UI panels are structural (GameObjects exist, scripts wired) but **buttons and text elements need to be added manually in Unity** since TextMeshPro components require TMP to be imported first and use dynamic font atlas generation that can't be pre-serialized.

Quick steps:
1. Open Level01 scene
2. Select each panel (MainMenuPanel, HUDPanel, etc.)
3. Add child Button-TextMeshPro and Text-TextMeshPro elements
4. Drag them into the UIController's button/text slots in the Inspector
5. Follow `Docs/PLAN.md` section 7 for exact UI layout

## Build for Android

1. **File > Build Settings** > Select **Android** > **Switch Platform**
2. **Add Open Scenes** to add Level01
3. **Player Settings**: set orientation to Portrait
4. **Build and Run** with a connected Android device

---

That's it. The architecture, scripts, scene hierarchy, and asset references are all ready.
