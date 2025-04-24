using Sirenix.OdinInspector;
using UnityEngine;

public class TempPlatform : PlaceableBase
{
    [SerializeField] private int _maxRangeToBuild = 20;
    [SerializeField] private float _buildSectionTime = 0.25f;
    [SerializeField] private Transform _platformSegment;

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _validPlacement;
    [SerializeField, ReadOnly] private bool _done;
    [SerializeField, ReadOnly] private int _leftDistance = -1;
    [SerializeField, ReadOnly] private int _rightDistance = -1;
    [SerializeField, ReadOnly] private float _buildTimer;
    [SerializeField, ReadOnly] private float _buildDistance;

    public override void Place(WorldGenerator world)
    {
        base.Place(world);
        transform.position -= Vector3.down * world.TileSizeMult;

        int x = _world.WorldToTileX(transform.position.x);
        int y = _world.WorldToTileY(transform.position.y);

        bool leftValid = false;
        bool rightValid = false;

        for (int i = 0; i <= _maxRangeToBuild; i++)
        {
            if (!leftValid && _world.GetTile(x - i, y) != TileType.Air)
            {
                _leftDistance = i;
                leftValid = true;
            }
            if (!rightValid && _world.GetTile(x + i, y) != TileType.Air)
            {
                _rightDistance = i;
                rightValid = true;
            }
        }
        if (!leftValid) _leftDistance = -1;
        if (!rightValid) _rightDistance = -1;
        _validPlacement = leftValid && rightValid;
        _buildTimer = _buildSectionTime;
        if (!_validPlacement)
        {
            // TODO: Feedback?
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!_validPlacement || _done)
        {
            return;
        }

        _buildTimer -= Time.deltaTime;
        if (_buildTimer <= 0)
        {
            _buildTimer += _buildSectionTime;
            _buildDistance++;
            if (_buildDistance <= _leftDistance)
            {
                var pos = _platformSegment.position - _platformSegment.right * _buildDistance;
                var segment = Instantiate(_platformSegment, pos, _platformSegment.rotation, transform);
            }
            if (_buildDistance <= _rightDistance)
            {
                var pos = _platformSegment.position + _platformSegment.right * _buildDistance;
                var segment = Instantiate(_platformSegment, pos, _platformSegment.rotation, transform);
            }
            if (_buildDistance > _leftDistance && _buildDistance > _rightDistance)
            {
                _done = true;
            }
        }
    }
}
