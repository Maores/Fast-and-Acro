using UnityEngine;

/// <summary>
/// Smooth camera follow with offset, look-ahead, and procedural screen shake.
/// Attach to the Main Camera. Set target to the Car.
/// Uses LateUpdate to avoid jitter.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;

    [Header("Offset")]
    [Tooltip("Position offset from the car (e.g. 0, 8, -6 for a behind-and-above view)")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 8f, -6f);

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (higher = snappier)")]
    [SerializeField] private float _smoothSpeed = 8f;

    [Tooltip("Extra forward offset so the player can see upcoming obstacles")]
    [SerializeField] private float _lookAheadZ = 5f;

    // Shake state
    private float _shakeIntensity;
    private float _shakeDuration;
    private float _shakeElapsed;
    private bool _isShaking;

    /// <summary>
    /// Triggers a positional screen shake on the camera.
    /// Safe to call while a shake is already running — it will restart with the new values.
    /// </summary>
    /// <param name="intensity">Maximum offset in world units (e.g. 0.3f).</param>
    /// <param name="duration">Total shake duration in seconds (e.g. 0.4f).</param>
    public void Shake(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeDuration  = duration;
        _shakeElapsed   = 0f;
        _isShaking      = true;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position + _offset;
        targetPosition.z += _lookAheadZ;

        // Frame-rate-independent exponential decay smoothing
        float t = 1f - Mathf.Exp(-_smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);

        // Always look at a point slightly ahead of the car
        Vector3 lookTarget = _target.position + Vector3.forward * _lookAheadZ;
        transform.LookAt(lookTarget);

        // Apply shake offset on top of the smoothed position
        if (_isShaking)
        {
            _shakeElapsed += Time.deltaTime;

            if (_shakeElapsed >= _shakeDuration)
            {
                _isShaking = false;
            }
            else
            {
                // Envelope: sine wave decaying linearly from full intensity to zero
                float progress = _shakeElapsed / _shakeDuration;
                float envelope = (1f - progress) * Mathf.Sin(progress * Mathf.PI * 8f);
                float offsetMagnitude = _shakeIntensity * envelope;

                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * offsetMagnitude,
                    Random.Range(-1f, 1f) * offsetMagnitude,
                    0f
                );
                transform.position += shakeOffset;
            }
        }
    }
}
