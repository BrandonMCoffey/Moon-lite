using UnityEngine;

public class TerrainClickToDestroy : MonoBehaviour
{
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
                    chunk.World.DestroyTileAt(pointToDestroy);
                }
            }
        }
    }
}