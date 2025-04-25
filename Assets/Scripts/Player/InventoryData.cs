using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Inventory Data")]
public class InventoryData : SerializedScriptableObject
{
    public event System.Action OnInventoryUpdated = delegate { };

    // TODO: This stores the weapons and ammo that the player starts with
    [SerializeField] private List<WeaponData> _startWeapons = new List<WeaponData>();
    [SerializeField] private List<MineralData> _equippedAmmos = new List<MineralData>();

    public List<WeaponData> StartWeapons => _startWeapons;
    public List<MineralData> EquippedAmmos => _equippedAmmos;

    [DictionaryDrawerSettings(KeyLabel = "Item Data", ValueLabel = "Count")]
    [SerializeField] private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();

    public int GetInventoryCount(ItemData item)
    {
        int count = 0;
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

    public void RemoveItemFromInventory(ItemData item, int count = 1)
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
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(ItemData)}", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemData scriptableObject = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (scriptableObject != null)
            {
                AddItemToInventory(scriptableObject, 500);
            }
        }
    }
#endif
}
