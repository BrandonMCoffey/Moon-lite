using CoffeyUtils;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private int _width = 320;
    [SerializeField] private int _height = 960;
    [SerializeField] private int _chunkSize = 32;
    [SerializeField] private int _tilesPerUnit = 2;
    [SerializeField, ReadOnly] private float _tileSizeMult = 0.5f;
    [SerializeField] private bool _centerX;
    [SerializeField] private bool _downY;

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

    [Header("Chunking")]
    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private Material _terrainMaterial;

    public int ChunkSize => _chunkSize;
    private float OffsetX => transform.position.x + (_centerX ? -_width * _tileSizeMult * 0.5f : 0f);
    private float OffsetY => transform.position.y + (_downY ? -_height * _tileSizeMult : 0f);

    private TileType[,] _grid;
    private Dictionary<Vector2Int, Chunk> _chunks;

    private float _noiseOffsetX;
    private float _noiseOffsetY;

    private void OnValidate()
    {
        _tileSizeMult = 1f / _tilesPerUnit;
    }

    private void Start()
    {
        InitializeWorld();
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

        if (_worldSeed < 0) _worldSeed = Random.Range(1, 100000);
        Random.InitState(_worldSeed);

        _noiseOffsetX = Random.Range(0f, 1000f);
        _noiseOffsetY = Random.Range(0f, 1000f);

        GenerateTerrain();
        GenerateCaves();
        GenerateMinerals();
        CreateChunks();
        OptimizeChunks();
        Debug.Log("World Generation Complete");
    }

    private void GenerateTerrain()
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
                    _grid[x, y] = TileType.Stone;
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
            float thresholdOffset = Mathf.Clamp01((y + 10 - maxCaveHeight) * 0.1f) * (1 - _caveThreshold);

            for (int x = 0; x < _width; x++)
            {
                // TODO: Edge bias (reduce caves near edge)

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
                    
                    if (caveNoiseValue > _caveThreshold + thresholdOffset)
                    {
                        _grid[x, y] = TileType.Air;
                    }
                }
            }
        }
    }

    private void GenerateMinerals()
    {
        // TODO: Move minerals to SOs and make a foreach loop using SO properties

        float copperNoiseOffsetX = _noiseOffsetX + 200f;
        float copperNoiseOffsetY = _noiseOffsetY + 200f;
        float ironNoiseOffsetX = _noiseOffsetX + 300f;
        float ironNoiseOffsetY = _noiseOffsetY + 300f;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == TileType.Stone)
                {
                    float copperNoise = Mathf.PerlinNoise(
                        (x + copperNoiseOffsetX) * _mineralNoiseScale * _tileSizeMult,
                        (y + copperNoiseOffsetY) * _mineralNoiseScale * _tileSizeMult);
                    if (copperNoise < _copperFrequency)
                    {
                        _grid[x, y] = TileType.Copper;
                        continue;
                    }

                    float ironNoise = Mathf.PerlinNoise(
                        (x + ironNoiseOffsetX) * _mineralNoiseScale * _tileSizeMult,
                        (y + ironNoiseOffsetY) * _mineralNoiseScale * _tileSizeMult);
                    if (ironNoise < _ironFrequency)
                    {
                        _grid[x, y] = TileType.Iron;
                        continue;
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
                chunkGO.transform.localScale = Vector3.one * _tileSizeMult;
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

    public void DestroyTileAt(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - OffsetX) * _tilesPerUnit);
        int y = Mathf.FloorToInt((worldPos.y - OffsetY) * _tilesPerUnit);

        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            Debug.LogWarning($"Attempted to destroy tile outside world bounds at ({x},{y})");
            return;
        }

        if (_grid[x, y] == TileType.Air)
        {
            return;
        }

        _grid[x, y] = TileType.Air;

        Vector2Int modifiedChunkCoord = new Vector2Int(x / _chunkSize, y / _chunkSize);

        UpdateChunkAt(modifiedChunkCoord);

        if (x % _chunkSize == 0 && x > 0)
        {
            UpdateChunkAt(new Vector2Int(modifiedChunkCoord.x - 1, modifiedChunkCoord.y));
        }
        else if (x % _chunkSize == _chunkSize - 1 && x < _width - 1)
        {
            UpdateChunkAt(new Vector2Int(modifiedChunkCoord.x + 1, modifiedChunkCoord.y));
        }

        if (y % _chunkSize == 0 && y > 0)
        {
            UpdateChunkAt(new Vector2Int(modifiedChunkCoord.x, modifiedChunkCoord.y - 1));
        }
        else if (y % _chunkSize == _chunkSize - 1 && y < _height - 1)
        {
            UpdateChunkAt(new Vector2Int(modifiedChunkCoord.x, modifiedChunkCoord.y + 1));
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