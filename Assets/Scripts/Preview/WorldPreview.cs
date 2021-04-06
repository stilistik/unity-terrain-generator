using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPreview : MonoBehaviour
{

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;
    public Material terrainMaterial;
    public Material waterMaterial;

    public int previewSize;
    public bool autoUpdate;


    GameObject water;
    GameObject terrain;
    GameObject entities;
    LODSetting[] detailLevels = { new LODSetting(LOD.One, int.MaxValue) };

    public void UpdatePreview()
    {
        DestroyImmediate(terrain);
        DestroyImmediate(water);
        DestroyImmediate(entities);

        water = new GameObject("Water");
        water.transform.parent = gameObject.transform;
        terrain = new GameObject("Terrain");
        terrain.transform.parent = gameObject.transform;
        entities = new GameObject("Entities");
        entities.transform.parent = gameObject.transform;

        for (int y = -previewSize; y <= previewSize; y++)
        {
            for (int x = -previewSize; x <= previewSize; x++)
            {
                Vector2 coord = new Vector2(x, y);

                TerrainChunk terrainChunk = new TerrainChunk(coord, heightMapSettings, meshSettings, detailLevels, 0, terrain.transform, terrainMaterial, gameObject.transform);
                terrainChunk.OnLoad += HandleChunkLoaded;
                terrainChunk.Load();
                terrainChunk.SetVisible(true);

                WaterChunk waterChunk = new WaterChunk(coord, meshSettings, water.transform, waterMaterial, gameObject.transform);
                waterChunk.Load();
                waterChunk.SetVisible(true);
            }
        }
    }


    void HandleChunkLoaded(Chunk _chunk)
    {
        TerrainChunk chunk = _chunk as TerrainChunk;
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(5, Vector2.one * meshSettings.meshWorldSize, 10);

        EntityManager mgr = FindObjectOfType<EntityManager>();

        foreach (Vector2 p in points)
        {
            float xPosition = p.x + chunk.worldPosition.x - meshSettings.meshWorldSize / 2;
            float yPosition = -p.y + chunk.worldPosition.y + meshSettings.meshWorldSize / 2;
            float height = chunk.GetHeightAtPosition((int)p.x, (int)p.y) + 2f;
            if (height < 3 || height > 40) continue;

            Vector3 pos = new Vector3(xPosition, height, yPosition);
            GameObject prefab = mgr.prefabs[(int)Random.Range(0, mgr.prefabs.Count)];
            GameObject entity = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            entity.transform.parent = entities.transform;

        }

    }

}
