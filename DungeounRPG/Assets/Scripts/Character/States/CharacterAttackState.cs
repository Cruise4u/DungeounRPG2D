using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Real-time attack loop: executes this unit's CharacterUnitAttackConfig action against a single
/// target once per cooldown. Returns to Idle when the target is gone so the unit
/// re-acquires a new one there.
///
/// All attack knowledge lives here — which action, how often, what the timing rules are,
/// and what happens when the unit is not set up to attack. CharacterStateMachine holds none of it.
///
/// Who to hit is not attack knowledge, so it is not stored here: the state reads
/// CharacterStateMachine.CurrentTarget, whoever wrote it (Idle today, the Team Brain later).
/// </summary>
public class CharacterAttackState : ICharacterState
{
    private readonly CharacterStateMachine _machine;

    /// <summary>Who we are hitting. Owned by the machine, not by this state — see CurrentTarget.</summary>
    private Character Target => _machine.CurrentTarget;

    private Character _attacker;
    private CharacterUnitAttackConfig _config;
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

        if (_config == null || !_config.IsUsable)
        {
            // A setup mistake rather than a runtime condition. Report once and sit still —
            // dropping back to Idle would just re-enter this state next frame and spam.
            Debug.LogError($"[CharacterAttackState] {_machine.gameObject.name} has no usable CharacterUnitAttackConfig — it cannot attack.", _machine);
            _cannotAttack = true;
            return;
        }

        // Start "charged" so the first swing lands immediately; later swings respect the cooldown.
        _timer = _config.AttackCooldown;
    }

    public void Tick(float deltaTime)
    {
        if (_cannotAttack) return;

        var target = Target;
        if (target == null || !target.IsAlive)
        {
            // Drop the stale target so Idle re-acquires from scratch rather than inheriting a corpse.
            _machine.CurrentTarget = null;
            _machine.ChangeState(new CharacterIdleState(_machine));
            return;
        }

        _timer += deltaTime;
        if (_timer < _config.AttackCooldown) return;

        _timer = 0f;
        _config.AttackAction.Execute(_attacker, new List<ITarget> { target });
    }

    public void Exit() { }
}
