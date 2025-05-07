using UnityEngine;
using UnityEngine.UI;

public class WeaponVisual : ShowPopupOnHover
{
    [SerializeField] protected WeaponData _weaponData;

    [Header("References")]
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected Image _effectImage;
    [SerializeField] private GameObject _selectedVisual;

    public WeaponData WeaponData => _weaponData;

    private void OnValidate()
    {
        if (_iconImage != null) _iconImage.sprite = _weaponData.Sprite;
    }

    public void UpdateData(WeaponData weapon)
    {
        _weaponData = weapon;
        _iconImage.sprite = _weaponData.Sprite;
    }

    public void SetSelected(bool selected) => _selectedVisual.SetActive(selected);

    public void EquipAmmo(MineralData equipped)
    {
        if (equipped != null && equipped.AmmoSpriteEffect != null)
        {
            _effectImage.sprite = equipped.AmmoSpriteEffect;
            _effectImage.gameObject.SetActive(true);
        }
        else
        {
            _effectImage.gameObject.SetActive(false);
        }
    }

    public override string GetHoverText()
    {
        return _weaponData.Name + "\n" + _weaponData.Description;
    }

    // TODO: Use button UI instead
    public override void ClickOnVisual()
    {
        Player.WeaponManager.SwitchToWeapon(_weaponData);
    }
}
