using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class EntityManager : MonoBehaviour
// {
//     TerrainGenerator terrainGenerator;
//     public GameObject prefab;

//     List<GameObject> entities = new List<GameObject>();

//     void Start()
//     {
//         terrainGenerator = FindObjectOfType<TerrainGenerator>();
//         terrainGenerator.OnChunkLoaded += HandleChunkLoaded;
//     }

//     void HandleChunkLoaded(TerrainChunk chunk)
//     {
//         float size = chunk.meshSettings.numVertsPerLine;
//         for (int i = 0; i < size; i += 10)
//         {
//             for (int j = 0; j < size; j += 10)
//             {
//                 Vector3 position = chunk.worldPosition + new Vector3(i, chunk.GetHeightAtPosition(i, j), -j);
//                 GameObject entity = Instantiate(prefab, position, Quaternion.identity);
//                 entities.Add(entity);
//             }

//         }
//     }

// }
