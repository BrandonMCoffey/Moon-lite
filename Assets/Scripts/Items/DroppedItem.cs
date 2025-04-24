using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private float _lifespan = 100f;
    [SerializeField] private float _despawnRange = 20f;
    [SerializeField] private float _mergeUpdateTime = 0.5f;
    [SerializeField] private float _mergeRadius = 0.25f;
    [SerializeField] private LayerMask _groundLayer = 1;
    [SerializeField] private float _radius = 0.1f;

    [SerializeField] private SpriteRenderer _renderer;

    private Player _player;
    private ItemData _item;
    private int _itemCount;
    private float _timer;
    private float _mergeTimer = 0f;
    private static Collider[] nearbyColliders = new Collider[10];
    private float _velocity = 0f;
    private bool _grounded = false;

    private void OnValidate()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _player = Player.Instance;
        _mergeTimer = Random.value * _mergeUpdateTime;
        _mergeRadius += Random.Range(-0.05f, 0.05f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _mergeTimer += Time.deltaTime;

        if (_mergeTimer >= _mergeUpdateTime)
        {
            _mergeTimer = 0f;
            CheckMergeNearby();
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
            transform.position = Vector3.MoveTowards(transform.position, pos, _player.MagnetItemSpeed * Time.deltaTime);
            if (transform.position.y > pos.y) ApplyGravity();
        }
        else if (_timer > _lifespan || dist >= _despawnRange)
        {
            DroppedItemManager.AddItemToPool(this);
            return;
        }
        else
        {
            ApplyGravity();
        }
    }

    public void InitItem(Vector3 position, ItemData data)
    {
        transform.position = position;
        _item = data;
        _itemCount = 1;
        _renderer.sprite = data.Sprite;
        _renderer.color = data.SpriteColor;
        _timer = 0;
        gameObject.SetActive(true);
    }

    private void CheckMergeNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _mergeRadius, nearbyColliders);

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

    private void ApplyGravity()
    {
        float offset = _velocity * Time.deltaTime;
        // TODO: This is expensive because a lot of items are called. Maybe move to fixed update or every other frame?
        _grounded = Physics.Raycast(transform.position, Vector3.down, out var hit, offset + _radius, _groundLayer);

        if (_grounded)
        {
            _velocity = 0f;
            if (hit.distance > _radius)
            {
                transform.position += new Vector3(0f, hit.distance - _radius, 0f);
            }
        }
        else
        {
            _velocity += -9.81f * Time.deltaTime;
            _velocity = Mathf.Max(_velocity, -20f);
            transform.position += new Vector3(0, offset, 0);
        }
    }
}
