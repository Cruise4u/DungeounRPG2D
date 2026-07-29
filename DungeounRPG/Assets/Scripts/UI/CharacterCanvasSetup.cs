using UnityEngine;

/// <summary>
/// Assigns the world-space Canvas's event camera from the character root's
/// CharacterPrefabConfig. Needed because pooled instances can be instantiated
/// (and immediately disabled) before anything else has a chance to wire the
/// Canvas up by hand.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CharacterCanvasSetup : MonoBehaviour, IPoolSetup
{
    public CharacterPrefabConfig characterPrefabConfig;
    private Canvas _canvas;

    private void Awake()
    {
        characterPrefabConfig =  GetComponentInParent<CharacterPrefabConfig>();
        AssignCamera();
    }

    public void OnPoolSetup()
    {
        AssignCamera();
    }

    private void AssignCamera()
    {
        if (_canvas == null)
            _canvas = GetComponent<Canvas>();

        if (_canvas.renderMode != RenderMode.WorldSpace) return;
        if (_canvas.worldCamera != null) return;

        if (characterPrefabConfig == null)
        {
            Debug.LogError($"[CharacterCanvasSetup] No CharacterPrefabConfig found in parent hierarchy of {gameObject.name}.", this);
            return;
        }

        _canvas.worldCamera = characterPrefabConfig.CharacterCamera;
    }
}
