using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The four phases of a match.
/// MergingPhase — player stages/merges on the prep grid; no unit acts.
/// CombatPhase  — real-time auto-battle; units drive themselves until one side is wiped.
/// Victory/Defeat — terminal.
/// </summary>
public enum CombatState { MergingPhase, CombatPhase, Victory, Defeat }

/// <summary>
/// Owns the match phase and arbitrates the transitions between them.
///
/// This is phase-driven, not turn-driven: there is no loop that hands out turns, because the
/// fight is automatic. CombatManager's job is to decide *when* units are allowed to act and to
/// notice when the fight is over — the units decide what they do (CharacterStateMachine).
///
/// Entering CombatPhase is the single place that activates every unit's state machine, and
/// leaving it is the single place that deactivates them, so "are units fighting right now?" has
/// exactly one owner.
/// </summary>
public class CombatManager : MonoBehaviour
{
    public TargetManager targetManager;

    public Team playerTeam;
    public Team cpuTeam;

    public CombatState State { get; private set; } = CombatState.MergingPhase;
    public int Round { get; private set; }

    public List<ITarget> AlivePlayerCharacters => playerTeam.AliveMembers;
    public List<ITarget> AliveEnemyCharacters  => cpuTeam.AliveMembers;

    /// <summary>Raised after every phase transition: (previousPhase, newPhase).</summary>
    public event Action<CombatState, CombatState> OnPhaseChanged;

    public bool IsOver => State is CombatState.Victory or CombatState.Defeat;

    /// <summary>Puts the match at its start: round 0, prep grid open, nobody fighting.</summary>
    public void StartCombat()
    {
        Round = 0;
        EnterPhase(CombatState.MergingPhase);
    }

    /// <summary>Hook this to the Fight button. Ignored outside MergingPhase.</summary>
    public void BeginCombatPhase()
    {
        if (State != CombatState.MergingPhase)
        {
            Debug.LogWarning($"[CombatManager] Fight pressed during {State} — ignored.");
            return;
        }

        EnterPhase(CombatState.CombatPhase);
    }

    // The win/lose check is polled rather than event-driven because units die from many places
    // (damage, status effects, future hazards); polling the two alive-lists means no death path
    // can forget to report itself.
    private void Update()
    {
        if (State != CombatState.CombatPhase) return;

        // Checked before victory so a mutual wipe resolves as a defeat — losing your whole team
        // ends the match regardless of what happened to the enemy.
        if (AlivePlayerCharacters.Count == 0)
            EnterPhase(CombatState.Defeat);
        else if (AliveEnemyCharacters.Count == 0)
            EnterPhase(CombatState.Victory);
    }

    public void EnterPhase(string state)
    {
        switch (state)
        {
            case "CombatPhase":
                Round++;
                GameEventSingleton.OnRoundStart.Raise(Round);
                SetUnitsActive(true);
                break;
        }
    }
    
    private void EnterPhase(CombatState next)
    {
        var previous = State;
        State = next;

        switch (next)
        {
            case CombatState.MergingPhase:
                SetUnitsActive(false);
                break;

            case CombatState.CombatPhase:
                Round++;
                GameEventSingleton.OnRoundStart.Raise(Round);
                SetUnitsActive(true);
                break;

            case CombatState.Victory:
                SetUnitsActive(false);
                GameEventSingleton.OnPlayerVictory.Raise();
                break;

            case CombatState.Defeat:
                SetUnitsActive(false);
                GameEventSingleton.OnPlayerDefeat.Raise();
                break;
        }

        OnPhaseChanged?.Invoke(previous, State);
    }

    
    
    private void SetUnitsActive(bool active)
    {
        SetTeamActive(playerTeam, active);
        SetTeamActive(cpuTeam, active);
    }

    private static void SetTeamActive(Team team, bool active)
    {
        if (team == null) return;

        foreach (var member in team.Members)
        {
            if (member == null) continue;
            if (!member.TryGetComponent<CharacterStateMachine>(out var machine)) continue;

            if (active) machine.Activate();
            else machine.Deactivate();
        }
    }
}
