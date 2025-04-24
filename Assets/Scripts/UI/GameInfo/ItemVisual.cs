using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemVisual : ShowPopupOnHover
{
    [SerializeField] protected InventoryData _inventory;
    [SerializeField] protected ItemData _itemData;
    [SerializeField] protected int _itemCount;

    [Header("References")]
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected TMP_Text _countText;

    protected virtual void OnEnable()
    {
        _inventory.OnInventoryUpdated += UpdateCount;
        UpdateCount();
    }

    protected virtual void OnDisable()
    {
        _inventory.OnInventoryUpdated -= UpdateCount;
    }

    protected virtual void OnValidate()
    {
        if (_iconImage != null)
        {
            _iconImage.color = _itemData == null ? Color.white : _itemData.SpriteColor;
            _iconImage.sprite = _itemData == null ? null : _itemData.Sprite;
        }
        if (_countText == null) _countText = GetComponentInChildren<TMP_Text>();
        if (_countText != null) _countText.text = _itemCount.ToString();
    }

    protected virtual void UpdateData(ItemData data)
    {
        _itemData = data;
        _iconImage.sprite = _itemData.Sprite;
        _iconImage.color = _itemData.SpriteColor;
    }

    protected virtual void UpdateCount()
    {
        _itemCount = _inventory.GetInventoryCount(_itemData);
        _countText.text = _itemCount.ToString();
    }
}
