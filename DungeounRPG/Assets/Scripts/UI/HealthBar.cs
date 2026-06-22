using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IPoolSetup
{
    // [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;

    public GameObject rootGO;
    private CharacterStats _stats;

    private void Awake()
    {
        ResolveStats();
    }

    public void OnPoolSetup()
    {
        ResolveStats();
    }

    private void ResolveStats()
    {
        if (_stats != null) return;

        _stats = rootGO.GetComponent<CharacterStats>();
        if (_stats == null)
            Debug.LogError($"[HealthBar] No CharacterStats found in parent hierarchy of {gameObject.name}.", this);
    }

    private void OnEnable()
    {
        if (_stats != null)
            _stats.OnHpChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (_stats != null)
            _stats.OnHpChanged -= UpdateUI;
    }

    private void Start()
    {
        if (_stats != null)
            UpdateUI(_stats.CurrentHp, _stats.MaxHp);
    }

    private void UpdateUI(int currentHp, int maxHp)
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHp / (float)maxHp;
        }
    }

}
