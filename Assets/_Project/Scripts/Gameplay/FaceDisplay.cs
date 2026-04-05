using UnityEngine;

/// <summary>
/// Displays a face image above the car using preset sprites (MVP).
/// Attach to a Quad child of the Car, positioned above it.
/// Post-MVP: add gallery upload by calling SetFaceTexture() with a Texture2D.
/// </summary>
public class FaceDisplay : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Renderer _faceRenderer;

    [Tooltip("Height above the car to display the face")]
    [SerializeField] private float _heightAboveCar = 1.5f;

    [Header("Preset Faces")]
    [SerializeField] private Texture2D[] _presetFaces;

    private int _selectedPresetIndex;
    private Transform _carTransform;
    private Camera _mainCamera;

    private const string SAVE_KEY = "SelectedFaceIndex";

    private void Start()
    {
        _carTransform = transform.parent;
        _mainCamera = Camera.main;

        // Load saved selection
        _selectedPresetIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);
        ApplyPresetFace(_selectedPresetIndex);
    }

    private void LateUpdate()
    {
        if (_carTransform == null || _mainCamera == null) return;

        // Position above car
        Vector3 pos = _carTransform.position;
        pos.y += _heightAboveCar;
        transform.position = pos;

        // Billboard: always face camera
        transform.LookAt(
            transform.position + _mainCamera.transform.rotation * Vector3.forward,
            _mainCamera.transform.rotation * Vector3.up
        );
    }

    // --- Public API ---

    /// <summary>
    /// Select a preset face by index. Saves to PlayerPrefs.
    /// </summary>
    public void SelectPreset(int index)
    {
        if (index < 0 || index >= _presetFaces.Length) return;

        _selectedPresetIndex = index;
        PlayerPrefs.SetInt(SAVE_KEY, index);
        PlayerPrefs.Save();

        ApplyPresetFace(index);
    }

    /// <summary>
    /// Get the number of available preset faces.
    /// </summary>
    public int PresetCount => _presetFaces != null ? _presetFaces.Length : 0;

    /// <summary>
    /// Post-MVP: Apply a custom Texture2D (from gallery/camera).
    /// </summary>
    public void SetFaceTexture(Texture2D texture)
    {
        if (_faceRenderer != null && texture != null)
        {
            _faceRenderer.material.mainTexture = texture;
        }
    }

    // --- Internal ---

    private void ApplyPresetFace(int index)
    {
        if (_presetFaces == null || _presetFaces.Length == 0) return;

        index = Mathf.Clamp(index, 0, _presetFaces.Length - 1);

        if (_faceRenderer != null && _presetFaces[index] != null)
        {
            _faceRenderer.material.mainTexture = _presetFaces[index];
        }
    }
}
