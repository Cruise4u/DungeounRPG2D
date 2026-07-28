using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global directory of every live Team, so a unit can answer "who are my teammates?" and
/// "who are my enemies?" without anyone handing it the answer.
///
/// Static rather than a scene singleton on purpose: there is nothing to add to the scene and
/// nothing to forget to wire. Teams register themselves in OnEnable and drop out in OnDisable,
/// and the list is cleared before every play session so editor statics never leak between runs.
///
/// Allegiance is derived from one fact only — which Team a Character belongs to (Character.Team,
/// assigned by Team.AddMember). Nothing here type-checks PlayerCharacter vs AICharacter, so
/// mirror matches and a future third faction work without changes.
/// </summary>
public static class TeamRegistry
{
    private static readonly List<Team> RegisteredTeams = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetForNewSession() => RegisteredTeams.Clear();

    /// <summary>Every registered team, minus any that have since been destroyed.</summary>
    public static IReadOnlyList<Team> AllTeams
    {
        get
        {
            Prune();
            return RegisteredTeams;
        }
    }

    public static void Register(Team team)
    {
        if (team == null || RegisteredTeams.Contains(team)) return;
        RegisteredTeams.Add(team);
    }

    public static void Unregister(Team team) => RegisteredTeams.Remove(team);

    /// <summary>The team this character belongs to, or null if it is not on one.</summary>
    public static Team GetTeamOf(Character character)
    {
        if (character == null) return null;
        if (character.Team != null) return character.Team;

        // Fallback for characters that were never routed through Team.AddMember.
        Prune();
        foreach (var team in RegisteredTeams)
            if (team.Contains(character)) return team;

        return null;
    }

    /// <summary>Living members of the given character's own team, excluding itself.</summary>
    public static List<Character> AliveTeammatesOf(Character character)
    {
        var result = new List<Character>();
        var team = GetTeamOf(character);
        if (team == null) return result;

        foreach (var member in team.Members)
            if (member != null && member != character && member.IsAlive)
                result.Add(member);

        return result;
    }

    /// <summary>Every registered team other than the given one.</summary>
    public static List<Team> EnemyTeamsOf(Team team)
    {
        Prune();
        var result = new List<Team>();
        if (team == null) return result;

        foreach (var other in RegisteredTeams)
            if (other != team) result.Add(other);

        return result;
    }

    /// <summary>Living members of every team other than the given one.</summary>
    public static List<Character> AliveEnemiesOf(Team team)
    {
        var result = new List<Character>();
        if (team == null) return result;

        foreach (var other in EnemyTeamsOf(team))
            foreach (var member in other.Members)
                if (member != null && member.IsAlive)
                    result.Add(member);

        return result;
    }

    /// <summary>True when both characters are on the same team. A character is not its own ally.</summary>
    public static bool AreAllies(Character a, Character b)
    {
        if (a == null || b == null || a == b) return false;

        var teamA = GetTeamOf(a);
        return teamA != null && teamA == GetTeamOf(b);
    }

    /// <summary>True when both characters are on teams, and those teams differ.</summary>
    public static bool AreEnemies(Character a, Character b)
    {
        if (a == null || b == null || a == b) return false;

        var teamA = GetTeamOf(a);
        var teamB = GetTeamOf(b);
        return teamA != null && teamB != null && teamA != teamB;
    }

    private static void Prune() => RegisteredTeams.RemoveAll(team => team == null);
}
