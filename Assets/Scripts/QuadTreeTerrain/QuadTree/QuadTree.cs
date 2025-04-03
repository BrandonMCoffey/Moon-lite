using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree : IEnumerable<Quad>
{
    private TerrainData _terrainData;
    private Quad _root;
    
    public QuadTree(TerrainData terrainData)
    {
        _terrainData = terrainData;
        _root = new Quad(0,0, terrainData.Width ,  terrainData.Height);
        Stack<Quad> quadsToCheck = new Stack<Quad>();
        quadsToCheck.Push(_root);
        while (quadsToCheck.TryPop(out Quad quad))
        {
            if (!terrainData.IsQuadUniform(quad))
            {
                Quad[] children = quad.Subdivide();
                foreach (Quad child in children)
                {
                    if (child.IsDivisible())
                    {
                        quadsToCheck.Push(child);
                    }
                }
            }
        }
    }

    public void UpdateQuadTree(TerrainData terrainData, int centerX, int centerY, int range)
    {
        // TODO: Update only affected region instead of rebuilding entire quadtree
    }

    public IEnumerator<Quad> GetEnumerator()
    {
        return new QuadEnumerator(_root);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

#if UNITY_EDITOR
    public void DrawGizmos(float pixelsPerUnit)
    {
        Stack<Quad> quadsToDraw = new Stack<Quad>();
        quadsToDraw.Push(_root);
        foreach (Quad quad in this)
        {
            if (!quad.HasChildren)
            {
                bool isSolid = _terrainData.IsSolid(quad);
                Color color = isSolid ? Color.red : Color.blue;
                DrawQuadGizmos(quad, color, pixelsPerUnit);
            }
        }
    }

    private void DrawQuadGizmos(Quad quad, Color color, float pixelsPerUnit)
    {
        float x = quad.XAdress + (quad.Width / 2f);
        float y = quad.YAdress + (quad.Height / 2f); ;
        Vector3 center = new Vector3(x, y, 0) / pixelsPerUnit;
        Vector3 size = new Vector3(quad.Width, quad.Height) / pixelsPerUnit;
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
        color.a = 0.4f;
        Gizmos.color = color;
        Gizmos.DrawCube(center, size);
    }
#endif
}
