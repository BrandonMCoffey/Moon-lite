using UnityEngine;
using UnityEngine.UI;

public class WeaponVisual : ShowPopupOnHover
{
    [SerializeField] protected WeaponData _weaponData;

    [Header("References")]
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected Image _effectImage;
    [SerializeField] private GameObject _selectedVisual;

    private void OnEnable()
    {
        PlayerWeaponManager.OnEquipWeapon += EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo += EquipAmmo;
        // TODO: Get equipped weapon (Singleton?)
    }

    private void OnDisable()
    {
        PlayerWeaponManager.OnEquipWeapon -= EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo -= EquipAmmo;
    }

    private void OnValidate()
    {
        if (_iconImage != null) _iconImage.sprite = _weaponData.Sprite;
    }

    private void EquipWeapon(WeaponData equipped)
    {
        _selectedVisual.SetActive(_weaponData == equipped);
    }

    private void EquipAmmo(MineralData equipped)
    {
        if (equipped.AmmoSpriteEffect != null)
        {
            _effectImage.sprite = equipped.AmmoSpriteEffect;
            _effectImage.gameObject.SetActive(true);
        }
        else
        {
            _effectImage.gameObject.SetActive(false);
        }
    }
}
