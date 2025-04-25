using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/World Settings")]
public class WorldSettings : SerializedScriptableObject
{
    [Header("Settings")]
    [SerializeField] private int _minSurfaceDepth = 5;
    [SerializeField] private int _maxSurfaceDepth = 20;
    [SerializeField, Range(0f, 0.5f)] private float _surfaceNoiseScale = 0.04f;
    [SerializeField] private int _caveStartDepth = 30;
    [SerializeField, Range(0f, 1f)] private float _grassDecoChance = 0.2f;
    [SerializeField, Range(0f, 0.5f)] private float _caveNoiseScale = 0.08f;
    [SerializeField, Range(0f, 1f)] private float _caveThreshold = 0.55f;
    [SerializeField, Range(0f, 1f)] public float _caveNoiseDetail = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _caveCeilingDecoChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _caveFloorDecoChance = 0.1f;

    public int MinSurfaceDepth => _minSurfaceDepth;
    public int MaxSurfaceDepth => _maxSurfaceDepth;
    public float SurfaceNoiseScale => _surfaceNoiseScale;
    public int CaveStartDepth => _caveStartDepth;
    public float GrassDecoChance => _grassDecoChance;
    public float CaveNoiseScale => _caveNoiseScale;
    public float CaveThreshold => _caveThreshold;
    public float CaveNoiseDetail => _caveNoiseDetail;
    public float CaveCeilingDecoChance => _caveCeilingDecoChance;
    public float CaveFloorDecoChance => _caveFloorDecoChance;

    [Header("References")]
    [SerializeField] private Material _terrainMaterial;
    [SerializeField] private int _gridSize = 8;
    [SerializeField] private Vector2Int _damageOffsetUV = new Vector2Int(1, 0);
    [SerializeField] private LevelDecoration _decorationPrefab;
    [SerializeField] private StructureData _shopStructure;

    public Material TerrainMaterial => _terrainMaterial;
    public int GridSize => _gridSize;
    public Vector2Int DamageOffsetUV => _damageOffsetUV;
    public LevelDecoration DecorationPrefab => _decorationPrefab;
    public StructureData ShopStructure => _shopStructure;

    [Header("Tile Types")]
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

    // Type, StartDepth, NoiseScale, Frequency
    public List<(TileType, int, float, float)> GetMineralGenerationData()
    {
        var list = new List<(TileType, int, float, float)>();
        foreach (var type in _data)
        {
            if (type.Value.Mineral)
            {
                list.Add((type.Key, type.Value.StartDepth, type.Value.NoiseScale, type.Value.Frequency));
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
            _data.Add(type, new TerrainTileDataStruct(row, new Vector2(row, column), 1, mineral, 0.1f, mineral ? (row + (column - 1) * 4) * 100 : 0));
        }
    }
#endif
}

public struct TerrainTileDataStruct
{
    public int TileHealth;
    [Tooltip("X is left-right, Y is bottom-top")]
    public Vector2 UVCoords;
    public Vector2 UVOffsetSides;
    public int DamageUVCount;
    public ItemData ItemData;
    public bool Mineral;
    [ShowIf("Mineral"), Range(0f, 0.5f)] public float NoiseScale;
    [ShowIf("Mineral"), Range(0f, 1f)] public float Frequency;
    [ShowIf("Mineral")] public int StartDepth;

    public TerrainTileDataStruct(int health, Vector2 uvData, int damageUVCount, bool mineral, float frequency, int startDepth)
    {
        TileHealth = health;
        UVCoords = uvData;
        UVOffsetSides = new Vector2(0, 0);
        DamageUVCount = damageUVCount;
        ItemData = null;
        Mineral = mineral;
        NoiseScale = 0.2f;
        Frequency = frequency;
        StartDepth = startDepth;
    }
}