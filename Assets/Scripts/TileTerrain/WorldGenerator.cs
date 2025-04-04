using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private bool _centerX;
    [SerializeField] private bool _downY;
    [SerializeField] private int _width = 320;
    [SerializeField] private int _height = 960;
    [SerializeField] private int _chunkSize = 32;
    [SerializeField] private int _tilesPerUnit = 2;
    [SerializeField, ReadOnly] private float _tileSizeMult = 0.5f;
    [SerializeField, ReadOnly] private int _chunkCount;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Generation Settings")]
    [SerializeField] private int _worldSeed = -1;
    [SerializeField] private int _surfaceLevelSize = 20;
    [SerializeField] private float _surfaceNoiseScale = 0.04f;
    [SerializeField] private float _caveNoiseScale = 0.08f;
    [SerializeField, Range(0f, 1f)] private float _caveThreshold = 0.55f;
    [SerializeField, Range(0f, 1f)] public float _caveNoiseDetail = 0.1f;
    [SerializeField] private float _mineralNoiseScale = 0.1f;
    [SerializeField, Range(0f, 1f)] public float _copperFrequency = 0.1f;
    [SerializeField, Range(0f, 1f)] public float _ironFrequency = 0.05f;

    [Header("Destruction")]
    [SerializeField] private int _damageableTileStoreCount = 100;

    [Header("References")]
    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private TerrainTileData _tileData;
    [SerializeField] private Material _terrainMaterial;
    [SerializeField] private DroppedItemManager _itemManager;

    public TerrainTileData TileData => _tileData;
    public int ChunkSize => _chunkSize;
    private float OffsetX => transform.position.x + (_centerX ? -_width * _tileSizeMult * 0.5f : 0f);
    private float OffsetY => transform.position.y + (_downY ? -_height * _tileSizeMult : 0f);

    private TileType[,] _grid;
    private Dictionary<Vector2Int, Chunk> _chunks;
    private List<Vector2Int> _modifiedChunks = new List<Vector2Int>();
    private Queue<TileDamageData> _tileDamageData;

    private float _noiseOffsetX;
    private float _noiseOffsetY;

    private void OnValidate()
    {
        _tileSizeMult = 1f / _tilesPerUnit;
        _chunkCount = Mathf.CeilToInt(_width / _chunkSize) * Mathf.CeilToInt(_height / _chunkSize);
    }

    private void Start()
    {
        InitializeWorld();
    }

    private void LateUpdate()
    {
        if (_modifiedChunks.Count > 0)
        {
            foreach (var coords in _modifiedChunks)
            {
                UpdateChunkAt(coords);
            }
            _modifiedChunks.Clear();
        }
    }

    [Button]
    private void ResetWorld()
    {
        if (_chunks != null)
        {
            foreach (var chunk in _chunks.Values)
            {
                Destroy(chunk.gameObject);
            }
        }
        InitializeWorld();
    }

    private void InitializeWorld()
    {
        _grid = new TileType[_width, _height];
        _chunks = new Dictionary<Vector2Int, Chunk>();
        _tileDamageData = new Queue<TileDamageData>(_damageableTileStoreCount);

        if (_worldSeed < 0) _worldSeed = Random.Range(1, 100000);
        Random.InitState(_worldSeed);

        _noiseOffsetX = Random.Range(0f, 1000f);
        _noiseOffsetY = Random.Range(0f, 1000f);

        GenerateSurface();
        GenerateCaves();
        GenerateMinerals();
        CreateChunks();
        OptimizeChunks();
        Debug.Log("World Generation Complete");
    }

    private void GenerateSurface()
    {
        for (int x = 0; x < _width; x++)
        {
            float noiseValue = Mathf.PerlinNoise((x + _noiseOffsetX) * _surfaceNoiseScale * _tileSizeMult,
                                                  _noiseOffsetY * _surfaceNoiseScale * _tileSizeMult);
            int surfaceHeight = Mathf.RoundToInt(Mathf.Lerp(_height - 10, _height - _surfaceLevelSize - 10, noiseValue) + Random.Range(0, 1));
            surfaceHeight = Mathf.Clamp(surfaceHeight, 1, _height - 1);

            for (int y = 0; y < _height; y++)
            {
                if (y < surfaceHeight - 5)
                {
                    int rand = Random.Range(0, 2) + Random.Range(0, 2);
                    if (x < 5 - rand || x > _width - 5 + rand)
                    {
                        _grid[x, y] = TileType.StoneEdge;
                    }
                    else
                    {
                        _grid[x, y] = TileType.Stone;
                    }
                }
                else if (y < surfaceHeight)
                {
                    _grid[x, y] = TileType.Dirt;
                }
                else if (y == surfaceHeight)
                {
                    _grid[x, y] = TileType.Grass;
                }
                else
                {
                    _grid[x, y] = TileType.Air;
                }
            }
        }
    }

    private void GenerateCaves()
    {
        float caveNoiseOffsetX = _noiseOffsetX + 100f;
        float caveNoiseOffsetY = _noiseOffsetY + 100f;

        int maxCaveHeight = _height - _surfaceLevelSize - 10;
        int minCaveHeight = 10;

        for (int y = minCaveHeight; y < maxCaveHeight; y++)
        {
            // Suface bias (reduce caves near surface)
            float surfaceThresholdOffset = Mathf.Clamp01((y + 10 - maxCaveHeight) * 0.1f) * (1 - _caveThreshold);

            for (int x = 0; x < _width; x++)
            {
                if (_grid[x, y] != TileType.Air)
                {
                    float caveNoiseValue = Mathf.PerlinNoise(
                        (x + caveNoiseOffsetX) * _caveNoiseScale * _tileSizeMult,
                        (y + caveNoiseOffsetY) * _caveNoiseScale * _tileSizeMult
                    );
                    float caveNoiseValue2 = Mathf.PerlinNoise(
                        (x + caveNoiseOffsetX * 0.25f) * _caveNoiseScale * _tileSizeMult * 4f,
                        (y + caveNoiseOffsetY * 0.25f) * _caveNoiseScale * _tileSizeMult * 4f
                    );
                    caveNoiseValue = Mathf.Lerp(caveNoiseValue, caveNoiseValue2, _caveNoiseDetail);

                    // Edge bias (reduce caves near edge)
                    float edgeThresholdOffset = 0;
                    if (x < 12) edgeThresholdOffset += (10 - (x - 2)) * 0.1f;
                    if (x > _width - 12) edgeThresholdOffset += (10 - (_width - x - 2)) * 0.1f;
                    edgeThresholdOffset = Mathf.Clamp01(edgeThresholdOffset * (1 - _caveThreshold));

                    if (caveNoiseValue > _caveThreshold + surfaceThresholdOffset + edgeThresholdOffset)
                    {
                        _grid[x, y] = TileType.Air;
                    }
                }
            }
        }
    }

    private void GenerateMinerals()
    {
        var mineralData = _tileData.GetMineralGenerationData();
        float offX = _noiseOffsetX + 200f;
        float offY = _noiseOffsetY + 200f;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == TileType.Stone)
                {
                    for (int i = 0; i < mineralData.Count; i++)
                    {
                        if (y > _height - mineralData[i].Item2)
                        {
                            continue;
                        }
                        float noise = Mathf.PerlinNoise(
                            (x + offX + i * 100f) * _mineralNoiseScale * _tileSizeMult,
                            (y + offY + i * 100f) * _mineralNoiseScale * _tileSizeMult);
                        if (noise < mineralData[i].Item3)
                        {
                            _grid[x, y] = mineralData[i].Item1;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void CreateChunks()
    {
        int numChunksX = Mathf.CeilToInt((float)_width / _chunkSize);
        int numChunksY = Mathf.CeilToInt((float)_height / _chunkSize);

        for (int y = 0; y < numChunksY; y++)
        {
            for (int x = 0; x < numChunksX; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                Vector3 chunkPosition = new Vector3(x * _chunkSize * _tileSizeMult + OffsetX, y * _chunkSize * _tileSizeMult + OffsetY, 0);

                GameObject chunkGO = Instantiate(_chunkPrefab, chunkPosition, Quaternion.identity, transform);
                chunkGO.transform.localScale = new Vector3(_tileSizeMult, _tileSizeMult, 1);
                chunkGO.layer = 8;
                chunkGO.name = $"Chunk_{x}_{y}";

                Chunk chunkScript = chunkGO.GetComponent<Chunk>();
                if (chunkScript != null)
                {
                    chunkScript.Initialize(this, chunkCoord, _terrainMaterial);
                    _chunks.Add(chunkCoord, chunkScript);
                }
                else
                {
                    Debug.LogError($"Chunk prefab is missing the Chunk script on {chunkGO.name}");
                    Destroy(chunkGO);
                    return;
                }
            }
        }
    }

    private void OptimizeChunks()
    {
        foreach (var chunk in _chunks.Values)
        {
            chunk.OptimizeChunkMesh();
        }
    }

    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return TileType.Air;
        }
        return _grid[x, y];
    }

    public void DestroyTileAt(Vector3 worldPos, float radius)
    {
        int xTile = Mathf.FloorToInt((worldPos.x - OffsetX) * _tilesPerUnit);
        int yTile = Mathf.FloorToInt((worldPos.y - OffsetY) * _tilesPerUnit);
        int range = Mathf.CeilToInt(radius * _tilesPerUnit);

        int xOrigin = Mathf.Max(0, xTile - range);
        int xEnd = Mathf.Min(_width - 1, xTile + range);
        int yOrigin = Mathf.Max(0, yTile - range);
        int yEnd = Mathf.Min(_height - 1, yTile + range);

        for (int x = xOrigin; x < xEnd; x++)
        {
            for (int y = yOrigin; y < yEnd; y++)
            {
                int xDelta = x - xTile;
                int yDelta = y - yTile;
                float sqrDistance = (xDelta * xDelta) + (yDelta * yDelta);

                if (sqrDistance < range * range)
                {
                    //_itemManager.NewDroppedItem(worldPos, _tileData.GetTileData(_grid[x, y]).ItemData);
                    _grid[x, y] = TileType.Air;
                    Vector2Int modifiedChunkCoord = new Vector2Int(x / _chunkSize, y / _chunkSize);
                    SetChunkModified(modifiedChunkCoord);
                }
            }
        }
    }

    public void DestroyTileAt(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - OffsetX) * _tilesPerUnit);
        int y = Mathf.FloorToInt((worldPos.y - OffsetY) * _tilesPerUnit);

        if (x < 0 || x >= _width || y < 0 || y >= _height) return;

        if (_grid[x, y] == TileType.Air)
        {
            return;
        }

        //_itemManager.NewDroppedItem(worldPos, _tileData.GetTileData(_grid[x, y]).ItemData);
        _grid[x, y] = TileType.Air;

        Vector2Int modifiedChunkCoord = new Vector2Int(x / _chunkSize, y / _chunkSize);
        SetChunkModified(modifiedChunkCoord);

        if (x % _chunkSize == 0 && x > 0)
        {
            SetChunkModified(new Vector2Int(modifiedChunkCoord.x - 1, modifiedChunkCoord.y));
        }
        else if (x % _chunkSize == _chunkSize - 1 && x < _width - 1)
        {
            SetChunkModified(new Vector2Int(modifiedChunkCoord.x + 1, modifiedChunkCoord.y));
        }
        if (y % _chunkSize == 0 && y > 0)
        {
            SetChunkModified(new Vector2Int(modifiedChunkCoord.x, modifiedChunkCoord.y - 1));
        }
        else if (y % _chunkSize == _chunkSize - 1 && y < _height - 1)
        {
            SetChunkModified(new Vector2Int(modifiedChunkCoord.x, modifiedChunkCoord.y + 1));
        }
    }

    private void SetChunkModified(Vector2Int coord)
    {
        if (!_modifiedChunks.Contains(coord))
        {
            _modifiedChunks.Add(coord);
        }
    }

    private void UpdateChunkAt(Vector2Int chunkCoord)
    {
        if (_chunks.TryGetValue(chunkCoord, out Chunk chunkToUpdate))
        {
            chunkToUpdate.UpdateChunk();
        }
        else
        {
            Debug.LogWarning($"Could not find chunk to update at coordinates {chunkCoord}");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        int numChunksX = Mathf.CeilToInt((float)_width / _chunkSize);
        int numChunksY = Mathf.CeilToInt((float)_height / _chunkSize);

        for (int y = 0; y < numChunksY; y++)
        {
            for (int x = 0; x < numChunksX; x++)
            {
                Vector3 center = new Vector3(_tileSizeMult * ((x * _chunkSize) + _chunkSize / 2f) + OffsetX,
                                             _tileSizeMult * ((y * _chunkSize) + _chunkSize / 2f) + OffsetY, -0.5f);
                Vector3 size = new Vector3(_chunkSize * _tileSizeMult, _chunkSize * _tileSizeMult, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
#endif
}