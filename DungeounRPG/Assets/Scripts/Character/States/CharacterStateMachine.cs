using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Real-time behavioural state machine for a combat-zone unit (State pattern).
///
/// Holds only what a state machine needs: which state is current, how to transition, ticking
/// the current state, and the context every state reads — this unit's own team. All behaviour
/// lives in the states (CharacterIdleState / CharacterAttackState / CharacterDeadState);
/// all allegiance queries live in TeamRegistry.
///
/// A unit does nothing until something puts it into a state — CurrentState starts null and
/// Update ticks nothing.
/// </summary>
[RequireComponent(typeof(Character))]
public class CharacterStateMachine : MonoBehaviour
{
    [Tooltip("The team this unit fights for. Leave empty to resolve it from Character.Team.")]
    [SerializeField] public Team myTeam;

    public bool isStateMachineActive;

    /// <summary>
    /// The unit this one is currently fighting — shared context, exactly like <see cref="myTeam"/>.
    /// Idle writes it once it acquires someone, Attack reads it. Milestone 2's Team Brain writes it
    /// directly, so pushing a target never means constructing a state differently.
    /// </summary>
    public Character CurrentTarget { get; set; }

    public ICharacterState CurrentState { get; private set; }

    public ECharacterStateID CurrentStateId => CurrentState?.Id ?? ECharacterStateID.Inactive;

    /// <summary>Raised after every transition: (previousStateId, newStateId).</summary>
    public event Action<ECharacterStateID, ECharacterStateID> OnStateChanged;

    public List<ICharacterState> States { get; private set; }

    private CharacterIdleState _idleState;

    private void Awake()
    {
        _idleState = new CharacterIdleState(this);
        States = new List<ICharacterState> { _idleState, new CharacterAttackState(this) };

        // The Inspector field wins; otherwise take the team the Character was registered to.
        if (myTeam == null && TryGetComponent<Character>(out var character))
            myTeam = character.Team;
    }

    /// <summary>
    /// Puts the unit into the fight. Called by CombatManager when CombatPhase begins — units
    /// never activate themselves, so "is this unit fighting?" has a single owner.
    /// </summary>
    public void Activate()
    {
        Debug.Log("Activating Character StateMachine");
        if (isStateMachineActive) return;

        isStateMachineActive = true;
        ChangeState(_idleState);
    }

    /// <summary>Takes the unit out of the fight and back to Inactive (prep / post-match).</summary>
    public void Deactivate()
    {
        isStateMachineActive = false;
        CurrentTarget = null;
        ChangeState(null);
    }

    private void Update()
    {
        CurrentState?.Tick(Time.deltaTime);
    }

    public void ChangeState(ICharacterState next)
    {
        Debug.Log("Changing State");
        var previous = CurrentStateId;
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState?.Enter();
        OnStateChanged?.Invoke(previous, CurrentStateId);
    }
}
