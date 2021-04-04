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


    ChunkGenerator<TerrainChunk> generator;

    void Start()
    {
        generator = new ChunkGenerator<TerrainChunk>(CreateTerrainChunk, meshSettings, viewer);
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
