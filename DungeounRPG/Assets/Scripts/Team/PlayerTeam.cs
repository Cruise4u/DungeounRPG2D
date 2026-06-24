using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTeam : Team
{
    private readonly HashSet<PlayerCharacter> _pending = new();
    private PlayerCharacter _selectedCharacter;
    private PlayerCharacter _currentCharacter;

    public IReadOnlyCollection<PlayerCharacter> PendingCharacters => _pending;

    // Called by PlayerController when the player clicks a character to act.
    public void SelectCharacter(PlayerCharacter character)
    {
        if (_pending.Contains(character))
            _selectedCharacter = character;
    }
}


