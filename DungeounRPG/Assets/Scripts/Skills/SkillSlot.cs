using System.Collections.Generic;
using UnityEngine;

// Holds the skills equipped to a character. Add this component alongside Character.
public class SkillSlot : MonoBehaviour
{
    [SerializeField] private List<SkillSO> equippedSkills = new();

    public IReadOnlyList<SkillSO> EquippedSkills => equippedSkills;

    public int Count => equippedSkills.Count;

    public SkillSO GetSkill(int index)
    {
        if (index < 0 || index >= equippedSkills.Count)
        {
            Debug.LogWarning($"[SkillSlot] Skill index {index} out of range on {gameObject.name}.");
            return null;
        }
        return equippedSkills[index];
    }

    public bool TryGetSkill(int index, out SkillSO skill)
    {
        skill = null;
        if (index < 0 || index >= equippedSkills.Count) return false;
        skill = equippedSkills[index];
        return skill != null;
    }
}
