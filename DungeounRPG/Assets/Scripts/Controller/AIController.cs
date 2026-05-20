using System.Collections.Generic;
using UnityEngine;

// Attach alongside AICharacter. Decides which action to take and who to target.
public class AIController : CharacterController
{
    [SerializeField] private AITargetStrategy strategy = AITargetStrategy.Random;
    [SerializeField] private CharacterActionSO defaultAction;

    // Called by AICharacter.TakeTurn. Returns false if no valid action could be resolved.
    public bool PerformAction(CombatManager combat, AICharacter character)
    {
        if (defaultAction == null)
        {
            Debug.LogWarning($"[AIController] No action assigned on {character.TargetName}.", this);
            return false;
        }

        ITarget target = combat.TargetManager.GetBestTarget(defaultAction.TargetType, strategy, character);
        if (target == null) return false;

        ConfirmAction(character, new List<ITarget> { target }, defaultAction);
        return true;
    }
}
