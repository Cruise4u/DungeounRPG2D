public interface ITarget
{
    string TargetName { get; }
    bool IsAlive { get; }
    void TakeDamage(int damage);
    void Heal(int amount);
    void GetSE(StatusEffect effect);
}
