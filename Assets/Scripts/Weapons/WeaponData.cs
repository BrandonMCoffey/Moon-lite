using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string _weaponName;
    [SerializeField, TextArea] private string _weaponDescription;
    [SerializeField] private Sprite _weaponSprite;

    [Header("Reload and Ammo Properties")]
    [SerializeField] protected int _maxMagSize = 10; // Ammo capacity of the magazine.
    [SerializeField] protected float _reloadTime = 2f; // Time it takes to reload.
    [SerializeField] protected bool _isAutomatic = false;
    [SerializeField] protected int _ammoPerShot = 1; // Amount of ammo consumed per shot

    [Header("Fire Settings")]
    [SerializeField] protected float _fireRate = 4f; // Default to 4 bullets per second.
    [SerializeField] protected float _bulletSpeed = 20f;
    [SerializeField] protected float _sprayAngle = 0f; // Maximum spray angle in degrees
    [SerializeField] protected Vector2 _recoilForce = Vector2.zero;

    [Header("Bullet Modifiers (Multiply)")] // Multiplied on to bullet settings
    [SerializeField] protected float _bulletSize = 1f;
    [SerializeField] protected float _bulletDamage = 1f;
    [SerializeField] protected float _bulletKnockback = 1f;
    [SerializeField] protected float _bulletLifetime = 1f;
    [SerializeField] protected float _bulletGravityScale = 1f;
    [SerializeField] protected float _aoeRadius = 1f;

    public string Name => _weaponName;
    public string Description => _weaponDescription;
    public Sprite Sprite => _weaponSprite;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(_weaponName)) _weaponName = name;
    }
}
