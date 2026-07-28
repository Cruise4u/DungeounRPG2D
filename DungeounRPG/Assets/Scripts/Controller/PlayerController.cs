using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterController
{
    // ─── Combat targeting ─────────────────────────────────────────────────────

    private Camera playerCamera;
    
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private LayerMask characterLayerMask = ~0;
    [SerializeField] private Team playerTeam;
    [SerializeField] private PlayerCharacter _activeCharacter;

    private bool _isTargeting;
    private CharacterActionSO _pendingAction;
    private List<ITarget> _currentValidTargets = new();

    // ─── Token drag & merge ───────────────────────────────────────────────────

    [Header("Token Drag & Merge")]
    [SerializeField] private CharacterFigurineMerger characterFigurineMerger;
    [SerializeField] private LayerMask tokenLayerMask = ~0;
    [SerializeField] private float snapThreshold = 0.5f;

    private IInputProvider _input;
    private CharacterFigurine _draggedFigurine;
    private Vector3 _dragOrigin;
    private CharacterFigurine _snapCandidate;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        playerCamera = FindFirstObjectByType<Camera>();
        if (targetManager == null)
            targetManager = FindFirstObjectByType<TargetManager>();

        _input = Application.isMobilePlatform
            ? (IInputProvider)new TouchInputProvider()
            : new MouseInputProvider();
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        if (_input.DragStarted)
        {
            TryBeginDrag();
        }
        else if (_input.IsDragging && _draggedFigurine != null)
        {
            UpdateDrag();
        }
        else if (_input.DragEnded && _draggedFigurine != null)
        {
            EndDrag();
        }
    }

    // ─── Figurine drag ───────────────────────────────────────────────────────────

    private void TryBeginDrag()
    {
        var hit = OverlapAtPointer(tokenLayerMask);
        if (hit == null) return;

        var token = hit.GetComponent<CharacterFigurine>();
        if (token == null) return;

        _draggedFigurine = token;
        _dragOrigin   = token.transform.position;
    }

    private void UpdateDrag()
    {
        _draggedFigurine.transform.position = PointerWorldPosition();

        _snapCandidate = FindSnapCandidate();
        if (_snapCandidate != null)
            _draggedFigurine.transform.position = _snapCandidate.transform.position;
    }

    private void EndDrag()
    {
        if (_snapCandidate != null && characterFigurineMerger != null)
        {
            bool merged = characterFigurineMerger.MergeTokens(_draggedFigurine, _snapCandidate);
            if (!merged)
                _draggedFigurine.transform.position = _dragOrigin;
        }
        else
        {
            _draggedFigurine.transform.position = _dragOrigin;
        }

        _draggedFigurine  = null;
        _snapCandidate = null;
    }

    // Returns the nearest same-evolution figurine within snapThreshold, excluding the dragged figurine itself.
    private CharacterFigurine FindSnapCandidate()
    {
        Vector2 pos = _draggedFigurine.transform.position;
        var hits = Physics2D.OverlapCircleAll(pos, snapThreshold, tokenLayerMask);

        CharacterFigurine best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var token = hit.GetComponent<CharacterFigurine>();
            if (token == null || token == _draggedFigurine) continue;
            if (token.EvolutionType != _draggedFigurine.EvolutionType) continue;

            float dist = Vector2.Distance(pos, hit.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = token;
            }
        }

        return best;
    }
    
    // ─── Helpers ──────────────────────────────────────────────────────────────
    
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
