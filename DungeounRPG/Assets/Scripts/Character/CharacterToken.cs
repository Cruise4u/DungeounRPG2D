using UnityEngine;

public enum EEvolutionTypeID
{
    Basic,
    Super,
    Mega,
    Ultra
}

public class CharacterToken : MonoBehaviour
{
    [Tooltip("Must match the poolId configured for this token's prefab.")]
    public ETokenPoolID poolId;

    [SerializeField] private GameObject characterPrefab;
    public GameObject CharacterPrefab => characterPrefab;

    public CharacterSlot CurrentSlot { get; private set; }
    public EEvolutionTypeID EvolutionType { get; private set; } = EEvolutionTypeID.Basic;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSlot(CharacterSlot slot)
    {
        CurrentSlot = slot;
    }

    public void SetEvolution(EEvolutionTypeID evolution, Color color)
    {
        EvolutionType = evolution;
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
    }

    public void ResetToken()
    {
        EvolutionType = EEvolutionTypeID.Basic;
        CurrentSlot = null;
    }
}
