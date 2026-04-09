using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Single UI controller managing all panels: MainMenu, HUD, LevelComplete, GameOver.
/// Each panel is a child GameObject that gets toggled via SetActive().
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager _gameManager;

    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private GameObject _levelCompletePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _pausePanel;

    [Header("HUD Elements")]
    [SerializeField] private RectTransform _hpBarFill;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _coinText;

    [Header("Level Complete Elements")]
    [SerializeField] private Image[] _starImages;
    [SerializeField] private TMP_Text _completionTimeText;
    [SerializeField] private TMP_Text _collisionCountText;

    [Header("Star Colors")]
    [SerializeField] private Color _starActiveColor = Color.yellow;
    [SerializeField] private Color _starInactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _retryButtonGameOver;
    [SerializeField] private Button _menuButtonComplete;
    [SerializeField] private Button _menuButtonGameOver;
    [SerializeField] private Button _skinsButton;
    [SerializeField] private Button _quitButton;

    private AudioManager _audioManager;

    private void Awake()
    {
        // Wire up all button listeners
        _playButton.onClick.AddListener(OnPlayClicked);
        _pauseButton.onClick.AddListener(OnPauseClicked);
        _resumeButton.onClick.AddListener(OnResumeClicked);
        _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        _retryButtonGameOver.onClick.AddListener(OnRetryClicked);
        _menuButtonComplete.onClick.AddListener(OnMenuClicked);
        _menuButtonGameOver.onClick.AddListener(OnMenuClicked);

        if (_skinsButton != null)
            _skinsButton.onClick.AddListener(OnSkinsClicked);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitClicked);

        _audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Update()
    {
        // Update timer on HUD while playing (GC-free formatting)
        if (_gameManager.CurrentState == GameState.Playing && _timerText != null)
        {
            float elapsed = _gameManager.ElapsedTime;
            int minutes = (int)(elapsed / 60f);
            int seconds = (int)(elapsed % 60f);
            _timerText.SetText("{0:0}:{1:00}", minutes, seconds);
        }
    }

    // --- Panel Switching ---

    public void ShowMainMenu()
    {
        HideAll();
        _mainMenuPanel.SetActive(true);
    }

    public void ShowHUD()
    {
        HideAll();
        _hudPanel.SetActive(true);
    }

    public void ShowPause()
    {
        _pausePanel.SetActive(true);
    }

    public void ShowLevelComplete(int stars, float completionTime, int collisions)
    {
        HideAll();
        _levelCompletePanel.SetActive(true);

        // Light up earned stars
        for (int i = 0; i < _starImages.Length; i++)
        {
            _starImages[i].color = i < stars ? _starActiveColor : _starInactiveColor;
        }

        // Display stats
        int minutes = (int)(completionTime / 60f);
        int seconds = (int)(completionTime % 60f);
        _completionTimeText.SetText("Time: {0:0}:{1:00}", minutes, seconds);
        _collisionCountText.SetText("Hits: {0}", collisions);
    }

    public void ShowGameOver()
    {
        HideAll();
        _gameOverPanel.SetActive(true);
    }

    // --- HUD Updates ---

    public void UpdateHP(int currentHP, int maxHP)
    {
        if (_hpBarFill != null)
        {
            float ratio = (float)currentHP / maxHP;
            _hpBarFill.anchorMax = new Vector2(ratio, _hpBarFill.anchorMax.y);
        }
    }

    public void UpdateCoins(int coins)
    {
        if (_coinText != null)
            _coinText.SetText("{0}", coins);
    }

    // --- Button Handlers ---

    private void OnPlayClicked()
    {
        PlayClickSound();
        _gameManager.ShowLevelSelect();
    }

    private void OnPauseClicked()
    {
        PlayClickSound();
        _gameManager.PauseGame();
    }

    private void OnResumeClicked()
    {
        PlayClickSound();
        _gameManager.ResumeGame();
    }

    private void OnNextLevelClicked()
    {
        PlayClickSound();
        _gameManager.NextLevel();
    }

    private void OnRetryClicked()
    {
        PlayClickSound();
        _gameManager.RestartLevel();
    }

    private void OnMenuClicked()
    {
        PlayClickSound();
        _gameManager.ReturnToMenu();
    }

    private void OnSkinsClicked()
    {
        PlayClickSound();
        _gameManager.ShowCarSkins();
    }

    private void OnQuitClicked()
    {
        PlayClickSound();
        _gameManager.ReturnToMenu();
    }

    private void PlayClickSound()
    {
        if (_audioManager != null)
            _audioManager.PlayButtonClick();
    }

    // --- Helpers ---

    public void HideAll()
    {
        _mainMenuPanel.SetActive(false);
        _hudPanel.SetActive(false);
        _levelCompletePanel.SetActive(false);
        _gameOverPanel.SetActive(false);
        _pausePanel.SetActive(false);
    }
}
