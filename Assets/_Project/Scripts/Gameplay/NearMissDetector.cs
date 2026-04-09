using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Detects when an obstacle passes the car without hitting it.
/// Uses a trigger zone slightly larger than the car's own collider.
/// When a near-miss occurs:
///   - Fires the static OnNearMiss event (GameManager subscribes)
///   - Spawns a rising "+10 CLOSE CALL!" world-space label
///
/// Attach to the Car GameObject alongside CarController and CollisionHandler.
/// The car must already have a BoxCollider (non-trigger) for its actual hit detection.
/// This script creates a second, larger BoxCollider (trigger) at runtime.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class NearMissDetector : MonoBehaviour
{
    // --- Configuration ---

    [Header("Detection Zone")]
    [Tooltip("How much to expand the car's BoxCollider on X and Z axes for the near-miss zone")]
    [SerializeField] private float _zoneExpansion = 0.5f;

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between consecutive near-miss triggers")]
    [SerializeField] private float _cooldown = 0.5f;

    [Header("Floating Label")]
    [Tooltip("How high the label floats (world units)")]
    [SerializeField] private float _labelFloatDistance = 2.5f;

    [Tooltip("How long the label stays visible before fully fading")]
    [SerializeField] private float _labelLifetime = 1.2f;

    [Tooltip("Font size for the near-miss label")]
    [SerializeField] private float _labelFontSize = 5f;

    // --- Static event ---

    /// <summary>
    /// Fired each time a near-miss is confirmed.
    /// GameManager subscribes to track the count.
    /// </summary>
    public static event Action OnNearMiss;

    // --- Private state ---

    // BoxCollider that is the actual hit detection collider (non-trigger).
    private BoxCollider _hitCollider;

    // The larger trigger collider we create at runtime.
    private BoxCollider _nearMissTrigger;

    // Obstacles currently inside the expanded trigger zone.
    private readonly HashSet<Obstacle> _trackedObstacles = new HashSet<Obstacle>();

    private float _cooldownTimer;

    // --- Lifecycle ---

    private void Awake()
    {
        // Find the existing non-trigger BoxCollider on this GameObject.
        // There may be multiple BoxColliders (e.g., trigger from another system),
        // so we look specifically for the non-trigger one as the size reference.
        BoxCollider[] allBoxColliders = GetComponents<BoxCollider>();
        foreach (var bc in allBoxColliders)
        {
            if (!bc.isTrigger)
            {
                _hitCollider = bc;
                break;
            }
        }

        if (_hitCollider == null)
        {
            // Fallback: just use the first BoxCollider found
            _hitCollider = GetComponent<BoxCollider>();
        }

        // Create a second BoxCollider as the near-miss detection trigger
        _nearMissTrigger = gameObject.AddComponent<BoxCollider>();
        _nearMissTrigger.isTrigger = true;
        _nearMissTrigger.center = _hitCollider.center;

        // Expand the size on X and Z only (not Y — no need to detect above/below)
        Vector3 expandedSize = _hitCollider.size;
        expandedSize.x += _zoneExpansion * 2f;
        expandedSize.z += _zoneExpansion * 2f;
        _nearMissTrigger.size = expandedSize;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    // --- Trigger callbacks ---

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        Obstacle obs = other.GetComponent<Obstacle>();
        if (obs != null)
            _trackedObstacles.Add(obs);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        Obstacle obs = other.GetComponent<Obstacle>();
        if (obs == null) return;

        bool wasTracked = _trackedObstacles.Remove(obs);
        if (!wasTracked) return;

        // Only count as near-miss if the car was NOT hit and cooldown has elapsed
        if (!obs.WasHit && _cooldownTimer <= 0f)
        {
            _cooldownTimer = _cooldown;
            TriggerNearMiss();
        }
    }

    private void OnDisable()
    {
        _trackedObstacles.Clear();
    }

    // --- Near-miss response ---

    private void TriggerNearMiss()
    {
        OnNearMiss?.Invoke();
        StartCoroutine(SpawnFloatingLabel());
    }

    private IEnumerator SpawnFloatingLabel()
    {
        // Create a world-space Canvas for the label
        GameObject canvasGO = new GameObject("NearMissCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        // Position just above the car
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        canvasGO.transform.position = spawnPos;
        canvasGO.transform.localScale = Vector3.one * 0.02f;

        // Make the canvas face the camera
        Camera cam = Camera.main;
        if (cam != null)
            canvasGO.transform.rotation = Quaternion.LookRotation(
                canvasGO.transform.position - cam.transform.position);

        // Create the TMP_Text child
        GameObject textGO = new GameObject("NearMissText");
        textGO.transform.SetParent(canvasGO.transform, false);

        TMP_Text label = textGO.AddComponent<TextMeshProUGUI>();
        label.text = "+10 CLOSE CALL!";
        label.fontSize = _labelFontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.85f, 0f, 1f); // Bright gold

        // Size the RectTransform so it can fit the text
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500f, 120f);
        rt.anchoredPosition = Vector2.zero;

        // Animate: float upward and fade out
        float elapsed = 0f;
        Vector3 startPos = canvasGO.transform.position;
        Color startColor = label.color;

        while (elapsed < _labelLifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _labelLifetime;

            // Float upward
            canvasGO.transform.position = startPos + Vector3.up * (_labelFloatDistance * t);

            // Face camera each frame while moving
            if (cam != null)
                canvasGO.transform.rotation = Quaternion.LookRotation(
                    canvasGO.transform.position - cam.transform.position);

            // Fade out in the last 40% of lifetime
            float fadeT = Mathf.InverseLerp(0.6f, 1f, t);
            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - fadeT);

            yield return null;
        }

        Destroy(canvasGO);
    }
}
