using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooling : MonoBehaviour
{
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private EnemyBulletBase _enemyBulletPrefab;
    [SerializeField] private int _startPoolSize = 100;
    [SerializeField, ReadOnly] private int _bulletPoolSize;
    [SerializeField, ReadOnly] private int _enemyBulletPoolSize;

    private static BulletPooling _instance;
    private List<BulletBase> _bulletPool;
    private List<EnemyBulletBase> _enemyBulletPool;

    private void Awake()
    {
        _instance = this;

        _bulletPool = new List<BulletBase>(_startPoolSize);
        for (int i = 0; i < _startPoolSize; i++)
        {
            var bullet = Instantiate(_bulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            _bulletPool.Add(bullet);
        }
        _bulletPoolSize = _startPoolSize;

        _enemyBulletPool = new List<EnemyBulletBase>(_startPoolSize);
        for (int i = 0; i < _startPoolSize; i++)
        {
            var bullet = Instantiate(_enemyBulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            _enemyBulletPool.Add(bullet);
        }
        _enemyBulletPoolSize = _startPoolSize;
    }

    public static BulletBase SpawnBullet(Vector3 pos) => _instance.GetBulletFromPool(pos);
    private BulletBase GetBulletFromPool(Vector3 pos)
    {
        if (_bulletPool.Count > 0)
        {
            var bullet = _bulletPool[0];
            _bulletPool.RemoveAt(0);
            bullet.ResetBullet();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            _bulletPoolSize++;
            return Instantiate(_bulletPrefab, pos, Quaternion.identity, transform);
        }
    }

    public static EnemyBulletBase SpawnEnemyBullet(Vector3 pos) => _instance.GetEnemyBulletFromPool(pos);
    private EnemyBulletBase GetEnemyBulletFromPool(Vector3 pos)
    {
        if (_enemyBulletPool.Count > 0)
        {
            var bullet = _enemyBulletPool[0];
            _enemyBulletPool.RemoveAt(0);
            bullet.ResetBullet();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            _enemyBulletPoolSize++;
            return Instantiate(_enemyBulletPrefab, pos, Quaternion.identity, transform);
        }
    }

    public static void AddBulletToPool(BulletBase bullet) => _instance.AddBulletToPoolLocal(bullet);
    private void AddBulletToPoolLocal(BulletBase bullet)
    {
        if (bullet == null || _bulletPool.Contains(bullet)) return;
        bullet.gameObject.SetActive(false);
        bullet.ResetBullet();
        _bulletPool.Add(bullet);
    }

    public static void AddEnemyBulletToPool(EnemyBulletBase bullet) => _instance.AddEnemyBulletToPoolLocal(bullet);
    private void AddEnemyBulletToPoolLocal(EnemyBulletBase bullet)
    {
        if (bullet == null || _enemyBulletPool.Contains(bullet)) return;
        bullet.gameObject.SetActive(false);
        bullet.ResetBullet();
        _enemyBulletPool.Add(bullet);
    }
}
