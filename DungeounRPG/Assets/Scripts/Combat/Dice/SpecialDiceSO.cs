using UnityEngine;

[CreateAssetMenu(fileName = "SpecialDice", menuName = "Dice/Special Dice")]
public class SpecialDiceSO : ScriptableObject
{
    [Min(1)]
    public int minRoll = 1;

    [Min(1)]
    public int maxRoll = 6;

    public DicePipID pipType;

    [Tooltip("Flat value added to the roll. Used by the Addition pip.")]
    public int bonus = 0;

    [Tooltip("Multiplier applied to the roll. Used by the Multiplication pip.")]
    public int multiplier = 1;

    [Tooltip("Status effect type applied to the target. Used by the StatusEffect pip.")]
    public StatusEffectType statusEffectType;

    [Tooltip("Strength of the status effect. Used by the StatusEffect pip.")]
    public float statusEffectMagnitude = 1f;

    /// <summary>
    /// Builds and returns the <see cref="ISpecialPip"/> described by this SO.
    /// </summary>
    public ISpecialPip CreatePip()
    {
        return pipType switch
        {
            DicePipID.Addition       => new AdditionPip(bonus),
            DicePipID.Multiplication => new MultiplierPip(multiplier),
            DicePipID.Summoning      => new SummoningPip(),
            DicePipID.StatusEffect   => new StatusEffectPip(statusEffectType, statusEffectMagnitude),
            _                        => null
        };
    }
}
