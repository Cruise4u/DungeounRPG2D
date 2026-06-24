using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterController
{
    // ─── Combat targeting ─────────────────────────────────────────────────────

    private Camera playerCamera;
    
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private LayerMask characterLayerMask = ~0;
    [SerializeField] private PlayerTeam _activeTeam;
    [SerializeField] private PlayerCharacter _activeCharacter;

    private bool _isTargeting;
    private CharacterActionSO _pendingAction;
    private List<ITarget> _currentValidTargets = new();

    // ─── Token drag & merge ───────────────────────────────────────────────────

    [Header("Token Drag & Merge")]
    [SerializeField] private TokenMerger tokenMerger;
    [SerializeField] private LayerMask tokenLayerMask = ~0;
    [SerializeField] private float snapThreshold = 0.5f;

    private ITokenInputProvider _input;
    private CharacterToken _draggedToken;
    private Vector3 _dragOrigin;
    private CharacterToken _snapCandidate;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        playerCamera = FindFirstObjectByType<Camera>();
        if (targetManager == null)
            targetManager = FindFirstObjectByType<TargetManager>();

        _input = Application.isMobilePlatform
            ? (ITokenInputProvider)new TouchInputProvider()
            : new MouseInputProvider();
    }

    private void OnEnable()
    {
        GameEventSingleton.OnPlayerTurnStart.Subscribe(OnTeamTurnStart);
        GameEventSingleton.OnPlayerTurnEnd.Subscribe(OnTeamTurnEnd);
        GameEventSingleton.OnPlayerCharacterTurnStart.Subscribe(OnCharacterTurnStart);
        GameEventSingleton.OnPlayerCharacterTurnEnd.Subscribe(OnCharacterTurnEnd);
    }

    private void OnDisable()
    {
        GameEventSingleton.OnPlayerTurnStart.Unsubscribe(OnTeamTurnStart);
        GameEventSingleton.OnPlayerTurnEnd.Unsubscribe(OnTeamTurnEnd);
        GameEventSingleton.OnPlayerCharacterTurnStart.Unsubscribe(OnCharacterTurnStart);
        GameEventSingleton.OnPlayerCharacterTurnEnd.Unsubscribe(OnCharacterTurnEnd);
    }

    private void Update()
    {
        if (_input.DragStarted)
        {
            if (_isTargeting)
                HandleTargetClick();
            else
                TryBeginDrag();
        }
        else if (_input.IsDragging && _draggedToken != null)
        {
            UpdateDrag();
        }
        else if (_input.DragEnded && _draggedToken != null)
        {
            EndDrag();
        }
    }

    // ─── Token drag ───────────────────────────────────────────────────────────

    private void TryBeginDrag()
    {
        var hit = OverlapAtPointer(tokenLayerMask);
        if (hit == null) return;

        var token = hit.GetComponent<CharacterToken>();
        if (token == null) return;

        _draggedToken = token;
        _dragOrigin   = token.transform.position;
    }

    private void UpdateDrag()
    {
        _draggedToken.transform.position = PointerWorldPosition();

        _snapCandidate = FindSnapCandidate();
        if (_snapCandidate != null)
            _draggedToken.transform.position = _snapCandidate.transform.position;
    }

    private void EndDrag()
    {
        if (_snapCandidate != null && tokenMerger != null)
        {
            bool merged = tokenMerger.MergeTokens(_draggedToken, _snapCandidate);
            if (!merged)
                _draggedToken.transform.position = _dragOrigin;
        }
        else
        {
            _draggedToken.transform.position = _dragOrigin;
        }

        _draggedToken  = null;
        _snapCandidate = null;
    }

    // Returns the nearest same-evolution token within snapThreshold, excluding the dragged token itself.
    private CharacterToken FindSnapCandidate()
    {
        Vector2 pos = _draggedToken.transform.position;
        var hits = Physics2D.OverlapCircleAll(pos, snapThreshold, tokenLayerMask);

        CharacterToken best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var token = hit.GetComponent<CharacterToken>();
            if (token == null || token == _draggedToken) continue;
            if (token.EvolutionType != _draggedToken.EvolutionType) continue;

            float dist = Vector2.Distance(pos, hit.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = token;
            }
        }

        return best;
    }

    // ─── Combat targeting ─────────────────────────────────────────────────────

    public void BeginTargeting(CharacterActionSO action)
    {
        if (_activeCharacter == null || action == null) return;

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

    public void EndTurn()
    {
        ClearActionState();
    }

    private void HandleTargetClick()
    {
        var hit = OverlapAtPointer(characterLayerMask);
        if (hit == null) return;

        ITarget target = hit.GetComponent<ITarget>();
        if (target == null) return;

        if (!targetManager.IsValidTarget(target, _pendingAction.TargetType, _activeCharacter)) return;

        ClearHighlights(_currentValidTargets);
        _isTargeting = false;
        ConfirmAction(_activeCharacter, new List<ITarget> { target }, _pendingAction);
        _pendingAction = null;
    }

    // ─── Team / character events ──────────────────────────────────────────────

    private void OnTeamTurnStart(PlayerTeam team)     => _activeTeam = team;
    private void OnTeamTurnEnd(PlayerTeam team)       => _activeTeam = null;
    private void OnCharacterTurnStart(PlayerCharacter c) => _activeCharacter = c;

    private void OnCharacterTurnEnd(PlayerCharacter c)
    {
        ClearActionState();
        _activeCharacter = null;
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

    private Vector3 PointerWorldPosition()
    {
        Vector3 screen = _input.PointerScreenPosition;
        screen.z = -Camera.main.transform.position.z;
        return playerCamera.ScreenToWorldPoint(screen);
    }

    private Collider2D OverlapAtPointer(LayerMask mask)
    {
        return Physics2D.OverlapPoint(PointerWorldPosition(), mask);
    }

    private void HighlightTargets(bool on)
    {
        foreach (var t in _currentValidTargets)
            if (t is Character c) c.SetHighlighted(on);
    }

    private void ClearHighlights(List<ITarget> targets)
    {
        foreach (var t in targets)
            if (t is Character c) c.SetHighlighted(false);
        targets.Clear();
    }
    
    
}
