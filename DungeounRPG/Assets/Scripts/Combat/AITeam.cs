using System.Collections;
using System.Linq;
using UnityEngine;

public class AITeam : Team
{
    public override IEnumerator TakeTurn(CombatManager combat)
    {
        GameEventSingleton.OnAITurnStart.Raise(this);

        foreach (var character in members.OfType<AICharacter>().Where(c => c.IsAlive).ToList())
            yield return character.TakeTurn(combat);

        GameEventSingleton.OnAITurnEnd.Raise(this);
    }
}
