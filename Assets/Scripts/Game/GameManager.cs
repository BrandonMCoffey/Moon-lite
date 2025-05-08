using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private GameDataReferences _gameDataReferences;
    [SerializeField] private CurrentScene _currentScene = CurrentScene.MainMenu;
    [SerializeField] private JsonPlayerPrefs _activeSaveGame;
    private static JsonPlayerPrefs ActiveSaveGame => Instance._activeSaveGame;

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

    public static void LoadShipScene()
    {
        GameSaveData.UpdateLastPlayed(ActiveSaveGame);

        if (!GameSaveData.GetSaveStarted(ActiveSaveGame))
        {
            SceneManager.LoadScene("IntroCutscene");
            GameSaveData.SetSaveStarted(ActiveSaveGame, true);
        }
        else
        {
            SceneManager.LoadScene("ShipHub");
        }
        ActiveSaveGame.Save();
    }

    public static void LoadPlanetScene()
    {
        SceneManager.LoadScene("Planet");
    }

    public static void OnPlayerDeath()
    {
        if (Instance._currentScene == CurrentScene.Planet)
        {
            SceneManager.LoadScene("ShipHub");
            GameSaveData.IncrementPlanetsExplored(ActiveSaveGame);
            GameSaveData.UpdateLastPlayed(ActiveSaveGame);
            ActiveSaveGame.Save();
        }
        else
        {
            Debug.LogWarning("Died outside of planet scene?!");
            SceneManager.LoadScene("ShipHub");
        }
    }

    public static bool UseStartingInventory => ActiveSaveGame == null || !GameSaveData.HasInventoryData(ActiveSaveGame);
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
        ActiveSaveGame.Save();
    }

    [Button]
    private void SaveActive()
    {
        _activeSaveGame.Save();
    }
}

public enum CurrentScene
{
    MainMenu,
    IntroCutscene,
    ShipHub,
    Planet,
}