using UnityEngine;

/// <summary>
/// Default fight state: stand by and look for something to attack.
///
/// Target acquisition lives here rather than on the state machine, because wanting a target is
/// Idle's concern. The unit asks TeamRegistry who its enemies are — derived from its own team,
/// so no one has to hand it an "opposing team".
///
/// TEMP for milestone 1: picks the nearest living enemy and publishes it as machine.CurrentTarget.
/// Milestone 2's Team Brain will write that same field itself, and this local search goes away.
/// </summary>
public class CharacterIdleState : ICharacterState
{
    private readonly CharacterStateMachine _stateMachine;

    public ECharacterStateID Id => ECharacterStateID.Idle;

    public CharacterIdleState(CharacterStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
    }

    public void Enter()
    {
        
    }

    public void Tick(float deltaTime)
    {
        if (!_stateMachine.isStateMachineActive) return;
        
        Debug.Log("Character ticking...");
        var target = FindNearestEnemy();
        if (target == null) return;

        // Publish it as machine context, then transition. Attack reads it from there — the
        // transition itself stays a plain state change with nothing riding on it.
        _stateMachine.CurrentTarget = target;
        _stateMachine.ChangeState(new CharacterAttackState(_stateMachine));
    }

    public void Exit()
    {
        
    }

    /// <summary>Nearest living member of any team that is not this unit's own.</summary>
    private Character FindNearestEnemy()
    {
        var enemies = TeamRegistry.AliveEnemiesOf(_stateMachine.myTeam);

        Character nearest = null;
        float nearestSqrDistance = float.MaxValue;
        var origin = _stateMachine.transform.position;

        foreach (var enemy in enemies)
        {
            float sqrDistance = (enemy.transform.position - origin).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
                Debug.Log(nearest);
            }
        }

        return nearest;
    }
}
