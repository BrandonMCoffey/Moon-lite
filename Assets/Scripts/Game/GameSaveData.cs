public static class GameSaveData
{
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

    // Unlocks

    // Inventory
    public const string CurrentMoney = "Current Money";
    public const string InventoryPrefix = "Inventory_";

    // Equipment for Planets
    public const string EquippedWeapons = "Equipped Weapons";
    public const string EquippedAmmo = "Equipped Ammo";

    public static string FormatLastPlayed()
    {
        var dt = System.DateTime.Now;
        return dt.ToString("dd MMMM, yyyy h:mm tt");
    }
}
