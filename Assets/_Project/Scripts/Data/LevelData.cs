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

    [Header("Speed Ramp")]
    [Tooltip("Starting forward speed (overrides GameConfig if > 0)")]
    public float startSpeed = 0f;

    [Tooltip("Speed increase per second while driving")]
    public float speedIncrement = 0.5f;

    [Tooltip("Maximum speed the car can reach")]
    public float maxSpeed = 30f;

    [Header("Visual Theme")]
    public Color skyColor = new Color(0.53f, 0.69f, 0.87f, 1f);
    public Color roadColor = new Color(0.25f, 0.25f, 0.27f, 1f);
    public Color fogColor = new Color(0.53f, 0.69f, 0.87f, 1f);

    [Tooltip("Enable distance fog for atmosphere")]
    public bool enableFog = false;

    [Tooltip("Fog density when enabled")]
    [Range(0f, 0.05f)]
    public float fogDensity = 0.01f;

    [Tooltip("Ambient light color tint")]
    public Color ambientColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("Scoring — Star Thresholds")]
    [Tooltip("Max collisions allowed for 3 stars")]
    public int threeStarMaxCollisions = 0;

    [Tooltip("Max collisions allowed for 2 stars")]
    public int twoStarMaxCollisions = 2;

    [Tooltip("Max completion time for 3 stars (seconds)")]
    public float threeStarMaxTime = 30f;

    [Tooltip("Max completion time for 2 stars (seconds)")]
    public float twoStarMaxTime = 45f;

    /// <summary>
    /// Calculate star rating based on per-level thresholds.
    /// Returns 1-3 stars.
    /// </summary>
    public int CalculateStars(int collisions, float time)
    {
        int stars = 3;

        // Collision penalty
        if (collisions > twoStarMaxCollisions)
            stars -= 2;
        else if (collisions > threeStarMaxCollisions)
            stars -= 1;

        // Time penalty
        if (time > twoStarMaxTime)
            stars -= 2;
        else if (time > threeStarMaxTime)
            stars -= 1;

        return Mathf.Clamp(stars, 1, 3);
    }
}
