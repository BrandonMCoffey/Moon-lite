using System.Collections.Generic;
using UnityEngine;

public class LevelDecoration : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private bool _flipCaveFloorSprites;
    [SerializeField] private bool _flipCaveCeilingSprites;
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

    private void OnDestroy()
    {
        WorldGenerator.OnTileDestroyed -= ConnectedTileDestroyed;
    }

    public void ConnectToTile(int x, int y)
    {
        _connectedTileX = x;
        _connectedTileY = y;
        // TODO: Does this need to be optimized?
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
        transform.position += transform.forward * 0.25f;
        transform.localScale = new Vector3(_randomFlipX && Random.value > 0.5f ? -1f : 1f, 1f, 1f);
    }

    public void RandomizeCave(bool floor)
    {
        _renderer.material = GetRandomMaterial(floor ? _caveFloorSprites : _caveCeilingSprites);
        transform.position += transform.forward * 0.25f;
        float x = _randomFlipX && Random.value > 0.5f ? -1f : 1f;
        float y = (floor && _flipCaveFloorSprites) || (!floor && _flipCaveCeilingSprites) ? -1f : 1f;
        transform.localScale = new Vector3(x, y, 1f);
    }

    private Material GetRandomMaterial(List<Material> mats) => mats[Random.Range(0, mats.Count - 1)];
}
