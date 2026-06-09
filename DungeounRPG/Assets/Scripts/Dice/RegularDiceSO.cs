using UnityEngine;

[CreateAssetMenu(fileName = "RegularDice", menuName = "Dice/Regular Dice")]
public class RegularDiceSO : ScriptableObject
{
    [Min(1)]
    public int minRoll = 1;

    [Min(1)]
    public int maxRoll = 6;
}
