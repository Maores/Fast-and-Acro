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

    [Header("References")]
    [SerializeField] private UIController _ui;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private CarController _car;
    [SerializeField] private CollisionHandler _collisionHandler;
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private CameraFollow _cameraFollow;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public GameConfig Config => _config;

    // Scoring data for current run
    private float _levelStartTime;
    private int _collisionCount;

    private void Start()
    {
        Application.targetFrameRate = _config.targetFrameRate;
        Time.fixedDeltaTime = 0.04f; // 25 Hz physics — sufficient for this game, saves battery

        SetState(GameState.Menu);
    }

    // --- State Transitions ---

    public void StartGame()
    {
        _collisionCount = 0;
        _levelStartTime = Time.time;

        _collisionHandler.Initialize(_config.maxHP);
        _levelManager.SetupLevel();
        _car.SetLane(_config.laneCount / 2); // Start in center lane

        SetState(GameState.Playing);
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
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Scene index 0 = MainMenu
    }

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
        int stars = 3;

        // Collision penalty
        if (collisions > _config.twoStarMaxCollisions)
            stars -= 2;
        else if (collisions > _config.threeStarMaxCollisions)
            stars -= 1;

        // Time penalty
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
                break;
            case GameState.Playing:
                _ui.ShowHUD();
                _car.SetMovementEnabled(true);
                _ui.UpdateHP(_collisionHandler.CurrentHP, _config.maxHP);
                break;
            case GameState.Paused:
                _ui.ShowPause();
                break;
            // LevelComplete and GameOver are handled in their trigger methods
        }
    }
}
