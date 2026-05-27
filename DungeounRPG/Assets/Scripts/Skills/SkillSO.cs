using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skills/Skill")]
public class SkillSO : CharacterActionSO
{
    [Tooltip("Scales the user's AttackPower before the dice multiplier is applied. E.g. 0.5 = 50% of raw power.")]
    [Min(0f)] public float DamageMultiplier = 0.5f;

    public override void Execute(Character user, List<ITarget> targets)
    {
        var rollResult = user.CurrentRollResult;

        if (rollResult == null)
        {
            Debug.LogWarning($"[SkillSO] {user.name} has no DiceRollResult. Was RollAll called this turn?");
            return;
        }

        int damage = Mathf.RoundToInt(user.Stats.AttackPower * DamageMultiplier * rollResult.NumericValue);

        foreach (var target in targets.Where(t => t != null && t.IsAlive))
            target.TakeDamage(damage);

        // Non-numeric effects (regen, summoning, buffs) are applied to the caster
        if (rollResult.Effects.Count > 0)
        {
            var effectHandler = user.GetComponent<StatusEffectHandler>();
            if (effectHandler != null)
                effectHandler.Apply(rollResult.Effects, user);
            else
                Debug.LogWarning($"[SkillSO] {user.name} has dice effects but no StatusEffectHandler component.");
        }

        Debug.Log($"[SkillSO] {user.name} used {ActionName} for {damage} damage (roll: {rollResult.NumericValue}).");
    }
}

public abstract class Skill
{
    public abstract void ExecuteSkill(Character user, ITarget[] targets);
}

public class HolyLightSkill : Skill
{
    public int damageValue;

    public int stunRounds;
    
    public StunSE Stun;

    public override void ExecuteSkill(Character user, ITarget[] targets)
    {
        foreach (var target in targets)
        {
            if (target.IsAlive)
            {
                target.TakeDamage(damageValue);
                target.GetSE(Stun);
                
            }
        }
    }
}

public abstract class StatusEffect
{
    public abstract void ApplyEffect();
}

public class StunSE : StatusEffect
{
    public int stunRounds;
    
    public override void ApplyEffect()
    {
        throw new System.NotImplementedException();
    }
}