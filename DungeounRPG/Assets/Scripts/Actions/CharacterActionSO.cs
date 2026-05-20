using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterActionSO : ScriptableObject
{
    public string ActionName;
    public TargetType TargetType;

    // Executes this action from `user` onto the resolved target list.
    public abstract void Execute(Character user, List<ITarget> targets);
}
