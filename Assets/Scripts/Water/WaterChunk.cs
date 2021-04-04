using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterChunk : Chunk
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    bool meshRequested = false;
    bool meshReceived = false;

    public WaterChunk(Vector2 coordinate, MeshSettings meshSettings, Transform parent, Material material, Transform viewer) : base(coordinate, meshSettings, viewer)
    {
        gameObject.name = "WaterChunk";
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.material = material;
        gameObject.transform.parent = parent;
        gameObject.transform.position = new Vector3(position.x, 2, position.y);
    }

    public override void Load()
    {
        RequestMesh();
    }

    public override void UpdateImpl() { }

    void OnMeshReceived(object meshDataObject)
    {
        meshFilter.mesh = ((MeshData)meshDataObject).CreateMesh();
        meshReceived = true;
    }

    void RequestMesh()
    {
        meshRequested = true;
        ThreadedDataLoader.RequestData(() => MeshGenerator.GenerateFlatMesh(meshSettings, LOD.One), OnMeshReceived);
    }

}