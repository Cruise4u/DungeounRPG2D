using UnityEngine;

/// <summary>
/// Per-unit attack data: which action this unit executes and how often. Owned by the
/// attack behaviour, not by CharacterStateMachine — the machine has no idea this exists.
/// Read by CharacterAttackState. A unit without this component simply cannot attack.
/// </summary>
public class CharacterUnitAttackConfig : MonoBehaviour
{
    [Tooltip("Action executed on each attack cycle while in the Attacking state.")]
    [SerializeField] private CharacterActionSO attackAction;

    [Tooltip("Seconds between attack executions while in the Attacking state.")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    [Tooltip("How this unit picks among eligible targets when its action targets a single unit.")]
    [SerializeField] private AITargetStrategy targetStrategy = AITargetStrategy.Nearest;

    public CharacterActionSO AttackAction => attackAction;
    public float AttackCooldown => attackCooldown;
    public AITargetStrategy TargetStrategy => targetStrategy;
    public bool IsUsable => attackAction != null;
}
