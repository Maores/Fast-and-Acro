using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility that ensures the SkinsButton is positioned below PlayButton
/// and that UIController._skinsButton is wired in the Inspector.
///
/// Runs automatically after every compile via [InitializeOnLoad].
/// Also available manually at: Tools > Fix Skins Button
/// </summary>
[InitializeOnLoad]
public static class FixSkinsButton
{
    // Guard so we only run once per domain reload, not every editor tick.
    static FixSkinsButton()
    {
        // Defer execution until the editor is fully initialised and a scene is loaded.
        EditorApplication.delayCall += RunFix;
    }

    [MenuItem("Tools/Fix Skins Button")]
    public static void RunFix()
    {
        // Never run during play mode — MarkSceneDirty throws InvalidOperationException.
        if (EditorApplication.isPlaying) return;

        // Only operate when a scene with actual content is loaded.
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.Log("[FixSkinsButton] No scene loaded — skipping.");
            return;
        }

        RectTransform skinsRect  = FindRectTransform("SkinsButton");
        RectTransform playRect   = FindRectTransform("PlayButton");

        if (skinsRect == null) { Debug.LogWarning("[FixSkinsButton] SkinsButton RectTransform not found."); return; }
        if (playRect  == null) { Debug.LogWarning("[FixSkinsButton] PlayButton RectTransform not found.");  return; }

        bool changed = false;

        // --- Fix 1: PlayButton anchors (upper slot) ---
        // Target: anchorMin (0.3, 0.48)  anchorMax (0.7, 0.58)
        var playTargetMin = new Vector2(0.3f, 0.48f);
        var playTargetMax = new Vector2(0.7f, 0.58f);
        if (!ApproxEqual(playRect.anchorMin, playTargetMin) ||
            !ApproxEqual(playRect.anchorMax, playTargetMax) ||
            playRect.anchoredPosition != Vector2.zero)
        {
            Undo.RecordObject(playRect, "Fix PlayButton anchors");
            playRect.anchorMin        = playTargetMin;
            playRect.anchorMax        = playTargetMax;
            playRect.anchoredPosition = Vector2.zero;
            playRect.sizeDelta        = Vector2.zero;
            playRect.pivot            = new Vector2(0.5f, 0.5f);
            changed = true;
            Debug.Log("[FixSkinsButton] PlayButton anchors corrected.");
        }

        // --- Fix 2: SkinsButton anchors (lower slot, gap below PlayButton) ---
        // Target: anchorMin (0.3, 0.33)  anchorMax (0.7, 0.43)
        var skinsTargetMin = new Vector2(0.3f, 0.33f);
        var skinsTargetMax = new Vector2(0.7f, 0.43f);
        if (!ApproxEqual(skinsRect.anchorMin, skinsTargetMin) ||
            !ApproxEqual(skinsRect.anchorMax, skinsTargetMax) ||
            skinsRect.anchoredPosition != Vector2.zero)
        {
            Undo.RecordObject(skinsRect, "Fix SkinsButton anchors");
            skinsRect.anchorMin        = skinsTargetMin;
            skinsRect.anchorMax        = skinsTargetMax;
            skinsRect.anchoredPosition = Vector2.zero;
            skinsRect.sizeDelta        = Vector2.zero;
            skinsRect.pivot            = new Vector2(0.5f, 0.5f);
            skinsRect.localScale       = Vector3.one;
            skinsRect.localRotation    = Quaternion.identity;
            changed = true;
            Debug.Log("[FixSkinsButton] SkinsButton anchors corrected.");
        }

        // --- Fix 3: SkinsButton image colour (blue to distinguish from green PlayButton) ---
        var img = skinsRect.GetComponent<Image>();
        if (img != null)
        {
            var targetColor = new Color(0.2f, 0.5f, 0.7f, 1f);
            if (!ApproxEqual(img.color, targetColor))
            {
                Undo.RecordObject(img, "Fix SkinsButton color");
                img.color = targetColor;
                changed = true;
                Debug.Log("[FixSkinsButton] SkinsButton color corrected.");
            }
        }

        // --- Fix 4: Wire _skinsButton SerializeField on UIController ---
        var uiController = Object.FindFirstObjectByType<UIController>(FindObjectsInactive.Include);
        if (uiController != null)
        {
            var so   = new SerializedObject(uiController);
            var prop = so.FindProperty("_skinsButton");
            if (prop != null)
            {
                var btn = skinsRect.GetComponent<Button>();
                if (prop.objectReferenceValue != (Object)btn)
                {
                    prop.objectReferenceValue = btn;
                    so.ApplyModifiedProperties();
                    changed = true;
                    Debug.Log("[FixSkinsButton] _skinsButton wired on UIController.");
                }
            }
            else
            {
                // Property missing = UIController hasn't been compiled with _skinsButton yet.
                Debug.LogWarning("[FixSkinsButton] _skinsButton property not found on UIController. " +
                                 "Ensure UIController.cs contains: [SerializeField] private Button _skinsButton;");
            }
        }
        else
        {
            Debug.LogWarning("[FixSkinsButton] UIController not found in scene.");
        }

