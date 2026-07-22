using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single character's panel on the TeamBoardUI.
/// Bind via CharacterCardUI.Bind(character) after instantiating the prefab.
/// </summary>
public class CharacterCardUI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private TextMeshProUGUI nameLabel;

    [Header("HP")]
    [SerializeField] private TextMeshProUGUI hpLabel;

    // Energy is not in the data model yet — wire these up once CharacterStats exposes it.
    // [Header("Energy")]
    // [SerializeField] private Slider energySlider;
    // [SerializeField] private TextMeshProUGUI energyLabel;

    [Header("Status Effects")]
    [SerializeField] private Transform statusEffectContainer;
    [SerializeField] private StatusEffectIconUI statusEffectIconPrefab;

    [Header("State")]
    [SerializeField] private GameObject inCombatIndicator;

    private CharacterStats _stats;
    private StatusEffectHandler _effectHandler;
    private readonly List<StatusEffectIconUI> _iconPool = new();

    public void Bind(Character character)
    {
        if (nameLabel != null)
            nameLabel.text = character.TargetName;
        
        _stats = character.Stats;
        _effectHandler = character.GetComponent<StatusEffectHandler>();
        
        _stats.OnHpChanged += OnHpChanged;
        OnHpChanged(_stats.CurrentHp, _stats.MaxHp);

        if (_effectHandler != null)
        {
            _effectHandler.OnEffectsChanged += RefreshStatusIcons;
            RefreshStatusIcons();
        }
    }

    private void OnDisable()
    {
        if (_stats != null)
            _stats.OnHpChanged -= OnHpChanged;

        if (_effectHandler != null)
            _effectHandler.OnEffectsChanged -= RefreshStatusIcons;
    }

    private void OnHpChanged(int current, int max)
    {
        if (hpLabel != null)
            hpLabel.text = $"{current}/{max}";
    }

    public void SetInCombat(bool inCombat)
    {
        if (inCombatIndicator != null)
            inCombatIndicator.SetActive(inCombat);
    }

    private void RefreshStatusIcons()
    {
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
}
