using UnityEngine;

/// <summary>
/// Renders stretched billboard speed-line streaks that scale with the car's current speed.
/// Attach to the Main Camera GameObject.
/// The particle system is built fully programmatically — no prefab required.
/// Streaks are only visible above 70% of the car's max speed.
/// </summary>
[RequireComponent(typeof(Camera))]
public class SpeedLinesVFX : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The CarController whose CurrentSpeed drives the effect intensity.")]
    [SerializeField] private CarController _car;

    [Header("Speed Thresholds")]
    [Tooltip("Fraction of maxSpeed above which speed lines become visible (default 0.7).")]
    [SerializeField] private float _activationFraction = 0.7f;

    [Tooltip("The car's maximum speed — should match the LevelData maxSpeed value or be set to a sensible upper bound.")]
    [SerializeField] private float _maxSpeed = 60f;

    [Header("Particle Settings")]
    [Tooltip("Maximum emission rate (particles per second) at full speed.")]
    [SerializeField] private float _maxEmissionRate = 30f;

    [Tooltip("Length multiplier for the stretched billboard streaks.")]
    [SerializeField] private float _stretchAmount = 4f;

    // Runtime
    private ParticleSystem _ps;
    private ParticleSystem.EmissionModule _emission;
    private bool _isBuilt;

    private void Awake()
    {
        BuildParticleSystem();
    }

    private void Update()
    {
        if (!_isBuilt || _car == null) return;

        float speed       = _car.CurrentSpeed;
        float speedFrac   = (_maxSpeed > 0f) ? (speed / _maxSpeed) : 0f;
        float threshold   = _activationFraction;

        if (speedFrac < threshold)
        {
            // Below threshold — stop emitting
            if (_ps.isEmitting)
                _ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        // Above threshold — remap [threshold, 1] → [0, 1] for emission scaling
        float t = Mathf.InverseLerp(threshold, 1f, speedFrac);
        float rate = Mathf.Lerp(0f, _maxEmissionRate, t);

        if (!_ps.isPlaying)
            _ps.Play();

        _emission.rateOverTime = rate;
    }

    // -------------------------------------------------------------------------
    // Programmatic particle system construction
    // -------------------------------------------------------------------------

    private void BuildParticleSystem()
    {
        GameObject go = new GameObject("SpeedLinesVFX");
        go.transform.SetParent(transform, false);
        // Position just in front of camera in view-space
        go.transform.localPosition = new Vector3(0f, 0f, 1f);

        _ps = go.AddComponent<ParticleSystem>();

        // ---- Main ----
        ParticleSystem.MainModule main = _ps.main;
        main.loop             = true;
        main.playOnAwake      = false;
        main.duration         = 1f;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.55f),
            new Color(0.85f, 0.92f, 1f, 0.3f)
        );
        main.gravityModifier  = 0f;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.maxParticles     = 60;

        // ---- Emission (start stopped) ----
        _emission = _ps.emission;
        _emission.enabled        = true;
        _emission.rateOverTime   = 0f;

        // ---- Shape: thin cone that fans out from camera ----
        ParticleSystem.ShapeModule shape = _ps.shape;
        shape.enabled     = true;
        shape.shapeType   = ParticleSystemShapeType.Cone;
        shape.angle       = 35f;
        shape.radius      = 0.01f;
        shape.radiusThickness = 1f;

        // ---- Velocity over lifetime: push outward radially ----
        ParticleSystem.VelocityOverLifetimeModule vel = _ps.velocityOverLifetime;
        vel.enabled    = true;
        vel.space      = ParticleSystemSimulationSpace.Local;
        vel.radial     = new ParticleSystem.MinMaxCurve(6f);

        // ---- Renderer: stretched billboard for streak look ----
        ParticleSystemRenderer rend = go.GetComponent<ParticleSystemRenderer>();
        rend.renderMode    = ParticleSystemRenderMode.Stretch;
        rend.velocityScale = _stretchAmount;
        rend.lengthScale   = 0f;
        rend.sortingOrder  = 10; // Render above 3D world, below UI

        // Leave material default — Unity's built-in particle material
        // renders correctly under URP without Shader.Find().

        _isBuilt = true;
    }
}
