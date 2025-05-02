using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooling : MonoBehaviour
{
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private int _startPoolSize = 100;
    [SerializeField, ReadOnly] private int _actualPoolSize;

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
        _actualPoolSize = _startPoolSize;
    }

    public static BulletBase SpawnBullet(Vector3 pos)
    {
        var bullet = _instance.GetItemFromPool();
        bullet.transform.SetPositionAndRotation(pos, Quaternion.identity);
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    private BulletBase GetItemFromPool()
    {
        if (_bulletPool.Count > 0)
        {
            var bullet = _bulletPool[0];
            _bulletPool.RemoveAt(0);
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            _actualPoolSize++;
            return Instantiate(_bulletPrefab, transform);
        }
    }

    public static void AddItemToPool(BulletBase bullet) => _instance.AddItemToPoolLocal(bullet);
    private void AddItemToPoolLocal(BulletBase bullet)
    {
        if (bullet == null || _bulletPool.Contains(bullet)) return;
        bullet.gameObject.SetActive(false);
        _bulletPool.Add(bullet);
    }
}
