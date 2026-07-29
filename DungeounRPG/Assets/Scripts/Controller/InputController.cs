using UnityEngine;

/// <summary>
/// Translates raw pointer input into prep-grid actions. Player-only by design: the AI has no
/// input to mimic, it just behaves, so nothing on the enemy side ever needs one of these.
///
/// Scope is the prep grid — dragging figurines and merging them. Combat is automatic and runs
/// in the CombatZone, so no input is read once Fight is pressed.
/// </summary>
public class InputController : MonoBehaviour
{
    public Camera InputCamera { get => inputCamera; private set =>  inputCamera = value; }
    
    [Header("Token Drag & Merge")]
    [SerializeField] private CharacterFigurineMerger characterFigurineMerger;
    [SerializeField] private LayerMask tokenLayerMask = ~0;
    [SerializeField] private float snapThreshold = 0.5f;

    private Camera inputCamera;
    private IInputProvider _input;
    private CharacterFigurine _draggedFigurine;
    private Vector3 _dragOrigin;
    private CharacterFigurine _snapCandidate;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        inputCamera = Camera.main;

        _input = Application.isMobilePlatform
            ? (IInputProvider)new TouchInputProvider()
            : new MouseInputProvider();
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

    private Vector3 PointerWorldPosition()
    {
        Vector3 screen = _input.PointerScreenPosition;
        screen.z = -inputCamera.transform.position.z;
        return inputCamera.ScreenToWorldPoint(screen);
    }

    private Collider2D OverlapAtPointer(LayerMask mask)
    {
        return Physics2D.OverlapPoint(PointerWorldPosition(), mask);
    }
}
