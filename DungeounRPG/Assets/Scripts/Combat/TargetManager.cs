using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;

    private void Awake()
    {
        if (combatManager == null)
            combatManager = GetComponent<CombatManager>();
    }

    // Returns all living targets valid for the given type from the requestor's perspective.
    public List<ITarget> GetValidTargets(TargetType type, Character requestor)
    {
        bool isPlayer = requestor is PlayerCharacter;

        return type switch
        {
            TargetType.Self       => new List<ITarget> { requestor },
            TargetType.SingleAlly => isPlayer
                ? combatManager.AlivePlayerCharacters
                : combatManager.AliveEnemyCharacters,
            TargetType.SingleEnemy => isPlayer
                ? combatManager.AliveEnemyCharacters
                : combatManager.AlivePlayerCharacters,
            TargetType.AllAllies => isPlayer
                ? combatManager.AlivePlayerCharacters
                : combatManager.AliveEnemyCharacters,
            TargetType.AllEnemies => isPlayer
                ? combatManager.AliveEnemyCharacters
                : combatManager.AlivePlayerCharacters,
            TargetType.All => combatManager.AlivePlayerCharacters
                .Concat(combatManager.AliveEnemyCharacters).ToList(),
            _ => new List<ITarget>()
        };
    }

    public bool IsValidTarget(ITarget target, TargetType type, Character requestor)
        => GetValidTargets(type, requestor).Contains(target);
    
    // Picks a single random valid target — used by AI for simple random behaviour.
    public ITarget GetRandomTarget(TargetType type, Character requestor)
    {
        var targets = GetValidTargets(type, requestor);
        return targets.Count == 0 ? null : targets[Random.Range(0, targets.Count)];
    }

    // Picks a single target using a decision strategy — used by smarter AI.
    public ITarget GetBestTarget(TargetType type, AITargetStrategy strategy, Character requestor)
    {
        var targets = GetValidTargets(type, requestor);
        if (targets.Count == 0) return null;

        var chars = targets.OfType<Character>().ToList();
        if (chars.Count == 0) return targets[Random.Range(0, targets.Count)];

        return strategy switch
        {
            AITargetStrategy.LowestHp    => chars.OrderBy(c => c.Stats.CurrentHp).First(),
            AITargetStrategy.HighestHp   => chars.OrderByDescending(c => c.Stats.CurrentHp).First(),
            AITargetStrategy.LowestArmor => chars.OrderBy(c => c.Stats.Armor).First(),
            _                            => targets[Random.Range(0, targets.Count)]
        };
    }
}
