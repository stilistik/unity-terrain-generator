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


    ChunkGenerator<TerrainChunk> generator;

    void Start()
    {
        generator = new ChunkGenerator<TerrainChunk>(CreateTerrainChunk, viewer, meshSettings.meshWorldSize, viewDistance);
        generator.Start();
    }

    void Update()
    {
        generator.Update();
    }

    TerrainChunk CreateTerrainChunk(Vector2 coord)
    {
        return new TerrainChunk(coord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, gameObject.transform, terrainMaterial, viewer);
    }
}
