using UnityEngine;

// ── Initialization diagram ────────────────────────────────────────────────────
//
//  AWAKE  (Unity runs all Awake calls before any Start)
//  │
//  ├─ [Order -100] GameManager.Awake       — singleton guard, DontDestroyOnLoad
//  ├─ Character.Awake                      — grabs CharacterStats (same GO)
//  ├─ AICharacter.Awake                    — grabs AIController   (same GO)
//  └─ TargetManager.Awake                  — resolves CombatManager ref
//
//  ON_ENABLE  (all OnEnable calls run after all Awake, before any Start)
//  │
//  └─ PlayerController.OnEnable            — subscribes to GameEventSingleton events
//
//  START  (all Start calls run last — all listeners are registered by here)
//  │
//  └─ GameManager.Start → CombatManager.StartCombat()
//                         Combat loop begins.
//
// ─────────────────────────────────────────────────────────────────────────────

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CombatManager combatManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        combatManager.StartCombat();
    }
}
