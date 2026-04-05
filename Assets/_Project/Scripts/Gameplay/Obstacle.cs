using UnityEngine;

/// <summary>
/// Obstacle behavior — static or side-to-side moving.
/// Must have a Collider with IsTrigger = true and tag "Obstacle".
/// </summary>
[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private bool _isMoving;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _moveRange = 2f;

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
