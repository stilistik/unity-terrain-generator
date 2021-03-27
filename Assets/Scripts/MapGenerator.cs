using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { None, NoiseMap, Mesh };
    public enum LOD { One = 1, Two = 2, Four = 4, Six = 6, Eight = 8 }

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;
    public Material terrainMaterial;

    [Header("Debug Settings")]
    public DrawMode drawMode;
    public LOD editorPreviewLod;
    public bool autoUpdate;

    private HeightMap heightMap;



    void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

    public void DrawMapInEditor()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, new Vector2(0, 0));

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLod));
        }
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }



    void OnValidate()
    {
        if (meshSettings != null)
        {
            // unsubscribe if already subscribed before resubscribing
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            // unsubscribe if already subscribed before resubscribing
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