        // --- Fix 5: Wire CarSkinUI references (_backButton, _skinManager, _gameManager) ---
        var carSkinUI = Object.FindFirstObjectByType<CarSkinUI>(FindObjectsInactive.Include);
        if (carSkinUI != null)
        {
            var cso = new SerializedObject(carSkinUI);

            // Wire _backButton
            var backBtnProp = cso.FindProperty("_backButton");
            if (backBtnProp != null)
            {
                // Find BackButton inside CarSkinPanel
                var backBtnRT = FindRectTransform("BackButton");
                if (backBtnRT != null)
                {
                    var backBtn = backBtnRT.GetComponent<Button>();
                    if (backBtnProp.objectReferenceValue != (Object)backBtn)
                    {
                        backBtnProp.objectReferenceValue = backBtn;
                        changed = true;
                        Debug.Log("[FixSkinsButton] CarSkinUI._backButton wired.");
                    }
                }
            }

            // Wire _gameManager
            var gmProp = cso.FindProperty("_gameManager");
            if (gmProp != null)
            {
                var gm = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
                if (gm != null && gmProp.objectReferenceValue != (Object)gm)
                {
                    gmProp.objectReferenceValue = gm;
                    changed = true;
                    Debug.Log("[FixSkinsButton] CarSkinUI._gameManager wired.");
                }
            }

            // Wire _skinManager
            var smProp = cso.FindProperty("_skinManager");
            if (smProp != null)
            {
                var sm = Object.FindFirstObjectByType<CarSkinManager>(FindObjectsInactive.Include);
                if (sm != null && smProp.objectReferenceValue != (Object)sm)
                {
                    smProp.objectReferenceValue = sm;
                    changed = true;
                    Debug.Log("[FixSkinsButton] CarSkinUI._skinManager wired.");
                }
            }

            // Wire _skinPanel (the CarSkinPanel GameObject itself)
            var panelProp = cso.FindProperty("_skinPanel");
            if (panelProp != null && panelProp.objectReferenceValue == null)
            {
                panelProp.objectReferenceValue = carSkinUI.gameObject;
                changed = true;
                Debug.Log("[FixSkinsButton] CarSkinUI._skinPanel wired.");
            }

            // Wire _skinButtonContainer (SkinGrid)
            var containerProp = cso.FindProperty("_skinButtonContainer");
            if (containerProp != null)
            {
                var grid = FindRectTransform("SkinGrid");
                if (grid != null && containerProp.objectReferenceValue != (Object)grid)
                {
                    containerProp.objectReferenceValue = grid;
                    changed = true;
                    Debug.Log("[FixSkinsButton] CarSkinUI._skinButtonContainer wired.");
                }
            }

            // Wire _skinButtonPrefab
            var prefabProp = cso.FindProperty("_skinButtonPrefab");
            if (prefabProp != null && prefabProp.objectReferenceValue == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/SkinButton.prefab");
                if (prefab != null)
                {
                    prefabProp.objectReferenceValue = prefab;
                    changed = true;
                    Debug.Log("[FixSkinsButton] CarSkinUI._skinButtonPrefab wired.");
                }
            }

            // Wire _coinBalanceText
            var coinProp = cso.FindProperty("_coinBalanceText");
            if (coinProp != null)
            {
                var coinText = FindRectTransform("CoinBalance");
                if (coinText != null)
                {
                    var tmp = coinText.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null && coinProp.objectReferenceValue != (Object)tmp)
                    {
                        coinProp.objectReferenceValue = tmp;
                        changed = true;
                        Debug.Log("[FixSkinsButton] CarSkinUI._coinBalanceText wired.");
                    }
                }
            }

            if (changed) cso.ApplyModifiedProperties();
        }

        // --- Fix 6: Wire GameManager._carSkinUI ---
        var gmObj = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (gmObj != null && carSkinUI != null)
        {
            var gmSo = new SerializedObject(gmObj);
            var csuiProp = gmSo.FindProperty("_carSkinUI");
            if (csuiProp != null && csuiProp.objectReferenceValue != (Object)carSkinUI)
            {
                csuiProp.objectReferenceValue = carSkinUI;
                gmSo.ApplyModifiedProperties();
                changed = true;
                Debug.Log("[FixSkinsButton] GameManager._carSkinUI wired.");
            }
        }

        // --- Mark scene dirty only if something actually changed ---
        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[FixSkinsButton] Done. Save the scene with Ctrl+S.");
        }
        else
        {
            Debug.Log("[FixSkinsButton] Everything already correct — no changes needed.");
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>Find a RectTransform anywhere in the scene by GameObject name.</summary>
    private static RectTransform FindRectTransform(string goName)
    {
        var all = Object.FindObjectsByType<RectTransform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rt in all)
            if (rt.gameObject.name == goName)
                return rt;
        return null;
    }

    private static bool ApproxEqual(Vector2 a, Vector2 b, float eps = 0.001f)
        => Mathf.Abs(a.x - b.x) < eps && Mathf.Abs(a.y - b.y) < eps;

    private static bool ApproxEqual(Color a, Color b, float eps = 0.01f)
        => Mathf.Abs(a.r - b.r) < eps && Mathf.Abs(a.g - b.g) < eps &&
           Mathf.Abs(a.b - b.b) < eps && Mathf.Abs(a.a - b.a) < eps;
}
