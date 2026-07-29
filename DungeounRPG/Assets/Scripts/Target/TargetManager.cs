using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Answers one question: who is eligible for this action, from this character's point of view.
///
/// Sides come from TeamRegistry and never from character types, so mirror matches and a future
/// third faction work without a change here. Eligibility only — geometry is deliberately absent.
/// An area attack resolves the units it may affect; collision at impact decides who was actually
/// caught. That keeps spell shapes entirely outside this system.
///
/// Stateless on purpose. Units die constantly in a real-time fight, so every call resolves fresh
/// against the live registry rather than caching a list that goes stale mid-swing.
///
/// Still a MonoBehaviour only because CombatManager holds a serialized reference to the type.
/// The API is static so nothing has to be wired up; once that reference is gone this can become
/// a plain static class.
/// </summary>
public class TargetManager : MonoBehaviour
{
    /// <summary>
    /// Everyone this action may legally affect. Single* resolves to at most one entry chosen by
    /// <paramref name="strategy"/>; All* returns the whole living set. Never returns null.
    /// </summary>
    public static List<ITarget> Resolve(Character requestor, TargetType type,
                                        AITargetStrategy strategy = AITargetStrategy.Nearest)
    {
        var result = new List<ITarget>();
        if (requestor == null) return result;

        switch (type)
        {
            case TargetType.Self:
                result.Add(requestor);
                break;

            case TargetType.SingleAlly:
                AddIfPresent(result, Pick(Allies(requestor), strategy, requestor));
                break;

            case TargetType.SingleEnemy:
                AddIfPresent(result, Pick(Enemies(requestor), strategy, requestor));
                break;

            case TargetType.AllAllies:
                result.AddRange(Allies(requestor));
                break;

            case TargetType.AllEnemies:
                result.AddRange(Enemies(requestor));
                break;
        }

        return result;
    }

    /// <summary>
    /// Whether this action has anyone to act on right now. Idle's entire decision — it asks the
    /// question, this owns the answer.
    /// </summary>
    public static bool HasEligibleTarget(Character requestor, TargetType type)
        => Resolve(requestor, type).Count > 0;

    /// <summary>
    /// The single unit an action anchors on, or null when nobody is eligible. For All* types this
    /// is the first of the set — enough to measure range and to decide when an engagement is over.
    /// </summary>
    public static Character ResolvePrimary(Character requestor, TargetType type, AITargetStrategy strategy)
    {
        var eligible = Resolve(requestor, type, strategy);
        return eligible.Count > 0 ? eligible[0] as Character : null;
    }

    // Teammates exclude the requestor itself — TeamRegistry's rule, kept so "ally" means the same
    // thing everywhere. A unit that should also affect itself wants Self alongside AllAllies.
    private static List<Character> Allies(Character requestor)
        => TeamRegistry.AliveTeammatesOf(requestor);

    private static List<Character> Enemies(Character requestor)
        => TeamRegistry.AliveEnemiesOf(TeamRegistry.GetTeamOf(requestor));

    // Chooses one from the eligible set. Nearest needs the requestor's position; the stat-based
    // strategies do not care where anyone is standing.
    private static Character Pick(List<Character> candidates, AITargetStrategy strategy, Character requestor)
    {
        if (candidates.Count == 0) return null;

        return strategy switch
        {
            AITargetStrategy.Nearest     => Nearest(candidates, requestor.transform.position),
            AITargetStrategy.LowestHp    => candidates.OrderBy(c => c.Stats.CurrentHp).First(),
            AITargetStrategy.HighestHp   => candidates.OrderByDescending(c => c.Stats.CurrentHp).First(),
            AITargetStrategy.LowestArmor => candidates.OrderBy(c => c.Stats.Armor).First(),
            _                            => candidates[Random.Range(0, candidates.Count)]
        };
    }

    // Squared distance: the square root would change the cost, not the ordering.
    private static Character Nearest(List<Character> candidates, Vector3 origin)
    {
        Character nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (var candidate in candidates)
        {
            float sqrDistance = (candidate.transform.position - origin).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = candidate;
        }

        return nearest;
    }

    private static void AddIfPresent(List<ITarget> targets, Character candidate)
    {
        if (candidate != null) targets.Add(candidate);
    }
}
