using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooling : MonoBehaviour
{
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private int _startBulletPoolSize = 100;
    [SerializeField, ReadOnly] private int _bulletPoolSize;
    [SerializeField] private LineRenderer _laserVisualPrefab;
    [SerializeField] private int _startLaserPoolSize = 5;
    [SerializeField, ReadOnly] private int _laserPoolSize;
    [SerializeField] private ParticleSystem _hitParticles;

    private static BulletPooling _instance;
    private List<BulletBase> _bulletPool;
    private List<LineRenderer> _laserPool;

    private Transform _bulletParent;
    private Transform _laserParent;

    private void Awake()
    {
        _instance = this;
        _bulletParent = new GameObject("Bullets").transform;
        _bulletParent.SetParent(transform, false);
        _bulletPool = new List<BulletBase>(_startBulletPoolSize);
        for (int i = 0; i < _startBulletPoolSize; i++)
        {
            var bullet = Instantiate(_bulletPrefab, _bulletParent);
            bullet.gameObject.SetActive(false);
            _bulletPool.Add(bullet);
        }
        _bulletPoolSize = _startBulletPoolSize;

        _laserParent = new GameObject("Lasers").transform;
        _laserParent.SetParent(transform, false);
        _laserPool = new List<LineRenderer>();
        for (int i = 0; i < _startLaserPoolSize; i++)
        {
            var laser = Instantiate(_laserVisualPrefab, _laserParent);
            laser.gameObject.SetActive(false);
            _laserPool.Add(laser);
        }
        _laserPoolSize = _startLaserPoolSize;
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

    private LineRenderer GetLaserFromPool()
    {
        if (_laserPool.Count > 0)
        {
            var laser = _laserPool[0];
            _laserPool.RemoveAt(0);
            laser.gameObject.SetActive(true);
            return laser;
        }
        else
        {
            _laserPoolSize++;
            return Instantiate(_laserVisualPrefab, transform);
        }
    }

    public static void CreateLaserVisual(Vector3 start, Vector3 end, Color color, float bulletSize) => _instance.CreateLaserVisualLocal(start, end, color, bulletSize);
    private void CreateLaserVisualLocal(Vector3 start, Vector3 end, Color color, float bulletSize)
    {
        var laser = GetLaserFromPool();

        // Configure the LineRenderer
        laser.positionCount = 2;
        laser.SetPosition(0, start);
        laser.SetPosition(1, end);
        laser.startColor = color;
        laser.endColor = color;
        laser.startWidth = 0.05f * bulletSize;
        laser.endWidth = 0.05f * bulletSize;
        //laser.material = new Material(Shader.Find("Sprites/Default")); // Use a basic material

        // Start fading the laser
        StartCoroutine(FadeLaser(laser, 0.33f));
    }

    private IEnumerator FadeLaser(LineRenderer laser, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = laser.startColor;
        Color endColor = new Color(0, 0, 0, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            laser.startColor = currentColor;
            laser.endColor = currentColor;

            yield return null;
        }

        laser.gameObject.SetActive(false);
        _laserPool.Add(laser);
    }

    public static void BulletHitFeedback(Color color, Vector3 pos, Vector3 forward) => _instance.BulletHitFeedbacklocal(color, pos, forward);
    private void BulletHitFeedbacklocal(Color color, Vector3 pos, Vector3 forward)
    {
        if (_hitParticles != null)
        {
            var main = _hitParticles.main;
            main.startColor = color;
            _hitParticles.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward));
            _hitParticles.Emit(10);
        }
    }
}
