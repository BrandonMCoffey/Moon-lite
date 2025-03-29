using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private List<Vector3> _normals;

    public MeshBuilder()
    {
        _triangles = new List<int>();
        _vertices = new List<Vector3>();
        _uvs = new List<Vector2>();
        _normals = new List<Vector3>();
    }
    
    public Mesh ConstructMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.normals = _normals.ToArray();
        mesh.uv = _uvs.ToArray();
        return mesh;
    }

    public void AddMeshSection(VertexData vertexA, VertexData vertexB, VertexData vertexC)
    {
        Vector3 normal = VertexUtility.ComputeNormal(vertexA, vertexB, vertexC);
        
        vertexA.Normal = normal;
        vertexB.Normal = normal;
        vertexC.Normal = normal;
            
        int indexA = AddVertex(vertexA);
        int indexB = AddVertex(vertexB);
        int indexC = AddVertex(vertexC);
        
        AddTriangle(indexA, indexB, indexC);
    }

    private void AddTriangle(int indexA, int indexB, int indexC)
    {
        _triangles.Add(indexA);
        _triangles.Add(indexB);
        _triangles.Add(indexC);
    }

    private int AddVertex(VertexData vertex)
    {
        _vertices.Add(vertex.Postion);
        _uvs.Add(vertex.Uv);
        _normals.Add(vertex.Normal);
        return _vertices.Count - 1;
    }
}
