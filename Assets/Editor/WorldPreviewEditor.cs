using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldPreview))]
public class WorldPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WorldPreview preview = (WorldPreview)target;

        if (DrawDefaultInspector())
        {
            if (preview.autoUpdate)
            {
                preview.UpdatePreview();
            }
        }

        if (GUILayout.Button("Update"))
        {
            preview.UpdatePreview();
        }
    }
}
