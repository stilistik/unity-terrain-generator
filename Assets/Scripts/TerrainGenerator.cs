using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const int chunkUpdateThreshold = 25;
    const int sqrChunkUpdateThreshold = chunkUpdateThreshold * chunkUpdateThreshold;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public Transform viewer;
    public LODSetting[] detailLevels;
    public int colliderLODIndex;
    public Material meshMaterial;

    static Vector2 viewerPosition;
    static Vector2 viewerPositionOld;

    int chunksVisibleInViewDistance;


    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


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
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshSettings.meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshSettings.meshWorldSize);

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
                    TerrainChunk chunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, meshMaterial, viewer);
                    terrainChunks.Add(viewedChunkCoord, chunk);
                    chunk.OnVisibleChanged += OnChunkVisibilityChanged;
                    chunk.Load();
                }
            }
        }
    }

    void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        if (visible)
        {
            terrainChunksVisibleLastUpdate.Add(chunk);
        }
        else
        {
            terrainChunksVisibleLastUpdate.Remove(chunk);
        }
    }
}

[System.Serializable]
public struct LODSetting
{
    public LOD lod;
    public int distanceThreshold;

    public int sqrDistanceTreshold
    {
        get
        {
            return distanceThreshold * distanceThreshold;
        }
    }
}