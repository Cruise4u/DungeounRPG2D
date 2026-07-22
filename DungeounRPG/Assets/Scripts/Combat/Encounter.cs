using System.Collections.Generic;
using UnityEngine;

/// <summary>One enemy type and how many of it this encounter spawns.</summary>
[System.Serializable]
public class EncounterEnemyEntry
{
    public ECharacterPoolID poolId;
    [Min(1)] public int count = 1;
}

/// <summary>
/// Defines a single combat encounter: which enemy types spawn, how many of each, and which
/// CharacterSlots in the combat zone they're allowed to occupy. Enemies are pulled from a
/// CharacterSpawner (the same pooling abstraction used for the player's evolved characters)
/// instead of being Instantiate'd directly. Unlike the player's tokens, enemies never merge —
/// they're placed directly into the combat zone fully formed, and the player only
/// adjusts/merges their own side around them.
/// </summary>
public class Encounter : MonoBehaviour
{
    private const int MaxInitialEnemies = 5;

    [SerializeField] private List<EncounterEnemyEntry> enemies = new();
    [SerializeField] private CharacterSlot[] enemySlots;
    [SerializeField] private AITeam enemyTeam;
    [SerializeField] private CharacterSpawner enemySpawner;
    [SerializeField] private AIController enemyController;

    private void Start()
    {
        SpawnEncounter();
    }

    /// <summary>
    /// Spawns up to MaxInitialEnemies enemies (one per configured entry, in order) from the
    /// character pool, places each into the next free enemy slot, and registers it with the
    /// AI team.
    /// </summary>
    public void SpawnEncounter()
    {
        int slotIndex = 0;
        int spawned = 0;

        foreach (var entry in enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                if (spawned >= MaxInitialEnemies)
                {
                    Debug.LogWarning($"[Encounter] Reached the {MaxInitialEnemies}-enemy cap — remaining enemies not spawned.");
                    return;
                }

                if (slotIndex >= enemySlots.Length)
                {
                    Debug.LogWarning("[Encounter] Ran out of enemy slots — remaining enemies not spawned.");
                    return;
                }

                var slot = enemySlots[slotIndex++];
                Debug.Log("enemySpawner is : " + enemySpawner);
                var go = enemySpawner.Spawn(entry.poolId, slot.transform.position);
                if (go == null || !go.TryGetComponent<Character>(out var character)) continue;
                if (character is AICharacter aiCharacter)
                    enemyTeam.AddMember(character);
                slot.OccupyCharacter(character);
                spawned++;
            }
        }
    }
}
