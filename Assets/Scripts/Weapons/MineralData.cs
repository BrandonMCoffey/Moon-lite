using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Mineral Data")]
public class MineralData : ItemData
{
    [Header("Bullet Data")]
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private Sprite _bulletSpriteLaser;
    [SerializeField] private Sprite _bulletSpriteShotgun;
    [SerializeField] private Sprite _bulletSpriteBullet;
    [SerializeField] private Sprite _bulletSpriteEffect;
    [Tooltip("Modifies the speed of the bullet from the gun by multiplying")]
    [SerializeField, Range(0.01f, 5f)] private float _bulletSpeedModifier = 1f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _knockback = 3f;
    [SerializeField] private float _size = 0.1f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private bool _hasAOE = false;
    [SerializeField, ShowIf(nameof(_hasAOE))] private float _aoeRadius = 1f;

    public BulletBase BulletPrefab => _bulletPrefab;
    public Sprite BulletSprite => _bulletSpriteBullet;
    public float ShotSpeed => _bulletSpeedModifier;
    public float Damage => _damage;
    public float Knockback => _knockback;
    public float Size => _size;
    public float Lifetime => _lifetime;
    public float GravityScale => _gravityScale;
    public bool HasAOE => _hasAOE;
    public float AoeRadius => _aoeRadius;
}
