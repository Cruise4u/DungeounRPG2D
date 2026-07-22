using System.Collections.Generic;
using UnityEngine;

public enum ETokenPoolID
{
    TokenA,
    TokenB,
    TokenC
}

public enum ECharacterPoolID
{
    CharacterA,
    CharacterB,
    CharacterC
}

/// <summary>Base config for a single named pool. Concrete subclasses provide a typed, enum-backed poolId.</summary>
[System.Serializable]
public abstract class PoolEntry
{
    public GameObject prefab;
    [Min(1)] public int initialSize = 5;

    [System.NonSerialized] public Queue<GameObject> Pool = new();

    /// <summary>String key used internally by PoolSpawner's pool dictionary.</summary>
    public abstract string PoolKey { get; }
}
