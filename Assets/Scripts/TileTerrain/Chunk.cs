using UnityEngine;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _collider;
    private Mesh _mesh;

    private WorldGenerator _world;
    private Vector2Int _chunkCoord;
    private float _uvSize = 0.125f;

    public WorldGenerator World => _world;

    public void Initialize(WorldGenerator worldGenerator, Vector2Int coordinate, Material material)
    {
        _world = worldGenerator;
        _chunkCoord = coordinate;

        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<MeshCollider>();

        _meshRenderer.material = material;

        GenerateMesh();
    }

    public void UpdateChunk() => GenerateMesh();
    private void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int vertexIndex = 0;

        int startX = _chunkCoord.x * _world.ChunkSize;
        int startY = _chunkCoord.y * _world.ChunkSize;
        _uvSize = 1f / _world.Settings.GridSize;

        float borderOffset = 0f;

        // TODO: Quad Tree Optimizations (Combine matching neighbors)
        // TODO: Merge matching vertices
        for (int chunkY = 0; chunkY < _world.ChunkSize; chunkY++)
        {
            for (int chunkX = 0; chunkX < _world.ChunkSize; chunkX++)
            {
                int worldX = startX + chunkX;
                int worldY = startY + chunkY;

                TileType currentTile = _world.GetTile(worldX, worldY);

                if (currentTile != TileType.Air)
                {
                    // Choose int based on data min/max and percent of damage done
                    int damage = Mathf.CeilToInt(_world.GetDamage(worldX, worldY));

                    float x = chunkX + borderOffset;
                    float y = chunkY + borderOffset;
                    float unit = 1 - borderOffset;

                    // Check Top Face
                    if (_world.GetTile(worldX, worldY + 1) == TileType.Air)
                    {
                        AddFace(ref vertices, ref triangles, ref uvs, ref vertexIndex,
                                new Vector3(x, y + unit, 0),     // Top-Left
                                new Vector3(x + unit, y + unit, 0), // Top-Right
                                new Vector3(x, y + unit, -1),    // Back Top-Left (for thickness)
                                new Vector3(x + unit, y + unit, -1),// Back Top-Right (for thickness)
                                currentTile, false, false, damage);
                    }

                    // Check Bottom Face
                    if (_world.GetTile(worldX, worldY - 1) == TileType.Air)
                    {
                        AddFace(ref vertices, ref triangles, ref uvs, ref vertexIndex,
                               new Vector3(x, y, -unit),     // Back Bottom-Left
                               new Vector3(x + unit, y, -1), // Back Bottom-Right
                               new Vector3(x, y, 0),      // Front Bottom-Left
                               new Vector3(x + unit, y, 0),  // Front Bottom-Right
                               currentTile, false, false, damage);
                    }

                    // Check Right Face
                    if (_world.GetTile(worldX + 1, worldY) == TileType.Air)
                    {
                        AddFace(ref vertices, ref triangles, ref uvs, ref vertexIndex,
                                new Vector3(x + unit, y, 0),      // Front Bottom-Right
                                new Vector3(x + unit, y + 1, 0),  // Front Top-Right
                                new Vector3(x + unit, y, -1),     // Back Bottom-Right
                                new Vector3(x + unit, y + unit, -1), // Back Top-Right
                                currentTile, true, false, damage);
                    }

                    // Check Left Face
                    if (_world.GetTile(worldX - 1, worldY) == TileType.Air)
                    {
                        AddFace(ref vertices, ref triangles, ref uvs, ref vertexIndex,
                                new Vector3(x, y + unit, -1), // Back Top-Left
                                new Vector3(x, y, -1),     // Back Bottom-Left
                                new Vector3(x, y + unit, 0),  // Front Top-Left
                                new Vector3(x, y, 0),      // Front Bottom-Left
                                currentTile, false, false, damage);
                    }

                    // Front Face
                    AddFace(ref vertices, ref triangles, ref uvs, ref vertexIndex,
                               new Vector3(x, y, -1),         // Bottom-Left
                               new Vector3(x + unit, y, -1),     // Bottom-Right
                               new Vector3(x, y + unit, -1),     // Top-Left
                               new Vector3(x + unit, y + unit, -1), // Top-Right
                               currentTile, true, true, damage);
                }
            }
        }

        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = $"Chunk_{_chunkCoord.x}_{_chunkCoord.y}_Mesh";
        }
        else
        {
            _mesh.Clear();
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.uv = uvs.ToArray();

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
        _collider.sharedMesh = _mesh;
    }

    public void OptimizeChunkMesh()
    {
        _mesh.Optimize();
    }

    private void AddFace(ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs, ref int vertexIndex,
                         Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr,
                         TileType tileType, bool reverse, bool front, int damage)
    {
        // Add Vertices
        vertices.Add(bl); // 0
        vertices.Add(br); // 1
        vertices.Add(tl); // 2
        vertices.Add(tr); // 3

        // Add Triangles
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + (reverse ? 2 : 1));
        triangles.Add(vertexIndex + (reverse ? 1 : 2));

        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + (reverse ? 2 : 3));
        triangles.Add(vertexIndex + (reverse ? 3 : 2));

        vertexIndex += 4;

        // Add UVs
        Vector2[] tileUVs = GetUVsForTile(tileType, reverse, front, damage);
        uvs.AddRange(tileUVs);
    }

    private Vector2[] GetUVsForTile(TileType type, bool reverse, bool front, int damage)
    {
        // TODO: Optimize this, its expensive atm
        var data = _world.Settings.GetTileData(type);
        Vector2 uvCoord = data.UVCoords;
        if (damage > 0)
        {
            int offset = Mathf.RoundToInt(data.DamageUVCount * (damage / data.TileHealth));
            uvCoord.x += _world.Settings.DamageOffsetUV.x * offset;
            uvCoord.y += _world.Settings.DamageOffsetUV.y * offset;
        }
        uvCoord.y = _world.Settings.GridSize - 1 - uvCoord.y;
        if (!front && type == TileType.Grass) uvCoord.y -= 1;

        float border = _uvSize * 0.02f;

        float uMin = uvCoord.x * _uvSize + border;
        float vMin = uvCoord.y * _uvSize + border;
        float uMax = uMin + _uvSize - border * 2;
        float vMax = vMin + _uvSize - border * 2;

        // Return UV array matching the AddFace vertex order (BL, BR, TL, TR)
        // Adjust to change texture orientation
        if (reverse)
        {
            return new Vector2[] {
                new Vector2(uMin, vMin), // Bottom-Left UV
                new Vector2(uMax, vMin), // Bottom-Right UV
                new Vector2(uMin, vMax), // Top-Left UV
                new Vector2(uMax, vMax), // Top-Right UV
            };
        }
        else
        {
            return new Vector2[] {
                new Vector2(uMin, vMin), // Bottom-Left UV
                new Vector2(uMin, vMax), // Top-Left UV
                new Vector2(uMax, vMin), // Bottom-Right UV
                new Vector2(uMax, vMax), // Top-Right UV
            };
        }
    }
}