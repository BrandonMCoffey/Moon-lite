using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance => _instance;
    private static Player _instance;

    // TODO: Move this to where health is calculated
    public static System.Action<float> OnHealthChanged = delegate { };

    [SerializeField] private InventoryData _inventory;
    [SerializeField] private PlayerWeaponManager _weaponManager;
    [SerializeField] private PlayerMovementCustom _movement;
    [SerializeField] private float _magnetItemRange = 2.5f;
    [SerializeField] private float _magnetItemSpeed = 5f;
    [SerializeField] private float _pickupItemRange = 1f;

    [Header("Throwables and Placeables")]
    [SerializeField] private SimplePhysicsObject _glowstickPrefab;
    [SerializeField] private SimplePhysicsObject _dirtBombPrefab;
    [SerializeField] private PlaceableBase _tempPlatformPrefab;
    [SerializeField] private PlaceableBase _autoMinerPrefab;

    public static PlayerWeaponManager WeaponManager => Instance._weaponManager;
    public static PlayerMovementCustom Movement => Instance._movement;
    public float MagnetItemRange => _magnetItemRange;
    public float MagnetItemSpeed => _magnetItemSpeed;
    public float PickupItemRange => _pickupItemRange;

    private WorldGenerator _world;

    private void Awake()
    {
        _instance = this;
        if (_weaponManager == null) _weaponManager = GetComponent<PlayerWeaponManager>();
        if (_movement == null) _movement = GetComponent<PlayerMovementCustom>();
    }

    private void Start()
    {
        _world = WorldGenerator.Instance;
    }

    private void Update()
    {
        if (PlayerInput.Throwable01) Throw(_glowstickPrefab);
        if (PlayerInput.Throwable02) Throw(_dirtBombPrefab);
        if (PlayerInput.Placeable01) Place(_tempPlatformPrefab);
        if (PlayerInput.Placeable02) Place(_autoMinerPrefab);
    }

    public void AddItemToInventory(ItemData item, int count = 1) => _inventory.AddItemToInventory(item, count);
    public float GetInventoryCount(ItemData item) => _inventory.GetInventoryCount(item);

    private void Throw(SimplePhysicsObject prefab)
    {
        var pos = PlayerInput.MousePositionWorld(transform.position.z);
        var dir = (pos - transform.position).normalized;

        var throwable = Instantiate(prefab, transform.position + dir * 0.2f, Quaternion.identity);
        throwable.SetVelocity(dir * 10f);
    }

    private void Place(PlaceableBase prefab)
    {
        var pos = _world.TileToWorld(_world.WorldToTileX(transform.position.x), _world.WorldToTileY(transform.position.y));
        var placeable = Instantiate(prefab, pos, Quaternion.identity);
        placeable.Place(_world);
    }
}
