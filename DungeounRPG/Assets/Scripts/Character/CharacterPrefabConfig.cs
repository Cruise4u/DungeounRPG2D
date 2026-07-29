using UnityEngine;

/// <summary>
/// Prefab-level configuration shared by a character's sub-objects (UI, VFX, etc.).
/// Lives on the root GameObject alongside Character so per-prefab wiring (like which
/// camera a world-space Canvas should render through) doesn't have to live on
/// Character/CharacterStats itself.
/// </summary>
public class CharacterPrefabConfig : MonoBehaviour
{
    [SerializeField] private Camera cachedCamera;
    public Camera CharacterCamera => cachedCamera != null ? cachedCamera : FindFirstObjectByType<Camera>();

    public void Awake()
    {
        cachedCamera = FindFirstObjectByType<Camera>();
    }
}
