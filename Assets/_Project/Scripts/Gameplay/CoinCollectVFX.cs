using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a gold particle burst at the world position of every collected coin.
/// Place this script on a single persistent GameObject in the scene (e.g. VFXManager).
/// No prefab required — the ParticleSystem is built programmatically in Awake.
/// </summary>
public class CoinCollectVFX : MonoBehaviour
{
    [Header("Burst Settings")]
    [Tooltip("Number of particles emitted per coin collect burst.")]
    [SerializeField] private int _burstCount = 20;

    [Tooltip("How long the particle system lives before being returned to the pool.")]
    [SerializeField] private float _lifetime = 1f;

    [Tooltip("How fast particles travel outward (world units per second).")]
    [SerializeField] private float _particleSpeed = 4f;

    [Tooltip("Particle start size.")]
    [SerializeField] private float _particleSize = 0.15f;

    // Pool of reusable particle systems (keeps allocations minimal at runtime)
    private const int POOL_SIZE = 4;
    private ParticleSystem[] _pool;
    private Coroutine[] _poolCoroutines;
    private int _poolIndex;
    private WaitForSeconds _waitLifetime;

    private void Awake()
    {
        _waitLifetime = new WaitForSeconds(_lifetime);
        BuildPool();
    }

    private void OnEnable()
    {
        Coin.OnCoinCollectedAt += HandleCoinCollected;
    }

    private void OnDisable()
    {
        Coin.OnCoinCollectedAt -= HandleCoinCollected;
    }

    // -------------------------------------------------------------------------
    // Event handler
    // -------------------------------------------------------------------------

    private void HandleCoinCollected(Vector3 worldPosition)
    {
        PlayBurstAt(worldPosition);
    }

    // -------------------------------------------------------------------------
    // Burst playback
    // -------------------------------------------------------------------------

    private void PlayBurstAt(Vector3 worldPosition)
    {
        int idx = _poolIndex;
        _poolIndex = (_poolIndex + 1) % POOL_SIZE;

        // Cancel previous coroutine on this slot to prevent stale deactivation
        if (_poolCoroutines[idx] != null)
            StopCoroutine(_poolCoroutines[idx]);

        ParticleSystem ps = _pool[idx];
        ps.transform.position = worldPosition;
        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();

        _poolCoroutines[idx] = StartCoroutine(ReturnToPoolAfter(ps, idx));
    }

    private IEnumerator ReturnToPoolAfter(ParticleSystem ps, int slotIndex)
    {
        yield return _waitLifetime;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        _poolCoroutines[slotIndex] = null;
    }

    // -------------------------------------------------------------------------
    // Pool management
    // -------------------------------------------------------------------------

    private void BuildPool()
    {
        _pool = new ParticleSystem[POOL_SIZE];
        _poolCoroutines = new Coroutine[POOL_SIZE];
        for (int i = 0; i < POOL_SIZE; i++)
        {
            _pool[i] = BuildParticleSystem($"CoinVFX_{i}");
        }
    }

    private ParticleSystem BuildParticleSystem(string goName)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        go.SetActive(false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        // ---- Main module ----
        ParticleSystem.MainModule main = ps.main;
        main.loop             = false;
        main.playOnAwake      = false;
        main.duration         = 0.5f;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(_particleSpeed * 0.6f, _particleSpeed);
        main.startSize        = new ParticleSystem.MinMaxCurve(_particleSize * 0.7f, _particleSize * 1.3f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
            new Color(1.0f, 0.85f, 0.1f, 1f),   // bright gold
            new Color(1.0f, 0.65f, 0.0f, 1f)    // deep amber
        );
        main.gravityModifier  = new ParticleSystem.MinMaxCurve(0.3f);
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.maxParticles     = _burstCount * POOL_SIZE;

        // ---- Emission: single burst ----
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] {
            new ParticleSystem.Burst(0f, (short)_burstCount)
        });

        // ---- Shape: sphere for a nice outward pop ----
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Sphere;
        shape.radius       = 0.2f;
        shape.radiusThickness = 1f;

        // ---- Size over lifetime: shrink toward end ----
        ParticleSystem.SizeOverLifetimeModule sizeMod = ps.sizeOverLifetime;
        sizeMod.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        sizeMod.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ---- Renderer: use the default particle shader (URP-safe) ----
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // Leave material as the default; Unity assigns the "Default-Particle" material
        // automatically, which works with URP without any Shader.Find() calls.

        return ps;
    }
}
