using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DroppedItemManager : MonoBehaviour
{
    [SerializeField] private DroppedItem _itemPrefab;
    [SerializeField] private int _startPoolSize = 100;
    [SerializeField, ReadOnly] private int _actualPoolSize;

    private static DroppedItemManager _instance;

    private List<DroppedItem> _itemPool;

    private void Awake()
    {
        _instance = this;
        _itemPool = new List<DroppedItem>(_startPoolSize);
        for (int i = 0; i < _startPoolSize; i++)
        {
            var item = Instantiate(_itemPrefab, transform);
            item.gameObject.SetActive(false);
            _itemPool.Add(item);
        }
        _actualPoolSize = _startPoolSize;
    }

    public void SpawnDroppedItem(Vector3 location, ItemData itemData)
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
            item.gameObject.SetActive(true);
            return item;
        }
        else
        {
            _actualPoolSize++;
            return Instantiate(_itemPrefab, transform);
        }
    }

    public static void AddItemToPool(DroppedItem item) => _instance.AddItemToPoolLocal(item);
    private void AddItemToPoolLocal(DroppedItem item)
    {
        if (item == null || _itemPool.Contains(item)) return;
        item.gameObject.SetActive(false);
        _itemPool.Add(item);
    }
}
