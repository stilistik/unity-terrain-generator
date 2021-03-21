using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public Transform viewer;
    public LODSetting[] detailLevels;
    public Material meshMaterial;

    static Vector2 viewerPosition;

    int chunkSize;
    int chunksVisibleInViewDistance;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    const int chunkUpdateThreshold = 25;
    const int sqrChunkUpdateThreshold = chunkUpdateThreshold * chunkUpdateThreshold;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(mapGenerator.maxViewDistance / chunkSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        Vector2 position = new Vector2(viewer.position.x, viewer.position.z);
        if ((position - viewerPosition).sqrMagnitude > sqrChunkUpdateThreshold)
        {
            viewerPosition = position;
            UpdateVisibleChunks();

        }
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

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
                    terrainChunks[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, meshMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 position;

        GameObject meshObject;
        Bounds bounds;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        MapData mapData;
        bool mapDataReceived = false;

        LODSetting[] detailLevels;
        LODMesh[] lodMeshes;

        int prevLodIndex = -1;

        public TerrainChunk(Vector2 coordinate, int size, LODSetting[] detailLevels, Transform parent, Material material)
        {
            position = coordinate * size;
            Vector3 position3d = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position, Vector2.one * size);

            this.detailLevels = detailLevels;

            meshObject = new GameObject("TerrainChunk");

            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;
            meshObject.transform.parent = parent;
            meshObject.transform.position = position3d;

            mapGenerator.RequestMapData(position, OnMapDataReceived);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            setVisible(false);
        }

        public void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            meshRenderer.material.mainTexture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            mapDataReceived = true;
            UpdateTerrainChunk();
        }

        public void OnMeshDataReceived(MeshData meshData)
        {
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool isVisible = viewerDistanceFromNearestEdge <= mapGenerator.maxViewDistance;

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
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequested)
                        {
                            lodMesh.RequestMeshData(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                setVisible(isVisible);
            }

        }

        public void setVisible(bool visible)
        {
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

        System.Action updateCallback;

        public LODMesh(MapGenerator.LOD lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void RequestMeshData(MapData mapData)
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
    }
}