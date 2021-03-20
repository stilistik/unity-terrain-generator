using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { None, NoiseMap, ColorMap, Mesh };
    public enum LOD { One = 1, Two = 2, Four = 4, Eight = 8, Sixteen = 16 }
    public const int mapChunkSize = 239;

    [Header("Noise Settings")]
    public float noiseScale;
    public bool autoUpdate;
    public float persistence;
    public float lacunarity;
    public int octaves;
    public int seed;
    public Vector2 offset;

    [Header("Terrain Settings")]
    public int mapSize;
    public float maxViewDistance;
    public TerrainType[] terrainTypes;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    [Header("Debug Settings")]
    public DrawMode drawMode;
    public LOD editorPreviewLod;

    private MapData mapData;

    Queue<MapThreadInfo<MapData>> mapDataQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(new Vector2(0, 0));
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLod), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
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
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseScale, seed, octaves, lacunarity, persistence, center + offset, mapSize);
        Color[] colorMap = GetColorMap(noiseMap);
        return new MapData(noiseMap, colorMap);
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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, lod);
        MapThreadInfo<MeshData> threadInfo = new MapThreadInfo<MeshData>(meshData, callback);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(threadInfo);
        }
    }

    public Color[] GetColorMap(float[,] heightMap)
    {
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = heightMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (currentHeight <= terrainTypes[i].height)
                    {
                        colorMap[x + y * mapChunkSize] = terrainTypes[i].color;
                        break;
                    }
                }
            }
        }
        return colorMap;
    }

    void OnValidate()
    {
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public Color color;
    public float height;
}


public struct MapData
{
    public float[,] noiseMap;
    public Color[] colorMap;
    public MapData(float[,] noiseMap, Color[] colorMap)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
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


