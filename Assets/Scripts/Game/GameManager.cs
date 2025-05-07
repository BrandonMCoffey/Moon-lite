using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This class might be a static class and not a MonoBehaviour
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private static string SaveFilePrefix = "SaveGame";
    public static string GetSaveFile(int index) => SaveFilePrefix + index;

    private static JsonPlayerPrefs ActiveSaveGame => Instance._activeSaveGame;
    [SerializeField] private JsonPlayerPrefs _activeSaveGame;

    [SerializeField] private GameDataReferences _gameDataReferences;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void SetActiveSaveGame(string fileName)
    {
        Instance._activeSaveGame = new JsonPlayerPrefs(fileName);
    }

    public static void LoadGameScene()
    {
        // TODO: Load scene from ActiveSaveGame
        // If save not started, load an intro cutscene instead of ship hub?)
        SceneManager.LoadScene(1);
        ActiveSaveGame.SetBool(GameSaveData.SaveStarted, true);
        ActiveSaveGame.SetString(GameSaveData.LastPlayed, GameSaveData.FormatLastPlayed());
        Instance.SaveActive();
    }

    public static bool UseStartingInventory => ActiveSaveGame == null || !ActiveSaveGame.HasKey(GameSaveData.EquippedWeapons) || !ActiveSaveGame.HasKey(GameSaveData.EquippedAmmo);
    public static List<InventorySlot> GetHeldItems()
    {
        var heldItems = new List<InventorySlot>();
        foreach (var item in Instance._gameDataReferences.AllItems)
        {
            var count = ActiveSaveGame.GetFloat(GameSaveData.InventoryPrefix + item.Name, 0f);
            if (count > 0) heldItems.Add(new InventorySlot(item, count));
        }
        return heldItems;
    }
    public static List<WeaponData> GetEquippedWeapons() => Instance._gameDataReferences.GetWeaponsByName(ActiveSaveGame.GetStringArray(GameSaveData.EquippedWeapons).ToList());
    public static List<MineralData> GetEquippedAmmo() => Instance._gameDataReferences.GetAmmoByNames(ActiveSaveGame.GetStringArray(GameSaveData.EquippedAmmo).ToList());
    public static void SaveInventoryData(InventoryData inventory)
    {
        ActiveSaveGame.DeleteKeysStartingWith(GameSaveData.InventoryPrefix);
        foreach (var kvp in inventory.Inventory)
        {
            if (kvp.Value > 0) ActiveSaveGame.SetFloat(GameSaveData.InventoryPrefix + kvp.Key.Name, kvp.Value);
        }
        ActiveSaveGame.SetStringArray(GameSaveData.EquippedWeapons, inventory.EquippedWeapons.ConvertAll(weapon => weapon.Name).ToArray());
        ActiveSaveGame.SetStringArray(GameSaveData.EquippedAmmo, inventory.EquippedAmmos.ConvertAll(ammo => ammo.Name).ToArray());
        Instance.SaveActive();
    }

    [Button]
    private void SaveActive()
    {
        _activeSaveGame.Save();
    }
}
