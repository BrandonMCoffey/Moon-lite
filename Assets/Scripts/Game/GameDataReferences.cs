using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Game Data References")]
public class GameDataReferences : ScriptableObject
{
    public List<ItemData> _allItems;
    public List<WeaponData> _allWeapons;

    public string GetItemName(ItemData item) => item.Name;
    public ItemData GetItemByName(string name)
    {
        foreach (var item in _allItems)
        {
            if (item.Name == name)
            {
                return item;
            }
        }
        Debug.LogError($"Item with name {name} not found.");
        return null;
    }

    public string GetWeaponName(WeaponData weapon) => weapon.Name;
    public WeaponData GetWeaponByName(string name)
    {
        foreach (var weapon in _allWeapons)
        {
            if (weapon.Name == name)
            {
                return weapon;
            }
        }
        Debug.LogError($"Weapon with name {name} not found.");
        return null;
    }

    [Button]
    private void ValidateData()
    {
        var items = AssetDatabase.FindAssets("t:ItemData");
        var weapons = AssetDatabase.FindAssets("t:WeaponData");
        _allItems = new List<ItemData>(items.Length);
        _allWeapons = new List<WeaponData>(weapons.Length);
        foreach (var item in items)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            var data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null)
            {
                _allItems.Add(data);
            }
        }
        foreach (var weapon in weapons)
        {
            var path = AssetDatabase.GUIDToAssetPath(weapon);
            var data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (data != null)
            {
                _allWeapons.Add(data);
            }
        }

        foreach (var item in _allItems)
        {
            foreach (var otherItem in _allItems)
            {
                if (item != otherItem && item.Name == otherItem.Name)
                {
                    Debug.LogError($"Duplicate item name found: {item.Name}");
                }
            }
        }
        foreach (var weapon in _allWeapons)
        {
            foreach (var otherWeapon in _allWeapons)
            {
                if (weapon != otherWeapon && weapon.Name == otherWeapon.Name)
                {
                    Debug.LogError($"Duplicate weapon name found: {weapon.Name}");
                }
            }
        }
    }
}
