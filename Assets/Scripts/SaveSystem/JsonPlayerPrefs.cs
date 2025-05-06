using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class JsonPlayerPrefs
{
    // EXAMPLE USAGE:
    // JsonPlayerPrefs prefs = new JsonPlayerPrefs("Preferences");
    // prefs.SetInt("testKey", 18);
    // prefs.Save();
    // int i = prefs.GetInt("testKey");

    [Serializable]
    internal class PlayerPref
    {
        public string key;
        public string value;

        public PlayerPref(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [SerializeField] private List<PlayerPref> _playerPrefs = new List<PlayerPref>();
    [SerializeField, ReadOnly] private string _fileName;

    private string DirectoryPath => Path.Combine(Application.persistentDataPath, _fileName);

    public JsonPlayerPrefs(string fileName)
    {
        _fileName = fileName + ".json";
        if (File.Exists(DirectoryPath))
        {
            using (StreamReader reader = new StreamReader(DirectoryPath))
            {
                string json = reader.ReadToEnd();
                JsonPlayerPrefs data = JsonUtility.FromJson<JsonPlayerPrefs>(json);
                _playerPrefs = data._playerPrefs;
            }
        }
    }

    public bool HasKey(string key)
    {
        foreach (PlayerPref p in _playerPrefs)
        {
            if (p.key == key)
            {
                return true;
            }
        }
        return false;
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (float.TryParse(playerPref.value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (int.TryParse(playerPref.value, out int value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (bool.TryParse(playerPref.value, out bool value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            return playerPref.value;
        }
        return defaultValue;
    }

    private bool TryGetPlayerPref(string key, out PlayerPref playerPref)
    {
        playerPref = null;
        for (int i = 0; i < _playerPrefs.Count; i++)
        {
            if (_playerPrefs[i].key == key)
            {
                playerPref = _playerPrefs[i];
                return true;
            }
        }
        return false;
    }

    public void SetBool(string key, bool value) => SetString(key, value.ToString());
    public void SetFloat(string key, float value) => SetString(key, value.ToString());
    public void SetInt(string key, int value) => SetString(key, value.ToString());
    public void SetString(string key, string value)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            playerPref.value = value;
        }
        else
        {
            _playerPrefs.Add(new PlayerPref(key, value));
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DirectoryPath));
        string json = JsonUtility.ToJson(this);
        using (StreamWriter writer = new StreamWriter(DirectoryPath))
        {
            writer.WriteLine(json);
        }
    }

    public void DeleteKey(string key)
    {
        for (int i = _playerPrefs.Count - 1; i >= 0; i--)
        {
            if (_playerPrefs[i].key == key)
            {
                _playerPrefs.RemoveAt(i);
            }
        }
    }

    public void DeleteAll()
    {
        _playerPrefs.Clear();
    }

    public string[] GetAllKeysAndValues()
    {
        List<string> allKeys = new List<string>(_playerPrefs.Count);
        foreach (PlayerPref p in _playerPrefs)
        {
            allKeys.Add(p.key);
            allKeys.Add(p.value);
        }
        return allKeys.ToArray();
    }
}