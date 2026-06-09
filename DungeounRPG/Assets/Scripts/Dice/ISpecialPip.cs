public interface ISpecialPip
{
    void MergePip(DiceRollResult result);
}


// ── Special Pips ──────────────────────────────────────────────────────────────

// Special pips operate on the shared result rather than returning a raw value

public class AdditionPip : ISpecialPip
{
    private readonly int bonus;
    public AdditionPip(int bonus) => this.bonus = bonus;
    public void MergePip(DiceRollResult result) => result.NumericValue += bonus;
}

public class MultiplierPip : ISpecialPip
{
    private readonly int multiplier;
    public MultiplierPip(int multiplier) => this.multiplier = multiplier;
    public void MergePip(DiceRollResult result) => result.NumericValue *= multiplier;
}

public class SummoningPip : ISpecialPip
{
    public void MergePip(DiceRollResult result) => result.Effects.Add(new DiceSummonEffect());
}

public class StatusEffectPip : ISpecialPip
{
    private readonly StatusEffectType type;
    private readonly float            magnitude;

    public StatusEffectPip(StatusEffectType type, float magnitude)
    {
        this.type      = type;
        this.magnitude = magnitude;
    }

    public void MergePip(DiceRollResult result) => result.Effects.Add(new DiceStatusEffect(type, magnitude));
}

public class RegenPip : ISpecialPip
{
    public void MergePip(DiceRollResult result) { }
}
