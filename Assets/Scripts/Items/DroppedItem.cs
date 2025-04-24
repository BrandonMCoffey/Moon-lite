using Sirenix.OdinInspector;
using UnityEngine;

public class DroppedItem : SimplePhysicsObject
{
    [Header("Dropped Item Settings")]
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private float _lifespan = 100f;
    [SerializeField] private float _despawnRange = 20f;
    [SerializeField] private bool _canMerge = true;
    [SerializeField, ShowIf(nameof(_canMerge))] private float _mergeUpdateTime = 0.5f;
    [SerializeField, ShowIf(nameof(_canMerge))] private float _mergeRadius = 0.25f;
    [SerializeField, ShowIf(nameof(_canMerge))] private LayerMask _itemMergeLayer = 1;

    private Player _player;
    private ItemData _item;
    private int _itemCount;
    private float _lifeTimer;
    private float _mergeTimer = 0f;
    private static Collider[] nearbyColliders = new Collider[10];

    private void OnValidate()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
    }

    protected void Start()
    {
        _player = Player.Instance;
        _mergeTimer = Random.value * _mergeUpdateTime;
        _mergeRadius += Random.Range(-0.05f, 0.05f);
    }

    protected override void Update()
    {
        base.Update();
        _lifeTimer += Time.deltaTime;
        if (_canMerge)
        {
            _mergeTimer += Time.deltaTime;
            if (_mergeTimer >= _mergeUpdateTime)
            {
                _mergeTimer = 0f;
                CheckMergeNearby();
            }
        }

        var pos = _player.transform.position + Vector3.down * 0.25f;
        float dist = Vector3.Distance(transform.position, pos);
        if (dist <= _player.PickupItemRange)
        {
            _player.AddItemToInventory(_item, _itemCount);
            DroppedItemManager.AddItemToPool(this);
            return;
        }
        else if (dist <= _player.MagnetItemRange)
        {
            SetPhysicsEnabled(false);
            transform.position = Vector3.MoveTowards(transform.position, pos, _player.MagnetItemSpeed * Time.deltaTime);
            if (transform.position.y > pos.y) transform.position += Vector3.down * _gravityScale * Time.deltaTime;
        }
        else if (_lifeTimer > _lifespan || dist >= _despawnRange)
        {
            DroppedItemManager.AddItemToPool(this);
            return;
        }
        else
        {
            SetPhysicsEnabled(true);
        }
    }

    public void InitItem(Vector3 position, ItemData data)
    {
        transform.position = position;
        _item = data;
        _itemCount = 1;
        _renderer.sprite = data.Sprite;
        _renderer.color = data.SpriteColor;
        _lifeTimer = 0;
        ResetPhysics();
        gameObject.SetActive(true);
    }

    private void CheckMergeNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _mergeRadius, nearbyColliders, _itemMergeLayer);

        for (int i = 0; i < count; i++)
        {
            Collider col = nearbyColliders[i];
            if (col == null || col.gameObject == gameObject) continue;

            DroppedItem other = col.GetComponent<DroppedItem>();
            if (other != null && other._item == _item)
            {
                transform.position = (transform.position + other.transform.position) * 0.5f;
                _itemCount += other._itemCount;
                other.gameObject.SetActive(false);
                DroppedItemManager.AddItemToPool(other);
            }
        }
    }
}
