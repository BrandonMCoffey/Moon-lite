using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance => _instance;
    private static WorldGenerator _instance;

    public static event System.Action<int, int> OnTileDestroyed = delegate { };

    [Header("Generation Settings")]
    [SerializeField] private int _worldSeed = -1;
    [SerializeField] private WorldSettings _worldSettings;

    [Header("World Settings")]
    [SerializeField] private bool _centerX;
    [SerializeField] private bool _downY;
    [SerializeField] private int _width = 320;
    [SerializeField] private int _height = 960;
    [SerializeField] private int _chunkSize = 32;
    [SerializeField] private int _tilesPerUnit = 2;
    [SerializeField, ReadOnly] private float _tileSizeMult = 0.5f;
    [SerializeField, ReadOnly] private int _chunkCount;
    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private DroppedItemManager _itemManager;

    public WorldSettings Settings => _worldSettings;
    public int ChunkSize => _chunkSize;
    public float TileSizeMult => _tileSizeMult;
    private float OffsetX => transform.position.x + (_centerX ? -_width * _tileSizeMult * 0.5f : 0f);
    private float OffsetY => transform.position.y + (_downY ? -_height * _tileSizeMult : 0f);
    public int WorldToTileX(float worldX) => Mathf.FloorToInt((worldX - OffsetX) * _tilesPerUnit);
    public int WorldToTileY(float worldY) => Mathf.FloorToInt((worldY - OffsetY) * _tilesPerUnit);
    public Vector3 TileToWorld(int x, int y, bool center = true) => new Vector3(x * _tileSizeMult + OffsetX + (center ? _tileSizeMult * 0.5f : 0),
                                                   y * _tileSizeMult + OffsetY + (center ? _tileSizeMult * 0.5f : 0), transform.position.z - 0.5f);

    private TileType[,] _grid;
    private Dictionary<Vector2Int, Chunk> _chunks;
    private List<Vector2Int> _modifiedChunks;
    private Dictionary<Vector2Int, float> _tileDamageData;
    private List<StructureData> _structures;
    private Transform _player;

    private float _noiseOffsetX;
    private float _noiseOffsetY;
    private Vector3 _damageRegion;
    private float _damageRegionSize;

    private void OnValidate()
    {
        _tileSizeMult = 1f / _tilesPerUnit;
        _chunkCount = Mathf.CeilToInt(_width / _chunkSize) * Mathf.CeilToInt(_height / _chunkSize);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _player = Player.Instance.transform;
        InitializeWorld();
    }

    private void LateUpdate()
    {
        if (_modifiedChunks.Count > 0)
        {
            UpdateChunkAt(_modifiedChunks[0]);
            _modifiedChunks.RemoveAt(0);
            /*
            foreach (var coords in _modifiedChunks)
            {
                UpdateChunkAt(coords);
            }
            _modifiedChunks.Clear();
            */
        }
    }

    [Button]
    private void NewWorld()
    {
        _worldSeed = -1;
        if (_chunks != null)
        {
            foreach (var chunk in _chunks.Values)
            {
                Destroy(chunk.gameObject);
            }
            for (int i = _structures.Count - 1; i >= 0; i--)
            {
                Destroy(_structures[i].gameObject);
            }
            _structures.Clear();
        }
        InitializeWorld();
    }

    private void InitializeWorld()
    {
        _grid = new TileType[_width, _height];
        _chunks = new Dictionary<Vector2Int, Chunk>();
        _modifiedChunks = new List<Vector2Int>();
        _tileDamageData = new Dictionary<Vector2Int, float>(100);
        _structures = new List<StructureData>();

        if (_worldSeed < 0) _worldSeed = Random.Range(1, 100000);
        Random.InitState(_worldSeed);

        _noiseOffsetX = Random.Range(0f, 1000f);
        _noiseOffsetY = Random.Range(0f, 1000f);

        GenerateSurface();
        GenerateCaves();
        GenerateMinerals();
        GenerateStructures();
        CreateChunks();
        OptimizeChunks();
        Debug.Log("World Generation Complete");
    }

    private void GenerateSurface()
    {
        for (int x = 0; x < _width; x++)
        {
            float noiseValue = Mathf.PerlinNoise((x + _noiseOffsetX) * Settings.SurfaceNoiseScale * _tileSizeMult,
                                                  _noiseOffsetY * Settings.SurfaceNoiseScale * _tileSizeMult);
            int surfaceHeight = Mathf.RoundToInt(Mathf.Lerp(_height - 10, _height - Settings.SurfaceLevelSize - 10, noiseValue) + Random.Range(0, 1));
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
                    if (Random.value < Settings.GrassDecoChance)
                    {
                        var grass = Instantiate(Settings.DecorationPrefab, TileToWorld(x, y + 1), transform.rotation, transform);
                        grass.RandomizeGrass();
                        grass.ConnectToTile(x, y);
                    }
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

        int maxCaveHeight = _height - Settings.SurfaceLevelSize - 10;
        int minCaveHeight = 10;

        for (int y = maxCaveHeight - 1; y >= minCaveHeight; y--)
        {
            // Suface bias (reduce caves near surface)
            float surfaceThresholdOffset = Mathf.Clamp01((y + 10 - maxCaveHeight) * 0.1f) * (1 - Settings.CaveThreshold);

            for (int x = 0; x < _width; x++)
            {
                if (_grid[x, y] == TileType.Stone)
                {
                    float caveNoiseValue = Mathf.PerlinNoise(
                        (x + caveNoiseOffsetX) * Settings.CaveNoiseScale * _tileSizeMult,
                        (y + caveNoiseOffsetY) * Settings.CaveNoiseScale * _tileSizeMult
                    );
                    float caveNoiseValue2 = Mathf.PerlinNoise(
                        (x + caveNoiseOffsetX * 0.25f) * Settings.CaveNoiseScale * _tileSizeMult * 4f,
                        (y + caveNoiseOffsetY * 0.25f) * Settings.CaveNoiseScale * _tileSizeMult * 4f
                    );
                    caveNoiseValue = Mathf.Lerp(caveNoiseValue, caveNoiseValue2, Settings.CaveNoiseDetail);

                    // Edge bias (reduce caves near edge)
                    float edgeThresholdOffset = 0;
                    if (x < 12) edgeThresholdOffset += (10 - (x - 2)) * 0.1f;
                    if (x > _width - 12) edgeThresholdOffset += (10 - (_width - x - 2)) * 0.1f;
                    edgeThresholdOffset = Mathf.Clamp01(edgeThresholdOffset * (1 - Settings.CaveThreshold));

                    if (caveNoiseValue > Settings.CaveThreshold + surfaceThresholdOffset + edgeThresholdOffset)
                    {
                        _grid[x, y] = TileType.Air;
                        // If current tile is air, and above tile is stone
                        if (_grid[x, y + 1] == TileType.Stone)
                        {
                            if (Random.value < Settings.CaveCeilingDecoChance)
                            {
                                var deco = Instantiate(Settings.DecorationPrefab, TileToWorld(x, y), transform.rotation, transform);
                                deco.RandomizeCave(false);
                                deco.ConnectToTile(x, y + 1);
                            }
                        }
                    }
                    // If current tile is stone, and above tile is air
                    else if (_grid[x, y] == TileType.Stone && _grid[x, y + 1] == TileType.Air)
                    {
                        if (Random.value < Settings.CaveFloorDecoChance)
                        {
                            var deco = Instantiate(Settings.DecorationPrefab, TileToWorld(x, y + 1), transform.rotation, transform);
                            deco.RandomizeCave(true);
                            deco.ConnectToTile(x, y);
                        }
                    }
                }
            }
        }
    }

    private void GenerateMinerals()
    {
        var mineralData = Settings.GetMineralGenerationData();
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
                            (x + offX + i * 100f) * mineralData[i].Item3 * _tileSizeMult,
                            (y + offY + i * 100f) * mineralData[i].Item3 * _tileSizeMult);
                        if (noise < mineralData[i].Item4)
                        {
                            _grid[x, y] = mineralData[i].Item1;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void GenerateStructures()
    {
        for (int i = 2; i <= 7; i++)
        {
            int depth = i * _height / 8;
            GenerateStructureShop(Random.Range(_width / 8, _width - _width / 8), _height - depth + Random.Range(-25, 25));
        }
    }

    private void GenerateStructureShop(int xCenter, int yCenter)
    {
        var shop = Instantiate(Settings.ShopStructure, TileToWorld(xCenter, yCenter, false), Quaternion.identity, transform);
        _structures.Add(shop);
        var s = Settings.ShopStructure.Sizes * _tilesPerUnit;
        var xMin = Mathf.Clamp(xCenter - Mathf.FloorToInt(s.x), 0, _width-1);
        var xMax = Mathf.Clamp(xCenter + Mathf.FloorToInt(s.y + s.z), 0, _width-1);
        var yMax = Mathf.Clamp(yCenter + Mathf.FloorToInt(s.w), 0, _height-1);
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yCenter; y < yMax; y++)
            {
                if ((x == xMin || x == xMax - 1) && (y == yCenter || y == yMax - 1))
                {
                    continue;
                }
                _grid[x, y] = TileType.Air;
            }
        }
        for (int yOff = 0; yOff < 3; yOff++)
        {
            int y = yCenter - yOff - 1;
            for (int x = xCenter + yOff * 2; x < xCenter + s.y - yOff * 2; x++)
            {
                if (_grid[x, y] == TileType.Air) _grid[x, y] = TileType.Stone;
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
                Vector3 chunkPosition = new Vector3(x * _chunkSize * _tileSizeMult + OffsetX, y * _chunkSize * _tileSizeMult + OffsetY, transform.position.z);

                GameObject chunkGO = Instantiate(_chunkPrefab, chunkPosition, Quaternion.identity, transform);
                chunkGO.transform.localScale = new Vector3(_tileSizeMult, _tileSizeMult, 1);
                chunkGO.layer = 8;
                chunkGO.name = $"Chunk_{x}_{y}";

                Chunk chunkScript = chunkGO.GetComponent<Chunk>();
                if (chunkScript != null)
                {
                    chunkScript.Initialize(this, chunkCoord, Settings.TerrainMaterial);
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

    public void FillTileInCircle(Vector3 worldPos, float radius, TileType type) => FillTileInCircle(WorldToTileX(worldPos.x), WorldToTileY(worldPos.y), Mathf.RoundToInt(radius * _tilesPerUnit), type);
    public void FillTileInCircle(int xCenter, int yCenter, int radius, TileType type) => AffectTileInCircle(xCenter, yCenter, radius, 0, type);

    public void DamageTileInCircle(Vector3 worldPos, float radius, float damage = 1) => DamageTileInCircle(WorldToTileX(worldPos.x), WorldToTileY(worldPos.y), Mathf.RoundToInt(radius * _tilesPerUnit), damage);
    public void DamageTileInCircle(int xCenter, int yCenter, int range, float damage = 1) => AffectTileInCircle(xCenter, yCenter, range, damage, TileType.Air);

    public void AffectTileInCircle(int xCenter, int yCenter, int range, float damage, TileType type)
    {
        int xMin = Mathf.Clamp(xCenter - range + 1, 0, _width - 1);
        int xMax = Mathf.Clamp(xCenter + range, 0, _width - 1);
        int yMin = Mathf.Clamp(yCenter - range + 1, 0, _height - 1);
        int yMax = Mathf.Clamp(yCenter + range, 0, _height - 1);

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                int xDelta = x - xCenter;
                int yDelta = y - yCenter;
                int sqrDistance = (xDelta * xDelta) + (yDelta * yDelta);

                if (sqrDistance < range * (range - 1))
                {
                    if (damage > 0) DamageTileAt(x, y, damage);
                    else if (type != TileType.Air) FillTileAt(x, y, type);
                }
            }
        }
    }

    public void FillTileAt(Vector3 worldPos, TileType type) => FillTileAt(WorldToTileX(worldPos.x), WorldToTileY(worldPos.y), type);
    public void FillTileAt(int x, int y, TileType type)
    {
        if (_grid[x, y] != TileType.Air) return;
        // If too close to player
        if (Vector3.Distance(TileToWorld(x, y), _player.transform.position) < 1f) return;

        _grid[x, y] = type;
        ModifyChunk(x, y);
    }

    public bool DamageTileAt(Vector3 worldPos, float damage = 1)
    {
        var x = WorldToTileX(worldPos.x);
        var y = WorldToTileY(worldPos.y);
        _damageRegion = TileToWorld(x, y);
        _damageRegionSize = _tileSizeMult * 0.5f;
        bool success = DamageTileAt(x, y, damage);

        // If tile not damaged, Check if worldPos was close to edge. If so, destroy adjacent tile
        if (!success)
        {
            float checkValue = ((worldPos.x - OffsetX) * _tilesPerUnit % 1 - 0.5f);
            int offX = checkValue < 0 ? -1 : 1;
            if (checkValue < 0) checkValue = -checkValue;

            if (checkValue > 0.45f) success = DamageTileAt(x + offX, y, damage);

            if (!success)
            {
                checkValue = ((worldPos.y - OffsetX) * _tilesPerUnit % 1 - 0.5f);
                int offY = checkValue < 0 ? -1 : 1;
                if (checkValue < 0) checkValue = -checkValue;

                if (checkValue > 0.45f) success = DamageTileAt(x, y + offY, damage);

                if (!success && checkValue > 0.45f && checkValue > 0.45f)
                {
                    success = DamageTileAt(x + offX, y + offY, damage);
                }
            }
        }
        return success;
    }
    public bool DamageTileAt(int x, int y, float damage = 1)
    {
        if (GetTile(x, y) == TileType.Air)
        {
            return false;
        }
        var data = Settings.GetTileData(_grid[x, y]);

        var coord = new Vector2Int(x, y);
        float currentDamage = 0;
        bool hasDamage = _tileDamageData.TryGetValue(coord, out currentDamage);
        if (data.TileHealth <= currentDamage + damage)
        {
            DestroyTileAt(x, y, data.ItemData);
            if (hasDamage) _tileDamageData.Remove(coord);
        }
        else
        {
            if (hasDamage) _tileDamageData[coord] = currentDamage + damage;
            else _tileDamageData.Add(coord, currentDamage + damage);
            ModifyChunk(x, y);
        }
        return true;
    }

    public float GetDamage(int x, int y)
    {
        float damage = 0;
        _tileDamageData.TryGetValue(new Vector2Int(x, y), out damage);
        return damage;
    }

    private void DestroyTileAt(int x, int y, ItemData item)
    {
        _grid[x, y] = TileType.Air;

        var pos = TileToWorld(x, y) + (Vector3)(Random.insideUnitCircle * _tileSizeMult * 0.5f);
        _itemManager.SpawnDroppedItem(pos, item);

        ModifyChunk(x, y);
        OnTileDestroyed?.Invoke(x, y);
    }

    private void ModifyChunk(int x, int y)
    {
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
                Vector3 center = new Vector3(_tileSizeMult * ((x * _chunkSize) + _chunkSize * 0.5f) + OffsetX,
                                             _tileSizeMult * ((y * _chunkSize) + _chunkSize * 0.5f) + OffsetY, transform.position.z - 0.5f);
                Vector3 size = new Vector3(_chunkSize * _tileSizeMult, _chunkSize * _tileSizeMult, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_damageRegion, new Vector3(_damageRegionSize*2, _damageRegionSize*2, 1));
    }
#endif
}