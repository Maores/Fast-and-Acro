using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Builds the entire Fast and Acro game scene from scratch.
/// Menu: Tools > Fast and Acro > Build Scene
/// </summary>
public static class SceneBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/GameScene.unity";
    private const string DataPath = "Assets/_Project/Data/";
    private const string PrefabPath = "Assets/_Project/Prefabs/";

    [MenuItem("Tools/Fast and Acro/Build Scene")]
    public static void BuildScene()
    {
        // Ensure tags exist
        TagCreator.EnsureTags();

        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Load assets
        var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(DataPath + "DefaultGameConfig.asset");
        var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(DataPath + "Level01Data.asset");
        var obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "Obstacle.prefab");

        if (gameConfig == null) Debug.LogWarning("[SceneBuilder] DefaultGameConfig.asset not found at " + DataPath);
        if (levelData == null) Debug.LogWarning("[SceneBuilder] Level01Data.asset not found at " + DataPath);
        if (obstaclePrefab == null) Debug.LogWarning("[SceneBuilder] Obstacle.prefab not found at " + PrefabPath);

        // ===== 1. Main Camera =====
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0f, 5f, -10f);
        cameraGO.transform.eulerAngles = new Vector3(20f, 0f, 0f);
        var camera = cameraGO.AddComponent<Camera>();
        cameraGO.AddComponent<AudioListener>();
        var cameraFollow = cameraGO.AddComponent<CameraFollow>();

        // ===== 2. Directional Light =====
        var lightGO = new GameObject("Directional Light");
        lightGO.transform.eulerAngles = new Vector3(50f, -30f, 0f);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.84f); // Warm sunlight
        light.intensity = 1f;

        // ===== 3. Car =====
        var carGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        carGO.name = "Car";
        carGO.tag = "Player";
        carGO.transform.position = new Vector3(0f, 0.5f, 0f);

        // Replace default collider with sized BoxCollider
        Object.DestroyImmediate(carGO.GetComponent<BoxCollider>());
        var carCollider = carGO.AddComponent<BoxCollider>();
        carCollider.size = new Vector3(1f, 1f, 2f);
        carCollider.isTrigger = true; // Needs trigger for obstacle detection

        var carRb = carGO.AddComponent<Rigidbody>();
        carRb.useGravity = false;
        carRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        var carController = carGO.AddComponent<CarController>();
        var collisionHandler = carGO.AddComponent<CollisionHandler>();
        var carRenderer = carGO.GetComponent<Renderer>();

        // Face quad child
        var faceQuadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        faceQuadGO.name = "FaceQuad";
        faceQuadGO.transform.SetParent(carGO.transform);
        faceQuadGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        // Remove collider from quad (not needed)
        Object.DestroyImmediate(faceQuadGO.GetComponent<Collider>());
        var faceDisplay = faceQuadGO.AddComponent<FaceDisplay>();

        // ===== 4. Road =====
        var roadGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roadGO.name = "Road";
        roadGO.transform.position = new Vector3(0f, 0f, 100f);
        roadGO.transform.localScale = new Vector3(3f, 0.1f, 200f);
        // BoxCollider already exists from CreatePrimitive

        // ===== 5. FinishLine =====
        var finishGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishGO.name = "FinishLine";
        finishGO.transform.position = new Vector3(0f, 1f, 195f);
        finishGO.transform.localScale = new Vector3(3f, 2f, 0.5f);
        var finishCollider = finishGO.GetComponent<BoxCollider>();
        finishCollider.isTrigger = true;
        var finishLine = finishGO.AddComponent<FinishLine>();
        // Give it a distinct color
        var finishRenderer = finishGO.GetComponent<Renderer>();
        var finishMat = new Material(Shader.Find("Standard"));
        finishMat.color = Color.green;
        finishRenderer.sharedMaterial = finishMat;

        // ===== 6. GameManager =====
        var gmGO = new GameObject("GameManager");
        var gameManager = gmGO.AddComponent<GameManager>();

        // ===== 7. LevelManager =====
        var lmGO = new GameObject("LevelManager");
        var levelManager = lmGO.AddComponent<LevelManager>();
        var objectPool = lmGO.AddComponent<ObjectPool>();

        // ===== 8. AudioManager =====
        var amGO = new GameObject("AudioManager");
        var audioManager = amGO.AddComponent<AudioManager>();
        var sfxSource = amGO.GetComponent<AudioSource>(); // Added by RequireComponent
        var engineSource = amGO.AddComponent<AudioSource>();
        engineSource.playOnAwake = false;

        // ===== 9. GameCanvas =====
        var canvasGO = new GameObject("GameCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        var uiController = canvasGO.AddComponent<UIController>();

        // --- SafeAreaPanel ---
        var safeAreaGO = CreateUIObject("SafeAreaPanel", canvasGO.transform);
        StretchRectTransform(safeAreaGO);
        safeAreaGO.AddComponent<SafeAreaPanel>();

        // --- MainMenuPanel ---
        var mainMenuPanel = CreateUIObject("MainMenuPanel", safeAreaGO.transform);
        StretchRectTransform(mainMenuPanel);
        mainMenuPanel.SetActive(true);

        // Title text
        var titleGO = CreateTMPText("TitleText", mainMenuPanel.transform, "Fast and Acro",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, -100f), new Vector2(0f, -50f),
            48, TextAlignmentOptions.Top);

        // Play button
        var playButton = CreateTMPButton("PlayButton", mainMenuPanel.transform, "Play",
            new Vector2(0.5f, 0.5f), new Vector2(300f, 80f));

        // --- HUDPanel ---
        var hudPanel = CreateUIObject("HUDPanel", safeAreaGO.transform);
        StretchRectTransform(hudPanel);
        hudPanel.SetActive(false);

        // HP bar background
        var hpBarBG = CreateUIObject("HPBarBackground", hudPanel.transform);
        var hpBGRect = hpBarBG.GetComponent<RectTransform>();
        hpBGRect.anchorMin = new Vector2(0.1f, 0.92f);
        hpBGRect.anchorMax = new Vector2(0.9f, 0.96f);
        hpBGRect.offsetMin = Vector2.zero;
        hpBGRect.offsetMax = Vector2.zero;
        var hpBGImage = hpBarBG.AddComponent<Image>();
        hpBGImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // HP bar fill
        var hpBarFillGO = CreateUIObject("HPBarFill", hpBarBG.transform);
        StretchRectTransform(hpBarFillGO);
        var hpBarFill = hpBarFillGO.AddComponent<Image>();
        hpBarFill.color = Color.red;
        hpBarFill.type = Image.Type.Filled;
        hpBarFill.fillMethod = Image.FillMethod.Horizontal;
        hpBarFill.fillAmount = 1f;

        // Timer text
        var timerTextGO = CreateTMPText("TimerText", hudPanel.transform, "0:00",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-75f, -60f), new Vector2(75f, -20f),
            36, TextAlignmentOptions.Center);
        var timerText = timerTextGO.GetComponent<TMP_Text>();

        // Pause button
        var pauseButton = CreateTMPButton("PauseButton", hudPanel.transform, "||",
            new Vector2(0.9f, 0.95f), new Vector2(60f, 60f));

        // --- PausePanel ---
        var pausePanel = CreateUIObject("PausePanel", safeAreaGO.transform);
        StretchRectTransform(pausePanel);
        pausePanel.SetActive(false);
        var pauseBG = pausePanel.AddComponent<Image>();
        pauseBG.color = new Color(0f, 0f, 0f, 0.7f);

        CreateTMPText("PausedText", pausePanel.transform, "PAUSED",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f),
            new Vector2(-150f, -30f), new Vector2(150f, 30f),
            48, TextAlignmentOptions.Center);

        var resumeButton = CreateTMPButton("ResumeButton", pausePanel.transform, "Resume",
            new Vector2(0.5f, 0.5f), new Vector2(300f, 80f));

        var quitButtonPause = CreateTMPButton("QuitButton", pausePanel.transform, "Quit",
            new Vector2(0.5f, 0.4f), new Vector2(300f, 80f));

        // --- LevelCompletePanel ---
        var lcPanel = CreateUIObject("LevelCompletePanel", safeAreaGO.transform);
        StretchRectTransform(lcPanel);
        lcPanel.SetActive(false);
        var lcBG = lcPanel.AddComponent<Image>();
        lcBG.color = new Color(0f, 0f, 0f, 0.7f);

        CreateTMPText("LevelCompleteText", lcPanel.transform, "Level Complete!",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f),
            new Vector2(-200f, -30f), new Vector2(200f, 30f),
            48, TextAlignmentOptions.Center);

        // Star display (3 Image placeholders)
        var starContainer = CreateUIObject("StarContainer", lcPanel.transform);
        var starContainerRect = starContainer.GetComponent<RectTransform>();
        starContainerRect.anchorMin = new Vector2(0.5f, 0.6f);
        starContainerRect.anchorMax = new Vector2(0.5f, 0.6f);
        starContainerRect.sizeDelta = new Vector2(240f, 60f);
        var starLayout = starContainer.AddComponent<HorizontalLayoutGroup>();
        starLayout.spacing = 20f;
        starLayout.childAlignment = TextAnchor.MiddleCenter;

        var starImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var starGO = CreateUIObject($"Star{i}", starContainer.transform);
            var starImg = starGO.AddComponent<Image>();
            starImg.color = Color.yellow;
            var starLE = starGO.AddComponent<LayoutElement>();
            starLE.preferredWidth = 60f;
            starLE.preferredHeight = 60f;
            starImages[i] = starImg;
        }

        // Completion time text
        var completionTimeGO = CreateTMPText("CompletionTimeText", lcPanel.transform, "Time: 0:00",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-150f, -15f), new Vector2(150f, 15f),
            28, TextAlignmentOptions.Center);
        var completionTimeText = completionTimeGO.GetComponent<TMP_Text>();

        // Collision count text
        var collisionCountGO = CreateTMPText("CollisionCountText", lcPanel.transform, "Hits: 0",
            new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f),
            new Vector2(-150f, -15f), new Vector2(150f, 15f),
            28, TextAlignmentOptions.Center);
        var collisionCountText = collisionCountGO.GetComponent<TMP_Text>();

        var nextButton = CreateTMPButton("NextButton", lcPanel.transform, "Next",
            new Vector2(0.5f, 0.35f), new Vector2(300f, 80f));

        var menuButtonLC = CreateTMPButton("MenuButton", lcPanel.transform, "Menu",
            new Vector2(0.5f, 0.25f), new Vector2(300f, 80f));

        // --- GameOverPanel ---
        var goPanel = CreateUIObject("GameOverPanel", safeAreaGO.transform);
        StretchRectTransform(goPanel);
        goPanel.SetActive(false);
        var goBG = goPanel.AddComponent<Image>();
        goBG.color = new Color(0f, 0f, 0f, 0.7f);

        CreateTMPText("GameOverText", goPanel.transform, "Game Over",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f),
            new Vector2(-200f, -30f), new Vector2(200f, 30f),
            48, TextAlignmentOptions.Center);

        var retryButtonGO_go = CreateTMPButton("RetryButton", goPanel.transform, "Retry",
            new Vector2(0.5f, 0.5f), new Vector2(300f, 80f));

        var menuButtonGO_go = CreateTMPButton("MenuButton", goPanel.transform, "Menu",
            new Vector2(0.5f, 0.4f), new Vector2(300f, 80f));

        // ===== 10. EventSystem =====
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // ============================================================
        // WIRE SERIALIZED FIELDS VIA SerializedObject
        // ============================================================

        // --- GameManager ---
        SetSerializedRef(gameManager, "_config", gameConfig);
        SetSerializedRef(gameManager, "_ui", uiController);
        SetSerializedRef(gameManager, "_levelManager", levelManager);
        SetSerializedRef(gameManager, "_car", carController);
        SetSerializedRef(gameManager, "_collisionHandler", collisionHandler);
        SetSerializedRef(gameManager, "_audioManager", audioManager);
        SetSerializedRef(gameManager, "_cameraFollow", cameraFollow);

        // --- CarController ---
        SetSerializedRef(carController, "_config", gameConfig);

        // --- CollisionHandler ---
        SetSerializedRef(collisionHandler, "_config", gameConfig);
        SetSerializedRef(collisionHandler, "_gameManager", gameManager);
        SetSerializedRef(collisionHandler, "_carRenderer", carRenderer);

        // --- LevelManager ---
        SetSerializedRef(levelManager, "_levelData", levelData);
        SetSerializedRef(levelManager, "_gameConfig", gameConfig);
        SetSerializedRef(levelManager, "_obstaclePool", objectPool);
        SetSerializedRef(levelManager, "_finishLine", finishLine);
        SetSerializedRef(levelManager, "_road", roadGO.transform);

        // --- ObjectPool ---
        SetSerializedRef(objectPool, "_prefab", obstaclePrefab);

        // --- CameraFollow ---
        SetSerializedRef(cameraFollow, "_target", carGO.transform);

        // --- FinishLine ---
        SetSerializedRef(finishLine, "_gameManager", gameManager);

        // --- AudioManager ---
        SetSerializedRef(audioManager, "_engineSource", engineSource);

        // --- FaceDisplay ---
        SetSerializedRef(faceDisplay, "_faceRenderer", faceQuadGO.GetComponent<Renderer>());

        // --- UIController ---
        SetSerializedRef(uiController, "_gameManager", gameManager);
        SetSerializedRef(uiController, "_mainMenuPanel", mainMenuPanel);
        SetSerializedRef(uiController, "_hudPanel", hudPanel);
        SetSerializedRef(uiController, "_levelCompletePanel", lcPanel);
        SetSerializedRef(uiController, "_gameOverPanel", goPanel);
        SetSerializedRef(uiController, "_pausePanel", pausePanel);
        SetSerializedRef(uiController, "_hpBarFill", hpBarFill);
        SetSerializedRef(uiController, "_timerText", timerText);
        SetSerializedRef(uiController, "_completionTimeText", completionTimeText);
        SetSerializedRef(uiController, "_collisionCountText", collisionCountText);

        // Star images array
        SetSerializedArray(uiController, "_starImages", starImages);

        // Buttons
        SetSerializedRef(uiController, "_playButton", playButton.GetComponent<Button>());
        SetSerializedRef(uiController, "_pauseButton", pauseButton.GetComponent<Button>());
        SetSerializedRef(uiController, "_resumeButton", resumeButton.GetComponent<Button>());
        SetSerializedRef(uiController, "_retryButtonComplete", nextButton.GetComponent<Button>()); // Next level acts as retry on complete
        SetSerializedRef(uiController, "_retryButtonGameOver", retryButtonGO_go.GetComponent<Button>());
        SetSerializedRef(uiController, "_menuButtonComplete", menuButtonLC.GetComponent<Button>());
        SetSerializedRef(uiController, "_menuButtonGameOver", menuButtonGO_go.GetComponent<Button>());

        // ============================================================
        // SAVE SCENE
        // ============================================================

        // Ensure Scenes directory exists
        string scenesDir = Path.GetDirectoryName(ScenePath);
        if (!Directory.Exists(scenesDir))
        {
            Directory.CreateDirectory(scenesDir);
            AssetDatabase.Refresh();
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Scene Builder",
            "GameScene built successfully!\n\n" +
            "Scene saved to:\n" + ScenePath + "\n\n" +
            "All SerializeField references have been wired.\n" +
            "Check the Console for any warnings about missing assets.",
            "OK");

        Debug.Log("[SceneBuilder] GameScene built and saved to " + ScenePath);
    }

    // ============================================================
    // UTILITY: Set private [SerializeField] via SerializedObject
    // ============================================================

    private static void SetSerializedRef(Component component, string fieldName, Object value)
    {
        if (value == null)
        {
            Debug.LogWarning($"[SceneBuilder] Skipping null ref: {component.GetType().Name}.{fieldName}");
            return;
        }

        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"[SceneBuilder] Property not found: {component.GetType().Name}.{fieldName}");
        }
    }

    private static void SetSerializedArray(Component component, string fieldName, Object[] values)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.isArray)
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"[SceneBuilder] Array property not found: {component.GetType().Name}.{fieldName}");
        }
    }

    // ============================================================
    // UI HELPERS
    // ============================================================

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchRectTransform(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return go;
    }

    private static GameObject CreateTMPButton(string name, Transform parent, string label,
        Vector2 anchorPosition, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorPosition;
        rt.anchorMax = anchorPosition;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue button

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.8f);
        button.colors = colors;

        // Text child
        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);

        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }
}
