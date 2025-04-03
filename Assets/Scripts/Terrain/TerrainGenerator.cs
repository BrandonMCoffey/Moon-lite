using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshCollider _meshCollider;
    [SerializeField] private int _width = 200;
    [SerializeField] private int _height = 200;
    [SerializeField] private float _pixelsPerUnit = 10f;
    [SerializeField] private float _destructionRadius =3f;

    private TerrainData _terrainData;
    private QuadTree _quadTree;
    private Camera _camera;
    private Vector3 _mousePosition = Vector3.zero;
    private float _destroyTimer = 0;

    private void Awake()
    {
        _camera = Camera.main;
        GenerateTerrain();
    }

    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_terrainData != null)
        {
            //_terrainData.DrawGizmos(_pixelsPerUnit);
        }
        if (_quadTree != null)
        {
            _quadTree.DrawGizmos(_pixelsPerUnit);
        }
        if (_mousePosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_mousePosition, _destructionRadius);
        }
    }

    private void Update()
    {
        if (_destroyTimer > 0)
        {
            _destroyTimer -= Time.deltaTime;
            return;
        }
        if (Input.GetMouseButton(0))
        {
            DestroyArea(_camera.ScreenToWorldPoint(Input.mousePosition));
            _destroyTimer = 0.08f;
        }
    }

    public void GenerateTerrain()
    {
        _terrainData = new TerrainData(_width, _height);
        _quadTree = new QuadTree(_terrainData);
        ConstructMeshes();
    }

    private void DestroyArea(Vector3 position)
    {
        int x = (int) (position.x * _pixelsPerUnit);
        int y = (int) (position.y * _pixelsPerUnit);
        int r = (int)(_destructionRadius * _pixelsPerUnit);
        _terrainData.DestroyTerrain(x, y, r);
        _quadTree.UpdateQuadTree(_terrainData, x, y, r);
        _quadTree = new QuadTree(_terrainData);
        ConstructMeshes();
    }

    private void ConstructMeshes()
    {
        MeshBuilder meshContructionHelper = new MeshBuilder();
        foreach (Quad quad in _quadTree)
        {
            if (!quad.HasChildren)
            {
                bool isACornerSolid = _terrainData.IsSolid(quad);

                if (isACornerSolid)
                {
                    ConstructQuad(quad, meshContructionHelper);
                }
            }
        }
        _meshFilter.mesh = meshContructionHelper.ConstructMesh();
        if (_meshCollider != null)
        {
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;
        }
    }
    
    private void ConstructQuad(Quad quad, MeshBuilder meshConstructionHelper)
    {
        VertexData[] vertices = new[]
        {
            CoordsToVertex(quad.XAdress, quad.YAdress),
            CoordsToVertex(quad.XAdress + quad.Width, quad.YAdress),
            CoordsToVertex(quad.XAdress + quad.Width, quad.YAdress + quad.Height),
            CoordsToVertex(quad.XAdress, quad.YAdress + quad.Height),
        };
        meshConstructionHelper.AddMeshSection(vertices[0], vertices[2], vertices[1]);
        meshConstructionHelper.AddMeshSection(vertices[0], vertices[3], vertices[2]);
    }
    
    private VertexData CoordsToVertex(int x, int y)
    {
        float uvX = (x / (float) _width);
        float uvY = (y / (float) _height);
        Vector3 position = new Vector3(x, y, 0) / (_pixelsPerUnit);
        return new VertexData()
        {
            Postion = position,
            Uv = new Vector2(uvX, uvY),
            Normal = transform.forward
        };
    }
}
