using UnityEngine;

public class SpecialDice : Dice
{
    [SerializeField] private SpecialDiceSO data;

    private ISpecialPip pip;

    private void Awake()
    {
        DiceID = DiceID.Special;
        pip    = data != null ? data.CreatePip() : null;
    }

    public override void Roll(DiceRollResult result)
    {
        int min = data != null ? data.minRoll : 1;
        int max = data != null ? data.maxRoll : 6;
        result.NumericValue = Random.Range(min, max + 1);
        pip?.MergePip(result);
    }
}