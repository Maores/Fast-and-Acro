using UnityEngine;

/// <summary>
/// Trigger zone placed at the end of the track.
/// When the car enters, it notifies GameManager to complete the level.
/// Must have a Collider with IsTrigger = true.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        _gameManager.CompleteLevel();
    }

    /// <summary>
    /// Reset for level restart.
    /// </summary>
    public void ResetTrigger()
    {
        _triggered = false;
    }
}
