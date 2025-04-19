using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Moonlite/Terrain Tile Data")]
public class TerrainTileData : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "Tile Type", ValueLabel = "Tile Data")]
    [SerializeField] private Dictionary<TileType, TerrainTileDataStruct> _data = new Dictionary<TileType, TerrainTileDataStruct>();
    public TerrainTileDataStruct GetTileData(TileType type)
    {
        if (_data.TryGetValue(type, out var data))
        {
            return data;
        }
        throw new System.Exception("Attempted to get tile data that does not exist in data set");
    }

    [Button]
    private void AddMissingToDictionary()
    {
        foreach (var type in (TileType[])System.Enum.GetValues(typeof(TileType)))
        {
            if (_data.ContainsKey(type)) continue;
            if (type == TileType.Air) continue;
            int index = (int)type - 1;
            int row = index % 4;
            int column = index / 4;
            bool mineral = column >= 1;
            _data.Add(type, new TerrainTileDataStruct(row, new Vector2(row, column), mineral, 0.1f, mineral ? (row + (column - 1) * 4) * 100 : 0));
        }
    }

    public List<(TileType, int, float)> GetMineralGenerationData()
    {
        var list = new List<(TileType, int, float)>();
        foreach (var type in _data)
        {
            if (type.Value.Mineral)
            {
                list.Add((type.Key, type.Value.StartDepth, type.Value.Frequency));
            }
        }
        return list;
    }
}

public struct TerrainTileDataStruct
{
    public int TileHealth;
    [Tooltip("X is left-right, Y is bottom-top")]
    public Vector2 UVData;
    public ItemData ItemData;
    public bool Mineral;
    [ShowIf("Mineral"), Range(0f, 1f)] public float Frequency;
    [ShowIf("Mineral")] public int StartDepth;

    public TerrainTileDataStruct(int health, Vector2 uvData, bool mineral, float frequency, int startDepth)
    {
        TileHealth = health;
        UVData = uvData;
        ItemData = null;
        Mineral = mineral;
        Frequency = frequency;
        StartDepth = startDepth;
    }
}