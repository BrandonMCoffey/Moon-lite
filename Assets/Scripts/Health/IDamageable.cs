internal interface IDamageable
{
    void TakeDamage(float damage);
    void Heal(float amount);
    float GetHealth();
    bool IsAlive();
    void Die();
}