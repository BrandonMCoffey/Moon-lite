using Sirenix.OdinInspector;
using UnityEngine;

public class TerrainClickToDestroy : MonoBehaviour
{
    [SerializeField] private WorldGenerator _world;
    [SerializeField] private bool _single;
    [SerializeField] private float _radius = 10;
    [SerializeField] private float _destroyTime = 0.1f;

    private float _destroyTimer;

    private void Awake()
    {
        if (_world == null) _world = FindObjectOfType<WorldGenerator>();
    }

    private void Update()
    {
        if (PlayerInput.MouseLeftPressed)
        {
            _destroyTimer -= Time.deltaTime;
            if (_destroyTimer < 0)
            {
                _destroyTimer = _destroyTime;
                var pos = PlayerInput.MousePositionWorld(transform.position.z);
                if (_single) _world.DamageTileAt(pos);
                else _world.DamageTileInCircle(pos, _radius);
            }
        }
    }
}