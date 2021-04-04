using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : Chunk
{
    const int colliderUpdateThreshold = 5;
    const int sqrColliderUpdateThreshold = colliderUpdateThreshold * colliderUpdateThreshold;

    Vector2 sampleCenter;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    HeightMap heightMap;
    LODSetting[] detailLevels;
    LODMesh[] lodMeshes;
    HeightMapSettings heightMapSettings;

    bool heightMapReceived = false;
    int colliderLODIndex;
    bool hasSetCollider = false;
    int prevLodIndex = -1;

    public TerrainChunk(Vector2 coordinate, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODSetting[] detailLevels, int colliderLODIndex, Transform parent, Material material, Transform viewer) : base(coordinate, meshSettings, viewer)
    {
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.detailLevels = detailLevels;

        sampleCenter = position / meshSettings.scale;

        gameObject.name = "TerrainChunk";
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;
        gameObject.transform.parent = parent;
        gameObject.transform.position = new Vector3(position.x, 0, position.y);
    }

    public override void Load()
    {
        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            LODMesh lodMesh = new LODMesh(detailLevels[i].lod);
            lodMesh.updateCallback += Update;
            lodMeshes[i] = lodMesh;
            if (i == colliderLODIndex)
            {
                lodMesh.updateCallback += UpdateCollider;
            }
        }

        ThreadedDataLoader.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
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

    public override void UpdateImpl()
    {
        if (isVisible && heightMapReceived)
        {
            int lodIndex = GetLODIndex();

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
                    lodMesh.RequestMeshData(heightMap, meshSettings);
                }
            }

            if (!hasSetCollider) UpdateCollider();
        }
    }

    int GetLODIndex()
    {
        float viewerDistance = GetViewerDistanceFromEdge();
        int lodIndex = detailLevels.Length - 1;
        for (int i = 0; i < detailLevels.Length; i++)
        {
            if (viewerDistance <= detailLevels[i].distanceThreshold)
            {
                lodIndex = i;
                break;
            }
        }
        return lodIndex;
    }


    float GetHeightAtPosition(int x, int y)
    {
        return heightMapSettings.heightCurve.Evaluate(heightMap.values[x, y]) * heightMapSettings.heightMultiplier;
    }

    public override void UpdateCollider()
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
                    colliderMesh.RequestMeshData(heightMap, meshSettings);
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
