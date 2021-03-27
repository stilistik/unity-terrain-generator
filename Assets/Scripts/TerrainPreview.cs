using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public enum DrawMode { None, NoiseMap, Mesh };
    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;
    public Material terrainMaterial;

    [Header("Debug Settings")]
    public DrawMode drawMode;
    public LOD editorPreviewLod;
    public bool autoUpdate;

    private HeightMap heightMap;

    public void DrawTexture(Texture2D texture)
    {
        meshRenderer.gameObject.SetActive(false);
        textureRenderer.gameObject.SetActive(true);
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;
    }

    public void DrawMesh(MeshData meshData)
    {
        meshRenderer.gameObject.SetActive(true);
        textureRenderer.gameObject.SetActive(false);
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    public void Clear()
    {
        meshRenderer.gameObject.SetActive(false);
        textureRenderer.gameObject.SetActive(false);
    }

    void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, new Vector2(0, 0));

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLod));
        }
        else
        {
            Clear();
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
