using System.Collections;
using UnityEngine;

/// <summary>
/// An enemy-side character. Deliberately has no controller: the AI never mimics input, it only
/// behaves, so its decisions live in the character's own state machine and (later) the team brain.
/// The fields below are that behaviour's configuration, previously carried by AIController.
/// </summary>
public class AICharacter : Character
{
    [SerializeField] private float actionDelay = 0.5f;

    [Header("Behaviour")]
    [SerializeField] private AITargetStrategy strategy = AITargetStrategy.Random;
    [SerializeField] private CharacterActionSO defaultAction;

    /// <summary>How this unit picks one victim out of the eligible set. Fed to TargetManager.Resolve.</summary>
    public AITargetStrategy Strategy => strategy;

    /// <summary>What this unit does when it has nothing better to do — its basic attack.</summary>
    public CharacterActionSO DefaultAction => defaultAction;

    /// <summary>Seconds this unit waits between actions.</summary>
    public float ActionDelay => actionDelay;

    protected override void Awake()
    {
        base.Awake();
    }
}
