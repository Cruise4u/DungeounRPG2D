using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO baseStats;

    public event Action<int, int> OnHpChanged; // (currentHp, maxHp)

    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int AttackPower { get; private set; }
    public int Armor { get; private set; }
    public float Speed { get; private set; }

    private void Awake()
    {
        if (baseStats == null)
        {
            Debug.LogError($"[CharacterStats] No CharacterStatsSO assigned on {gameObject.name}.", this);
            return;
        }

        LoadFromSO(baseStats);
    }

    private void LoadFromSO(CharacterStatsSO so)
    {
        MaxHp = so.maxHp;
        CurrentHp = so.maxHp;
        AttackPower = so.attackPower;
        Armor = so.armor;
        Speed = so.speed;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    public void TakeDamage(int rawDamage)
    {
        int mitigated = Mathf.Max(0, rawDamage - Armor);
        CurrentHp = Mathf.Max(0, CurrentHp - mitigated);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    public void Heal(int amount)
    {
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    public void ModifyAttackPower(int delta) => AttackPower = Mathf.Max(0, AttackPower + delta);
    public void ModifyArmor(int delta)       => Armor = Mathf.Max(0, Armor + delta);
    public void ModifySpeed(float delta)     => Speed = Mathf.Max(0f, Speed + delta);

    public void ResetToBase()
    {
        if (baseStats != null) LoadFromSO(baseStats);
    }
}
