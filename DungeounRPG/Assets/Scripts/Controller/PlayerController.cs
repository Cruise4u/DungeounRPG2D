using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterController
{
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private LayerMask characterLayerMask = ~0;

    [SerializeField]
    private PlayerTeam _activeTeam;
    [SerializeField]
    private PlayerCharacter _activeCharacter;
    private bool _isTargeting;
   
    [SerializeField]
    private CharacterActionSO _pendingAction;
    private List<ITarget> _currentValidTargets = new();
    
    private void OnEnable()
    {
        GameEventSingleton.OnPlayerTurnStart.Subscribe(OnTeamTurnStart);
        GameEventSingleton.OnPlayerTurnEnd.Subscribe(OnTeamTurnEnd);
        GameEventSingleton.OnPlayerCharacterTurnStart.Subscribe(OnCharacterTurnStart);
        GameEventSingleton.OnPlayerCharacterTurnEnd.Subscribe(OnCharacterTurnEnd);
    }
    
    private void Awake()
    {
        if (targetManager == null)
            targetManager = FindFirstObjectByType<TargetManager>();
    }
    
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        HandleTargetClick();
        // if (_activeTeam != null && _activeCharacter == null)
        //     HandleCharacterSelection();
    }
    
    private void OnDisable()
    {
        GameEventSingleton.OnPlayerTurnStart.Unsubscribe(OnTeamTurnStart);
        GameEventSingleton.OnPlayerTurnEnd.Unsubscribe(OnTeamTurnEnd);
        GameEventSingleton.OnPlayerCharacterTurnStart.Unsubscribe(OnCharacterTurnStart);
        GameEventSingleton.OnPlayerCharacterTurnEnd.Unsubscribe(OnCharacterTurnEnd);
    }
    
    // ─── Team-level events ────────────────────────────────────────────────────

    private void OnTeamTurnStart(PlayerTeam team)
    {
        _activeTeam = team;
    }

    private void OnTeamTurnEnd(PlayerTeam team)
    {
        _activeTeam = null;
    }

    // ─── Character-level events ───────────────────────────────────────────────

    private void OnCharacterTurnStart(PlayerCharacter character)
    {
        _activeCharacter = character;
    }

    private void OnCharacterTurnEnd(PlayerCharacter character)
    {
        ClearActionState();
        _activeCharacter = null;
    }

    // ─── Public API (called from action buttons) ──────────────────────────────

    public void BeginTargeting(CharacterActionSO action)
    {
        if (_activeCharacter == null || action == null) return;

        // Clicking the button counts as selecting this character to act next.
        // This unblocks PlayerTeam.TakeTurn which is waiting on _selectedCharacter.
        _activeTeam?.SelectCharacter(_activeCharacter);

        ClearHighlights(_currentValidTargets);
        _pendingAction = action;
        _currentValidTargets = targetManager.GetValidTargets(action.TargetType, _activeCharacter);

        bool needsClick = action.TargetType is TargetType.SingleAlly or TargetType.SingleEnemy;
        if (!needsClick)
        {
            ConfirmImmediate();
            return;
        }

        HighlightTargets(true);
        _isTargeting = true;
    }

    public void CancelTargeting()
    {
        ClearActionState();
    }

    /// <summary>
    /// Ends the player's turn immediately — assign this to Turn_Button via the Inspector.
    /// Signals the active character to stop and clears all remaining pending characters.
    /// </summary>
    public void EndTurn()
    {
        ClearActionState();
        _activeTeam?.EndTurn();
    }

    // Player clicks one of their own pending characters to make them act next.
    private void HandleCharacterSelection()
    {
        var hit = OverlapAtMouse();
        if (hit == null) return;

        var character = hit.GetComponent<PlayerCharacter>();
        if (character == null) return;

        _activeTeam.SelectCharacter(character);
    }

    // Player clicks an enemy (or ally) as the action target.
    private void HandleTargetClick()
    {
        if (!_isTargeting) return;  // ignore clicks that aren't part of an active targeting session

        var hit = OverlapAtMouse();
        if (hit == null) return;

        ITarget target = hit.GetComponent<ITarget>();
        if (target == null) return;

        if (!targetManager.IsValidTarget(target, _pendingAction.TargetType, _activeCharacter)) return;

        ClearHighlights(_currentValidTargets);
        _isTargeting = false;
        ConfirmAction(_activeCharacter, new List<ITarget> { target }, _pendingAction);
        _pendingAction = null;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void ConfirmImmediate()
    {
        _isTargeting = false;
        ConfirmAction(_activeCharacter, _currentValidTargets, _pendingAction);
        _pendingAction = null;
    }

    private void ClearActionState()
    {
        ClearHighlights(_currentValidTargets);
        _isTargeting = false;
        _pendingAction = null;
    }

    private Collider2D OverlapAtMouse()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return Physics2D.OverlapPoint(worldPos, characterLayerMask);
    }

    private void HighlightTargets(bool on)
    {
        foreach (var t in _currentValidTargets)
            if (t is Character c) c.SetHighlighted(on);
    }

    private void HighlightPending(bool on)
    {
        if (_activeTeam == null) return;
        foreach (var c in _activeTeam.PendingCharacters)
            c.SetHighlighted(on);
    }

    private void ClearHighlights(List<ITarget> targets)
    {
        foreach (var t in targets)
            if (t is Character c) c.SetHighlighted(false);
        targets.Clear();
    }
}
