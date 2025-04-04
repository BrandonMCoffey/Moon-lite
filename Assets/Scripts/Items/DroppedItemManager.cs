using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemManager : MonoBehaviour
{
    [SerializeField] private DroppedItem _itemPrefab;
    [SerializeField] private int _itemPoolSize;

    private List<DroppedItem> _itemPool;

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
        var item = GetItemFromPool();
        item.InitItem(location, itemData);
    }

    private DroppedItem GetItemFromPool()
    {
        if (_itemPool.Count > 0)
        {
            var item = _itemPool[0];
            _itemPool.RemoveAt(0);
            return item;
        }
        else
        {
            return Instantiate(_itemPrefab, transform);
        }
    }
}
