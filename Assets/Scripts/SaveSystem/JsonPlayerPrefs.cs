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

    [SerializeField, ReadOnly] public string FileName { get; private set; }
    [SerializeField] private List<PlayerPref> _playerPrefs = new List<PlayerPref>();

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName + ".json");
    private string BinaryFilePath => Path.Combine(Application.persistentDataPath, FileName + ".bin");

    public JsonPlayerPrefs(string fileName)
    {
        FileName = fileName;
        if (File.Exists(FilePath))
        {
            using (StreamReader reader = new StreamReader(FilePath))
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
            if (p.key.Equals(key, StringComparison.InvariantCulture))
            {
                return true;
            }
        }
        return false;
    }

    private const bool SaveDefaultOnGetValue = false;

    public float GetFloat(string key, float defaultValue = 0f, bool saveDefault = SaveDefaultOnGetValue)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (float.TryParse(playerPref.value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }
        }
        if (saveDefault) SetFloat(key, defaultValue);
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0, bool saveDefault = SaveDefaultOnGetValue)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (int.TryParse(playerPref.value, out int value))
            {
                return value;
            }
        }
        if (saveDefault) SetInt(key, defaultValue);
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false, bool saveDefault = SaveDefaultOnGetValue)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            if (bool.TryParse(playerPref.value, out bool value))
            {
                return value;
            }
        }
        if (saveDefault) SetBool(key, defaultValue);
        return defaultValue;
    }

    public string[] GetStringArray(string key, string[] defaultValue = null, bool saveDefault = SaveDefaultOnGetValue)
    {
        string value = GetString(key, "", false);
        if (value != "")
        {
            string[] values = value.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }
            return values;
        }
        if (saveDefault && defaultValue != null) SetStringArray(key, defaultValue);
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "", bool saveDefault = SaveDefaultOnGetValue)
    {
        if (TryGetPlayerPref(key, out PlayerPref playerPref))
        {
            return playerPref.value;
        }
        if (saveDefault) SetString(key, defaultValue);
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
    public void SetStringArray(string key, string[] values)
    {
        string value = "";
        for (int i = 0; i < values.Length; i++)
        {
            value += values[i] + (i < values.Length - 1 ? "," : "");
        }
        SetString(key, value);
    }
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
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        string json = JsonUtility.ToJson(this);
        using (StreamWriter writer = new StreamWriter(FilePath))
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

    public void DeleteKeysStartingWith(string prefix)
    {
        for (int i = _playerPrefs.Count - 1; i >= 0; i--)
        {
            if (_playerPrefs[i].key.StartsWith(prefix))
            {
                _playerPrefs.RemoveAt(i);
            }
        }
    }

    public void DeleteAll()
    {
        _playerPrefs.Clear();
    }

    // Maybe do this a better way? Trying to emulate a dictionary essentially lol
    public (string, string)[] GetKeyValuePairs()
    {
        List<(string, string)> keyValuePairs = new List<(string, string)>(_playerPrefs.Count);
        foreach (PlayerPref p in _playerPrefs)
        {
            keyValuePairs.Add((p.key, p.value));
        }
        return keyValuePairs.ToArray();
    }
}