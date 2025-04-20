using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance => _instance;
    private static Player _instance;

    [SerializeField] private InventoryData _inventory;
    [SerializeField] private float _magnetItemRange = 2.5f;
    [SerializeField] private float _magnetItemSpeed = 5f;
    [SerializeField] private float _pickupItemRange = 1f;

    [Header("Throwables and Placeables")]
    [SerializeField] private KeyCode _throwGlowstick = KeyCode.G;
    [SerializeField] private KeyCode _throwDirtBomb = KeyCode.B;
    [SerializeField] private KeyCode _buildTempPlatform = KeyCode.C;
    [SerializeField] private KeyCode _buildAutoMine = KeyCode.Z;
    [SerializeField] private ThrowableBase _glowstickPrefab;
    [SerializeField] private ThrowableBase _dirtBombPrefab;
    [SerializeField] private PlaceableBase _tempPlatformPrefab;
    [SerializeField] private PlaceableBase _autoMinerPrefab;

    public float MagnetItemRange => _magnetItemRange;
    public float MagnetItemSpeed => _magnetItemSpeed;
    public float PickupItemRange => _pickupItemRange;

    private WorldGenerator _world;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _world = WorldGenerator.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_throwGlowstick)) Throw(_glowstickPrefab);
        if (Input.GetKeyDown(_throwDirtBomb)) Throw(_dirtBombPrefab);
        if (Input.GetKeyDown(_buildTempPlatform)) Place(_tempPlatformPrefab);
        if (Input.GetKeyDown(_buildAutoMine)) Place(_autoMinerPrefab);
    }

    public void AddItemToInventory(ItemData item, int count = 1) => _inventory.AddItemToInventory(item, count);
    public int GetInventoryCount(ItemData item) => _inventory.GetInventoryCount(item);

    private void Throw(ThrowableBase prefab)
    {
        // TODO: Make this better? How to get mouse position?
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xy = new Plane(-Camera.main.transform.forward, new Vector3(0, 0, transform.position.z));
        xy.Raycast(ray, out float distance);
        var pos = ray.GetPoint(distance);

        var dir = (pos - transform.position).normalized;

        var throwable = Instantiate(prefab, transform.position + dir * 0.2f, Quaternion.identity);
        throwable.Throw(dir, 10f);
    }

    private void Place(PlaceableBase prefab)
    {
        var pos = _world.TileToWorld(_world.WorldToTileX(transform.position.x), _world.WorldToTileY(transform.position.y));
        var placeable = Instantiate(prefab, pos, Quaternion.identity);
        placeable.Place(_world);
    }
}
