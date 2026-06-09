public class DiceStatusEffect : IDiceEffect
{
    public StatusEffectType Type      { get; }
    public float            Magnitude { get; }

    public DiceStatusEffect(StatusEffectType type, float magnitude)
    {
        Type      = type;
        Magnitude = magnitude;
    }
}