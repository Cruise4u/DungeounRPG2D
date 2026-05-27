using UnityEngine;

public enum DiceID { Regular, Special }

public enum DicePipID { One, Two, Three, Four, Five, Six, Addition, Multiplication, Summoning, StatusEffect }

public enum StatusEffectType { Regen, Protection, DamageBuff }

// Shared result built up as regular + special dice are applied
public abstract class Dice : MonoBehaviour
{
    public DiceID DiceID { get; protected set; }

    public abstract void Roll(DiceRollResult result);
}