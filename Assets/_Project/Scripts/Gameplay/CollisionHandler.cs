using UnityEngine;

/// <summary>
/// HP system with invincibility frames and damage feedback.
/// Attach to the Car GameObject alongside CarController.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Renderer _carRenderer;

    private int _currentHP;
    private bool _isInvincible;
    private float _invincibilityTimer;
    private Color _originalColor;

    public int CurrentHP => _currentHP;

    public void Initialize(int maxHP)
    {
        _currentHP = maxHP;
        _isInvincible = false;
        _invincibilityTimer = 0f;

        if (_carRenderer != null)
            _originalColor = _carRenderer.material.color;
    }

    private void Update()
    {
        if (!_isInvincible) return;

        _invincibilityTimer -= Time.deltaTime;

        // Flash effect: toggle visibility every 0.1 seconds
        if (_carRenderer != null)
        {
            bool visible = Mathf.PingPong(_invincibilityTimer * 10f, 1f) > 0.5f;
            _carRenderer.material.color = visible ? _originalColor : Color.clear;
        }

        if (_invincibilityTimer <= 0f)
        {
            _isInvincible = false;
            if (_carRenderer != null)
                _carRenderer.material.color = _originalColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInvincible) return;
        if (!other.CompareTag("Obstacle")) return;

        TakeDamage();
    }

    private void TakeDamage()
    {
        _currentHP--;
        _isInvincible = true;
        _invincibilityTimer = _config.invincibilityDuration;

        // Notify GameManager (handles UI update, sound, and game-over check)
        _gameManager.OnPlayerHit(_currentHP);
    }
}
