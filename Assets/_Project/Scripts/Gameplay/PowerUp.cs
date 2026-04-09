using UnityEngine;

public enum PowerUpType { Shield, SpeedBoost, CoinMagnet }

[RequireComponent(typeof(Collider))]
public class PowerUp : MonoBehaviour
{
    public static event System.Action<PowerUpType> OnPowerUpCollected;

    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private float _bobAmplitude = 0.2f;
    [SerializeField] private float _bobFrequency = 1.5f;
    [SerializeField] private float _bobPhaseOffset = 1.0f;

    [Header("Visuals (one per type)")]
    [SerializeField] private GameObject _shieldVisual;
    [SerializeField] private GameObject _speedBoostVisual;
    [SerializeField] private GameObject _coinMagnetVisual;

    private Vector3 _basePosition;
    private float _bobOffset;
    private PowerUpType _type;
    private bool _isActive;

    public void Setup(Vector3 position, PowerUpType type)
    {
        transform.position = position;
        _basePosition = position;
        _type = type;
        _bobOffset = _bobPhaseOffset;
        _isActive = true;
        gameObject.SetActive(true);
        SetVisual(type);
    }

    private void SetVisual(PowerUpType type)
    {
        if (_shieldVisual != null) _shieldVisual.SetActive(type == PowerUpType.Shield);
        if (_speedBoostVisual != null) _speedBoostVisual.SetActive(type == PowerUpType.SpeedBoost);
        if (_coinMagnetVisual != null) _coinMagnetVisual.SetActive(type == PowerUpType.CoinMagnet);
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

        OnPowerUpCollected?.Invoke(_type);
        Deactivate();
    }
}
