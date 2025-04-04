using UnityEngine;

public class TerrainClickToDestroy : MonoBehaviour
{
    [SerializeField] private WorldGenerator worldGenerator;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                Chunk chunk = hit.collider.GetComponent<Chunk>();
                if (chunk != null && worldGenerator != null)
                {
                    Vector3 pointToDestroy = hit.point - (hit.normal * 0.1f);
                    worldGenerator.DestroyTileAt(pointToDestroy);
                }
            }
        }
    }
}