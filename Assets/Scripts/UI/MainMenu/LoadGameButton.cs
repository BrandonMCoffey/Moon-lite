using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
    public static event System.Action OnPlayGame = delegate { };

    [SerializeField] private GameObject _hasPlayed;
    [SerializeField] private GameObject _hasNotPlayed;
    [SerializeField] private TMP_Text _saveName;
    [SerializeField] private TMP_Text _planetsExplored;
    [SerializeField] private TMP_Text _money;
    [SerializeField] private TMP_Text _lastPlayed;

    private JsonPlayerPrefs _loadData;

    public void LinkGameSave(string loadName)
    {
        _loadData = new JsonPlayerPrefs(loadName);
        SetupVisuals();
    }

    public void PlayGame()
    {
        GameManager.SetActiveSaveGame(_loadData.FileName);
        OnPlayGame?.Invoke();
    }

    public void DeleteSaveData()
    {
        _loadData.DeleteAll();
        _loadData.Save();
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        bool started = _loadData.GetBool(GameSaveData.SaveStarted, false);

        _hasPlayed.SetActive(started);
        _hasNotPlayed.SetActive(!started);
        if (_hasPlayed)
        {
            string saveName = _loadData.GetString(GameSaveData.SaveName, "New Game");
            _saveName.text = saveName;

            int planetsExplored = _loadData.GetInt(GameSaveData.PlanetsExplored, 0);
            _planetsExplored.text = planetsExplored.ToString();

            int money = _loadData.GetInt(GameSaveData.CurrentMoney, 0);
            _money.text = money.ToString();

            string lastPlayed = _loadData.GetString(GameSaveData.LastPlayed, "Never");
            _lastPlayed.text = lastPlayed.ToString();
        }
    }
}
