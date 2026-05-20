using UnityEngine;

/// <summary>
/// Per-character combat settings. Assign one to each Character in the Inspector.
/// Create via: Right-click → Create → Combat → Combat Settings
/// </summary>
[CreateAssetMenu(fileName = "CombatSettings", menuName = "Combat/Combat Settings")]
public class CombatSettingsSO : ScriptableObject
{
    [Min(1)]
    [Tooltip("How many actions this character may take each round.")]
    public int actionsPerTurn = 1;
}
