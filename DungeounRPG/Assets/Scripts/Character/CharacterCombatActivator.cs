using UnityEngine;

/// <summary>
/// Kicks off the real-time fight: puts every unit on every registered team into its Idle state.
/// Wire the Fight button's OnClick to StartBattle(). Purely additive — it does not touch the
/// old CombatManager turn loop.
///
/// No "opposing team" is handed out anymore: each unit derives its enemies from its own team
/// via TeamRegistry, so this is one uniform pass over all teams rather than a mirrored pair
/// of calls per side.
/// </summary>
public class CharacterCombatActivator : MonoBehaviour
{
    public void StartBattle()
    {
        foreach (var team in TeamRegistry.AllTeams)
            SetTeamFighting(team, true);
    }

    public void StopBattle()
    {
        foreach (var team in TeamRegistry.AllTeams)
            SetTeamFighting(team, false);
    }

    private static void SetTeamFighting(Team team, bool fighting)
    {
        foreach (var member in team.Members)
        {
            if (member == null) continue;

            if (!member.TryGetComponent<CharacterStateMachine>(out var machine))
            {
                Debug.LogWarning($"[CharacterCombatActivator] {member.name} has no CharacterStateMachine — it will not fight.", member);
                continue;
            }

            machine.isStateMachineActive = fighting;
            machine.ChangeState(fighting ? new CharacterIdleState(machine) : null);
        }
    }
}
