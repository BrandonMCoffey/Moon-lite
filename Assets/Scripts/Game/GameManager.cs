using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This class might be a static class and not a MonoBehaviour
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private static string SaveFilePrefix = "SaveGame";
    public static string GetSaveFile(int index) => SaveFilePrefix + index;

    public static JsonPlayerPrefs ActiveSaveGame { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public static void SetActiveSaveGame(string fileName)
    {
        ActiveSaveGame = new JsonPlayerPrefs(fileName);
    }

    public static void LoadGameScene()
    {
        // TODO: Load scene from ActiveSaveGame
        SceneManager.LoadScene(1);
        ActiveSaveGame.SetBool(GameSaveData.SaveStarted, true);
        ActiveSaveGame.SetString(GameSaveData.LastPlayed, GameSaveData.FormatLastPlayed());
        ActiveSaveGame.Save();
    }
}
