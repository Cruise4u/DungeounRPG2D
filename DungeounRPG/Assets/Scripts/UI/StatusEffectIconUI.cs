using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single buff/debuff icon inside a CharacterCardUI.
/// Extend ActiveStatusEffect with an icon Sprite and display name when the data model grows.
/// </summary>
public class StatusEffectIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI roundsLabel;

    public void Set(ActiveStatusEffect effect)
    {
        // Icon sprite: swap in effect.Icon once ActiveStatusEffect exposes one.
        if (roundsLabel != null)
            roundsLabel.text = effect.RemainingRounds.ToString();
    }
}
