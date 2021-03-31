using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    const int chunkUpdateThreshold = 25;
    const int sqrChunkUpdateThreshold = chunkUpdateThreshold * chunkUpdateThreshold;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public Transform viewer;
    public LODSetting[] detailLevels;
    public int colliderLODIndex;
    public Material terrainMaterial;
    public Material waterMaterial;

    static Vector2 viewerPosition;
    static Vector2 viewerPositionOld;

    int chunksVisibleInViewDistance;

    Dictionary<Vector2, WorldChunk> chunks = new Dictionary<Vector2, WorldChunk>();
    static List<WorldChunk> chunksVisibleLastUpdate = new List<WorldChunk>();


    void Start()
    {
        chunksVisibleInViewDistance = Mathf.RoundToInt(meshSettings.maxViewDistance / meshSettings.meshWorldSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (WorldChunk chunk in chunksVisibleLastUpdate)
            {
                chunk.UpdateCollider();
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
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshSettings.meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshSettings.meshWorldSize);

        for (int i = chunksVisibleLastUpdate.Count - 1; i >= 0; i--)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
            chunksVisibleLastUpdate.RemoveAt(i);
        }

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (chunks.ContainsKey(viewedChunkCoord))
                {
                    chunks[viewedChunkCoord].Update();
                }
                else
                {
                    WorldChunk chunk = new WorldChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, terrainMaterial, waterMaterial, viewer);
                    chunks.Add(viewedChunkCoord, chunk);
                    chunk.OnVisibleChanged += OnChunkVisibilityChanged;
                    chunk.Load();
                }
            }
        }
    }

    void OnChunkVisibilityChanged(WorldChunk chunk, bool visible)
    {
        if (visible)
        {
            chunksVisibleLastUpdate.Add(chunk);
        }
        else
        {
            chunksVisibleLastUpdate.Remove(chunk);
        }
    }
}

[System.Serializable]
public struct LODSetting
{
    public LOD lod;
    public int distanceThreshold;

    public LODSetting(LOD lod, int distanceThreshold)
    {
        this.lod = lod;
        this.distanceThreshold = distanceThreshold;
    }

    public int sqrDistanceTreshold
    {
        get
        {
            return distanceThreshold * distanceThreshold;
        }
    }
}