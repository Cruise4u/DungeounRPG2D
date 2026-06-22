using UnityEngine;

public class CharacterTokenMerger : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private UnitSpawner unitSpawner;

    [Header("Combat Zone")]
    [SerializeField] private CharacterSlot[] combatSlots;

    [Header("Evolution Colors")]
    public Color basicColor    = Color.white;
    public Color superColor = Color.cyan;
    public Color megaColor     = Color.magenta;
    public Color ultraColor    = Color.yellow;

    private CharacterSlot[] _slots;

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
    /// Merges tokenFrom into tokenTo. tokenFrom is destroyed and its slot vacated.
    /// tokenTo advances one evolution tier and the merged character is spawned at its slot.
    /// Returns false if tokens have different EvolutionTypeID or tokenTo is already Ultra.
    /// </summary>
    public bool MergeTokens(CharacterToken tokenFrom, CharacterToken tokenTo)
    {
        if (tokenFrom.EvolutionType != tokenTo.EvolutionType)
        {
            Debug.LogWarning("[CharacterTokenMerger] Cannot merge tokens with different EvolutionTypeID.");
            return false;
        }

        if (tokenTo.EvolutionType == EEvolutionTypeID.Ultra)
        {
            Debug.LogWarning("[CharacterTokenMerger] Token is already at max evolution (Ultra).");
            return false;
        }

        EEvolutionTypeID next = NextEvolution(tokenTo.EvolutionType);
        tokenTo.SetEvolution(next, ColorForEvolution(next));

        tokenFrom.CurrentSlot?.Vacate();
        tokenFrom.ResetToken();
        Destroy(tokenFrom.gameObject);

        if (next == EEvolutionTypeID.Super)
            SpawnInCombatZone(tokenTo);

        return true;
    }

    private void SpawnInCombatZone(CharacterToken token)
    {
        if (token.CharacterPrefab == null) return;

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
            Debug.LogWarning("[CharacterTokenMerger] No free combat slot available to spawn character.");
            return;
        }

        var go = unitSpawner != null
            ? unitSpawner.SpawnCharacter(token.poolId, freeSlot.transform.position)
            : Instantiate(token.CharacterPrefab, freeSlot.transform.position, Quaternion.identity);
        if (go.TryGetComponent<Character>(out var character))
        {
            team.AddMember(character);
            freeSlot.OccupyCharacter(character);
        }
    }
}
