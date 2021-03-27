using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    const int chunkUpdateThreshold = 25;
    const int sqrChunkUpdateThreshold = chunkUpdateThreshold * chunkUpdateThreshold;
    const int colliderUpdateThreshold = 5;
    const int sqrColliderUpdateThreshold = colliderUpdateThreshold * colliderUpdateThreshold;

    public Transform viewer;
    public LODSetting[] detailLevels;
    public int colliderLODIndex;
    public Material meshMaterial;
    public GameObject prefabTree;

    static Vector2 viewerPosition;
    static Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDistance;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        meshWorldSize = mapGenerator.meshSettings.meshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(mapGenerator.meshSettings.maxViewDistance / meshWorldSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in terrainChunksVisibleLastUpdate)
            {
                chunk.UpdateColliderMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrChunkUpdateThreshold)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();

        }
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int i = terrainChunksVisibleLastUpdate.Count - 1; i >= 0; i--)
        {
            terrainChunksVisibleLastUpdate[i].setVisible(false);
            terrainChunksVisibleLastUpdate.RemoveAt(i);
        }

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunks.ContainsKey(viewedChunkCoord))
                {
                    terrainChunks[viewedChunkCoord].UpdateSelf();
                }
                else
                {
                    terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, meshWorldSize, detailLevels, colliderLODIndex, transform, meshMaterial, prefabTree));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 sampleCenter;
        float size;

        GameObject meshObject;
        Bounds bounds;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        HeightMap heightMap;
        bool mapDataReceived = false;

        LODSetting[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;
        bool hasSetCollider = false;

        int prevLodIndex = -1;

        GameObject prefabTree;

        List<GameObject> entities = new List<GameObject>();

        public TerrainChunk(Vector2 coordinate, float meshWorldSize, LODSetting[] detailLevels, int colliderLODIndex, Transform parent, Material material, GameObject prefabTree)
        {
            this.colliderLODIndex = colliderLODIndex;
            this.size = meshWorldSize;
            this.prefabTree = prefabTree;
            sampleCenter = coordinate * meshWorldSize / mapGenerator.meshSettings.scale;
            Vector2 position = coordinate * meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshWorldSize);

            this.detailLevels = detailLevels;

            meshObject = new GameObject("TerrainChunk");

            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;
            meshObject.transform.parent = parent;
            meshObject.transform.position = new Vector3(position.x, 0, position.y);

            mapGenerator.RequestHeightMap(sampleCenter, OnMapDataReceived);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                LODMesh lodMesh = new LODMesh(detailLevels[i].lod);
                lodMesh.updateCallback += UpdateSelf;
                lodMeshes[i] = lodMesh;
                if (i == colliderLODIndex)
                {
                    lodMesh.updateCallback += UpdateColliderMesh;
                }
            }

            setVisible(false);
        }

        public void OnMapDataReceived(HeightMap mapData)
        {
            this.heightMap = mapData;
            mapDataReceived = true;
            UpdateSelf();
        }

        public void OnMeshDataReceived(MeshData meshData)
        {
            UpdateSelf();
        }

        public void UpdateSelf()
        {
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool isVisible = viewerDistanceFromNearestEdge <= mapGenerator.meshSettings.maxViewDistance;

                if (isVisible)
                {
                    int lodIndex = detailLevels.Length - 1;
                    for (int i = 0; i < detailLevels.Length; i++)
                    {
                        if (viewerDistanceFromNearestEdge <= detailLevels[i].distanceThreshold)
                        {
                            lodIndex = i;
                            break;
                        }
                    }

                    if (lodIndex != prevLodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];

                        if (lodMesh.hasReceived)
                        {
                            prevLodIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequested)
                        {
                            lodMesh.RequestMeshData(heightMap);
                        }
                    }

                    PlaceEntities();

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                setVisible(isVisible);
            }

        }

        float GetHeightAtPosition(int x, int y)
        {
            return mapGenerator.heightMapSettings.heightCurve.Evaluate(heightMap.values[x, y]) * mapGenerator.heightMapSettings.heightMultiplier;
        }

        void RemoveEntities()
        {
            foreach (GameObject entity in entities)
            {
                Destroy(entity);
            }
            entities.Clear();
        }

        void PlaceEntities()
        {
            for (int y = 0; y < size; y += 10)
            {
                for (int x = 0; x < size; x += 10)
                {
                    float heightMapValue = heightMap.values[x, y];
                    if (heightMapValue < 0.6 && heightMapValue > 0.2)
                    {
                        float height = mapGenerator.heightMapSettings.heightCurve.Evaluate(heightMapValue) * mapGenerator.heightMapSettings.heightMultiplier;
                        GameObject tree = Instantiate(prefabTree, new Vector3(sampleCenter.x + x - size / 2, height, sampleCenter.y - y + size / 2), Quaternion.identity);
                        entities.Add(tree);
                    }
                }
            }
        }

        public void UpdateColliderMesh()
        {
            if (!hasSetCollider)
            {
                float sqrDistanceFromEdge = bounds.SqrDistance(viewerPosition);
                LODMesh colliderMesh = lodMeshes[colliderLODIndex];
                LODSetting colliderLODSetting = detailLevels[colliderLODIndex];

                if (sqrDistanceFromEdge < colliderLODSetting.sqrDistanceTreshold)
                {
                    if (!colliderMesh.hasRequested)
                    {
                        colliderMesh.RequestMeshData(heightMap);
                    }
                }


                if (sqrDistanceFromEdge < sqrColliderUpdateThreshold)
                {
                    if (colliderMesh.hasReceived)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
            }
        }

        public void setVisible(bool visible)
        {
            if (!visible)
            {
                RemoveEntities();
            }
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequested;
        public bool hasReceived;
        MapGenerator.LOD lod;

        public event System.Action updateCallback;

        public LODMesh(MapGenerator.LOD lod)
        {
            this.lod = lod;
        }

        public void RequestMeshData(HeightMap mapData)
        {
            hasRequested = true;
            mapGenerator.RequestMeshData(mapData, lod, onMeshDataReceived);
        }

        void onMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasReceived = true;
            updateCallback();
        }

    }

    [System.Serializable]
    public struct LODSetting
    {
        public MapGenerator.LOD lod;
        public int distanceThreshold;

        public int sqrDistanceTreshold
        {
            get
            {
                return distanceThreshold * distanceThreshold;
            }
        }
    }
}