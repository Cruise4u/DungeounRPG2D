using TMPro;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

    public GameObject rootGO;
    private CharacterStats _stats;

    private void Awake()
    {
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
        if (healthText != null)
            healthText.text = $"{currentHp} / {maxHp}";
    }
}
