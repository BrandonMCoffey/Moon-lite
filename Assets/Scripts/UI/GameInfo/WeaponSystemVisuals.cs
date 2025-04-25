using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystemVisuals : MonoBehaviour
{
    [SerializeField] private List<WeaponVisual> _weaponVisuals = new List<WeaponVisual>();
    [SerializeField] private List<AmmoVisual> _ammoVisuals = new List<AmmoVisual>();

    private PlayerWeaponManager _weaponManager;
    public PlayerWeaponManager WeaponManager
    {
        get
        {
            if (_weaponManager != null) return _weaponManager;
            _weaponManager = Player.Instance.GetComponent<PlayerWeaponManager>();
            return _weaponManager;
        }
    }

    private void Start()
    {
        UpdateAllVisuals();
    }

    private void OnEnable()
    {
        PlayerWeaponManager.OnEquipWeapon += EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo += EquipAmmo;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.OnEquipWeapon -= EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo -= EquipAmmo;
    }

    private void EquipWeapon(WeaponData weapon)
    {
        UpdateAllVisuals();
        foreach (var visual in _weaponVisuals)
        {
            visual.SetSelected(weapon == visual.WeaponData);
        }
        foreach (var visual in _ammoVisuals)
        {
            visual.EquipWeapon(weapon);
        }
    }

    private void EquipAmmo(MineralData ammo)
    {
        foreach (var visual in _weaponVisuals)
        {
            visual.EquipAmmo(ammo);
        }
        foreach (var visual in _ammoVisuals)
        {
            visual.SetSelected(ammo == visual.MineralData);
        }
    }

    private void UpdateAllVisuals()
    {
        int i = 0;
        for (; i < WeaponManager.Weapons.Count; i++)
        {
            _weaponVisuals[i].UpdateData(_weaponManager.Weapons[i].Data);
            _weaponVisuals[i].SetSelected(i == _weaponManager.EquippedWeaponIndex);
            _weaponVisuals[i].gameObject.SetActive(true);
        }
        for (; i < _weaponVisuals.Count; i++)
        {
            _weaponVisuals[i].gameObject.SetActive(false);
        }
        i = 0;
        for (; i < _weaponManager.Ammos.Count; i++)
        {
            _ammoVisuals[i].UpdateData(_weaponManager.Ammos[i]);
            _ammoVisuals[i].SetSelected(i == _weaponManager.EquippedAmmoIndex);
            _ammoVisuals[i].gameObject.SetActive(true);
        }
        for (; i < _ammoVisuals.Count; i++)
        {
            _ammoVisuals[i].gameObject.SetActive(false);
        }
    }
}
