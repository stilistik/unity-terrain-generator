using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk
{
    public event System.Action<WorldChunk, bool> OnVisibleChanged;
    public GameObject gameObject;
    TerrainChunk terrainChunk;
    LODSetting[] detailLevels;

    public WorldChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODSetting[] detailLevels, int colliderLODIndex, Transform parent, Material terrainMaterial, Transform viewer)
    {
        gameObject = new GameObject("WorldChunk");
        gameObject.tag = "WorldChunk";
        gameObject.transform.parent = parent;

        terrainChunk = new TerrainChunk(coord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, gameObject.transform, terrainMaterial, viewer);
        terrainChunk.OnVisibleChanged += HandleTerrainChunkVisibleChanged;
    }

    private void HandleTerrainChunkVisibleChanged(TerrainChunk chunk, bool visible)
    {
        if (OnVisibleChanged != null)
        {
            OnVisibleChanged(this, visible);
        }
        SetVisible(visible);
    }

    public void Load()
    {
        terrainChunk.Load();
    }

    public void Update()
    {
        terrainChunk.Update();
    }

    public void UpdateCollider()
    {
        terrainChunk.UpdateColliderMesh();
    }

    public void SetVisible(bool visible)
    {
        terrainChunk.setVisible(visible);
    }
}
