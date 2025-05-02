using Sirenix.OdinInspector;
using System.Collections;
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

    public static void CreateLaserVisual(Vector3 start, Vector3 end, Color color, float bulletSize) => _instance.CreateLaserVisualLocal(start, end, color, bulletSize);
    private void CreateLaserVisualLocal(Vector3 start, Vector3 end, Color color, float bulletSize)
    {
        GameObject laser = new GameObject("LaserRay");
        LineRenderer lineRenderer = laser.AddComponent<LineRenderer>();

        // Configure the LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.05f * bulletSize;
        lineRenderer.endWidth = 0.05f * bulletSize;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a basic material

        // Start fading the laser
        StartCoroutine(FadeLaser(laser, lineRenderer, 0.33f));
    }

    private IEnumerator FadeLaser(GameObject laser, LineRenderer lineRenderer, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = new Color(0, 0, 0, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            yield return null;
        }

        Destroy(laser);
    }
}
