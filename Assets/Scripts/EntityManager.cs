using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public List<GameObject> prefabs;
    public int size = 300;
    public int stride = 10;
    TerrainGenerator terrainGenerator;
    Dictionary<TerrainChunk, List<GameObject>> entitiesByChunk = new Dictionary<TerrainChunk, List<GameObject>>();

    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        terrainGenerator.OnChunkLoaded += HandleChunkLoaded;
        terrainGenerator.OnChunkVisibleChanged += HandleChunkVisibleChanged;
    }
    void HandleChunkLoaded(TerrainChunk chunk)
    {
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(5, Vector2.one * terrainGenerator.meshSettings.meshWorldSize, 10);
        List<GameObject> chunkEntities = new List<GameObject>();
        foreach (Vector2 point in points)
        {
            float xPosition = point.x + chunk.worldPosition.x - terrainGenerator.meshSettings.meshWorldSize / 2;
            float yPosition = -point.y + chunk.worldPosition.y + terrainGenerator.meshSettings.meshWorldSize / 2;
            float height = chunk.GetHeightAtPosition((int)point.x, (int)point.y) + 1.8f;
            if (height < 3 || height > 40) continue;

            Vector3 position = new Vector3(xPosition, height, yPosition);

            GameObject prefab = prefabs[(int)Random.Range(0, prefabs.Count)];
            GameObject entity = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            entity.transform.parent = gameObject.transform;
            entity.SetActive(false);
            chunkEntities.Add(entity);
        }
        entitiesByChunk.Add(chunk, chunkEntities);
    }

    void HandleChunkVisibleChanged(TerrainChunk chunk, bool visible)
    {
        List<GameObject> chunkEntities = entitiesByChunk[chunk];
        if (chunkEntities == null) return;
        foreach (GameObject entity in chunkEntities)
        {
            entity.SetActive(visible);
        }
    }
}
