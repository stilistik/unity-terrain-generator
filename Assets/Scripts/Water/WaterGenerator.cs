using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{

    public MeshSettings meshSettings;
    public Transform viewer;
    public LODSetting[] detailLevels;
    public int colliderLODIndex;
    public Material waterMaterial;

    ChunkGenerator<WaterChunk> generator;

    void Start()
    {
        generator = new ChunkGenerator<WaterChunk>(CreateWaterChunk, meshSettings, viewer);
        generator.Start();
    }

    void Update()
    {
        generator.Update();
    }

    WaterChunk CreateWaterChunk(Vector2 coord)
    {
        return new WaterChunk(coord, meshSettings, gameObject.transform, waterMaterial, viewer);
    }
}
