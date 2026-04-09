using UnityEngine;

/// <summary>
/// Obstacle behavior — static or side-to-side moving.
/// Static obstacles show the scaffolding model; moving obstacles show the eladi model.
/// Must have a Collider with IsTrigger = true and tag "Obstacle".
/// </summary>
[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private bool _isMoving;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _moveRange = 2f;

    [Header("3D Visuals")]
    [SerializeField] private GameObject _staticVisual;
    [SerializeField] private GameObject _movingVisual;
    [SerializeField] private GameObject _legacyVisual;

    private float _originX;

    /// <summary>
    /// Configure this obstacle when spawned from the pool.
    /// </summary>
    public void Setup(Vector3 position, bool isMoving, float moveSpeed)
    {
        transform.position = position;
        _originX = position.x;
        _isMoving = isMoving;
        _moveSpeed = moveSpeed;
        gameObject.SetActive(true);

        // Show the correct 3D model based on type
        if (_staticVisual != null) _staticVisual.SetActive(!isMoving);
        if (_movingVisual != null) _movingVisual.SetActive(isMoving);
        if (_legacyVisual != null) _legacyVisual.SetActive(false);

        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Return to pool — deactivate without destroying.
    /// </summary>
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isMoving) return;

        float newX = _originX + Mathf.Sin(Time.time * _moveSpeed) * _moveRange;
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;
    }
}
