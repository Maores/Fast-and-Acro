using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for selecting and unlocking car skins.
/// Shows color swatches, lock/coin-cost indicators, and handles purchases.
/// </summary>
public class CarSkinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSkinManager _skinManager;
    [SerializeField] private GameManager _gameManager;

    [Header("Panel")]
    [SerializeField] private GameObject _skinPanel;
    [SerializeField] private Button _backButton;

    [Header("Skin Grid")]
    [SerializeField] private Transform _skinButtonContainer;
    [SerializeField] private GameObject _skinButtonPrefab;

    [Header("Info")]
    [SerializeField] private TMP_Text _coinBalanceText;

    // Tracks whether the panel is currently visible so Update() knows when to listen.
    private bool _isVisible;

    /// <summary>
    /// Called by Unity whenever this GameObject (or _skinPanel) becomes active.
    /// Guarantees the close button exists regardless of HOW the panel was shown.
    /// </summary>
    private void OnEnable()
    {
        _isVisible = true;
        EnsureCloseButton();
        PopulateButtons();
        UpdateCoinBalance();
    }

    private void OnDisable()
    {
        _isVisible = false;
    }

    public void Show()
    {
        // If _skinPanel is a separate object, activate it (which triggers OnEnable on children).
        // If _skinPanel IS this object, SetActive triggers our own OnEnable — which already
        // calls EnsureCloseButton + PopulateButtons, so we skip duplicating that work here.
        if (_skinPanel != null)
            _skinPanel.SetActive(true);

        _isVisible = true;
        // OnEnable handles the rest (close button, populate, coin balance)
    }

    public void Hide()
    {
        if (_skinPanel != null)
            _skinPanel.SetActive(false);
        else
            gameObject.SetActive(false);
        _isVisible = false;
    }

    private void Awake()
    {
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackClicked);
    }

    // Handles Escape key on PC and the Android hardware back button (mapped to Escape).
    private void Update()
    {
        if (_isVisible && Input.GetKeyDown(KeyCode.Escape))
            OnBackClicked();
    }

    private void OnBackClicked()
    {
        // Hide the close button so it doesn't linger over other panels
        HideCloseButton();
        Hide();

        // Return to the main menu panel without reloading the scene.
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null)
            ui.ShowMainMenu();
        else if (_gameManager != null)
            _gameManager.ReturnToMenu();
    }

    private void HideCloseButton()
    {
        Transform panelTransform = _skinPanel != null ? _skinPanel.transform : transform;
        Canvas rootCanvas = panelTransform.GetComponentInParent<Canvas>();
        Transform buttonParent = rootCanvas != null ? rootCanvas.transform : panelTransform;
        Transform btn = buttonParent.Find("SkinCloseButton");
        if (btn != null)
            btn.gameObject.SetActive(false);
    }

    /// <summary>
    /// Ensures an X close button exists in the top-right corner.
    /// Idempotent — safe to call multiple times. Creates the button on the
    /// Canvas root so it's never clipped by panel layout groups or masks.
    /// </summary>
    private void EnsureCloseButton()
    {
        // Find our panel's root canvas to parent the button there (avoids layout/mask clipping)
        Transform panelTransform = _skinPanel != null ? _skinPanel.transform : transform;
        Canvas rootCanvas = panelTransform.GetComponentInParent<Canvas>();
        Transform buttonParent = rootCanvas != null ? rootCanvas.transform : panelTransform;

        // Check if button already exists
        Transform existing = buttonParent.Find("SkinCloseButton");
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return;
        }

        // Root object — parented to Canvas root, renders on top of everything
        var go = new GameObject("SkinCloseButton");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(buttonParent, false);

        // Top-right corner: 70x70, comfortably tappable on mobile
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(70f, 70f);

        // Force on top of all siblings
        rect.SetAsLastSibling();

        // Add a Canvas + GraphicRaycaster so the button always intercepts input
        var overlay = go.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 100;
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Red background — unmissable
        var img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0.15f, 0.15f, 0.95f);

        // Button component
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(OnBackClicked);

        // "X" label
        var textGo = new GameObject("Label");
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "X";
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
    }

    // Recursively searches for a child named "BackButton" under the given root.
    private static Transform FindBackButtonTransform(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == "BackButton")
                return child;
            Transform found = FindBackButtonTransform(child);
            if (found != null)
                return found;
        }
        return null;
    }

    private void PopulateButtons()
    {
        // Clear existing
        for (int i = _skinButtonContainer.childCount - 1; i >= 0; i--)
            Destroy(_skinButtonContainer.GetChild(i).gameObject);

        var skins = _skinManager.Skins;
        if (skins == null) return;

        for (int i = 0; i < skins.Length; i++)
        {
            int idx = i;
            GameObject btnObj = Instantiate(_skinButtonPrefab, _skinButtonContainer);

            // Color swatch
            Image swatch = btnObj.transform.Find("Swatch")?.GetComponent<Image>();
            if (swatch != null) swatch.color = skins[i].color;

            // Label
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            bool unlocked = _skinManager.IsSkinUnlocked(i);
            bool selected = _skinManager.SelectedSkinIndex == i;

            if (label != null)
            {
                if (selected) label.text = skins[i].name + "\n[EQUIPPED]";
                else if (unlocked) label.text = skins[i].name;
                else label.text = skins[i].name + "\n" + _skinManager.SkinCost + " coins";
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSkinClicked(idx));
            }
        }
    }

    private void OnSkinClicked(int index)
    {
        if (_skinManager.IsSkinUnlocked(index))
        {
            _skinManager.SelectSkin(index);
        }
        else
        {
            if (_skinManager.TryUnlockSkin(index))
                _skinManager.SelectSkin(index);
        }

        // Refresh UI
        PopulateButtons();
        UpdateCoinBalance();
    }

    private void UpdateCoinBalance()
    {
        if (_coinBalanceText != null)
        {
            int coins = PlayerPrefs.GetInt("TotalCoins", 0);
            _coinBalanceText.SetText("Coins: {0}", coins);
        }
    }
}
