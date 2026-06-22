using System.Collections.Generic;
using DungeonRPG.Grid;
using UnityEngine;

[System.Serializable]
public class UnitPool
{
    public string poolId;
    public GameObject prefab;
    [Min(1)] public int initialSize = 5;

    [System.NonSerialized] public Queue<GameObject> Pool = new();
    [System.NonSerialized] public Queue<GameObject> CharacterPool = new();
}

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private List<UnitPool> pools = new();
    [SerializeField] private Transform poolParent;
    [SerializeField] private float spawnHeightOffset = 0f;

    private readonly Dictionary<string, UnitPool> _poolMap = new();

    private void Awake()
    {
        foreach (var entry in pools)
        {
            _poolMap[entry.poolId] = entry;
            for (int i = 0; i < entry.initialSize; i++)
                entry.Pool.Enqueue(CreateInstance(entry));

            var characterPrefab = entry.prefab?.GetComponent<CharacterToken>()?.CharacterPrefab;
            if (characterPrefab == null) continue;

            int characterPoolSize = Mathf.Max(1, entry.initialSize / 4);
            for (int i = 0; i < characterPoolSize; i++)
                entry.CharacterPool.Enqueue(CreateInstance(characterPrefab));
        }
    }

    /// <summary>Spawns a unit from the named pool on a random free grid tile.</summary>
    public void SpawnOnGrid()
    {
        var tile = GridManager.Instance.GetRandomFreeTile();
        if (tile == null)
        {
            Debug.LogWarning("[UnitSpawner] No free tiles available.");
            return;
        }

        var randomPoolId = pools[Random.Range(0, pools.Count)].poolId;
        var unit = Spawn(randomPoolId, tile.transform.position + new Vector3(0f, spawnHeightOffset, 0f));
        var token = unit.GetComponent<CharacterToken>();
        token.poolId =  randomPoolId;
        if (unit == null) return;

        GridManager.Instance.PlaceItem(tile.X, tile.Y, TileItemType.Character, unit);
    }

    /// <summary>Spawns a unit from the named pool at an explicit world position.</summary>
    public GameObject Spawn(string poolId, Vector3 position)
    {
        if (!_poolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[UnitSpawner] Pool '{poolId}' not found.");
            return null;
        }

        var unit = entry.Pool.Count > 0 ? entry.Pool.Dequeue() : CreateInstance(entry);
        unit.transform.SetPositionAndRotation(position, Quaternion.identity);
        unit.SetActive(true);
        return unit;
    }

    /// <summary>Returns a unit to its pool.</summary>
    public void Despawn(string poolId, GameObject unit)
    {
        if (!_poolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[UnitSpawner] Pool '{poolId}' not found — destroying instead.");
            Destroy(unit);
            return;
        }

        unit.SetActive(false);
        unit.transform.SetParent(poolParent);
        entry.Pool.Enqueue(unit);
    }

    /// <summary>Returns a unit to its pool and clears the grid tile it occupied.</summary>
    public void DespawnFromGrid(string poolId, GameObject unit, int tileX, int tileY)
    {
        GridManager.Instance.ClearTile(tileX, tileY);
        Despawn(poolId, unit);
    }

    /// <summary>Spawns the evolved character from the named pool's character pool at an explicit world position.</summary>
    public GameObject SpawnCharacter(string poolId, Vector3 position)
    {
        if (!_poolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[UnitSpawner] Pool '{poolId}' not found.");
            return null;
        }

        var characterPrefab = entry.prefab?.GetComponent<CharacterToken>()?.CharacterPrefab;
        var unit = entry.CharacterPool.Count > 0 ? entry.CharacterPool.Dequeue() : CreateInstance(characterPrefab);
        unit.transform.SetPositionAndRotation(position, Quaternion.identity);
        unit.SetActive(true);
        return unit;
    }

    /// <summary>Returns a spawned character to its pool's character pool.</summary>
    public void DespawnCharacter(string poolId, GameObject character)
    {
        if (!_poolMap.TryGetValue(poolId, out var entry))
        {
            Debug.LogError($"[UnitSpawner] Pool '{poolId}' not found — destroying instead.");
            Destroy(character);
            return;
        }

        character.SetActive(false);
        character.transform.SetParent(poolParent);
        entry.CharacterPool.Enqueue(character);
    }

    private GameObject CreateInstance(UnitPool entry) => CreateInstance(entry.prefab);

    private GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab, poolParent);

        foreach (var setup in instance.GetComponentsInChildren<IPoolSetup>(true))
            setup.OnPoolSetup();

        instance.SetActive(false);
        return instance;
    }
}
