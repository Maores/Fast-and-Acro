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

    public string CurrentLevelName => _levelData.levelName;

    /// <summary>
    /// Called by GameManager.StartGame() to set up the level.
    /// </summary>
    public void SetupLevel()
    {
        _obstaclePool.ReturnAll();
        SpawnObstacles();
        PositionFinishLine();
        ScaleRoad();
    }

    private void SpawnObstacles()
    {
        int count = _levelData.obstacleCount;
        float startZ = _levelData.obstacleStartOffset;
        float endZ = _levelData.trackLength - 10f; // Leave room before finish
        float spacing = (endZ - startZ) / Mathf.Max(count, 1);

        for (int i = 0; i < count; i++)
        {
            float z = startZ + spacing * i;

            // Add some randomness to Z position
            z += Random.Range(-spacing * 0.3f, spacing * 0.3f);
            z = Mathf.Clamp(z, startZ, endZ);

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

        // Scale the road plane to match track length
        // Assumes road is a default plane (10x10 units) or cube
        float totalWidth = _gameConfig.laneWidth * _gameConfig.laneCount + 2f; // +2 for margins
        _road.localScale = new Vector3(totalWidth, 1f, _levelData.trackLength);
        _road.position = new Vector3(0f, -0.01f, _levelData.trackLength / 2f);
    }
}
