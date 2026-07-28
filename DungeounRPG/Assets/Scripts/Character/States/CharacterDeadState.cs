using UnityEngine;

/// <summary>
/// Terminal state — entered automatically when CharacterStats reports 0 HP.
/// Stops all behaviour and notifies listeners (teams, UI, future battle-end logic).
/// Team.AliveMembers stays consistent on its own since it also derives from HP.
/// </summary>
public class CharacterDeadState : ICharacterState
{
    private readonly CharacterStateMachine _machine;

    public ECharacterStateID Id => ECharacterStateID.Dead;

    public CharacterDeadState(CharacterStateMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        
    }

    public void Tick(float deltaTime) { }

    public void Exit()
    {
        // Only ever left via pooling reuse — stand the sprite back up.
        _machine.transform.rotation = Quaternion.identity;
    }
}
