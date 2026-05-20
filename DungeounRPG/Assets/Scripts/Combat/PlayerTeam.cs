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

    /// <summary>
    /// Ends the active character's remaining actions and skips all other pending
    /// characters this round. Wired via PlayerController.EndTurn → Turn_Button.
    /// </summary>
    public void EndTurn()
    {
        _currentCharacter?.EndTurn();   // break the active character's action loop
        _pending.Clear();               // skip any characters that haven't acted yet
    }

    public override IEnumerator TakeTurn(CombatManager combat)
    {
        _currentCharacter = null;
        _pending.Clear();
        foreach (var c in members.OfType<PlayerCharacter>().Where(c => c.IsAlive))
            _pending.Add(c);

        GameEventSingleton.OnPlayerTurnStart.Raise(this);

        while (_pending.Count > 0)
        {
            _selectedCharacter = null;
            yield return new WaitUntil(() => _selectedCharacter != null);

            _currentCharacter = _selectedCharacter;
            _pending.Remove(_currentCharacter);
            yield return _currentCharacter.TakeTurn(combat);
            _currentCharacter = null;
        }

        GameEventSingleton.OnPlayerTurnEnd.Raise(this);
    }
}
