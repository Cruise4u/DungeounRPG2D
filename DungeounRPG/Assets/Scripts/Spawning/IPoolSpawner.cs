using UnityEngine;

/// <summary>
/// Common contract for any object-pool-backed spawner (tokens, characters, UI panels, etc.),
/// regardless of what GameObject type it pools.
/// </summary>
public interface IPoolSpawner
{
    GameObject Spawn(string poolId, Vector3 position);
    void Despawn(string poolId, GameObject instance);
}
