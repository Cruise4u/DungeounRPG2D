using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    private Character character;
    private CharacterStats stats;
    
    public bool isStateMachineActive;
    
    public ICharacterState CurrentState { get; private set; }

    public ECharacterStateID CurrentStateId => CurrentState?.Id ?? ECharacterStateID.Inactive;

    /// <summary>Raised after every transition: (previousStateId, newStateId).</summary>
    public event Action<ECharacterStateID, ECharacterStateID> OnStateChanged;
    public List<ICharacterState> States { get; private set; }
    
    public Dictionary<ECharacterStateID,ICharacterState> StatesDictionary { get; private set; }
    
    private void Awake()
    {
        character = GetComponent<Character>();
        stats = GetComponent<CharacterStats>();

        StatesDictionary = new Dictionary<ECharacterStateID,ICharacterState>
        {
            { ECharacterStateID.Idle, new CharacterIdleState(this) },
            { ECharacterStateID.Attacking, new CharacterIdleState(this) },
            { ECharacterStateID.Dead, new CharacterIdleState(this) }
        };

        if (stats != null)
            stats.OnDied += HandleDied; 
        
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
        ChangeState(StatesDictionary[ECharacterStateID.Idle]);
    }

    /// <summary>Takes the unit out of the fight and back to Inactive (prep / post-match).</summary>
    public void Deactivate()
    {
        isStateMachineActive = false;
        ChangeState(null);
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnDied -= HandleDied;
    }

    /// <summary>
    /// Death outranks whatever the unit was doing — no state gets to veto it, so this transitions
    /// straight to Dead rather than asking the current state to agree.
    /// </summary>
    private void HandleDied(CharacterStats stats) => ChangeState(new CharacterDeadState(this));

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
