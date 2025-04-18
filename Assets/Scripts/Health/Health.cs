using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    [SerializeField] private GameObject _deathEffectPrefab; 
    [SerializeField] private GameObject _damageEffectPrefab;
    [SerializeField] private GameObject _healEffectPrefab;

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
}