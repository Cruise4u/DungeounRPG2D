using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object-pool spawner. Holds one Queue&lt;GameObject&gt; per named pool and
/// knows nothing about what kind of object it spawns — subclasses (TokenSpawner,
/// CharacterSpawner, UISpawner, ...) plug in their own TEntry type for extra per-pool data.
/// </summary>
public abstract class PoolSpawner<TEntry> : MonoBehaviour, IPoolSpawner where TEntry : PoolEntry
{
    [SerializeField] protected List<TEntry> pools = new();
    [SerializeField] protected Transform poolParent;

    protected readonly Dictionary<string, TEntry> PoolMap = new();

    public IReadOnlyList<TEntry> Pools => pools;

    protected virtual void Awake()
    {
        foreach (var entry in pools)
        {
            PoolMap[entry.PoolKey] = entry;
            for (int i = 0; i < entry.initialSize; i++)
                entry.Pool.Enqueue(CreateInstance(entry.prefab));
        }
    }

    public virtual GameObject Spawn(string poolId, Vector3 position)
    {
        if (!PoolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[{GetType().Name}] Pool '{poolId}' not found.");
            return null;
        }

        var instance = entry.Pool.Count > 0 ? entry.Pool.Dequeue() : CreateInstance(entry.prefab);
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        instance.SetActive(true);
        return instance;
    }

    public virtual void Despawn(string poolId, GameObject instance)
    {
        if (!PoolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[{GetType().Name}] Pool '{poolId}' not found — destroying instead.");
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(poolParent);
        entry.Pool.Enqueue(instance);
    }

    /// <summary>Instantiates a fresh pooled instance, running IPoolSetup on it before disabling it.</summary>
    protected GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab, poolParent);

        foreach (var setup in instance.GetComponentsInChildren<IPoolSetup>(true))
            setup.OnPoolSetup();

        instance.SetActive(false);
        return instance;
    }
}
