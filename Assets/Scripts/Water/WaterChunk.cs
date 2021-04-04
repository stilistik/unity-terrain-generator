using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterChunk : Chunk
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    bool meshRequested = false;
    bool meshReceived = false;

    public WaterChunk(Vector2 coordinate, MeshSettings meshSettings, Transform parent, Material material, Transform viewer) : base(coordinate, meshSettings, parent, viewer)
    {
        gameObject.name = "WaterChunk";
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = material;

    }

    public override void Load()
    {
        RequestMesh();
        Update();
    }

    public override void Update() { }

    void OnMeshReceived(object meshDataObject)
    {
        meshFilter.mesh = ((MeshData)meshDataObject).CreateMesh();
        meshReceived = true;
        NotifyLoaded();
    }

    void RequestMesh()
    {
        meshRequested = true;
        ThreadedDataLoader.RequestData(() => MeshGenerator.GenerateFlatMesh(meshSettings, LOD.One), OnMeshReceived);
    }

}
