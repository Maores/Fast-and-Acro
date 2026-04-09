using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Level select screen. Spawns a button per level in a grid container,
/// shows earned stars from PlayerPrefs, and enforces sequential unlock rules.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private LevelData[] _levels;

    [Header("Panel")]
    [SerializeField] private GameObject _levelSelectPanel;
    [SerializeField] private Button _backButton;

    [Header("Button Grid")]
    [SerializeField] private Transform _levelButtonContainer;
    [SerializeField] private GameObject _levelButtonPrefab;

    [Header("Star Colors")]
    [SerializeField] private Color _starActiveColor = new Color(1f, 0.92f, 0f, 1f);
    [SerializeField] private Color _starInactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private void Awake()
    {
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(OnBackClicked);
        }
    }

    // --- Public API ---

    public void Show()
    {
        _levelSelectPanel.SetActive(true);
        PopulateButtons();
    }

    public void Hide()
    {
        _levelSelectPanel.SetActive(false);
    }

    // --- Button Population ---

    public void PopulateButtons()
    {
        DestroyExistingButtons();

        if (_levels == null || _levels.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _levels.Length; i++)
        {
            int capturedIndex = i;
            LevelData level = _levels[i];

            GameObject buttonObj = Instantiate(_levelButtonPrefab, _levelButtonContainer);
            Button button = buttonObj.GetComponent<Button>();

            bool isUnlocked = IsLevelUnlocked(i);

            if (isUnlocked)
            {
                SetButtonLevelName(buttonObj, level.levelName);
                SetButtonStars(buttonObj, level.levelName);

                if (button != null)
                {
                    button.interactable = true;
                    button.onClick.AddListener(() => OnLevelSelected(capturedIndex));
                }
            }
            else
            {
                SetButtonLevelName(buttonObj, "[LOCKED]");
                SetButtonStars(buttonObj, level.levelName);

                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }
    }

    // --- Level Selection ---

    public void OnLevelSelected(int index)
    {
        _gameManager.StartGameAtLevel(index);
    }

    // --- Back Button ---

    private void OnBackClicked()
    {
        Hide();
        _gameManager.ReturnToMenu();
    }

    // --- Helpers ---

    private void DestroyExistingButtons()
    {
        for (int i = _levelButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_levelButtonContainer.GetChild(i).gameObject);
        }
    }

    private bool IsLevelUnlocked(int index)
    {
        if (index == 0)
        {
            return true;
        }

        LevelData previousLevel = _levels[index - 1];
        string previousKey = $"BestStars_{previousLevel.levelName}";
        int previousBestStars = PlayerPrefs.GetInt(previousKey, 0);
        return previousBestStars >= 1;
    }

    private void SetButtonLevelName(GameObject buttonObj, string displayName)
    {
        TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = displayName;
        }
    }

    private void SetButtonStars(GameObject buttonObj, string levelName)
    {
        Transform starContainer = buttonObj.transform.Find("StarContainer");
        if (starContainer == null)
        {
            return;
        }

        string key = $"BestStars_{levelName}";
        int bestStars = PlayerPrefs.GetInt(key, 0);

        for (int s = 0; s < starContainer.childCount; s++)
        {
            Image starImage = starContainer.GetChild(s).GetComponent<Image>();
            if (starImage != null)
            {
                starImage.color = s < bestStars ? _starActiveColor : _starInactiveColor;
            }
        }
    }
}
