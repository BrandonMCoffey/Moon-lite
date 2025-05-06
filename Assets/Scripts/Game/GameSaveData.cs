public static class GameSaveData
{
    public const string SaveStarted = "Save Started";
    public const string SaveName = "Save Name";
    public const string LastPlayed = "Last Played";
    public const string PlanetsExplored = "Planets Explored";
    public const string CurrentMoney = "Current Money";

    public static string FormatLastPlayed()
    {
        var dt = System.DateTime.Now;
        return dt.ToString("dd MMMM, yyyy h:mm tt");
    }
}
