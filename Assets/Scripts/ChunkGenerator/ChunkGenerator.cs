using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChunkGenerator<T> where T : Chunk
{
    const int chunkUpdateThreshold = 25;
    const int sqrChunkUpdateThreshold = chunkUpdateThreshold * chunkUpdateThreshold;

    Transform viewer;
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    int chunksVisibleInViewDistance;
    float maxViewDistance;
    float chunkSize;

    Dictionary<Vector2, T> chunks = new Dictionary<Vector2, T>();
    List<T> chunksVisibleLastUpdate = new List<T>();


    private Func<Vector2, T> CreateChunk;

    public ChunkGenerator(Func<Vector2, T> CreateChunk, Transform viewer, float chunkSize, float maxViewDistance)
    {
        this.CreateChunk = CreateChunk;
        this.viewer = viewer;
        this.maxViewDistance = maxViewDistance;
        this.chunkSize = chunkSize;
    }

    public void Start()
    {
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        UpdateVisibleChunks();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (T chunk in chunksVisibleLastUpdate)
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
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

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
                    T chunk = CreateChunk(viewedChunkCoord);
                    chunks.Add(viewedChunkCoord, chunk);
                    chunk.OnVisibleChanged += OnChunkVisibilityChanged;
                    chunk.Load();
                }
            }
        }
    }

    void OnChunkVisibilityChanged(Chunk chunk, bool visible)
    {
        if (visible)
        {
            chunksVisibleLastUpdate.Add(chunk as T);
        }
        else
        {
            chunksVisibleLastUpdate.Remove(chunk as T);
        }
    }
}