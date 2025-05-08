using System;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _currentHealth;
    public float HealthPercentage => _currentHealth / _maxHealth;
    [SerializeField] private GameObject _deathEffectPrefab; 
    [SerializeField] private GameObject _damageEffectPrefab;
    [SerializeField] private GameObject _healEffectPrefab;

    [SerializeField] List<DamageOverTimeEffect> _damageOverTimeEffects = new();
    public Action OnHealthModified;

    protected virtual void Start()
    {
        SetHealth(_maxHealth);
    }

    protected virtual void Update()
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

    public float GetHealth() => _currentHealth;
    public bool IsAlive() => _currentHealth > 0;

    public void Heal(float amount)
    {
        SetHealth(_currentHealth + amount);
        if (_healEffectPrefab != null) Instantiate(_healEffectPrefab, transform.position, Quaternion.identity);
    }

    public void TakeDamage(float damage)
    {
        if (damage >= _currentHealth)
        {
            Die();
        }
        else if (damage > 0)
        {
            SetHealth(_currentHealth - damage);
            if (_damageEffectPrefab != null) Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    public virtual void Die()
    {
        SetHealth(0);
        if (_deathEffectPrefab != null) Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void ApplyDamageOverTime(int damage, float duration, float interval, GameObject source)
    {
        var effect = new DamageOverTimeEffect(damage, duration, interval, source);
        _damageOverTimeEffects.Add(effect);
    }

    protected virtual void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
        OnHealthModified?.Invoke();
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