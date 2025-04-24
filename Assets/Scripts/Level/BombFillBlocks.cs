using UnityEngine;

public class BombFillBlocks : SimplePhysicsObject
{
    [Header("Bomb Fill Settings")]
    [SerializeField] private float _fillRadius = 2;
    [SerializeField] private int _bouncesBeforeFilling = 0;
    [SerializeField] private TileType _fillType = TileType.Dirt;

    protected override void OnHit(RaycastHit hit)
    {
        if (_bouncesBeforeFilling > 0)
        {
            _bouncesBeforeFilling--;
            base.OnHit(hit);
            return;
        }

        var chunk = hit.collider.GetComponent<Chunk>();
        if (chunk != null)
        {
            chunk.World.FillTileInCircle(hit.point + transform.forward * 0.01f, _fillRadius, _fillType);
            Destroy(gameObject);
        }
        else
        {
            base.OnHit(hit);
        }
    }
}
