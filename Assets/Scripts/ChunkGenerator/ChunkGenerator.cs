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

    int chunkCheckRadius;
    float maxViewDistance;
    float chunkSize;

    Dictionary<Vector2, T> chunks = new Dictionary<Vector2, T>();
    List<T> chunksVisibleLastUpdate = new List<T>();

    private Func<Vector2, T> CreateChunk;

    public event System.Action<Chunk, bool> OnChunkVisibleChanged;
    public event System.Action<Chunk> OnChunkLoaded;

    public ChunkGenerator(Func<Vector2, T> CreateChunk, Transform viewer, float chunkSize, float maxViewDistance)
    {
        this.CreateChunk = CreateChunk;
        this.viewer = viewer;
        this.maxViewDistance = maxViewDistance;
        this.chunkSize = chunkSize;
    }

    public void Start()
    {
        chunkCheckRadius = Mathf.RoundToInt(maxViewDistance / chunkSize) + 2;
        CreateOrUpdateChunks();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            UpdateChunkColliders();
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrChunkUpdateThreshold)
        {
            viewerPositionOld = viewerPosition;
            CreateOrUpdateChunks();
        }
    }

    void UpdateChunkColliders()
    {
        ForEachChunkInCheckRadius((Vector2 chunkCoordinate) =>
        {
            if (chunks.ContainsKey(chunkCoordinate))
            {
                T chunk = chunks[chunkCoordinate];
                chunk.UpdateCollider();
            }
        });

    }

    void CreateOrUpdateChunks()
    {
        ForEachChunkInCheckRadius((Vector2 chunkCoordinate) =>
        {
            if (chunks.ContainsKey(chunkCoordinate))
            {
                UpdateChunk(chunks[chunkCoordinate]);
            }
            else
            {
                T chunk = CreateChunk(chunkCoordinate);
                chunks.Add(chunkCoordinate, chunk);
                chunk.OnLoad += HandleChunkLoaded;
                chunk.Load();
                UpdateChunk(chunk);
            }
        });
    }

    void ForEachChunkInCheckRadius(Action<Vector2> callback)
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for (int yOffset = -chunkCheckRadius; yOffset <= chunkCheckRadius; yOffset++)
        {
            for (int xOffset = -chunkCheckRadius; xOffset <= chunkCheckRadius; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                callback(viewedChunkCoord);
            }
        }
    }

    void UpdateChunk(T chunk)
    {
        float distance = chunk.GetViewerDistanceFromEdge();
        bool isVisible = distance <= maxViewDistance;

        if (chunk.isVisible != isVisible)
        {
            chunk.SetVisible(isVisible);

            if (OnChunkVisibleChanged != null)
            {
                OnChunkVisibleChanged(chunk, isVisible);
            }

        }
    }

    void HandleChunkLoaded(Chunk chunk)
    {
        if (OnChunkLoaded != null)
        {
            OnChunkLoaded(chunk);
        }
    }
}