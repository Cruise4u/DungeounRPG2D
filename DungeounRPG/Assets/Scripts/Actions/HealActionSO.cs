using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Actions/Heal")]
public class HealActionSO : CharacterActionSO
{
    [Min(0)] public int HealAmount;

    public override void Execute(Character user, List<ITarget> targets)
    {
        foreach (var t in targets.Where(t => t != null && t.IsAlive))
            t.Heal(HealAmount);
    }
}
