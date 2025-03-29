using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TerrainGenerator terrain  = target as TerrainGenerator;
        if (GUILayout.Button("Generate terrain")) 
        {
            terrain.GenerateTerrain();
            EditorUtility.SetDirty(terrain);
        }
    }
}
