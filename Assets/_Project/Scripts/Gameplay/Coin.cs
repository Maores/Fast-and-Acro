using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    public static event System.Action OnCoinCollected;

    [SerializeField] private float _rotationSpeed = 120f;
    [SerializeField] private float _bobAmplitude = 0.15f;
    [SerializeField] private float _bobFrequency = 2f;

    private Vector3 _basePosition;
    private float _bobOffset;
    private bool _isActive;

    /// <summary>
    /// Current base position (before bob offset). Used by CoinMagnet.
    /// </summary>
    public Vector3 BasePosition => _basePosition;

    /// <summary>
    /// Updates the base position so external systems (e.g. CoinMagnet)
    /// can move the coin without being overwritten by the bob logic.
    /// </summary>
    public void SetBasePosition(Vector3 pos)
    {
        _basePosition = pos;
    }

    public void Setup(Vector3 position)
    {
        transform.position = position;
        _basePosition = position;
        _bobOffset = 0f;
        _isActive = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _isActive = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isActive) return;

        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);

        _bobOffset += Time.deltaTime;
        float bobY = Mathf.Sin(_bobOffset * _bobFrequency) * _bobAmplitude;
        transform.position = new Vector3(_basePosition.x, _basePosition.y + bobY, _basePosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        if (!other.CompareTag("Player")) return;

        OnCoinCollected?.Invoke();
        Deactivate();
    }
}
