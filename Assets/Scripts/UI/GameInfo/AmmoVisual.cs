using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoVisual : ShowPopupOnHover
{
    [SerializeField] private InventoryData _inventory;
    [SerializeField] private MineralData _mineralData;
    [SerializeField] private WeaponAmmoArtType _ammoArtType;
    [SerializeField] private int _itemCount;

    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private GameObject _selectedVisual;

    private void OnEnable()
    {
        _inventory.OnInventoryUpdated += UpdateCount;
        PlayerWeaponManager.OnEquipWeapon += EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo += EquipAmmo;
        UpdateCount();
        // TODO: Get equipped ammo (Singleton?)
    }

    private void OnDisable()
    {
        _inventory.OnInventoryUpdated -= UpdateCount;
        PlayerWeaponManager.OnEquipWeapon -= EquipWeapon;
        PlayerWeaponManager.OnEquipAmmo -= EquipAmmo;
    }

    private void OnValidate()
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = _mineralData == null ? null : _mineralData.GetAmmoSprite(_ammoArtType);
        }
        if (_countText == null) _countText = GetComponentInChildren<TMP_Text>();
        if (_countText != null) _countText.text = _itemCount.ToString();
    }

    private void UpdateData(MineralData data)
    {
        _mineralData = data;
        _iconImage.sprite = _mineralData.GetAmmoSprite(_ammoArtType);
    }

    private void UpdateCount()
    {
        _itemCount = _inventory.GetInventoryCount(_mineralData);
        _countText.text = _itemCount.ToString();
    }

    private void EquipWeapon(WeaponData weapon)
    {
        _ammoArtType = weapon.ArtType;
        _iconImage.sprite = _mineralData.GetAmmoSprite(_ammoArtType);
    }

    private void EquipAmmo(MineralData equipped)
    {
        _selectedVisual.SetActive(_mineralData == equipped);
    }
}
