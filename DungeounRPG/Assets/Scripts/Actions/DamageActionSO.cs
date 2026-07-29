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
        {
            // Armor is applied inside TakeDamage, so the only honest way to report what actually
            // landed is to measure the health that disappeared — the raw roll is not the same number.
            var stats = (t as Character)?.Stats;
            int hpBefore = stats != null ? stats.CurrentHp : 0;

            t.TakeDamage(damage);

            if (stats == null)
            {
                Debug.Log($"{user.TargetName} hit {t.TargetName} for {damage} raw.");
                continue;
            }

            int applied = hpBefore - stats.CurrentHp;
            string killed = t.IsAlive ? "" : " — down!";
            Debug.Log($"{user.TargetName} hit {t.TargetName} for {applied} (raw {damage}) — {stats.CurrentHp}/{stats.MaxHp} HP left{killed}");
        }
    }
}
