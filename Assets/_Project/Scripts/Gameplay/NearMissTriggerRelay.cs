using UnityEngine;

/// <summary>
/// Placed on the NearMissDetector's child trigger GameObject.
/// Forwards OnTriggerEnter/Exit events back to the parent NearMissDetector.
/// </summary>
public class NearMissTriggerRelay : MonoBehaviour
{
    private NearMissDetector _detector;

    public void Initialize(NearMissDetector detector)
    {
        _detector = detector;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_detector != null)
            _detector.HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_detector != null)
            _detector.HandleTriggerExit(other);
    }
}
