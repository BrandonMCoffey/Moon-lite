using UnityEngine;
interface IDamageable
{
    void TakeDamage(float damage);
    void Heal(float amount);
    void ApplyDamageOverTime(int damage, float duration, float interval, GameObject source);
    float GetHealth();
    bool IsAlive();
    void Die();
}