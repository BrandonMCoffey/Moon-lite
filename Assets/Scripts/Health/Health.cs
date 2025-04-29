using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    [SerializeField] private GameObject _deathEffectPrefab; 
    [SerializeField] private GameObject _damageEffectPrefab;
    [SerializeField] private GameObject _healEffectPrefab;

    [SerializeField] List<DamageOverTimeEffect> _damageOverTimeEffects = new();


    private void Start() => _currentHealth = _maxHealth;
    public float GetHealth() => _currentHealth;
    public bool IsAlive() => _currentHealth > 0;

    public void Die()
    {
        if (_deathEffectPrefab != null) Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
        if (_healEffectPrefab != null) Instantiate(_healEffectPrefab, transform.position, Quaternion.identity);
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0) { Die(); return; }
        if (_damageEffectPrefab != null) Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity);
    }

    public void ApplyDamageOverTime(int damage, float duration, float interval, GameObject source)
    {
        var effect = new DamageOverTimeEffect(damage, duration, interval, source);
        _damageOverTimeEffects.Add(effect);
    }

    private void Update()
    {
        for (int i = _damageOverTimeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _damageOverTimeEffects[i];

            effect.Duration -= Time.deltaTime;
            effect.TimeUntilNextTick -= Time.deltaTime;

            if (effect.Duration > 0)
            {
                if (effect.TimeUntilNextTick <= 0)
                {
                    TakeDamage(effect.Damage);
                    effect.TimeUntilNextTick = effect.Interval;
                }
            }
            else
            {
                _damageOverTimeEffects.RemoveAt(i);
            }
        }
    }

}

public class DamageOverTimeEffect
{
    public int Damage;
    public float Duration;
    public float Interval;
    public float TimeUntilNextTick;
    public GameObject Source;

    public DamageOverTimeEffect(int damage, float duration, float interval, GameObject source)
    {
        Damage = damage;
        Duration = duration;
        Interval = interval;
        TimeUntilNextTick = interval;
        Source = source;
    }
}