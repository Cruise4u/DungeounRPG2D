using UnityEngine;

[System.Serializable]
public class CharacterPoolEntry : PoolEntry
{
    public ECharacterPoolID poolId;
    public override string PoolKey => poolId.ToString();
}

/// <summary>
/// Pools the evolved CharacterPrefab for each of a TokenSpawner's figurine pools, matched by
/// enum ordinal (ETokenPoolID.TokenA -> ECharacterPoolID.CharacterA, etc.). Each character
/// pool is sized at half its matching figurine pool's initial size.
/// </summary>
public class CharacterSpawner : PoolSpawner<CharacterPoolEntry>
{
    [SerializeField] private TokenSpawner tokenSpawner;

    public GameObject Spawn(ECharacterPoolID poolId, Vector3 position) => Spawn(poolId.ToString(), position);

    public void Despawn(ECharacterPoolID poolId, GameObject instance) => Despawn(poolId.ToString(), instance);

    protected override void Awake()
    {
        if (tokenSpawner != null)
            BuildPoolsFromTokenSpawner();

        base.Awake();
    }

    private void BuildPoolsFromTokenSpawner()
    {
        foreach (var tokenEntry in tokenSpawner.Pools)
        {
            var characterPrefab = tokenEntry.prefab?.GetComponent<CharacterFigurine>()?.CharacterPrefab;
            if (characterPrefab == null) continue;

            pools.Add(new CharacterPoolEntry
            {
                poolId = (ECharacterPoolID)(int)tokenEntry.poolId,
                prefab = characterPrefab,
                initialSize = Mathf.Max(1, tokenEntry.initialSize / 2),
            });
        }
    }
}
