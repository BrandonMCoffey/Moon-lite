using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemManager : MonoBehaviour
{
    [SerializeField] private DroppedItem _itemPrefab;
    [SerializeField] private int _itemPoolSize;

    private List<DroppedItem> _itemPool;
    private List<DroppedItem> _activeItems;

    private void Awake()
    {
        _itemPool = new List<DroppedItem>(_itemPoolSize);
        for (int i = 0; i < _itemPoolSize; i++)
        {
            _itemPool.Add(Instantiate(_itemPrefab, transform));
        }
    }

    public void NewDroppedItem(Vector3 location, ItemData itemData)
    {
        // TODO: Get Item from Pool and set data
        var item = GetItemFromPool();
        item.InitItem(location, itemData);
    }

    private DroppedItem GetItemFromPool()
    {
        //if (_itemPool)
        return _itemPool[0];
    }
}
