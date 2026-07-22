using System;
using System.Collections.Generic;
using UnityEngine;

// Processes IDiceEffect instances produced by special dice pips and applies them to a character.
public class StatusEffectHandler : MonoBehaviour
{
    private readonly List<ActiveStatusEffect> _activeEffects = new();

    public event Action OnEffectsChanged;

    public IReadOnlyList<ActiveStatusEffect> ActiveEffects => _activeEffects;

    // Called by CombatManager at the start of each round to tick duration-based effects.
    public void TickEffects()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].RemainingRounds--;
            if (_activeEffects[i].RemainingRounds <= 0)
                _activeEffects.RemoveAt(i);
        }
        OnEffectsChanged?.Invoke();
    }

    // Removes all active effects (e.g. on combat end).
    public void ClearEffects()
    {
        _activeEffects.Clear();
        OnEffectsChanged?.Invoke();
    }

}

public class ActiveStatusEffect
{
    public float Magnitude { get; }
    public int StatDelta { get; }  // The actual stat value that was added — used to revert on expiry
    public int RemainingRounds { get; set; }


}
