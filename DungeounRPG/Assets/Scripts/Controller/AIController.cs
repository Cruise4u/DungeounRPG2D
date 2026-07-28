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
}
