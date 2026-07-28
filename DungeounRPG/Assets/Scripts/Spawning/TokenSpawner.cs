using DungeonRPG.Grid;
using UnityEngine;

[System.Serializable]
public class TokenPoolEntry : PoolEntry
{
    public ETokenPoolID poolId;
    public override string PoolKey => poolId.ToString();
}

/// <summary>Pools CharacterFigurine instances per poolId and places them on the grid.</summary>
public class TokenSpawner : PoolSpawner<TokenPoolEntry>
{
    [SerializeField] private float spawnHeightOffset = 0f;

    public GameObject Spawn(ETokenPoolID poolId, Vector3 position) => Spawn(poolId.ToString(), position);

    public void Despawn(ETokenPoolID poolId, GameObject instance) => Despawn(poolId.ToString(), instance);

    /// <summary>Spawns a figurine from a random pool on a random free grid tile.</summary>
    public void SpawnOnGrid()
    {
        var tile = GridManager.Instance.GetRandomFreeTile();
        if (tile == null)
        {
            Debug.LogWarning("[TokenSpawner] No free tiles available.");
            return;
        }

        var randomPoolId = pools[Random.Range(0, pools.Count)].poolId;
        var unit = Spawn(randomPoolId, tile.transform.position + new Vector3(0f, spawnHeightOffset, 0f));
        if (unit == null) return;

        var token = unit.GetComponent<CharacterFigurine>();
        token.poolId = randomPoolId;

        GridManager.Instance.PlaceItem(tile.X, tile.Y, TileItemType.Character, unit);
    }

    /// <summary>Returns a figurine to its pool and clears the grid tile it occupied.</summary>
    public void DespawnFromGrid(ETokenPoolID poolId, GameObject unit, int tileX, int tileY)
    {
        GridManager.Instance.ClearTile(tileX, tileY);
        Despawn(poolId, unit);
    }
}
