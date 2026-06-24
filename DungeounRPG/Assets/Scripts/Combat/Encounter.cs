using System.Collections.Generic;
using UnityEngine;

/// <summary>One enemy type and how many of it this encounter spawns.</summary>
[System.Serializable]
public class EncounterEnemyEntry
{
    public AICharacter prefab;
    [Min(1)] public int count = 1;
}

/// <summary>
/// Defines a single combat encounter: which enemy types spawn, how many of each, and which
/// CharacterSlots in the combat zone they're allowed to occupy. Unlike the player's tokens,
/// enemies never merge — they're placed directly into the combat zone fully formed, and the
/// player only adjusts/merges their own side around them.
/// </summary>
public class Encounter : MonoBehaviour
{
    [SerializeField] private List<EncounterEnemyEntry> enemies = new();
    [SerializeField] private CharacterSlot[] enemySlots;
    [SerializeField] private AITeam enemyTeam;

    /// <summary>Instantiates every configured enemy into the next free enemy slot and adds it to the AI team.</summary>
    public void SpawnEncounter()
    {
        int slotIndex = 0;

        foreach (var entry in enemies)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                if (slotIndex >= enemySlots.Length)
                {
                    Debug.LogWarning("[Encounter] Ran out of enemy slots — remaining enemies not spawned.");
                    return;
                }

                var slot = enemySlots[slotIndex++];
                var character = Instantiate(entry.prefab, slot.transform.position, Quaternion.identity);

                enemyTeam.AddMember(character);
                slot.OccupyCharacter(character);
            }
        }
    }
}
