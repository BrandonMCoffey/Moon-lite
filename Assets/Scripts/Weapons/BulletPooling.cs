using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooling : MonoBehaviour
{
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private int _startPoolSize = 100;
    [SerializeField, ReadOnly] private int _bulletPoolSize;

    private static BulletPooling _instance;
    private List<BulletBase> _bulletPool;

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
    }

    public static BulletBase SpawnBullet(Vector3 pos) => _instance.GetBulletFromPool(pos);
    private BulletBase GetBulletFromPool(Vector3 pos)
    {
        if (_bulletPool.Count > 0)
        {
            var bullet = _bulletPool[0];
            _bulletPool.RemoveAt(0);
            bullet.transform.SetPositionAndRotation(pos, Quaternion.identity);
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

    public static void AddBulletToPool(BulletBase bullet) => _instance.AddBulletToPoolLocal(bullet);
    private void AddBulletToPoolLocal(BulletBase bullet)
    {
        if (bullet == null || _bulletPool.Contains(bullet)) return;
        bullet.gameObject.SetActive(false);
        _bulletPool.Add(bullet);
    }
}
