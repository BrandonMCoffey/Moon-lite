using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Game Data References")]
public class GameDataReferences : ScriptableObject
{
    [SerializeField] private List<ItemData> _allItems = new List<ItemData>();
    [SerializeField] private List<WeaponData> _allWeapons = new List<WeaponData>();

    public List<ItemData> AllItems => _allItems;
    public List<WeaponData> AllWeapons => _allWeapons;

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

    public List<WeaponData> GetWeaponsByName(List<string> names)
    {
        var weapons = new List<WeaponData>();
        if (names == null) return weapons;
        foreach (var name in names)
        {
            var weapon = GetWeaponByName(name);
            if (weapon != null)
            {
                weapons.Add(weapon);
            }
        }
        return weapons;
    }

    public List<MineralData> GetAmmoByNames(List<string> names)
    {
        var ammo = new List<MineralData>();
        if (names == null) return ammo;
        foreach (var name in names)
        {
            var mineral = GetItemByName(name) as MineralData;
            if (mineral != null)
            {
                ammo.Add(mineral);
            }
        }
        return ammo;
    }

#if UNITY_EDITOR
    [Button]
    private void ValidateData()
    {
        var items = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        var weapons = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
        _allItems = new List<ItemData>(items.Length);
        _allWeapons = new List<WeaponData>(weapons.Length);
        foreach (var item in items)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(item);
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null)
            {
                _allItems.Add(data);
            }
        }
        foreach (var weapon in weapons)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(weapon);
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
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
#endif
}