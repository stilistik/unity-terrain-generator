using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public MeshSettings meshSettings;
    public Transform viewer;
    public Material waterMaterial;

    ChunkGenerator<WaterChunk> generator;

    void Start()
    {
        generator = new ChunkGenerator<WaterChunk>(CreateWaterChunk, viewer, meshSettings.meshWorldSize, meshSettings.maxViewDistance);
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
