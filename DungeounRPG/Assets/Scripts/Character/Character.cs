using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Character : MonoBehaviour, ITarget
{
    public CharacterStats Stats { get; private set; }

    // ITarget
    public string TargetName => gameObject.name;
    public bool IsAlive => Stats.CurrentHp >= 1;
    public void TakeDamage(int damage) => Stats.TakeDamage(damage);
    public void Heal(int amount)       => Stats.Heal(amount);

    [SerializeField] private CombatSettingsSO combatSettings;

    /// <summary>How many actions this character may take per round (driven by the assigned SO).</summary>
    public int ActionsPerTurn => combatSettings != null ? combatSettings.actionsPerTurn : 1;

    // Shared action state — set by ConfirmAction, consumed by TakeTurn.
    protected List<ITarget> PendingTargets;
    protected CharacterActionSO PendingAction;
    protected bool ActionConfirmed;

    /// <summary>Set to true to break out of the action loop early (End Turn).</summary>
    protected bool _turnEnded;

    protected virtual void Awake()
    {
        Stats = GetComponent<CharacterStats>();
        if (Stats == null)
            Debug.LogError($"[Character] Missing CharacterStats on {gameObject.name}.", this);
    }

    // Single entry point called by CombatManager for any characterRequisitor type.
    public void ConfirmAction(List<ITarget> targets, CharacterActionSO action)
    {
        if (ActionConfirmed) return;
        PendingTargets  = targets;
        PendingAction   = action;
        ActionConfirmed = true;
        // Do NOT execute here — PlayerCharacter.TakeTurn calls ExecutePendingAction()
        // after the WaitUntil, which is the single authoritative execution point.
    }

    // Executes the stored action against living targets. Call after ActionConfirmed.
    protected void ExecutePendingAction()
    {
        var valid = PendingTargets?.Where(t => t != null && t.IsAlive).ToList()
                    ?? new List<ITarget>();
        PendingAction?.Execute(this, valid);
    }

    protected void ResetActionState()
    {
        PendingTargets  = null;
        PendingAction   = null;
        ActionConfirmed = false;
    }

    public abstract IEnumerator TakeTurn(CombatManager combat);

    public virtual void SetHighlighted(bool highlighted)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = highlighted ? Color.yellow : Color.white;
    }

}
