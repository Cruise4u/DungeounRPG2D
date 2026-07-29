using UnityEngine;

/// <summary>
/// Closes the gap: walks toward the unit this action would act on until it is inside the action's
/// range, then hands over to Attack.
///
/// Split the same way targeting is — this state owns *when* to move, CharacterMovement owns *how*.
/// It picks its own target on Enter rather than being handed one, so a transition stays a plain
/// state change with nothing riding on it.
/// </summary>
public class CharacterMoveState : ICharacterState
{
    private readonly CharacterStateMachine _machine;

    private Character _mover;
    private CharacterMovement _movement;
    private CharacterUnitAttackConfig _config;
    private Character _target;
    private bool _cannotMove;

    public ECharacterStateID Id => ECharacterStateID.Moving;

    public CharacterMoveState(CharacterStateMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        _mover = _machine.GetComponent<Character>();
        _movement = _machine.GetComponent<CharacterMovement>();
        _config = _machine.GetComponent<CharacterUnitAttackConfig>();

        if (_movement == null || _config == null || !_config.IsUsable)
        {
            // A setup mistake rather than a runtime condition. Report once and sit still —
            // dropping back to Idle would just re-enter this state next frame and spam.
            Debug.LogError($"[CharacterMoveState] {_machine.gameObject.name} cannot move — it needs a CharacterMovement component and a usable CharacterUnitAttackConfig.", _machine);
            _cannotMove = true;
            return;
        }

        _target = TargetManager.ResolvePrimary(_mover, _config.AttackAction.TargetType, _config.TargetStrategy);
    }

    public void Tick(float deltaTime)
    {
        if (_cannotMove) return;

        // Nothing left to walk to. Idle owns re-acquisition.
        if (_target == null || !_target.IsAlive)
        {
            _machine.ChangeState(new CharacterIdleState(_machine));
            return;
        }

        if (_config.AttackAction.IsInRange(_mover, _target))
        {
            _machine.ChangeState(new CharacterAttackState(_machine));
            return;
        }

        _movement.MoveToward(_target.transform.position, deltaTime);
    }

    public void Exit() { }
}
