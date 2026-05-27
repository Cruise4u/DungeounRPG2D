using System.Collections.Generic;
using UnityEngine;

// Processes IDiceEffect instances produced by special dice pips and applies them to a character.
public class StatusEffectHandler : MonoBehaviour
{
    private readonly List<ActiveStatusEffect> _activeEffects = new();

    public IReadOnlyList<ActiveStatusEffect> ActiveEffects => _activeEffects;

    public void Apply(List<IDiceEffect> effects, Character target)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case DiceStatusEffect status:
                    ApplyStatus(status, target);
                    break;

                case DiceSummonEffect:
                    // Summoning is handled separately — flagged here for other systems to react
                    Debug.Log($"[StatusEffectHandler] Summon triggered on {target.name}.");
                    break;
            }
        }
    }

    // Called by CombatManager at the start of each round to tick duration-based effects.
    public void TickEffects()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].RemainingRounds--;
            if (_activeEffects[i].RemainingRounds <= 0)
                _activeEffects.RemoveAt(i);
        }
    }

    // Removes all active effects (e.g. on combat end).
    public void ClearEffects() => _activeEffects.Clear();

    private void ApplyStatus(DiceStatusEffect diceStatus, Character target)
    {
        switch (diceStatus.Type)
        {
            case StatusEffectType.Regen:
                int healAmount = Mathf.RoundToInt(target.Stats.MaxHp * diceStatus.Magnitude);
                if (target.Stats.CurrentHp < target.Stats.MaxHp)
                    target.Heal(healAmount);
                Debug.Log($"[StatusEffectHandler] Regen healed {target.name} for {healAmount} HP ({diceStatus.Magnitude * 100f:0}%).");
                break;

            case StatusEffectType.DamageBuff:
                int attackBonus = Mathf.RoundToInt(target.Stats.AttackPower * diceStatus.Magnitude);
                target.Stats.ModifyAttackPower(attackBonus);
                _activeEffects.Add(new ActiveStatusEffect(diceStatus.Type, diceStatus.Magnitude, attackBonus, rounds: 1));
                Debug.Log($"[StatusEffectHandler] DamageBuff gave {target.name} +{attackBonus} AttackPower for 1 round.");
                break;

            case StatusEffectType.Protection:
                int armorBonus = Mathf.RoundToInt(target.Stats.Armor * diceStatus.Magnitude);
                target.Stats.ModifyArmor(armorBonus);
                _activeEffects.Add(new ActiveStatusEffect(diceStatus.Type, diceStatus.Magnitude, armorBonus, rounds: 1));
                Debug.Log($"[StatusEffectHandler] Protection gave {target.name} +{armorBonus} Armor for 1 round.");
                break;
        }
    }
}

public class ActiveStatusEffect
{
    public StatusEffectType Type { get; }
    public float Magnitude { get; }
    public int StatDelta { get; }  // The actual stat value that was added — used to revert on expiry
    public int RemainingRounds { get; set; }

    public ActiveStatusEffect(StatusEffectType type, float magnitude, int statDelta, int rounds)
    {
        Type = type;
        Magnitude = magnitude;
        StatDelta = statDelta;
        RemainingRounds = rounds;
    }
}
