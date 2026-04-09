using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen red vignette flash triggered on player hit.
/// Attach to any active GameObject in the scene (e.g. HUD canvas root).
/// Assign a full-screen UI Image (red color, alpha 0 by default) to _flashImage
/// in the Inspector.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Full-screen UI Image — set its color to red (255, 0, 0) in the Inspector; alpha will be driven at runtime.")]
    [SerializeField] private Image _flashImage;

    [Header("Flash Settings")]
    [Tooltip("Peak alpha reached at the midpoint of the flash (0–1).")]
    [SerializeField] private float _peakAlpha = 0.3f;

    [Tooltip("Total duration of one full flash cycle in seconds.")]
    [SerializeField] private float _flashDuration = 0.3f;

    private Coroutine _activeFlash;

    private void OnDisable()
    {
        StopAllCoroutines();
        _activeFlash = null;
        if (_flashImage != null)
        {
            Color c = _flashImage.color;
            c.a = 0f;
            _flashImage.color = c;
        }
    }

    private void Awake()
    {
        // Ensure the overlay starts fully transparent so it never blocks gameplay.
        if (_flashImage != null)
        {
            Color c = _flashImage.color;
            c.a = 0f;
            _flashImage.color = c;
        }
    }

    /// <summary>
    /// Plays a red flash: alpha lerps 0 → peakAlpha → 0 over flashDuration seconds.
    /// Interrupts and restarts if already running.
    /// </summary>
    public void Flash()
    {
        if (_flashImage == null) return;

        if (_activeFlash != null)
            StopCoroutine(_activeFlash);

        _activeFlash = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float halfDuration = _flashDuration * 0.5f;
        float elapsed = 0f;

        // Fade in: 0 → peakAlpha
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(0f, _peakAlpha, elapsed / halfDuration));
            yield return null;
        }

        elapsed = 0f;

        // Fade out: peakAlpha → 0
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(_peakAlpha, 0f, elapsed / halfDuration));
            yield return null;
        }

        SetAlpha(0f);
        _activeFlash = null;
    }

    private void SetAlpha(float alpha)
    {
        Color c = _flashImage.color;
        c.a = alpha;
        _flashImage.color = c;
    }
}
