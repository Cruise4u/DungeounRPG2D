using UnityEngine;

/// <summary>
/// Default fight state: stand by until there is someone to act on.
///
/// Idle owns only the question. TargetManager owns the answer, and derives it from TeamRegistry,
/// so no search logic lives here and nobody has to hand this unit an "opposing team".
///
/// The question is asked in terms of the unit's own action — a healer waits for an ally the same
/// way a fighter waits for an enemy — and the answer's distance decides whether the unit walks
/// first or swings straight away.
/// </summary>
public class CharacterIdleState : ICharacterState
{
    private readonly CharacterStateMachine _stateMachine;

    private Character _character;
    private CharacterUnitAttackConfig _config;

    public ECharacterStateID Id => ECharacterStateID.Idle;

    public CharacterIdleState(CharacterStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
    }

    public void Enter()
    {
        _character = _stateMachine.GetComponent<Character>();
        _config = _stateMachine.GetComponent<CharacterUnitAttackConfig>();
    }

    public void Tick(float deltaTime)
    {
        if (!_stateMachine.isStateMachineActive) return;

        // No usable config is a setup mistake. Hand to Attack, which owns reporting it exactly once
        // — deciding that here as well would mean two states with an opinion about the same fault.
        if (_config == null || !_config.IsUsable)
        {
            _stateMachine.ChangeState(new CharacterAttackState(_stateMachine));
            return;
        }

        var action = _config.AttackAction;
        var target = TargetManager.ResolvePrimary(_character, action.TargetType, _config.TargetStrategy);
        if (target == null) return;

        ICharacterState next = action.IsInRange(_character, target)
            ? new CharacterAttackState(_stateMachine)
            : new CharacterMoveState(_stateMachine);

        _stateMachine.ChangeState(next);
    }

    public void Exit()
    {

    }
}
