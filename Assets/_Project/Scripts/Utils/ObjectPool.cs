using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple generic object pool. Pre-instantiates objects and recycles them.
/// Avoids GC spikes from Instantiate/Destroy on mobile.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialSize = 20;

    private readonly Queue<GameObject> _available = new Queue<GameObject>();
    private Transform _poolParent;

    private void Awake()
    {
        _poolParent = new GameObject($"Pool_{_prefab.name}").transform;
        _poolParent.SetParent(transform);

        for (int i = 0; i < _initialSize; i++)
        {
            CreateInstance();
        }
    }

    /// <summary>
    /// Get an inactive object from the pool. Returns null if pool is exhausted
    /// (shouldn't happen if sized correctly).
    /// </summary>
    public GameObject Get()
    {
        if (_available.Count == 0)
        {
            // Auto-expand if needed
            CreateInstance();
        }

        GameObject obj = _available.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _available.Enqueue(obj);
    }

    /// <summary>
    /// Return all active children to the pool.
    /// </summary>
    public void ReturnAll()
    {
        if (_poolParent == null) return;

        foreach (Transform child in _poolParent)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                _available.Enqueue(child.gameObject);
            }
        }
    }

    private void CreateInstance()
    {
        GameObject obj = Instantiate(_prefab, _poolParent);
        obj.SetActive(false);
        _available.Enqueue(obj);
    }
}
