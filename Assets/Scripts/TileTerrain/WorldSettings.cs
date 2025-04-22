using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/World Settings")]
public class WorldSettings : SerializedScriptableObject
{
    [Header("Settings")]
    [SerializeField] private int _surfaceLevelSize = 20;
    [SerializeField] private float _surfaceNoiseScale = 0.04f;
    [SerializeField, Range(0f, 1f)] private float _grassDecoChance = 0.2f;
    [SerializeField] private float _caveNoiseScale = 0.08f;
    [SerializeField, Range(0f, 1f)] private float _caveThreshold = 0.55f;
    [SerializeField, Range(0f, 1f)] public float _caveNoiseDetail = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _caveCeilingDecoChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _caveFloorDecoChance = 0.1f;
    [SerializeField] private float _mineralNoiseScale = 0.2f;

    public int SurfaceLevelSize => _surfaceLevelSize;
    public float SurfaceNoiseScale => _surfaceNoiseScale;
    public float GrassDecoChance => _grassDecoChance;
    public float CaveNoiseScale => _caveNoiseScale;
    public float CaveThreshold => _caveThreshold;
    public float CaveNoiseDetail => _caveNoiseDetail;
    public float CaveCeilingDecoChance => _caveCeilingDecoChance;
    public float CaveFloorDecoChance => _caveFloorDecoChance;
    public float MineralNoiseScale => _mineralNoiseScale;

    [Header("References")]
    [SerializeField] private Material _terrainMaterial;
    [SerializeField] private LevelDecoration _decorationPrefab;
    [SerializeField] private StructureData _shopStructure;

    [Header("Tile Types")]
    [DictionaryDrawerSettings(KeyLabel = "Tile Type", ValueLabel = "Tile Data")]
    [SerializeField] private Dictionary<TileType, TerrainTileDataStruct> _data = new Dictionary<TileType, TerrainTileDataStruct>();

    public Material TerrainMaterial => _terrainMaterial;
    public LevelDecoration DecorationPrefab => _decorationPrefab;
    public StructureData ShopStructure => _shopStructure;

    public TerrainTileDataStruct GetTileData(TileType type)
    {
        if (_data.TryGetValue(type, out var data))
        {
            return data;
        }
        throw new System.Exception("Attempted to get tile data that does not exist in data set");
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

#if UNITY_EDITOR
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
#endif
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