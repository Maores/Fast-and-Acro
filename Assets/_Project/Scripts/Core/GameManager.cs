using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    LevelComplete,
    GameOver
}

/// <summary>
/// Central game orchestrator. Manages state transitions, scoring, and level flow.
/// Place on a GameObject in the scene. Other scripts reference it via [SerializeField].
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig _config;

    [Header("Levels")]
    [SerializeField] private LevelData[] _levels;

    [Header("References")]
    [SerializeField] private UIController _ui;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private CarController _car;
    [SerializeField] private CollisionHandler _collisionHandler;
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private CameraFollow _cameraFollow;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public GameConfig Config => _config;

    [Header("UI - Level Select")]
    [SerializeField] private LevelSelectUI _levelSelectUI;

    [Header("UI - Car Skins")]
    [SerializeField] private CarSkinUI _carSkinUI;

    // Level progression
    private int _currentLevelIndex = 0;

    // Scoring data for current run
    private float _levelStartTime;
    private int _collisionCount;
    private int _coinsCollectedThisRun;

    private const string PREFS_PENDING_LEVEL = "PendingLevelIndex";
    private const string PREFS_AUTO_START = "AutoStartLevel";

    private void Start()
    {
        Application.targetFrameRate = _config.targetFrameRate;
        Time.fixedDeltaTime = 0.04f; // 25 Hz physics — sufficient for this game, saves battery

        // Check if we should auto-start a level after scene reload (from Retry/NextLevel)
        if (PlayerPrefs.GetInt(PREFS_AUTO_START, 0) == 1)
        {
            int pendingIndex = PlayerPrefs.GetInt(PREFS_PENDING_LEVEL, 0);
            PlayerPrefs.DeleteKey(PREFS_AUTO_START);
            PlayerPrefs.DeleteKey(PREFS_PENDING_LEVEL);
            PlayerPrefs.Save();
            StartGameAtLevel(pendingIndex);
            return;
        }

        SetState(GameState.Menu);
    }

    private void OnEnable()
    {
        Coin.OnCoinCollected += HandleCoinCollected;
    }

    private void OnDisable()
    {
        Coin.OnCoinCollected -= HandleCoinCollected;
    }

    // --- State Transitions ---

    public void StartGame()
    {
        // Hide overlays if open
        if (_levelSelectUI != null)
            _levelSelectUI.Hide();
        if (_carSkinUI != null)
            _carSkinUI.Hide();

        _collisionCount = 0;
        _coinsCollectedThisRun = 0;
        _levelStartTime = Time.time;

        _collisionHandler.Initialize(_config.maxHP);

        if (_levels != null && _levels.Length > 0)
        {
            var level = _levels[_currentLevelIndex];
            _levelManager.SetLevelData(level);
            _car.SetSpeedRamp(level.startSpeed, level.speedIncrement, level.maxSpeed);
        }

        _levelManager.SetupLevel();
        _car.SetLane(_config.laneCount / 2); // Start in center lane

        SetState(GameState.Playing);
    }

    /// <summary>
    /// Called by LevelSelectUI to start a specific level.
    /// </summary>
    public void StartGameAtLevel(int levelIndex)
    {
        _currentLevelIndex = Mathf.Clamp(levelIndex, 0, _levels.Length - 1);
        StartGame();
    }

    /// <summary>
    /// Show the level select screen.
    /// </summary>
    public void ShowLevelSelect()
    {
        if (_levelSelectUI != null)
        {
            _ui.HideAll();
            _levelSelectUI.Show();
        }
        else
        {
            StartGame(); // Fallback if no level select UI
        }
    }

    /// <summary>
    /// Show the car skin selection screen.
    /// </summary>
    public void ShowCarSkins()
    {
        if (_carSkinUI != null)
        {
            _ui.HideAll();
            _carSkinUI.Show();
        }
    }

    /// <summary>
    /// Advances to the next level and restarts. Wraps around to level 0 after the last level.
    /// </summary>
    public void NextLevel()
    {
        int nextIndex = _currentLevelIndex;
        if (_levels != null && _levels.Length > 0)
        {
            nextIndex = (_currentLevelIndex + 1) % _levels.Length;
        }

        PlayerPrefs.SetInt(PREFS_PENDING_LEVEL, nextIndex);
        PlayerPrefs.SetInt(PREFS_AUTO_START, 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void CompleteLevel()
    {
        if (CurrentState != GameState.Playing) return;

        float completionTime = Time.time - _levelStartTime;
        int stars = CalculateStars(completionTime, _collisionCount);

        _car.SetMovementEnabled(false);
        SetState(GameState.LevelComplete);

        _ui.ShowLevelComplete(stars, completionTime, _collisionCount);
        _audioManager.PlayLevelComplete();

        SaveBestScore(stars);
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        _car.SetMovementEnabled(false);
        SetState(GameState.GameOver);

        _ui.ShowGameOver();
        _audioManager.PlayGameOver();

        // Persist coins collected this run so they survive a crash/force-close
        PlayerPrefs.Save();
    }

    public void RestartLevel()
    {
        PlayerPrefs.SetInt(PREFS_PENDING_LEVEL, _currentLevelIndex);
        PlayerPrefs.SetInt(PREFS_AUTO_START, 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Scene index 0 = MainMenu
    }

    // --- Coin Tracking ---

    private void HandleCoinCollected()
    {
        _coinsCollectedThisRun++;
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0) + 1;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        _ui.UpdateCoins(_coinsCollectedThisRun);
        _audioManager.PlayCoinPickup();
    }

    public int TotalCoins => PlayerPrefs.GetInt("TotalCoins", 0);

    // --- Collision Tracking (called by CollisionHandler) ---

    public void OnPlayerHit(int remainingHP)
    {
        _collisionCount++;
        _ui.UpdateHP(remainingHP, _config.maxHP);
        _audioManager.PlayCollision();

        if (remainingHP <= 0)
        {
            TriggerGameOver();
        }
    }

    // --- Scoring ---

    private int CalculateStars(float time, int collisions)
    {
        // Delegate to per-level thresholds if a level is loaded
        if (_levels != null && _levels.Length > _currentLevelIndex && _levels[_currentLevelIndex] != null)
        {
            return _levels[_currentLevelIndex].CalculateStars(collisions, time);
        }

        // Fallback to global config thresholds
        int stars = 3;

        if (collisions > _config.twoStarMaxCollisions)
            stars -= 2;
        else if (collisions > _config.threeStarMaxCollisions)
            stars -= 1;

        if (time > _config.twoStarMaxTime)
            stars -= 2;
        else if (time > _config.threeStarMaxTime)
            stars -= 1;

        return Mathf.Clamp(stars, 1, 3);
    }

    private void SaveBestScore(int stars)
    {
        string key = $"BestStars_{_levelManager.CurrentLevelName}";
        int previousBest = PlayerPrefs.GetInt(key, 0);
        if (stars > previousBest)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    // --- Timer (read by UIController) ---

    public float ElapsedTime
    {
        get
        {
            if (CurrentState == GameState.Playing)
                return Time.time - _levelStartTime;
            return 0f;
        }
    }

    // --- Internal ---

    private void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Menu:
                _ui.ShowMainMenu();
                _car.SetMovementEnabled(false);
                _audioManager.PlayMenuMusic();
                break;
            case GameState.Playing:
                _ui.ShowHUD();
                _car.SetMovementEnabled(true);
                _ui.UpdateHP(_collisionHandler.CurrentHP, _config.maxHP);
                _ui.UpdateCoins(0);
                _audioManager.StartEngine();
                _audioManager.PlayGameplayMusic();
                break;
            case GameState.Paused:
                _ui.ShowPause();
                break;
            // LevelComplete and GameOver are handled in their trigger methods
        }
    }
}
