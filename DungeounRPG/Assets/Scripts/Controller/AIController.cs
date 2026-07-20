using System.Collections.Generic;
using UnityEngine;

// PerformAction is stateless per call (the acting character is always passed in), so a single
// AIController instance can be shared by many AICharacters instead of each one carrying its
// own. The instance is handed out explicitly (see Encounter/AICharacter.AssignController)
// rather than looked up globally, so different enemy groups can be given different
// AIControllers (e.g. a different AITargetStrategy/defaultAction) if needed later.
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
