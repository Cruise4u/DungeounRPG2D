using System.Collections;
using UnityEngine;

public enum CombatState { Idle, PlayerPhase, AIPhase, Victory, Defeat }

public class CombatManager : MonoBehaviour
{
    public TargetManager TargetManager;

    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private AITeam enemyTeam;

    public CombatState State { get; private set; } = CombatState.Idle;
    public int Round { get; private set; }

    public System.Collections.Generic.List<ITarget> AlivePlayerCharacters => playerTeam.AliveMembers;
    public System.Collections.Generic.List<ITarget> AliveEnemyCharacters  => enemyTeam.AliveMembers;

    public void StartCombat()
    {
        Round = 0;
        State = CombatState.Idle;
        // StartCoroutine(TestRoutine());
        StartCoroutine(CombatLoop());
    }

    private IEnumerator TestRoutine()
    {
        yield return new  WaitForEndOfFrame();
        Debug.Log("Message");
    }
    
    private IEnumerator CombatLoop()
    {
        while (true)
        {
            Debug.Log("Combat Loop Entered");
            Round++;
            GameEventSingleton.OnRoundStart.Raise(Round);
            Debug.Log("Event Round Start Triggered");

            State = CombatState.PlayerPhase;
            yield return StartCoroutine(playerTeam.TakeTurn(this));
            if (CheckVictory()) yield break;

            State = CombatState.AIPhase;
            yield return StartCoroutine(enemyTeam.TakeTurn(this));
            if (CheckDefeat()) yield break;
        }
    }

    private bool CheckVictory()
    {
        if (AliveEnemyCharacters.Count == 0 && AlivePlayerCharacters.Count > 0)
        {
            State = CombatState.Victory;
            GameEventSingleton.OnPlayerVictory.Raise();
            return true;
        }
        return false;
    }

    private bool CheckDefeat()
    {
        if (AlivePlayerCharacters.Count == 0)
        {
            State = CombatState.Defeat;
            GameEventSingleton.OnPlayerDefeat.Raise();
            return true;
        }
        return false;
    }
}
