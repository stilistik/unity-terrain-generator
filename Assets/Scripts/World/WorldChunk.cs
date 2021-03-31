using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk
{
    public event System.Action<WorldChunk, bool> OnVisibleChanged;
    public GameObject gameObject;
    public Vector2 coordinate;
    public Vector2 position;
    public TerrainChunk terrainChunk;
    public WaterChunk waterChunk;
    public LODSetting[] detailLevels;
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public Bounds bounds;
    Transform viewer;
    bool wasVisible = false;


    public WorldChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODSetting[] detailLevels, int colliderLODIndex, Transform parent, Material terrainMaterial, Material waterMaterial, Transform viewer)
    {
        this.coordinate = coord;
        this.meshSettings = meshSettings;
        this.heightMapSettings = heightMapSettings;
        this.viewer = viewer;
        this.detailLevels = detailLevels;

        position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        gameObject = new GameObject("WorldChunk");
        gameObject.tag = "WorldChunk";
        gameObject.transform.parent = parent;
        gameObject.transform.position = new Vector3(position.x, 0, position.y);

        terrainChunk = new TerrainChunk(this, terrainMaterial, colliderLODIndex);
        waterChunk = new WaterChunk(this, waterMaterial);
        SetVisible(false);
    }

    public void Load()
    {
        terrainChunk.Load();
        Update();
    }

    public void Update()
    {
        float viewerDistance = GetViewerDistanceFromEdge();
        bool isVisible = viewerDistance <= meshSettings.maxViewDistance;
        if (isVisible)
        {
            terrainChunk.Update();
            waterChunk.Update();
        }

        if (isVisible != wasVisible)
        {
            SetVisible(isVisible);
            if (OnVisibleChanged != null)
            {
                OnVisibleChanged(this, isVisible);
            }
        }
    }

    public Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public float GetViewerDistanceFromEdge()
    {
        return Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
    }

    public int GetLODIndex()
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

    public void UpdateCollider()
    {
        terrainChunk.UpdateColliderMesh();
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        wasVisible = visible;
    }
}
