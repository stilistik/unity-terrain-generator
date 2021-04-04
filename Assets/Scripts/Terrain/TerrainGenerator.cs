using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public Transform viewer;
    public LODSetting[] detailLevels;
    public int colliderLODIndex;
    public Material terrainMaterial;
    public int viewDistance;

    public event System.Action<TerrainChunk> OnChunkLoaded;
    public event System.Action<TerrainChunk, bool> OnChunkVisibleChanged;

    ChunkGenerator<TerrainChunk> generator;

    void Start()
    {
        generator = new ChunkGenerator<TerrainChunk>(CreateTerrainChunk, viewer, meshSettings.meshWorldSize, viewDistance);
        generator.OnChunkLoaded += HandleChunkLoaded;
        generator.OnChunkVisibleChanged += HandleChunkVisibleChanged;
        generator.Start();
    }

    void Update()
    {
        generator.Update();
    }

    void HandleChunkLoaded(Chunk chunk)
    {
        if (OnChunkLoaded != null)
        {
            OnChunkLoaded(chunk as TerrainChunk);
        }
    }

    void HandleChunkVisibleChanged(Chunk chunk, bool visible)
    {
        if (OnChunkVisibleChanged != null)
        {
            OnChunkVisibleChanged(chunk as TerrainChunk, visible);
        }
    }

    TerrainChunk CreateTerrainChunk(Vector2 coord)
    {
        return new TerrainChunk(coord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, gameObject.transform, terrainMaterial, viewer);
    }
}
