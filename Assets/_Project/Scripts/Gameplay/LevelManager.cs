using UnityEngine;

/// <summary>
/// Level setup and flow management. Spawns obstacles from pool,
/// positions the finish line, tracks level progress.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LevelData _levelData;
    [SerializeField] private GameConfig _gameConfig;

    [Header("References")]
    [SerializeField] private ObjectPool _obstaclePool;
    [SerializeField] private FinishLine _finishLine;
    [SerializeField] private Transform _road;

    [Header("Lane Markings")]
    [SerializeField] private Material _laneMarkingMaterial;
    [SerializeField] private Material _edgeLineMaterial;

    [Header("Visual Theme")]
    [SerializeField] private Material _roadMaterial;

    [Header("Obstacle Variants")]
    [SerializeField] private Material[] _obstacleColorVariants;

    [Header("Collectibles")]
    [SerializeField] private ObjectPool _coinPool;
    [SerializeField] private ObjectPool _powerUpPool;
    [SerializeField] private int _coinsPerLevel = 15;
    [SerializeField] private int _powerUpsPerLevel = 3;

    private GameObject _laneMarkingsParent;

    public string CurrentLevelName => _levelData.levelName;

    /// <summary>
    /// Called by GameManager before SetupLevel() to inject the active level's data.
    /// </summary>
    public void SetLevelData(LevelData data)
    {
        _levelData = data;
    }

    /// <summary>
    /// Called by GameManager.StartGame() to set up the level.
    /// </summary>
    public void SetupLevel()
    {
        _obstaclePool.ReturnAll();
        if (_coinPool != null) _coinPool.ReturnAll();
        if (_powerUpPool != null) _powerUpPool.ReturnAll();

        SpawnObstacles();
        SpawnCoins();
        SpawnPowerUps();
        PositionFinishLine();
        ScaleRoad();
        GenerateLaneMarkings();
        ApplyVisualTheme();
    }

    private void SpawnObstacles()
    {
        int count = _levelData.obstacleCount;
        float minSpacing = _levelData.minObstacleSpacing;

        // Safe zone: no obstacles for the first safeZoneRatio of track length.
        // obstacleStartOffset can shift the absolute start even further.
        float safeZoneEnd = _levelData.trackLength * _levelData.safeZoneRatio;
        float startZ = Mathf.Max(_levelData.obstacleStartOffset, safeZoneEnd);
        float endZ = _levelData.trackLength - 10f; // Leave room before finish line

        // Track placed Z positions to enforce minimum spacing
        var placedZ = new System.Collections.Generic.List<float>(count);

        for (int i = 0; i < count; i++)
        {
            // Bias random value toward higher Z positions using a power curve.
            // Mathf.Pow(Random.value, 0.6f) shifts distribution toward 1 (end of track),
            // so obstacles are sparser near the start and denser near the finish.
            float t = Mathf.Pow(Random.value, 0.6f);
            float z = Mathf.Lerp(startZ, endZ, t);

            // Enforce minimum spacing: scan placed positions and nudge forward if needed
            placedZ.Sort();
            foreach (float placedPos in placedZ)
            {
                if (Mathf.Abs(z - placedPos) < minSpacing)
                    z = placedPos + minSpacing;
            }
            z = Mathf.Clamp(z, startZ, endZ);

            placedZ.Add(z);

            // Pick a random lane for the obstacle
            int lane = Random.Range(0, _gameConfig.laneCount);
            float x = _gameConfig.GetLanePosition(lane);

            // Decide if this obstacle moves
            bool isMoving = Random.value < _levelData.movingObstacleRatio;

            GameObject obj = _obstaclePool.Get();
            Obstacle obstacle = obj.GetComponent<Obstacle>();
            obstacle.Setup(
                new Vector3(x, 0.5f, z),
                isMoving,
                _levelData.movingObstacleSpeed
            );
        }
    }

    private void SpawnCoins()
    {
        if (_coinPool == null) return;

        float startZ = _levelData.obstacleStartOffset + 5f;
        float endZ = _levelData.trackLength - 15f;
        float spacing = (endZ - startZ) / Mathf.Max(_coinsPerLevel, 1);

        for (int i = 0; i < _coinsPerLevel; i++)
        {
            float z = startZ + spacing * i + Random.Range(-spacing * 0.2f, spacing * 0.2f);
            z = Mathf.Clamp(z, startZ, endZ);

            int lane = Random.Range(0, _gameConfig.laneCount);
            float x = _gameConfig.GetLanePosition(lane);

            GameObject obj = _coinPool.Get();
            Coin coin = obj.GetComponent<Coin>();
            coin.Setup(new Vector3(x, 1.0f, z));
        }
    }

    private void SpawnPowerUps()
    {
        if (_powerUpPool == null) return;

        float startZ = _levelData.obstacleStartOffset + 20f;
        float endZ = _levelData.trackLength - 30f;
        float spacing = (endZ - startZ) / Mathf.Max(_powerUpsPerLevel, 1);

        var types = new[] { PowerUpType.Shield, PowerUpType.SpeedBoost, PowerUpType.CoinMagnet };

        for (int i = 0; i < _powerUpsPerLevel; i++)
        {
            float z = startZ + spacing * i + Random.Range(-spacing * 0.1f, spacing * 0.1f);
            z = Mathf.Clamp(z, startZ, endZ);

            int lane = Random.Range(0, _gameConfig.laneCount);
            float x = _gameConfig.GetLanePosition(lane);

            GameObject obj = _powerUpPool.Get();
            PowerUp pu = obj.GetComponent<PowerUp>();
            pu.Setup(new Vector3(x, 1.2f, z), types[i % types.Length]);
        }
    }

    private void PositionFinishLine()
    {
        if (_finishLine != null)
        {
            _finishLine.transform.position = new Vector3(0f, 0f, _levelData.trackLength);
        }
    }

    private void ScaleRoad()
    {
        if (_road == null) return;

        // Thin road slab — top surface at y=0
        float totalWidth = _gameConfig.laneWidth * _gameConfig.laneCount + 4f; // +4 for wide margins
        float roadThickness = 0.1f;
        _road.localScale = new Vector3(totalWidth, roadThickness, _levelData.trackLength);
        _road.position = new Vector3(0f, -roadThickness / 2f, _levelData.trackLength / 2f);
    }

    private void ApplyVisualTheme()
    {
        // Sky
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = _levelData.skyColor;

        // Road color
        if (_roadMaterial != null)
            _roadMaterial.SetColor("_BaseColor", _levelData.roadColor);

        // Fog
        RenderSettings.fog = _levelData.enableFog;
        if (_levelData.enableFog)
        {
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = _levelData.fogDensity;
            RenderSettings.fogColor = _levelData.fogColor;
        }

        // Ambient light
        RenderSettings.ambientLight = _levelData.ambientColor;
    }

    private void GenerateLaneMarkings()
    {
        // Destroy previous lane markings
        if (_laneMarkingsParent != null)
            Destroy(_laneMarkingsParent);

        _laneMarkingsParent = new GameObject("LaneMarkings");

        float markingY = 0.01f;
        float dashLength = 3f;
        float dashGap = 2f;
        float dashWidth = 0.12f;
        float dashHeight = 0.02f;
        float trackLength = _levelData.trackLength;

        // Lane divider positions (between adjacent lanes)
        float laneWidth = _gameConfig.laneWidth;
        int laneCount = _gameConfig.laneCount;
        float centerLane = (laneCount - 1) / 2f;

        // Create dashes between each pair of lanes
        for (int i = 0; i < laneCount - 1; i++)
        {
            float dividerX = ((i + 0.5f) - centerLane) * laneWidth;
            float z = 0f;
            while (z < trackLength)
            {
                var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = "Dash";
                dash.transform.SetParent(_laneMarkingsParent.transform);
                dash.transform.position = new Vector3(dividerX, markingY, z + dashLength / 2f);
                dash.transform.localScale = new Vector3(dashWidth, dashHeight, dashLength);
                if (_laneMarkingMaterial != null)
                    dash.GetComponent<Renderer>().sharedMaterial = _laneMarkingMaterial;
                Destroy(dash.GetComponent<BoxCollider>());
                z += dashLength + dashGap;
            }
        }

        // Edge lines (solid, on both sides of road)
        float roadHalfWidth = (_gameConfig.laneWidth * laneCount + 4f) / 2f;
        float edgeInset = 0.15f;
        float[] edgeX = { -roadHalfWidth + edgeInset, roadHalfWidth - edgeInset };

        foreach (float xPos in edgeX)
        {
            var edgeLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edgeLine.name = "EdgeLine";
            edgeLine.transform.SetParent(_laneMarkingsParent.transform);
            edgeLine.transform.position = new Vector3(xPos, markingY, trackLength / 2f);
            edgeLine.transform.localScale = new Vector3(dashWidth, dashHeight, trackLength);
            if (_edgeLineMaterial != null)
                edgeLine.GetComponent<Renderer>().sharedMaterial = _edgeLineMaterial;
            Destroy(edgeLine.GetComponent<BoxCollider>());
        }
    }
}
