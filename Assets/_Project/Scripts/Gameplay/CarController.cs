using UnityEngine;

/// <summary>
/// Handles auto-forward movement and lane-based swipe input.
/// Attach to the Car GameObject (needs a Collider for trigger detection).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CarController : MonoBehaviour
{
    [SerializeField] private GameConfig _config;

    private int _currentLane;
    private float _targetX;
    private bool _movementEnabled;

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

    // --- Movement ---

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * _config.forwardSpeed * Time.deltaTime, Space.World);
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
