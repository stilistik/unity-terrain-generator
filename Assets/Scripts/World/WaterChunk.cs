using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterChunk
{
    WorldChunk worldChunk;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    GameObject gameObject;

    bool meshRequested = false;
    bool meshReceived = false;

    public WaterChunk(WorldChunk worldChunk, Material material)
    {
        this.worldChunk = worldChunk;

        gameObject = new GameObject("WaterChunk");
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;
        gameObject.transform.parent = worldChunk.gameObject.transform;
        gameObject.transform.position = new Vector3(worldChunk.position.x, 2, worldChunk.position.y);
    }

    public void Update()
    {
        if (!meshRequested)
        {
            RequestMesh();
        }
    }

    void OnMeshReceived(object meshDataObject)
    {
        meshFilter.mesh = ((MeshData)meshDataObject).CreateMesh();
        meshReceived = true;
    }

    void RequestMesh()
    {
        meshRequested = true;
        ThreadedDataLoader.RequestData(() => MeshGenerator.GenerateFlatMesh(worldChunk.meshSettings, LOD.One), OnMeshReceived);
    }

}
