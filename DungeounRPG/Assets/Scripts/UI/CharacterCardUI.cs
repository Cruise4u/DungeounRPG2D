using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single character's panel on the TeamBoardUI.
/// Bind via CharacterCardUI.Bind(character) after instantiating the prefab; Unbind() releases it.
///
/// Every label is optional: leave a field empty in the Inspector and that part of the card is
/// simply skipped, so the same prefab can serve a detailed roster panel and a minimal strip.
/// The card is a pure view — it never writes to the character, it only listens.
/// </summary>
public class CharacterCardUI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private TextMeshProUGUI nameLabel;

    [Header("HP")]
    [SerializeField] private TextMeshProUGUI hpLabel;
    [SerializeField] private string hpFormat = "HP : {0}/{1}";

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI attackLabel;
    [SerializeField] private string attackFormat = "ATK : {0}";
    [SerializeField] private TextMeshProUGUI armorLabel;
    [SerializeField] private string armorFormat = "DEF : {0}";
    [SerializeField] private TextMeshProUGUI speedLabel;
    [SerializeField] private string speedFormat = "SPD : {0:0.#}";

    [Header("Evolution")]
    [SerializeField] private TextMeshProUGUI evolutionLabel;
    [SerializeField] private string evolutionFormat = "{0}";

    // Energy is not in the data model yet — wire these up once CharacterStats exposes it.
    // [Header("Energy")]
    // [SerializeField] private Slider energySlider;
    // [SerializeField] private TextMeshProUGUI energyLabel;

    [Header("Status Effects")]
    [SerializeField] private Transform statusEffectContainer;
    [SerializeField] private StatusEffectIconUI statusEffectIconPrefab;

    [Header("State")]
    [SerializeField] private GameObject inCombatIndicator;
    [SerializeField] private GameObject deadIndicator;

    private Character _character;
    private CharacterStats _stats;
    private StatusEffectHandler _effectHandler;
    private readonly List<StatusEffectIconUI> _iconPool = new();

    // Subscriptions are toggled by OnEnable/OnDisable as well as Bind/Unbind, so this guards
    // against double-subscribing (or double-unsubscribing) when both paths fire.
    private bool _subscribed;

    /// <summary>The character this card is currently showing, or null when free.</summary>
    public Character BoundCharacter => _character;

    public bool IsBound => _character != null;

    // ── binding ─────────────────────────────────────────────────────────────

    public void Bind(Character character)
    {
        // Re-binding the same character is a refresh, not a resubscribe.
        if (_character != null && _character == character)
        {
            RefreshAll();
            return;
        }

        Unbind();

        _character = character;
        if (_character == null) return;

        _stats = _character.Stats;
        _effectHandler = _character.GetComponent<StatusEffectHandler>();

        Subscribe();
        RefreshAll();
    }

    /// <summary>Releases the card. Safe to call on an already-free card.</summary>
    public void Unbind()
    {
        Unsubscribe();

        _character = null;
        _stats = null;
        _effectHandler = null;

        HideAllStatusIcons();
        SetActiveSafe(deadIndicator, false);
    }

    private void OnEnable()
    {
        // A recycled card comes back enabled still holding its character — re-arm it.
        if (_character == null) return;

        Subscribe();
        RefreshAll();
    }

    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (_subscribed || _character == null) return;

        _character.OnEvolutionChanged += HandleEvolutionChanged;

        if (_stats != null)
        {
            _stats.OnHpChanged += HandleHpChanged;
            _stats.OnStatsChanged += HandleStatsChanged;
        }

        if (_effectHandler != null)
            _effectHandler.OnEffectsChanged += RefreshStatusIcons;

        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;

        // Null checks cover the destroyed-character case (Unity's == treats a destroyed
        // object as null, and touching a member of one would throw).
        if (_character != null)
            _character.OnEvolutionChanged -= HandleEvolutionChanged;

        if (_stats != null)
        {
            _stats.OnHpChanged -= HandleHpChanged;
            _stats.OnStatsChanged -= HandleStatsChanged;
        }

        if (_effectHandler != null)
            _effectHandler.OnEffectsChanged -= RefreshStatusIcons;

        _subscribed = false;
    }

    // ── refresh ─────────────────────────────────────────────────────────────

    /// <summary>Pulls every field from the bound character. Cheap enough to call freely.</summary>
    public void RefreshAll()
    {
        if (_character == null) return;

        RefreshIdentity();
        RefreshHp();
        RefreshStats();
        RefreshEvolution();
        RefreshStatusIcons();
    }

    private void RefreshIdentity()
    {
        if (nameLabel != null)
            nameLabel.text = _character.TargetName;
    }

    private void RefreshHp()
    {
        if (_stats == null) return;
        HandleHpChanged(_stats.CurrentHp, _stats.MaxHp);
    }

    private void RefreshStats()
    {
        if (_stats == null) return;

        SetLabel(attackLabel, attackFormat, _stats.AttackPower);
        SetLabel(armorLabel, armorFormat, _stats.Armor);
        SetLabel(speedLabel, speedFormat, _stats.Speed);
    }

    private void RefreshEvolution()
    {
        SetLabel(evolutionLabel, evolutionFormat, _character.EvolutionType);
    }

    // ── event handlers ──────────────────────────────────────────────────────

    private void HandleHpChanged(int current, int max)
    {
        SetLabel(hpLabel, hpFormat, current, max);

        // Driven off IsDead rather than OnDied so a heal back above the threshold clears it too.
        if (_stats != null)
            SetActiveSafe(deadIndicator, _stats.IsDead);
    }

    private void HandleStatsChanged(CharacterStats stats) => RefreshStats();

    private void HandleEvolutionChanged(Character character) => RefreshEvolution();

    // ── state markers ───────────────────────────────────────────────────────

    public void SetInCombat(bool inCombat) => SetActiveSafe(inCombatIndicator, inCombat);

    // ── status effects ──────────────────────────────────────────────────────

    private void RefreshStatusIcons()
    {
        if (_effectHandler == null || statusEffectContainer == null || statusEffectIconPrefab == null)
            return;

        var effects = _effectHandler.ActiveEffects;

        // Grow pool if needed
        while (_iconPool.Count < effects.Count)
        {
            var icon = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            _iconPool.Add(icon);
        }

        for (int i = 0; i < _iconPool.Count; i++)
        {
            bool active = i < effects.Count;
            _iconPool[i].gameObject.SetActive(active);
            if (active)
                _iconPool[i].Set(effects[i]);
        }
    }

    private void HideAllStatusIcons()
    {
        foreach (var icon in _iconPool)
            if (icon != null) icon.gameObject.SetActive(false);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static void SetLabel(TextMeshProUGUI label, string format, params object[] values)
    {
        if (label == null) return;

        label.text = string.IsNullOrEmpty(format)
            ? string.Join(" ", values)
            : string.Format(format, values);
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null) target.SetActive(active);
    }
}
