using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Detects when an obstacle passes the car without hitting it.
/// Uses a child GameObject with a trigger collider slightly larger than the car's collider.
/// This avoids collision routing ambiguity with CollisionHandler on the parent.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class NearMissDetector : MonoBehaviour
{
    [Header("Detection Zone")]
    [SerializeField] private float _zoneExpansion = 0.5f;

    [Header("Cooldown")]
    [SerializeField] private float _cooldown = 0.5f;

    [Header("Floating Label")]
    [SerializeField] private float _labelFloatDistance = 2.5f;
    [SerializeField] private float _labelLifetime = 1.2f;
    [SerializeField] private float _labelFontSize = 5f;

    public static event Action OnNearMiss;

    private readonly HashSet<Obstacle> _trackedObstacles = new HashSet<Obstacle>();
    private float _cooldownTimer;
    private Camera _cachedCamera;

    // Label pool (avoids per-trigger allocation)
    private const int LABEL_POOL_SIZE = 3;
    private GameObject[] _labelPool;
    private TMP_Text[] _labelTexts;
    private int _labelIndex;
    private Coroutine[] _labelCoroutines;

    private void Awake()
    {
        _cachedCamera = Camera.main;

        // Find the car's non-trigger BoxCollider as size reference
        BoxCollider hitCollider = null;
        foreach (var bc in GetComponents<BoxCollider>())
        {
            if (!bc.isTrigger)
            {
                hitCollider = bc;
                break;
            }
        }
        if (hitCollider == null)
            hitCollider = GetComponent<BoxCollider>();

        // Create child GameObject for the near-miss trigger
        // This prevents collision routing ambiguity with CollisionHandler
        var triggerGO = new GameObject("NearMissTrigger");
        triggerGO.transform.SetParent(transform, false);
        triggerGO.transform.localPosition = Vector3.zero;
        triggerGO.layer = gameObject.layer;

        var trigger = triggerGO.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = hitCollider.center;
        Vector3 expandedSize = hitCollider.size;
        expandedSize.x += _zoneExpansion * 2f;
        expandedSize.z += _zoneExpansion * 2f;
        trigger.size = expandedSize;

        // Add the relay script to forward trigger events back to us
        var relay = triggerGO.AddComponent<NearMissTriggerRelay>();
        relay.Initialize(this);

        // Build label pool
        BuildLabelPool();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    /// <summary>Called by NearMissTriggerRelay on the child object.</summary>
    public void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;
        Obstacle obs = other.GetComponent<Obstacle>();
        if (obs != null)
            _trackedObstacles.Add(obs);
    }

    /// <summary>Called by NearMissTriggerRelay on the child object.</summary>
    public void HandleTriggerExit(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;
        Obstacle obs = other.GetComponent<Obstacle>();
        if (obs == null) return;

        bool wasTracked = _trackedObstacles.Remove(obs);
        if (!wasTracked) return;

        if (!obs.WasHit && _cooldownTimer <= 0f)
        {
            _cooldownTimer = _cooldown;
            TriggerNearMiss();
        }
    }

    private void OnDisable()
    {
        _trackedObstacles.Clear();
        StopAllCoroutines();
        // Hide all pooled labels
        if (_labelPool != null)
        {
            for (int i = 0; i < LABEL_POOL_SIZE; i++)
                _labelPool[i].SetActive(false);
        }
    }

    private void TriggerNearMiss()
    {
        OnNearMiss?.Invoke();
        ShowFloatingLabel();
    }

    // --- Label Pool ---

    private void BuildLabelPool()
    {
        _labelPool = new GameObject[LABEL_POOL_SIZE];
        _labelTexts = new TMP_Text[LABEL_POOL_SIZE];
        _labelCoroutines = new Coroutine[LABEL_POOL_SIZE];

        for (int i = 0; i < LABEL_POOL_SIZE; i++)
        {
            var canvasGO = new GameObject($"NearMissLabel_{i}");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            canvasGO.transform.localScale = Vector3.one * 0.02f;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(canvasGO.transform, false);
            var label = textGO.AddComponent<TextMeshProUGUI>();
            label.text = "+10 CLOSE CALL!";
            label.fontSize = _labelFontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, 0.85f, 0f, 1f);
            var rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 120f);

            canvasGO.SetActive(false);
            _labelPool[i] = canvasGO;
            _labelTexts[i] = label;
        }
    }

    private void ShowFloatingLabel()
    {
        int idx = _labelIndex;
        _labelIndex = (_labelIndex + 1) % LABEL_POOL_SIZE;

        // Stop previous coroutine on this slot if still running
        if (_labelCoroutines[idx] != null)
            StopCoroutine(_labelCoroutines[idx]);

        var go = _labelPool[idx];
        var label = _labelTexts[idx];
        go.SetActive(true);

        _labelCoroutines[idx] = StartCoroutine(AnimateLabel(go, label, idx));
    }

    private IEnumerator AnimateLabel(GameObject canvasGO, TMP_Text label, int slotIndex)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        canvasGO.transform.position = spawnPos;
        Color startColor = new Color(1f, 0.85f, 0f, 1f);
        label.color = startColor;

        if (_cachedCamera != null)
            canvasGO.transform.rotation = Quaternion.LookRotation(
                canvasGO.transform.position - _cachedCamera.transform.position);

        float elapsed = 0f;
        while (elapsed < _labelLifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _labelLifetime;

            canvasGO.transform.position = spawnPos + Vector3.up * (_labelFloatDistance * t);

            if (_cachedCamera != null)
                canvasGO.transform.rotation = Quaternion.LookRotation(
                    canvasGO.transform.position - _cachedCamera.transform.position);

            float fadeT = Mathf.InverseLerp(0.6f, 1f, t);
            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - fadeT);
            yield return null;
        }

        canvasGO.SetActive(false);
        _labelCoroutines[slotIndex] = null;
    }
}
