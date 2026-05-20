using System.Collections;
using UnityEngine;

public class PlayerCharacter : Character
{
    public override IEnumerator TakeTurn(CombatManager combat)
    {
        _turnEnded = false;
        GameEventSingleton.OnPlayerCharacterTurnStart.Raise(this);

        for (int i = 0; i < ActionsPerTurn; i++)
        {
            ResetActionState();

            // Wait until the player confirms an action OR manually ends their turn.
            yield return new WaitUntil(() => ActionConfirmed || _turnEnded);

            if (_turnEnded) break;

            ExecutePendingAction();
        }

        GameEventSingleton.OnPlayerCharacterTurnEnd.Raise(this);
    }

    /// <summary>
    /// Skips any remaining actions for this character this round.
    /// Called by PlayerTeam.EndTurn → PlayerController.EndTurn → Turn_Button.
    /// </summary>
    public void EndTurn() => _turnEnded = true;
}
