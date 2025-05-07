using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Research Unlockable Data")]
public class ResearchUnlockables : SerializedScriptableObject
{
    [Header("Weapons")]
    [DictionaryDrawerSettings(KeyLabel = "Weapon", ValueLabel = "Unlock Requirements")]
    [SerializeField] private Dictionary<WeaponData, UnlockRequirements> _weaponUnlockables = new Dictionary<WeaponData, UnlockRequirements>();

    [Header("Ammo")]
    [DictionaryDrawerSettings(KeyLabel = "Ammo", ValueLabel = "Unlock Requirements")]
    [SerializeField] private Dictionary<MineralData, UnlockRequirements> _ammoUnlockables = new Dictionary<MineralData, UnlockRequirements>();

    [Header("Currently Unlocked")]
    [SerializeField] private List<WeaponData> _unlockedWeapons = new List<WeaponData>();
}

public struct UnlockRequirements
{
    public ItemData _item;
    public int _itemCount;
    public int _days; // One day is one trip to planet?

    public UnlockRequirements(ItemData item, int itemCount = 50, int days = 1)
    {
        _item = item;
        _itemCount = itemCount;
        _days = days;
    }
}