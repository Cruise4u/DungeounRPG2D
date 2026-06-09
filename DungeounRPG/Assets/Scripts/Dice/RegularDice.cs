using UnityEngine;

public class RegularDice : Dice
{
    [SerializeField] private RegularDiceSO data;

    private void Awake() => DiceID = DiceID.Regular;

    public override void Roll(DiceRollResult result)
    {
        int min = data != null ? data.minRoll : 1;
        int max = data != null ? data.maxRoll : 6;
        result.NumericValue = Random.Range(min, max + 1);
    }
}