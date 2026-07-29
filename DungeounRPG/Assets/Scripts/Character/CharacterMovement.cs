using UnityEngine;

/// <summary>
/// Per-unit movement: how a unit crosses the combat zone. Owned by the movement behaviour, not by
/// CharacterStateMachine — the machine has no idea this exists. Read by CharacterMoveState.
///
/// Speed comes from the unit's authored Speed stat, so there is one place to tune how fast a
/// character is. Movement is a straight line with no collision: units may overlap, which is the
/// agreed behaviour for now rather than an oversight.
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class CharacterMovement : MonoBehaviour
{
    private CharacterStats _stats;

    /// <summary>World units per second.</summary>
    public float Speed => _stats != null ? _stats.Speed : 0f;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
    }

    /// <summary>
    /// Steps toward the destination by this frame's worth of travel. Stops exactly on arrival, so
    /// calling it after arriving is harmless.
    /// </summary>
    public void MoveToward(Vector3 destination, float deltaTime)
    {
        // 2D: a target's depth must never drag this sprite's z and change its sorting.
        destination.z = transform.position.z;

        transform.position = Vector3.MoveTowards(transform.position, destination, Speed * deltaTime);
    }
}
