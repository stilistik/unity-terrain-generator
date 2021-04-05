using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EntityManager : MonoBehaviour
{
    public List<GameObject> prefabs;
    public int batchSize = 50;
    TerrainGenerator terrainGenerator;
    Dictionary<TerrainChunk, GameObject> entities = new Dictionary<TerrainChunk, GameObject>();
    Queue<QueueItem> queue = new Queue<QueueItem>();

    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        terrainGenerator.OnChunkLoaded += HandleChunkLoaded;
        terrainGenerator.OnChunkVisibleChanged += HandleChunkVisibleChanged;
        StartCoroutine(CreateEntities());
    }

    IEnumerator CreateEntities()
    {

        while (true)
        {
            if (queue.Count > 0)
            {
                int createCount = batchSize > queue.Count ? queue.Count : batchSize;
                for (int i = 0; i < createCount; i++)
                {
                    QueueItem item = queue.Dequeue();
                    GameObject prefab = prefabs[(int)Random.Range(0, prefabs.Count)];
                    GameObject entity = Instantiate(prefab, item.position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
                    entity.transform.parent = item.parent;
                }
            }
            yield return null;
        }
    }

    void HandleChunkLoaded(TerrainChunk chunk)
    {
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(5, Vector2.one * terrainGenerator.meshSettings.meshWorldSize, 10);
        GameObject batch = new GameObject("EntityBatch");
        batch.transform.parent = gameObject.transform;
        batch.SetActive(chunk.isVisible);
        entities.Add(chunk, batch);

        foreach (Vector2 p in points)
        {
            float xPosition = p.x + chunk.worldPosition.x - terrainGenerator.meshSettings.meshWorldSize / 2;
            float yPosition = -p.y + chunk.worldPosition.y + terrainGenerator.meshSettings.meshWorldSize / 2;
            float height = chunk.GetHeightAtPosition((int)p.x, (int)p.y) + 2f;
            if (height < 3 || height > 40) continue;

            Vector3 pos = new Vector3(xPosition, height, yPosition);
            QueueItem item = new QueueItem { position = pos, parent = batch.transform };
            queue.Enqueue(item);
        }

    }

    void HandleChunkVisibleChanged(TerrainChunk chunk, bool visible)
    {
        if (entities.ContainsKey(chunk))
        {
            GameObject chunkEntities = entities[chunk];
            chunkEntities.SetActive(visible);
        }
    }

    struct QueueItem
    {
        public Vector3 position;
        public Transform parent;

    }


}
