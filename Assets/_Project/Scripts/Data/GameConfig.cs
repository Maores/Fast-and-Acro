using UnityEngine;

/// <summary>
/// Central configuration for all tunable game values.
/// Create via Assets > Create > FastAndAcro > GameConfig.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "FastAndAcro/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Constant forward speed of the car (units/sec)")]
    public float forwardSpeed = 15f;

    [Tooltip("How fast the car switches lanes (units/sec)")]
    public float laneSwitchSpeed = 10f;

    [Tooltip("Distance between lane centers")]
    public float laneWidth = 2f;

    [Tooltip("Number of lanes (typically 3 or 5)")]
    public int laneCount = 3;

    [Header("Health")]
    public int maxHP = 3;

    [Tooltip("Seconds of invincibility after taking damage")]
    public float invincibilityDuration = 1f;

    [Header("Scoring — Star Thresholds")]
    [Tooltip("Max collisions allowed for 3 stars")]
    public int threeStarMaxCollisions = 0;

    [Tooltip("Max collisions allowed for 2 stars")]
    public int twoStarMaxCollisions = 2;

    [Tooltip("Max completion time for 3 stars (seconds)")]
    public float threeStarMaxTime = 30f;

    [Tooltip("Max completion time for 2 stars (seconds)")]
    public float twoStarMaxTime = 45f;

    [Header("Input")]
    [Tooltip("Minimum swipe distance in pixels to register a lane change")]
    public float swipeThreshold = 50f;

    [Tooltip("Maximum swipe duration in seconds")]
    public float swipeMaxDuration = 0.3f;

    [Header("Mobile")]
    public int targetFrameRate = 60;

    /// <summary>
    /// Returns the X position for a given lane index.
    /// Lane 0 = leftmost, lane (laneCount-1) = rightmost.
    /// </summary>
    public float GetLanePosition(int laneIndex)
    {
        float centerLane = (laneCount - 1) / 2f;
        return (laneIndex - centerLane) * laneWidth;
    }
}
