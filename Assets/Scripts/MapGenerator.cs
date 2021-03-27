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

    Queue<MapThreadInfo<HeightMap>> mapDataQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> meshDataQueue = new Queue<MapThreadInfo<MeshData>>();

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

    void Update()
    {
        if (mapDataQueue.Count > 0)
        {
            while (mapDataQueue.Count > 0)
            {
                MapThreadInfo<HeightMap> threadInfo = mapDataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataQueue.Count > 0)
        {
            while (meshDataQueue.Count > 0)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public void RequestHeightMap(Vector2 center, System.Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            HeightMapThread(center, callback);
        };

        new Thread(threadStart).Start();
    }
    void HeightMapThread(Vector2 center, System.Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, center);
        MapThreadInfo<HeightMap> threadInfo = new MapThreadInfo<HeightMap>(heightMap, callback);
        lock (mapDataQueue)
        {
            mapDataQueue.Enqueue(threadInfo);
        }
    }

    public void RequestMeshData(HeightMap heightMap, LOD lod, System.Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(heightMap, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, LOD lod, System.Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        MapThreadInfo<MeshData> threadInfo = new MapThreadInfo<MeshData>(meshData, callback);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(threadInfo);
        }
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



public struct MapThreadInfo<T>
{
    public T parameter;
    public System.Action<T> callback;

    public MapThreadInfo(T parameter, System.Action<T> callback)
    {
        this.parameter = parameter;
        this.callback = callback;
    }
}


