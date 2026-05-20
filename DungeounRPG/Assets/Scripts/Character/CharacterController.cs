using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    // The sole owner of action confirmation — both Player and AI route through here.
    protected void ConfirmAction(Character character, List<ITarget> targets, CharacterActionSO action)
    {
        character.ConfirmAction(targets, action);
    }
}
