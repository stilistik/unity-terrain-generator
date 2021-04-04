using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreview : MonoBehaviour
{

    public enum DrawMode { None, NoiseMap, Chunk };
    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;
    public Material terrainMaterial;
    public Material waterMaterial;


    [Header("Debug Settings")]
    public DrawMode drawMode;
    public LOD editorPreviewLod;
    public bool autoUpdate;
    public Renderer textureRenderer;


    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.gameObject.SetActive(true);
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;
    }

    public void DrawChunk()
    {
        var chunks = GameObject.FindGameObjectsWithTag("WorldChunk");
        foreach (GameObject chunk in chunks)
        {
            DestroyImmediate(chunk);
        }
        textureRenderer.gameObject.SetActive(false);

        LODSetting[] lodSettings = { new LODSetting(editorPreviewLod, int.MaxValue) };
    }

    public void Clear()
    {
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
        else if (drawMode == DrawMode.Chunk)
        {
            DrawChunk();
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
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
