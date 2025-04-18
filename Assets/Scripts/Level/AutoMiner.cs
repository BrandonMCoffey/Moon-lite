using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMiner : PlaceableBase
{
    [SerializeField] private int _width = 2;
    [SerializeField] private float _destroyTime;

    private float _destroyTimer;
    private int _currentTileX;

    private void Update()
    {
        _destroyTimer += Time.deltaTime;
        if (_destroyTimer > _destroyTime)
        {
            _destroyTimer -= _destroyTime;
            DestroyNextTile();
        }
    }

    private void DestroyNextTile()
    {
        _currentTileX++;
        if (_currentTileX >= _width) _currentTileX = 0;

        _world.DamageTileAt(transform.position + Vector3.down * _world.TileSizeMult + Vector3.right * (_currentTileX * _world.TileSizeMult), 1);
    }
}
