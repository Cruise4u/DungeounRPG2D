using System.Collections.Generic;
using DungeonRPG.Grid;
using UnityEngine;

public class CharacterFigurineMerger : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private CharacterSpawner characterSpawner;
    [SerializeField] private CharacterMerger characterMerger;

    [Header("Combat Zone")]
    [SerializeField] private CharacterSlot[] combatSlots;

    [Header("Evolution Colors")]
    public Color basicColor    = Color.white;
    public Color superColor = Color.cyan;
    public Color megaColor     = Color.magenta;
    public Color ultraColor    = Color.yellow;

    private CharacterSlot[] _slots;

    // figurine → the combat-zone Character it currently owns (only set once a figurine reaches Super+)
    private readonly Dictionary<CharacterFigurine, Character> _combatCharacters = new();

    private void Awake()
    {
        _slots = FindObjectsByType<CharacterSlot>(FindObjectsSortMode.None);
    }

    private Color ColorForEvolution(EEvolutionTypeID evolution) => evolution switch
    {
        EEvolutionTypeID.Basic    => basicColor,
        EEvolutionTypeID.Super => superColor,
        EEvolutionTypeID.Mega     => megaColor,
        EEvolutionTypeID.Ultra    => ultraColor,
        _                         => basicColor,
    };

    private static EEvolutionTypeID NextEvolution(EEvolutionTypeID current) => current switch
    {
        EEvolutionTypeID.Basic    => EEvolutionTypeID.Super,
        EEvolutionTypeID.Super => EEvolutionTypeID.Mega,
        EEvolutionTypeID.Mega     => EEvolutionTypeID.Ultra,
        _                         => EEvolutionTypeID.Ultra,
    };

    /// <summary>
    /// Merges figurineFrom into figurineTo. figurineFrom is destroyed and its slot vacated.
    /// figurineTo advances one evolution tier and the merged character is spawned at its slot.
    /// Returns false if tokens have different EvolutionTypeID or figurineTo is already Ultra.
    /// </summary>
    public bool MergeTokens(CharacterFigurine figurineFrom, CharacterFigurine figurineTo)
    {
        if (figurineFrom.EvolutionType != figurineTo.EvolutionType)
        {
            Debug.LogWarning("[CharacterFigurineMerger] Cannot merge tokens with different EvolutionTypeID.");
            return false;
        }

        if (figurineTo.EvolutionType == EEvolutionTypeID.Ultra)
        {
            Debug.LogWarning("[CharacterFigurineMerger] Token is already at max evolution (Ultra).");
            return false;
        }

        EEvolutionTypeID next = NextEvolution(figurineTo.EvolutionType);
        figurineTo.SetEvolution(next, ColorForEvolution(next));

        // var characterFrom = _combatCharacters[figurineFrom];
        _combatCharacters.TryGetValue(figurineFrom, out var charFrom);
        _combatCharacters.TryGetValue(figurineTo, out var charTo);
        bool bothInCombat = charFrom != null && charTo != null;
        
        figurineFrom.CurrentSlot?.Vacate();
        GridManager.Instance.ClearTileWithItem(figurineFrom.gameObject);
        _combatCharacters.Remove(figurineFrom);
        figurineFrom.ResetToken();
        Destroy(figurineFrom.gameObject);

        if (bothInCombat)
            MergeCombatCharacters(charFrom, charTo, next);
        else if (next == EEvolutionTypeID.Super)
            SpawnInCombatZone(figurineTo, next);

        return true;
    }

    /// <summary>Both tokens already had a combat-zone Character: free charFrom's slot and merge its stats into charTo.</summary>
    private void MergeCombatCharacters(Character charFrom, Character charTo, EEvolutionTypeID evolution)
    {
        FindSlotForCharacter(charFrom)?.VacateCharacter();

        if (characterMerger != null)
            characterMerger.MergeCharacters(charFrom, charTo, evolution, ColorForEvolution(evolution));
    }

    private CharacterSlot FindSlotForCharacter(Character character)
    {
        foreach (var slot in combatSlots)
            if (slot != null && slot.OccupantCharacter == character) return slot;
        return null;
    }

    private void SpawnInCombatZone(CharacterFigurine figurine, EEvolutionTypeID evolution)
    {
        if (figurine.CharacterPrefab == null) return;

        CharacterSlot freeSlot = null;
        foreach (var slot in combatSlots)
        {
            if (slot != null && !slot.IsOccupied)
            {
                freeSlot = slot;
                break;
            }
        }

        if (freeSlot == null)
        {
            Debug.LogWarning("[CharacterFigurineMerger] No free combat slot available to spawn character.");
            return;
        }

        var go = characterSpawner != null
            ? characterSpawner.Spawn((ECharacterPoolID)(int)figurine.poolId, freeSlot.transform.position)
            : Instantiate(figurine.CharacterPrefab, freeSlot.transform.position, Quaternion.identity);
        if (go.TryGetComponent<Character>(out var character))
        {
            character.SetEvolution(evolution, ColorForEvolution(evolution));
            team.AddMember(character);
            freeSlot.OccupyCharacter(character);
            _combatCharacters[figurine] = character;
        }
    }
}
