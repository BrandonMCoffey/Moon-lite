using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoVisual : ShowPopupOnHover
{
    [SerializeField] private InventoryData _inventory;
    [SerializeField] private MineralData _mineralData;
    [SerializeField] private WeaponAmmoArtType _ammoArtType;
    [SerializeField] private float _itemCount;

    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private GameObject _selectedVisual;

    public MineralData MineralData => _mineralData;

    private void OnEnable()
    {
        _inventory.OnInventoryUpdated += UpdateCount;
        UpdateCount();
    }

    private void OnDisable()
    {
        _inventory.OnInventoryUpdated -= UpdateCount;
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

    public void SetSelected(bool selected) => _selectedVisual.SetActive(selected);

    public void UpdateData(MineralData data)
    {
        _mineralData = data;
        _iconImage.sprite = _mineralData.GetAmmoSprite(_ammoArtType);
        UpdateCount();
    }

    private void UpdateCount()
    {
        _itemCount = _inventory.GetInventoryCount(_mineralData);
        _countText.text = _itemCount.ToString();
    }

    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        _ammoArtType = weapon.ArtType;
        _iconImage.sprite = _mineralData.GetAmmoSprite(_ammoArtType);
    }

    public override string GetHoverText()
    {
        return _mineralData.Name + "\n" + _mineralData.Description;
    }

    public override void ClickOnVisual()
    {
        Player.WeaponManager.SwitchToAmmo(_mineralData);
    }
}
