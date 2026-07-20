public static class GameEventSingleton
{
    // ── Legacy ────────────────────────────────────────────────────────────
    public static readonly GameEvent OnPlayerDied      = new("OnPlayerDied");
    public static readonly GameEvent OnPlayerRespawned = new("OnPlayerRespawned");
    public static readonly GameEvent OnEnemyDied       = new("OnEnemyDied");
    public static readonly GameEvent OnLevelComplete   = new("OnLevelComplete");
    public static readonly TypedGameEvent<int> OnHealthChanged = new("OnHealthChanged");
    public static readonly TypedGameEvent<CharacterController> OnCharacterActionSelected = new("OnCharacterActionSelected");

    // ── Combat flow ───────────────────────────────────────────────────────
    public static readonly TypedGameEvent<int> OnRoundStart = new("OnRoundStart");
    public static readonly GameEvent OnPlayerVictory        = new("OnPlayerVictory");
    public static readonly GameEvent OnPlayerDefeat         = new("OnPlayerDefeat");
    
}

public readonly struct AttackData
{
    public readonly string AttackerName;
    public readonly string TargetName;
    public readonly int Damage;

    public AttackData(string attacker, string target, int damage)
    {
        AttackerName = attacker;
        TargetName   = target;
        Damage       = damage;
    }
}
