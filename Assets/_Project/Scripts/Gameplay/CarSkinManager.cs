using UnityEngine;

/// <summary>
/// Manages car color skins. Applies a color tint to all body renderers on the truck.
/// Skins are unlocked with coins and stored in PlayerPrefs.
/// </summary>
public class CarSkinManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int _skinCost = 50;

    [Header("Body Renderers")]
    [SerializeField] private Renderer[] _bodyRenderers;

    [Header("Skin Definitions")]
    [SerializeField] private CarSkin[] _skins;

    private int _selectedSkinIndex;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    // MaterialPropertyBlock avoids creating material instances (no GC, no leak)
    private MaterialPropertyBlock _propBlock;

    public CarSkin[] Skins => _skins;
    public int SelectedSkinIndex => _selectedSkinIndex;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);

        // Ensure default skin is always unlocked
        if (_skins != null && _skins.Length > 0)
            PlayerPrefs.SetInt("SkinUnlocked_0", 1);
    }

    private void Start()
    {
        ApplySkin(_selectedSkinIndex);
    }

    /// <summary>
    /// Apply a skin by index. Only works if the skin is unlocked.
    /// </summary>
    public void SelectSkin(int index)
    {
        if (index < 0 || index >= _skins.Length) return;
        if (!IsSkinUnlocked(index)) return;

        _selectedSkinIndex = index;
        PlayerPrefs.SetInt("SelectedSkin", index);
        ApplySkin(index);
    }

    /// <summary>
    /// Attempt to unlock a skin with coins. Returns true if successful.
    /// </summary>
    public bool TryUnlockSkin(int index)
    {
        if (index < 0 || index >= _skins.Length) return false;
        if (IsSkinUnlocked(index)) return true;

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (totalCoins < _skinCost) return false;

        PlayerPrefs.SetInt("TotalCoins", totalCoins - _skinCost);
        PlayerPrefs.SetInt("SkinUnlocked_" + index, 1);
        PlayerPrefs.Save();
        return true;
    }

    public bool IsSkinUnlocked(int index)
    {
        if (index == 0) return true; // Default is always free
        return PlayerPrefs.GetInt("SkinUnlocked_" + index, 0) == 1;
    }

    public int SkinCost => _skinCost;

    private void ApplySkin(int index)
    {
        if (index < 0 || index >= _skins.Length) return;

        Color color = _skins[index].color;

        // Try explicit renderer references first
        bool applied = false;
        if (_bodyRenderers != null && _bodyRenderers.Length > 0)
        {
            foreach (var r in _bodyRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(BaseColorID, color);
                r.SetPropertyBlock(_propBlock);
                applied = true;
            }
        }

        // Fallback: find TruckModel or any MeshRenderer on the car at runtime
        if (!applied)
        {
            var renderer = GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(BaseColorID, color);
                renderer.SetPropertyBlock(_propBlock);
            }
        }
    }
}

[System.Serializable]
public struct CarSkin
{
    public string name;
    public Color color;
}
