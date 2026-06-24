using UnityEngine;

/// <summary>
/// Merges two already-spawned combat-zone Characters into one. "from"'s stats are
/// absorbed into "to" (which fires CharacterStats.OnHpChanged so bound UI cards refresh),
/// then "from" is removed from the team and destroyed — freeing whichever combat slot
/// and roster/combat card it was occupying.
/// </summary>
public class CharacterMerger : MonoBehaviour
{
    [SerializeField] private Team team;

    public void MergeCharacters(Character from, Character to, EEvolutionTypeID evolution, Color color)
    {
        if (from == null || to == null) return;

        to.Stats.MergeFrom(from.Stats);
        to.SetEvolution(evolution, color);

        team.RemoveMember(from);
        Destroy(from.gameObject);
    }
}
