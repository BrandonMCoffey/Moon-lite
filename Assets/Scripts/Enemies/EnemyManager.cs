using System.Collections;
using System.Collections.Generic;
using SaiUtils.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private int _maxEnemies = 20;
    [SerializeField] private float _minSpawnRange = 10;
    [SerializeField] private float _maxSpawnRange = 30;
    [SerializeField] private float _despawnRange = 50;
    [SerializeField] private float _minDistBetweenSpawner = 2.5f;
    [SerializeField] private float _updateTime = 0.5f;
    [SerializeField] private Vector3 _playerOffset = Vector3.down * 5; // Assume player typically moves down

    [SerializeField, ReadOnly] private int _spawnPositionCount;

    private Transform _player;
    // TODO: Split into layers to not iterate through all spawn positions
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private List<Transform> _enemies = new List<Transform>();
    private float _updateTimer;
    public WorldGenerator World { get; set; }

    private Vector3 PlayerPosition => _player.position + _playerOffset;

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void Update()
    {
        if (_updateTimer > 0f)
        {
            _updateTimer -= Time.deltaTime;
            return;
        }
        _updateTimer = _updateTime;
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (enemy == null)
            {
                _enemies.RemoveAt(i);
            }
            else if (Vector3.Distance(enemy.position, PlayerPosition) > _despawnRange)
            {
                _enemies.RemoveAt(i);
                Destroy(enemy.gameObject);
            }
        }
        if (_enemies.Count >= _maxEnemies) return;

        SpawnEnemies(Mathf.Max(Random.Range(_maxEnemies / 2, _maxEnemies) - _enemies.Count, 1));
    }

    private void SpawnEnemies(int maxToSpawn)
    {
        int spawned = 0;
        // Reverse for loop bc we are removing items from the list and it prioritizes spawning enemies under the player
        for (int i = _spawnPositions.Count - 1; i >= 0; i--)
        {
            // Reduces vector3.distance calls
            if (Random.value < World.Settings.EnemySpawnChance)
            {
                float dist = Vector3.Distance(_spawnPositions[i], PlayerPosition);
                if (dist >= _minSpawnRange && dist <= _maxSpawnRange)
                {
                    SpawnEnemy(_spawnPositions[i]);
                    _spawnPositions.RemoveAt(i);
                    if (++spawned >= maxToSpawn) break;
                }
            }
        }
    }

    private void SpawnEnemy(Vector3 pos)
    {
        var obj = World.Settings.GetRandomEnemyPrefab();
        var enemy = Instantiate(obj, pos, Quaternion.identity, transform);
        _enemies.Add(enemy.transform);
    }

    public void AddSpawnPosition(Vector3 spawnPosition)
    {
        foreach (var spawn in _spawnPositions)
        {
            if (Vector3.Distance(spawn, spawnPosition) < _minDistBetweenSpawner) return;
        }
        _spawnPositions.Add(spawnPosition);
        _spawnPositionCount++;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var spawnPosition in _spawnPositions)
        {
            Gizmos.DrawSphere(spawnPosition, 0.5f);
        }
    }
}
