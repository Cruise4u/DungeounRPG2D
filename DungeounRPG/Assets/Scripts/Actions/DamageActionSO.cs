using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Actions/Damage")]
public class DamageActionSO : CharacterActionSO
{
    [Min(0f)] public float DamageMultiplier = 1f;

    public override void Execute(Character user, List<ITarget> targets)
    {
        int damage = Mathf.RoundToInt(user.Stats.AttackPower * DamageMultiplier);
        foreach (var t in targets.Where(t => t != null && t.IsAlive))
            t.TakeDamage(damage);
        Debug.Log("Deal Damage! Pimba!");
    }
}
