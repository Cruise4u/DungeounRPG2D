using System.Collections.Generic;
using DungeonRPG.Grid;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] private Transform poolParent;
    [SerializeField] private float spawnHeightOffset = 0f;

    private readonly Queue<GameObject> _pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
            _pool.Enqueue(CreatePooledInstance());
    }

    /// <summary>
    /// Picks a random free tile from the 4x4 region and spawns a unit on top of it.
    /// Returns null if no free tile is available.
    /// </summary>
    public GameObject SpawnOnGrid()
    {
        var tile = GridManager.Instance.GetRandomFreeTile();
        if (tile == null) return null;

        var spawnPos = tile.transform.position + new Vector3(0f, spawnHeightOffset, 0f);
        var unit = Spawn(spawnPos, Quaternion.identity);

        GridManager.Instance.PlaceItem(tile.X, tile.Y, TileItemType.Character, unit);

        return unit;
    }

    /// <summary>Despawn a unit and free its tile.</summary>
    public void DespawnFromGrid(GameObject unit, int tileX, int tileY)
    {
        GridManager.Instance.ClearTile(tileX, tileY);
        Despawn(unit);
    }

    // ── Pool core ──────────────────────────────────────────────────────────────

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        var unit = _pool.Count > 0 ? _pool.Dequeue() : CreatePooledInstance();
        unit.transform.SetPositionAndRotation(position, rotation);
        unit.SetActive(true);
        return unit;
    }

    public void Despawn(GameObject unit)
    {
        unit.SetActive(false);
        unit.transform.SetParent(poolParent);
        _pool.Enqueue(unit);
    }

    private GameObject CreatePooledInstance()
    {
        var instance = Instantiate(unitPrefab, poolParent);
        instance.SetActive(false);
        return instance;
    }
}
