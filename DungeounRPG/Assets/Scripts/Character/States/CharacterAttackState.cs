using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Real-time attack loop: executes this unit's CharacterUnitAttackConfig action once per cooldown,
/// then returns to Idle when the engagement is over so the unit re-acquires there.
///
/// All attack knowledge lives here — which action, how often, what the timing rules are, and what
/// happens when the unit is not set up to attack. CharacterStateMachine holds none of it.
///
/// The unit commits to one target on Enter and keeps it for the whole engagement, so a
/// single-target swing cannot flicker between victims mid-cooldown. Multi-target actions re-resolve
/// on every swing instead, because who is worth hitting genuinely changes between them.
/// </summary>
public class CharacterAttackState : ICharacterState
{
    private readonly CharacterStateMachine _machine;

    private Character _attacker;
    private CharacterUnitAttackConfig _config;
    private Character _primary;
    private float _timer;
    private bool _cannotAttack;

    public ECharacterStateID Id => ECharacterStateID.Attacking;

    public CharacterAttackState(CharacterStateMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        _attacker = _machine.GetComponent<Character>();
        _config = _machine.GetComponent<CharacterUnitAttackConfig>();
        
        Debug.Log("Enter On Attack State");
        Debug.Log("I'm the owner of attack state game object name : " + _attacker.gameObject.name);


        if (_config == null || !_config.IsUsable)
        {
            // A setup mistake rather than a runtime condition. Report once and sit still —
            // dropping back to Idle would just re-enter this state next frame and spam.
            Debug.LogError($"[CharacterAttackState] {_machine.gameObject.name} has no usable CharacterUnitAttackConfig — it cannot attack.", _machine);
            _cannotAttack = true;
            return;
        }

        _primary = CommitToTarget();

        // Start "charged" so the first swing lands immediately; later swings respect the cooldown.
        _timer = _config.AttackCooldown;
    }

    public void Tick(float deltaTime)
    {
        if (_cannotAttack) return;

        // The unit we committed to is gone, so this engagement is over. Idle owns re-acquisition.
        if (_primary == null || !_primary.IsAlive)
        {
            _machine.ChangeState(new CharacterIdleState(_machine));
            return;
        }

        // Drifted out of reach. Move owns closing the gap; the cooldown resets on the way back in,
        // so a unit cannot bank charge by chasing.
        if (!_config.AttackAction.IsInRange(_attacker, _primary))
        {
            _machine.ChangeState(new CharacterMoveState(_machine));
            return;
        }

        _timer += deltaTime;
        if (_timer < _config.AttackCooldown) return;

        var targets = ResolveTargets();
        if (targets.Count == 0) return;

        _timer = 0f;
        _config.AttackAction.Execute(_attacker, targets);
    }

    public void Exit() { }

    /// <summary>
    /// The unit this engagement is anchored to. For a multi-target action it is not the only one
    /// that gets hit — it is the one whose death ends the engagement.
    /// </summary>
    private Character CommitToTarget()
        => TargetManager.ResolvePrimary(_attacker, _config.AttackAction.TargetType, _config.TargetStrategy);

    /// <summary>
    /// Single-target actions swing at the committed unit. Multi-target actions ask again, because
    /// the living set changes between swings and hitting a stale list would revive the dead.
    /// </summary>
    private List<ITarget> ResolveTargets()
    {
        var targetType = _config.AttackAction.TargetType;

        bool isSingle = targetType == TargetType.Self
                     || targetType == TargetType.SingleAlly
                     || targetType == TargetType.SingleEnemy;

        return isSingle
            ? new List<ITarget> { _primary }
            : TargetManager.Resolve(_attacker, targetType, _config.TargetStrategy);
    }
}
