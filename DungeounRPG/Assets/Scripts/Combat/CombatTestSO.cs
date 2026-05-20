using UnityEngine;

public enum CombatTestAction { Damage, Heal }

/// <summary>
/// Drop this SO anywhere in your project, open it in the Inspector during Play Mode,
/// pick a target character from the dropdown and hit Execute — no mouse targeting needed.
/// Create via: Right-click → Create → Combat → Combat Test
/// </summary>
[CreateAssetMenu(fileName = "CombatTest", menuName = "Combat/Combat Test")]
public class CombatTestSO : ScriptableObject
{
    [Header("Action")]
    public CombatTestAction action = CombatTestAction.Damage;

    [Min(0)]
    public int amount = 10;

    [Header("Target")]
    [Tooltip("Populated via the dropdown in the Inspector during Play Mode.")]
    public string targetName;

    // ── Called by the custom editor buttons ───────────────────────────────────

    public void ExecuteAction()
    {
        var target = FindTarget();
        if (target == null) return;

        if (action == CombatTestAction.Damage)
            target.TakeDamage(amount);
        else
            target.Heal(amount);

        Debug.Log($"[CombatTest] {action} {amount} → {target.TargetName}");
    }

    public void EndPlayerTurn()
    {
        var controller = Object.FindFirstObjectByType<PlayerController>();
        if (controller == null)
        {
            Debug.LogWarning("[CombatTest] No PlayerController found in scene.");
            return;
        }

        controller.EndTurn();
        Debug.Log("[CombatTest] Player turn ended.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ITarget FindTarget()
    {
        if (string.IsNullOrEmpty(targetName))
        {
            Debug.LogWarning("[CombatTest] No target selected.");
            return null;
        }

        var go = GameObject.Find(targetName);
        if (go == null)
        {
            Debug.LogWarning($"[CombatTest] No GameObject named '{targetName}' found in scene.");
            return null;
        }

        var target = go.GetComponent<ITarget>();
        if (target == null)
            Debug.LogWarning($"[CombatTest] '{targetName}' does not implement ITarget.");

        return target;
    }
}
