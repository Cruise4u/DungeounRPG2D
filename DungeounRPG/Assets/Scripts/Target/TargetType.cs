/// <summary>
/// What an action targets: which side, and how many. Nothing about geometry — area effects are
/// decided by collision at impact, so blast shapes never reach this system.
///
/// Values are explicit because Unity serialises enums by integer, and existing action assets
/// already have these numbers baked in. Renumbering would silently repoint them at a different
/// meaning with no error and no warning, so new entries go on the end and old numbers stay put.
/// </summary>
public enum TargetType
{
    Self        = 0,
    SingleAlly  = 1,
    SingleEnemy = 2,
    AllAllies   = 3,
    AllEnemies  = 4
}

public enum AITargetStrategy
{
    Random      = 0,
    LowestHp    = 1,
    HighestHp   = 2,
    LowestArmor = 3,
    Nearest     = 4
}
