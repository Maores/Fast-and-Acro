using UnityEngine;

/// <summary>
/// Manages active power-up effects on the Car GameObject.
/// Subscribes to PowerUp.OnPowerUpCollected and applies timed effects:
/// Shield (5s), SpeedBoost (4s), CoinMagnet (6s).
/// </summary>
public class PowerUpEffect : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector references
    // -------------------------------------------------------------------------
    [SerializeField] private GameConfig _config;
    [SerializeField] private CollisionHandler _collisionHandler;
    [SerializeField] private GameObject _shieldVisual;

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------
    private const float SHIELD_DURATION  = 5f;
    private const float BOOST_DURATION   = 4f;
    private const float MAGNET_DURATION  = 6f;
    private const float BOOST_MULTIPLIER = 1.5f;
    private const float MAGNET_RADIUS    = 8f;
    private const float MAGNET_SPEED     = 15f;

    // -------------------------------------------------------------------------
    // Runtime state — shield
    // -------------------------------------------------------------------------
    private bool  _shieldActive;
    private float _shieldTimer;

    // -------------------------------------------------------------------------
    // Runtime state — speed boost
    // -------------------------------------------------------------------------
    private bool  _boostActive;
    private float _boostTimer;

    // -------------------------------------------------------------------------
    // Runtime state — coin magnet
    // -------------------------------------------------------------------------
    private bool  _magnetActive;
    private float _magnetTimer;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Multiplier read by CarController. 1f normally, 1.5f during boost.
    /// </summary>
    public float SpeedMultiplier { get; private set; } = 1f;

    public bool IsShieldActive  => _shieldActive;
    public bool IsBoostActive   => _boostActive;
    public bool IsMagnetActive  => _magnetActive;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        PowerUp.OnPowerUpCollected += ActivatePowerUp;
    }

    private void OnDisable()
    {
        PowerUp.OnPowerUpCollected -= ActivatePowerUp;
    }

    private void Update()
    {
        TickShield();
        TickBoost();
        TickMagnet();
    }

    // -------------------------------------------------------------------------
    // Public methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates the given power-up, resetting its timer if already active.
    /// </summary>
    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                _shieldActive = true;
                _shieldTimer  = SHIELD_DURATION;
                SetShieldVisual(true);
                break;

            case PowerUpType.SpeedBoost:
                _boostActive   = true;
                _boostTimer    = BOOST_DURATION;
                SpeedMultiplier = BOOST_MULTIPLIER;
                break;

            case PowerUpType.CoinMagnet:
                _magnetActive = true;
                _magnetTimer  = MAGNET_DURATION;
                break;
        }
    }

    /// <summary>
    /// Clears all active effects. Call this when a level starts or restarts.
    /// </summary>
    public void ResetAllEffects()
    {
        _shieldActive = false;
        _shieldTimer  = 0f;
        SetShieldVisual(false);

        _boostActive    = false;
        _boostTimer     = 0f;
        SpeedMultiplier = 1f;

        _magnetActive = false;
        _magnetTimer  = 0f;
    }

    // -------------------------------------------------------------------------
    // Private tick helpers
    // -------------------------------------------------------------------------

    private void TickShield()
    {
        if (!_shieldActive) return;

        _shieldTimer -= Time.deltaTime;
        if (_shieldTimer <= 0f)
        {
            _shieldActive = false;
            _shieldTimer  = 0f;
            SetShieldVisual(false);
        }
    }

    private void TickBoost()
    {
        if (!_boostActive) return;

        _boostTimer -= Time.deltaTime;
        if (_boostTimer <= 0f)
        {
            _boostActive    = false;
            _boostTimer     = 0f;
            SpeedMultiplier = 1f;
        }
    }

    private void TickMagnet()
    {
        if (!_magnetActive) return;

        _magnetTimer -= Time.deltaTime;
        if (_magnetTimer <= 0f)
        {
            _magnetActive = false;
            _magnetTimer  = 0f;
            return;
        }

        // Pull all active Coin objects within radius toward the player.
        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        Vector3 playerPos = transform.position;

        foreach (Coin coin in coins)
        {
            if (!coin.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(coin.transform.position, playerPos);
            if (distance > MAGNET_RADIUS) continue;

            coin.SetBasePosition(Vector3.MoveTowards(
                coin.BasePosition,
                playerPos,
                MAGNET_SPEED * Time.deltaTime
            ));
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void SetShieldVisual(bool active)
    {
        if (_shieldVisual != null)
            _shieldVisual.SetActive(active);
    }
}
