using UnityEngine;

/// <summary>
/// Handles auto-forward movement and lane-based swipe input.
/// Attach to the Car GameObject (needs a Collider for trigger detection).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CarController : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private PowerUpEffect _powerUpEffect;

    private int _currentLane;
    private float _targetX;
    private bool _movementEnabled;

    // Speed ramp
    private float _currentSpeed;
    private float _speedIncrement;
    private float _maxSpeed;

    // Swipe tracking
    private Vector2 _touchStartPos;
    private float _touchStartTime;
    private bool _swipeProcessed;

    private void Update()
    {
        if (!_movementEnabled) return;

        HandleInput();
        MoveForward();
        MoveToLane();
    }

    // --- Public API ---

    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;
    }

    public void SetLane(int laneIndex)
    {
        _currentLane = Mathf.Clamp(laneIndex, 0, _config.laneCount - 1);
        _targetX = _config.GetLanePosition(_currentLane);

        // Snap immediately on initial placement
        Vector3 pos = transform.position;
        pos.x = _targetX;
        transform.position = pos;
    }

    /// <summary>
    /// Called by GameManager before gameplay starts to set per-level speed params.
    /// </summary>
    public void SetSpeedRamp(float startSpeed, float increment, float max)
    {
        _currentSpeed = startSpeed > 0 ? startSpeed : _config.forwardSpeed;
        _speedIncrement = increment;
        _maxSpeed = max;
    }

    public float CurrentSpeed => _currentSpeed;

    // --- Movement ---

    private void MoveForward()
    {
        // Accelerate over time
        if (_speedIncrement > 0f && _currentSpeed < _maxSpeed)
        {
            _currentSpeed += _speedIncrement * Time.deltaTime;
            _currentSpeed = Mathf.Min(_currentSpeed, _maxSpeed);
        }

        float speedMult = _powerUpEffect != null ? _powerUpEffect.SpeedMultiplier : 1f;
        float speed = _currentSpeed > 0 ? _currentSpeed : _config.forwardSpeed;
        transform.Translate(Vector3.forward * speed * speedMult * Time.deltaTime, Space.World);
    }

    private void MoveToLane()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, _targetX, _config.laneSwitchSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void SwitchLane(int direction)
    {
        int newLane = _currentLane + direction;
        if (newLane < 0 || newLane >= _config.laneCount) return;

        _currentLane = newLane;
        _targetX = _config.GetLanePosition(_currentLane);
    }

    // --- Input Handling ---

    private void HandleInput()
    {
        // Keyboard fallback (for editor testing)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SwitchLane(-1);
            return;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SwitchLane(1);
            return;
        }

        // Touch / swipe input
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _touchStartPos = touch.position;
                _touchStartTime = Time.unscaledTime;
                _swipeProcessed = false;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Ended:
                if (_swipeProcessed) break;

                float elapsed = Time.unscaledTime - _touchStartTime;
                if (elapsed > _config.swipeMaxDuration) break;

                float deltaX = touch.position.x - _touchStartPos.x;

                if (Mathf.Abs(deltaX) >= _config.swipeThreshold)
                {
                    SwitchLane(deltaX > 0 ? 1 : -1);
                    _swipeProcessed = true;
                }
                break;
        }
    }
}
