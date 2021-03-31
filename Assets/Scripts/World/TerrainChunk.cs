using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    const int colliderUpdateThreshold = 5;
    const int sqrColliderUpdateThreshold = colliderUpdateThreshold * colliderUpdateThreshold;

    Vector2 sampleCenter;
    GameObject gameObject;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    HeightMap heightMap;
    bool heightMapReceived = false;

    LODMesh[] lodMeshes;
    int colliderLODIndex;
    bool hasSetCollider = false;
    int prevLodIndex = -1;

    WorldChunk worldChunk;

    public TerrainChunk(WorldChunk worldChunk, Material material, int colliderLODIndex)
    {
        this.worldChunk = worldChunk;
        this.colliderLODIndex = colliderLODIndex;

        sampleCenter = worldChunk.position / worldChunk.meshSettings.scale;

        gameObject = new GameObject("TerrainChunk");
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;
        gameObject.transform.parent = worldChunk.gameObject.transform;
        gameObject.transform.position = new Vector3(worldChunk.position.x, 0, worldChunk.position.y);
    }

    public void Load()
    {
        lodMeshes = new LODMesh[worldChunk.detailLevels.Length];
        for (int i = 0; i < worldChunk.detailLevels.Length; i++)
        {
            LODMesh lodMesh = new LODMesh(worldChunk.detailLevels[i].lod);
            lodMesh.updateCallback += Update;
            lodMeshes[i] = lodMesh;
            if (i == colliderLODIndex)
            {
                lodMesh.updateCallback += UpdateColliderMesh;
            }
        }

        ThreadedDataLoader.RequestData(() => HeightMapGenerator.GenerateHeightMap(worldChunk.meshSettings.numVertsPerLine, worldChunk.meshSettings.numVertsPerLine, worldChunk.heightMapSettings, sampleCenter), OnHeightMapReceived);
    }

    public void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        Update();
    }

    public void OnMeshDataReceived(MeshData meshData)
    {
        Update();
    }

    public void Update()
    {
        if (heightMapReceived)
        {
            int lodIndex = worldChunk.GetLODIndex();

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
                    lodMesh.RequestMeshData(heightMap, worldChunk.meshSettings);
                }
            }

            if (!hasSetCollider) UpdateColliderMesh();
        }
    }

    float GetHeightAtPosition(int x, int y)
    {
        return worldChunk.heightMapSettings.heightCurve.Evaluate(heightMap.values[x, y]) * worldChunk.heightMapSettings.heightMultiplier;
    }

    public void UpdateColliderMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDistanceFromEdge = worldChunk.bounds.SqrDistance(worldChunk.viewerPosition);
            LODMesh colliderMesh = lodMeshes[colliderLODIndex];
            LODSetting colliderLODSetting = worldChunk.detailLevels[colliderLODIndex];

            if (sqrDistanceFromEdge < colliderLODSetting.sqrDistanceTreshold)
            {
                if (!colliderMesh.hasRequested)
                {
                    colliderMesh.RequestMeshData(heightMap, worldChunk.meshSettings);
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
}

public class LODMesh
{
    public Mesh mesh;
    public bool hasRequested;
    public bool hasReceived;
    LOD lod;

    public event System.Action updateCallback;

    public LODMesh(LOD lod)
    {
        this.lod = lod;
    }

    public void RequestMeshData(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequested = true;
        ThreadedDataLoader.RequestData(() => MeshGenerator.GenerateMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasReceived = true;
        updateCallback();
    }

}
