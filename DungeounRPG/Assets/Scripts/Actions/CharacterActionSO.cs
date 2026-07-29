using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterActionSO : ScriptableObject
{
    public string ActionName;
    public TargetType TargetType;

    [Tooltip("How close the user must be to execute this action, in world units.")]
    [Min(0f)] public float Range = 1.5f;

    // Executes this action from `user` onto the resolved target list.
    public abstract void Execute(Character user, List<ITarget> targets);

    /// <summary>
    /// Whether the user can reach the target from where it stands. Squared comparison — the square
    /// root would change the cost, not the answer.
    /// </summary>
    public bool IsInRange(Character user, Character target)
    {
        if (user == null || target == null) return false;

        float sqrDistance = (target.transform.position - user.transform.position).sqrMagnitude;
        return sqrDistance <= Range * Range;
    }
}
