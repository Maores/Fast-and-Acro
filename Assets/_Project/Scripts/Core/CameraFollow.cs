using UnityEngine;

/// <summary>
/// Smooth camera follow with offset and look-ahead.
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

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position + _offset;
        targetPosition.z += _lookAheadZ;

        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);

        // Always look at a point slightly ahead of the car
        Vector3 lookTarget = _target.position + Vector3.forward * _lookAheadZ;
        transform.LookAt(lookTarget);
    }
}
