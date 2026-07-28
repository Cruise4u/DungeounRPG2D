/// <summary>
/// Identifies a CharacterStateMachine state without needing type checks.
/// Inactive = the machine has not been activated yet (unit is in prep / pre-fight).
/// Note: "Alive" is implicit — any state other than Dead means the unit is alive.
/// </summary>
public enum ECharacterStateID
{
    Inactive,
    Idle,
    Attacking,
    Dead,
}
