using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Mineral Data")]
public class MineralData : ItemData
{
    [Header("Bullet Data")]
    [SerializeField] private float _shotSpeed = 5;
    [SerializeField] private float _damage = 1;
    [SerializeField] private float _knockback = 3;
    [SerializeField] private float _size = 1;
    [SerializeField] private float _lifetime = 3;
    [SerializeField] private float _gravityScale = 0f;
    [SerializeField] private bool _hasAOE = false;
    [SerializeField] private float _aoeRadius = 1f;

    public float ShotSpeed => _shotSpeed;
    public float Damage => _damage;
    public float Knockback => _knockback;
    public float Size => _size;
    public float Lifetime => _lifetime;
    public float GravityScale => _gravityScale;
    public bool HasAOE => _hasAOE;
    public float AoeRadius => _aoeRadius;
}
