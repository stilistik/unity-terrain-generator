using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> OnVisibleChanged;
    const int colliderUpdateThreshold = 5;
    const int sqrColliderUpdateThreshold = colliderUpdateThreshold * colliderUpdateThreshold;

    Vector2 sampleCenter;
    float size;

    GameObject meshObject;
    Bounds bounds;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    HeightMap heightMap;
    bool heightMapReceived = false;

    LODSetting[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;
    bool hasSetCollider = false;
    bool wasVisible = false;

    int prevLodIndex = -1;

    MeshSettings meshSettings;
    HeightMapSettings heightMapSettings;

    Transform viewer;

    private Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }



    public TerrainChunk(Vector2 coordinate, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODSetting[] detailLevels, int colliderLODIndex, Transform parent, Material material, Transform viewer)
    {
        this.colliderLODIndex = colliderLODIndex;
        this.meshSettings = meshSettings;
        this.heightMapSettings = heightMapSettings;
        this.detailLevels = detailLevels;
        this.viewer = viewer;

        sampleCenter = coordinate * meshSettings.meshWorldSize / meshSettings.scale;
        Vector2 position = coordinate * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("TerrainChunk");

        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;
        meshObject.transform.parent = parent;
        meshObject.transform.position = new Vector3(position.x, 0, position.y);

        setVisible(false);
    }

    public void Load()
    {
        ThreadedDataLoader.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
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
    }

    public void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        UpdateSelf();
    }

    public void OnMeshDataReceived(MeshData meshData)
    {
        UpdateSelf();
    }

    public void UpdateSelf()
    {
        if (heightMapReceived)
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool isVisible = viewerDistanceFromNearestEdge <= meshSettings.maxViewDistance;

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
                        lodMesh.RequestMeshData(heightMap, meshSettings);
                    }
                }
            }
            if (wasVisible != isVisible)
            {
                setVisible(isVisible);
                if (OnVisibleChanged != null)
                {
                    OnVisibleChanged(this, isVisible);
                }

            }
        }

    }

    float GetHeightAtPosition(int x, int y)
    {
        return heightMapSettings.heightCurve.Evaluate(heightMap.values[x, y]) * heightMapSettings.heightMultiplier;
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

    public void setVisible(bool visible)
    {
        meshObject.SetActive(visible);
        wasVisible = visible;
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
    LOD lod;

    public event System.Action updateCallback;

    public LODMesh(LOD lod)
    {
        this.lod = lod;
    }

    public void RequestMeshData(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequested = true;
        ThreadedDataLoader.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasReceived = true;
        updateCallback();
    }

}
