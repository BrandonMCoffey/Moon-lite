using Sirenix.OdinInspector;
using UnityEngine;

public class TerrainClickToDestroy : MonoBehaviour
{
    [SerializeField] private bool _single;
    [SerializeField] private float _radius = 10;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                var chunk = hit.collider.GetComponent<Chunk>();
                if (chunk != null)
                {
                    Vector3 pointToDestroy = hit.point - (hit.normal * 0.1f);
                    if (_single)
                    {
                        chunk.World.DestroyTileAt(pointToDestroy);
                    }
                    else
                    {
                        chunk.World.DestroyTileAt(pointToDestroy, _radius);
                    }
                }
            }
        }
    }
}