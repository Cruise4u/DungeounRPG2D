/// <summary>
/// A single behavioural state of a unit during the real-time fight (State pattern).
/// States are plain C# classes owned and ticked by a CharacterStateMachine.
/// </summary>
public interface ICharacterState
{
    ECharacterStateID Id { get; }

    void Enter();
    void Tick(float deltaTime);
    void Exit();
}
