using System.Collections.Generic;

public class DiceRollResult
{
    public int NumericValue { get; set; }
    public List<IDiceEffect> Effects { get; } = new();
}