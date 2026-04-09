using UnityEngine;

/// <summary>
/// HP system with invincibility frames and damage feedback.
/// Attach to the Car GameObject alongside CarController.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private ParticleSystem _hitParticles;
    [SerializeField] private PowerUpEffect _powerUpEffect;

    private int _currentHP;
    private bool _isInvincible;
    private float _invincibilityTimer;

    // All renderers on the car for flash effect
    private Renderer[] _allRenderers;

    // Reference to CarController for speed-based raycast tunneling check
    private CarController _carController;

    public int CurrentHP => _currentHP;

    public void Initialize(int maxHP)
    {
        _currentHP = maxHP;
        _isInvincible = false;
        _invincibilityTimer = 0f;

        // Cache all renderers on the car for flash toggling
        _allRenderers = GetComponentsInChildren<Renderer>(true);

        // Cache CarController for speed queries in raycast tunneling check
        _carController = GetComponent<CarController>();
    }

    private void Update()
    {
        if (!_isInvincible) return;

        _invincibilityTimer -= Time.deltaTime;

        // Flash effect: toggle renderer visibility (no material modification = no magenta)
        if (_allRenderers != null)
        {
            bool visible = Mathf.PingPong(_invincibilityTimer * 10f, 1f) > 0.5f;
            foreach (var r in _allRenderers)
                r.enabled = visible;
        }

        if (_invincibilityTimer <= 0f)
        {
            _isInvincible = false;
            if (_allRenderers != null)
            {
                foreach (var r in _allRenderers)
                    r.enabled = true;
            }
        }
    }

    private void FixedUpdate()
    {
        // Forward raycast to catch tunneling at high speeds
        if (_isInvincible || _currentHP <= 0) return;
        if (_carController == null) return;

        float currentSpeed = _carController.CurrentSpeed;
        float rayDistance = currentSpeed * Time.fixedDeltaTime * 2f;

        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                TakeDamage();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInvincible) return;
        if (!other.CompareTag("Obstacle")) return;

        // Mark the obstacle so NearMissDetector knows this was a real hit, not a near-miss.
        Obstacle obstacle = other.GetComponent<Obstacle>();
        if (obstacle != null)
            obstacle.WasHit = true;

        TakeDamage();
    }

    private void TakeDamage()
    {
        if (_currentHP <= 0) return;

        // Shield blocks damage
        if (_powerUpEffect != null && _powerUpEffect.IsShieldActive)
            return;

        _currentHP--;
        _isInvincible = true;
        _invincibilityTimer = _config.invincibilityDuration;

        if (_hitParticles != null)
            _hitParticles.Play();

        // Notify GameManager (handles UI update, sound, and game-over check)
        _gameManager.OnPlayerHit(_currentHP);
    }
}
