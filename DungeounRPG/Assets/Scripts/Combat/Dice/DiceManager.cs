using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;

    [Tooltip("The special dice exclusive to the Player Team.")]
    [SerializeField] private SpecialDice playerSpecialDice;

    // Results from the last RollAll call — keyed by character.
    private readonly Dictionary<Character, DiceRollResult> _rolledValues = new();
    public IReadOnlyDictionary<Character, DiceRollResult> RolledValues => _rolledValues;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Rolls the regular Dice on every living character, then applies the player
    /// special dice on top of each player result. Stores each DiceRollResult.
    /// </summary>
    public void RollAll()
    {
        _rolledValues.Clear();

        var allCharacters = combatManager.AlivePlayerCharacters
            .Concat(combatManager.AliveEnemyCharacters)
            .OfType<Character>();

        foreach (var character in allCharacters)
        {
            var dice = character.GetComponent<Dice>();
            if (dice == null)
            {
                Debug.LogWarning($"[DiceManager] {character.name} has no Dice component — skipping.");
                continue;
            }

            var result = new DiceRollResult();
            dice.Roll(result);
            _rolledValues[character] = result;
            character.CurrentRollResult = result;
        }
    }

    /// <summary>
    /// Applies the player special dice on top of an existing roll result.
    /// Call this after RollAll when the player has a special dice equipped.
    /// </summary>
    public void ApplyPlayerSpecialDice(Character character)
    {
        if (playerSpecialDice == null)
        {
            Debug.LogWarning("[DiceManager] No SpecialDice assigned.");
            return;
        }

        if (!_rolledValues.TryGetValue(character, out var result))
        {
            Debug.LogWarning($"[DiceManager] No roll result found for {character.name}. Call RollAll first.");
            return;
        }

        playerSpecialDice.Roll(result);
        character.CurrentRollResult = result;
    }

    /// <summary>Returns the last roll result for a given character, or null if not yet rolled.</summary>
    public DiceRollResult GetResult(Character character)
        => _rolledValues.TryGetValue(character, out var result) ? result : null;
}
