using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Inventory Data")]
public class InventoryData : SerializedScriptableObject
{
    public event System.Action OnInventoryUpdated = delegate { };

    [Header("Starting data")]
    [DictionaryDrawerSettings(KeyLabel = "Item Data", ValueLabel = "Count")]
    [SerializeField] private Dictionary<ItemData, float> _startingInventory = new Dictionary<ItemData, float>();
    [SerializeField] private List<WeaponData> _startingWeapons = new List<WeaponData>();
    [SerializeField] private List<MineralData> _startingAmmo = new List<MineralData>();

    [Header("Current Inventory")]
    [DictionaryDrawerSettings(KeyLabel = "Item Data", ValueLabel = "Count")]
    [SerializeField] private Dictionary<ItemData, float> _inventory = new Dictionary<ItemData, float>();
    [SerializeField] private List<WeaponData> _equippedWeapons = new List<WeaponData>();
    [SerializeField] private List<MineralData> _equippedAmmos = new List<MineralData>();

    public Dictionary<ItemData, float> Inventory => _inventory;
    public List<WeaponData> EquippedWeapons => _equippedWeapons;
    public List<MineralData> EquippedAmmos => _equippedAmmos;

    [Button]
    public void LoadPlayerInventory()
    {
        if (GameManager.Instance == null || GameManager.UseStartingInventory)
        {
            LoadStartingInventory();
            return;
        }
        _inventory.Clear();
        _equippedWeapons = new List<WeaponData>();
        _equippedAmmos = new List<MineralData>();

        GameManager.GetHeldItems().ForEach(itemslot => { _inventory.Add(itemslot.item, itemslot.count); });
        GameManager.GetEquippedWeapons().ForEach(weapon => { _equippedWeapons.Add(weapon); });
        GameManager.GetEquippedAmmo().ForEach(ammo => { _equippedAmmos.Add(ammo); });
        Debug.Log("Loaded saved player inventory");

        OnInventoryUpdated?.Invoke();
    }

    private void LoadStartingInventory()
    {
        _inventory = new Dictionary<ItemData, float>();
        _equippedWeapons = new List<WeaponData>();
        _equippedAmmos = new List<MineralData>();
        foreach (var item in _startingInventory)
        {
            AddItemToInventory(item.Key, (int)item.Value);
        }
        _startingWeapons.ForEach(weapon => { _equippedWeapons.Add(weapon); });
        _startingAmmo.ForEach(ammo => { _equippedAmmos.Add(ammo); });
        Debug.Log("Loaded starting inventory");

        OnInventoryUpdated?.Invoke();
    }

    [Button]
    public void SavePlayerInventory() => GameManager.SaveInventoryData(this);

    public float GetInventoryCount(ItemData item)
    {
        float count = 0;
        _inventory.TryGetValue(item, out count);
        return count;
    }

    public void AddItemToInventory(ItemData item, int count = 1)
    {
        if (_inventory.ContainsKey(item))
        {
            _inventory[item] += count;
        }
        else
        {
            _inventory.Add(item, count);
        }
        OnInventoryUpdated?.Invoke();
    }

    public void RemoveItemFromInventory(ItemData item, float count = 1)
    {
        if (_inventory.ContainsKey(item))
        {
            _inventory[item] -= count;
        }
    }

#if UNITY_EDITOR
    [Button]
    private void CheatGetAll()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(ItemData)}", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData scriptableObject = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (scriptableObject != null)
            {
                AddItemToInventory(scriptableObject, 500);
            }
        }
    }
#endif
}

public struct InventorySlot
{
    public ItemData item;
    public float count;
    public InventorySlot(ItemData item, float count)
    {
        this.item = item;
        this.count = count;
    }
}