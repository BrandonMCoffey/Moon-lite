using UnityEngine;

public class TerrainData
{
    private bool [,] _points;
    private int _width;
    private int _height;

    public int Width => _width;
    public int Height => _height;

    public void DrawGizmos(float pixelsPerUnit)
    {
        if (_points != null)
        {
            float mult = 1f / pixelsPerUnit;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f, y + 0.5f, 0) * mult;
                    Gizmos.color = _points[x, y] ? Color.red : Color.blue;

                    Gizmos.DrawSphere(position, 0.5f / pixelsPerUnit);
                }
            }
        }
    }

    public TerrainData(int width, int height)
    {
        _width = width;
        _height = height;
        _points = new bool[_width, _height];
        float off = Random.value * 10;
        for (int x = 0; x < _width; x++)
        {
            float s = Mathf.Sin(x * 0.01f - off);
            off += (Random.value - 0.4f) * 0.2f;
            for (int y = 0; y < _height; y++)
            {
                _points[x, y] = !(s * 10 < y - _height + 15);
            }
        }
        for (int i = 0; i < 10; i++)
        {
            int xCenter = (int)(Random.Range(0.15f, 0.85f) * _width);
            int yCenter = _height - (i + 3) * Random.Range(20, 30);
            DestroyTerrain(xCenter, yCenter, Random.Range(25, 50));
        }
    }
    
    public bool IsQuadUniform(Quad quad)
    {
        bool isOriginSolid = _points[quad.XAdress, quad.YAdress];
        for (int xDelta = 0; xDelta <= quad.Width-1; xDelta++)
        {
            for (int yDelta = 0; yDelta <= quad.Height-1; yDelta++)
            {
                bool isOtherSolid = _points[quad.XAdress + xDelta, quad.YAdress + yDelta];
                if (isOriginSolid != isOtherSolid)
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    public bool IsSolid(Quad quad)
    {
        return _points[quad.XAdress, quad.YAdress];
    }

    public void DestroyTerrain(int centerX, int centerY, int range)
    {
        int xOrigin = Mathf.Max(0, centerX - range);
        int xEnd =  Mathf.Min(_width-1,centerX + range);
        int yOrigin = Mathf.Max(0,centerY - range);
        int yEnd =  Mathf.Min(_height-1, centerY + range);

        for (int x = xOrigin; x < xEnd; x++)
        {
            for (int y = yOrigin; y < yEnd; y++)
            {
                int xDelta = x - centerX;
                int yDelta = y - centerY;
                float sqrDistance = (xDelta * xDelta) + (yDelta*yDelta);

                if (sqrDistance < range * range)
                {
                    _points[x, y] = false;
                }
            }
        }
    }
}
