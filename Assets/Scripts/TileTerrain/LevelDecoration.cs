using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDecoration : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private bool _flipCeilingSprites;
    [SerializeField] private bool _randomFlipX;
    [SerializeField] private List<Material> _grassMaterials = new List<Material>();
    [SerializeField] private List<Material> _caveFloorSprites = new List<Material>();
    [SerializeField] private List<Material> _caveCeilingSprites = new List<Material>();
    [SerializeField] private int _connectedTileX;
    [SerializeField] private int _connectedTileY;

    private void OnValidate()
    {
        if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
    }

    public void ConnectToTile(int x, int y)
    {
        _connectedTileX = x;
        _connectedTileY = y;
        WorldGenerator.OnTileDestroyed += ConnectedTileDestroyed;
    }

    private void ConnectedTileDestroyed(int x, int y)
    {
        if (x == _connectedTileX && y == _connectedTileY)
        {
            Destroy(gameObject);
        }
    }

    public void RandomizeGrass()
    {
        _renderer.material = GetRandomMaterial(_grassMaterials);
        transform.localScale = new Vector3(_randomFlipX && Random.value > 0.5f ? -1f : 1f, 1f, 1f);
    }

    public void RandomizeCave(bool floor)
    {
        _renderer.material = GetRandomMaterial(floor ? _caveFloorSprites : _caveCeilingSprites);
        transform.localScale = new Vector3(_randomFlipX && Random.value > 0.5f ? -1f : 1f, !floor && _flipCeilingSprites ? -1f : 1f, 1f);
    }

    private Material GetRandomMaterial(List<Material> mats) => mats[Random.Range(0, mats.Count - 1)];
}
