using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string _itemName;
    [SerializeField, TextArea] private string _itemDescription;
    [SerializeField] private Sprite _itemIcon;

    public string Name => _itemName;
    public string Description => _itemDescription;
    public Sprite Sprite => _itemIcon;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(_itemName)) _itemName = name;
    }
}
