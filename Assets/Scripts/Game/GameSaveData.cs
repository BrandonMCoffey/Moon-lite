public static class GameSaveData
{
    private const string SaveFilePrefix = "SaveGame";
    public static string GetSaveFile(int index) => SaveFilePrefix + index;

    // Save Data
    public const string SaveStarted = "Save Started";
    public const string SaveName = "Save Name";
    public const string LastPlayed = "Last Played";
    public static bool GetSaveStarted(JsonPlayerPrefs save) => save.GetBool(SaveStarted, false);
    public static void SetSaveStarted(JsonPlayerPrefs save, bool started) => save.GetBool(SaveStarted, started);
    public static string GetLastPlayed(JsonPlayerPrefs save) => save.GetString(LastPlayed, "Never");
    public static void UpdateLastPlayed(JsonPlayerPrefs save) => save.SetString(LastPlayed, FormatLastPlayed());

    // Game Stats
    public const string PlanetsExplored = "Planets Explored";
    public static void IncrementPlanetsExplored(JsonPlayerPrefs save) => save.SetInt(PlanetsExplored, save.GetInt(PlanetsExplored, 0) + 1);

    // Unlocks
    public const string OxygenTankLevel = "Oxygen Tank Level";
    public static int GetOxygenTankLevel(JsonPlayerPrefs save) => save.GetInt(OxygenTankLevel, 0);
    public static void SetOxygenTankLevel(JsonPlayerPrefs save, int level) => save.SetInt(OxygenTankLevel, level);

    // Inventory
    public const string CurrentMoney = "Current Money";
    public const string InventoryPrefix = "Inventory_";
    public static bool HasInventoryData(JsonPlayerPrefs save) => save.HasKey(EquippedWeapons) || save.HasKey(EquippedAmmo);

    // Equipment for Planets
    public const string EquippedWeapons = "Equipped Weapons";
    public const string EquippedAmmo = "Equipped Ammo";

    private static string FormatLastPlayed()
    {
        var dt = System.DateTime.Now;
        return dt.ToString("dd MMMM, yyyy h:mm tt");
    }
}
