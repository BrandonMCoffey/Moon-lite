using UnityEngine;
using UnityEngine.UI;

public class WeaponVisual : MonoBehaviour
{
    [SerializeField] protected WeaponData _weaponData;

    [Header("References")]
    [SerializeField] protected Image _iconImage;
    [SerializeField] private GameObject _selectedVisual;

    private void OnEnable()
    {
        PlayerWeaponManager.OnEquipWeapon += EquipWeapon;
        // TODO: Get equipped weapon (Singleton?)
    }

    private void OnDisable()
    {
        PlayerWeaponManager.OnEquipWeapon -= EquipWeapon;
    }

    private void OnValidate()
    {
        if (_iconImage != null) _iconImage.sprite = _weaponData.Sprite;
    }

    private void EquipWeapon(WeaponData equipped)
    {
        _selectedVisual.SetActive(_weaponData == equipped);
    }
}
