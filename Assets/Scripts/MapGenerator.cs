using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { None, NoiseMap, Mesh };
    public enum LOD { One = 1, Two = 2, Four = 4, Six = 6, Eight = 8 }

    [Range(0, MeshGenerator.numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    public int mapChunkSize
    {
        get
        {
            return MeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }



    public NoiseData noiseData;
    public TerrainData terrainData;
    public TextureData textureData;
    public Material terrainMaterial;

    [Header("Debug Settings")]
    public DrawMode drawMode;
    public LOD editorPreviewLod;
    public bool autoUpdate;

    private MapData mapData;

    Queue<MapThreadInfo<MapData>> mapDataQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(new Vector2(0, 0));
        MapDisplay display = FindObjectOfType<MapDisplay>();

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLod));
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
                MapThreadInfo<MapData> threadInfo = mapDataQueue.Dequeue();
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

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.noiseScale, noiseData.seed, noiseData.octaves, noiseData.lacunarity, noiseData.persistence, center + noiseData.offset, terrainData.mapSize);
        return new MapData(noiseMap);
    }

    public void RequestMapData(Vector2 center, System.Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }
    void MapDataThread(Vector2 center, System.Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        MapThreadInfo<MapData> threadInfo = new MapThreadInfo<MapData>(mapData, callback);
        lock (mapDataQueue)
        {
            mapDataQueue.Enqueue(threadInfo);
        }
    }

    public void RequestMeshData(MapData mapData, LOD lod, System.Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, LOD lod, System.Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        MapThreadInfo<MeshData> threadInfo = new MapThreadInfo<MeshData>(meshData, callback);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(threadInfo);
        }
    }

    void OnValidate()
    {
        if (terrainData != null)
        {
            // unsubscribe if already subscribed before resubscribing
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            // unsubscribe if already subscribed before resubscribing
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }


}

public struct MapData
{
    public float[,] noiseMap;
    public MapData(float[,] noiseMap)
    {
        this.noiseMap = noiseMap;
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


