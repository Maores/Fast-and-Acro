using UnityEngine;

/// <summary>
/// Per-level configuration. Create one asset per level.
/// Create via Assets > Create > FastAndAcro > LevelData.
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "FastAndAcro/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 01";
    public string country = "USA";

    [Header("Track")]
    [Tooltip("Total track length in units (Z axis)")]
    public float trackLength = 200f;

    [Header("Obstacles")]
    [Tooltip("Number of obstacles to spawn along the track")]
    public int obstacleCount = 15;

    [Tooltip("Minimum Z distance between obstacles")]
    public float minObstacleSpacing = 8f;

    [Tooltip("Z offset from start where obstacles begin spawning")]
    public float obstacleStartOffset = 20f;

    [Header("Obstacle Types")]
    [Tooltip("Fraction of obstacles that move side-to-side (0-1)")]
    [Range(0f, 1f)]
    public float movingObstacleRatio = 0.3f;

    [Tooltip("Speed of moving obstacles")]
    public float movingObstacleSpeed = 2f;
}
