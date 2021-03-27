using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainPreview))]
public class TerrainPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainPreview generator = (TerrainPreview)target;

        if (DrawDefaultInspector())
        {
            if (generator.autoUpdate)
            {
                generator.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            generator.DrawMapInEditor();
        }
    }
}
