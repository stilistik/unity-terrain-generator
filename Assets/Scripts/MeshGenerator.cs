using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, MapGenerator.LOD lod)
    {
        AnimationCurve heightCurve = new AnimationCurve(meshHeightCurve.keys);

        int meshIncrement = (int)lod;
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        MeshData meshData = new MeshData(meshSize);

        int verticesPerLine = (meshSize - 1) / meshIncrement + 1;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int borderVertexIndex = -1;
        int meshVertexIndex = 0;

        for (int y = 0; y < borderedSize; y += meshIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshIncrement)
            {
                bool isBorderVertex = x == 0 || y == 0 || x == borderedSize - 1 || y == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2((x - meshIncrement) / (float)meshSize, (y - meshIncrement) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);
                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshIncrement, y];
                    int c = vertexIndicesMap[x, y + meshIncrement];
                    int d = vertexIndicesMap[x + meshIncrement, y + meshIncrement];
                    meshData.AddTriangle(d, a, b);
                    meshData.AddTriangle(a, d, c);
                }
            }
        }

        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    Vector3[] borderVertices;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    int[] triangles;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        int sqrVerticesPerLine = verticesPerLine * verticesPerLine;
        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[verticesPerLine * 24];
        vertices = new Vector3[sqrVerticesPerLine];
        uvs = new Vector2[sqrVerticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
    }

    public void BakeNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex += 3)
        {
            int vertexIndexA = triangles[triangleIndex];
            int vertexIndexB = triangles[triangleIndex + 1];
            int vertexIndexC = triangles[triangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int borderTriangleIndex = 0; borderTriangleIndex < borderTriangles.Length; borderTriangleIndex += 3)
        {
            int vertexIndexA = borderTriangles[borderTriangleIndex];
            int vertexIndexB = borderTriangles[borderTriangleIndex + 1];
            int vertexIndexC = borderTriangles[borderTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) vertexNormals[vertexIndexA] += triangleNormal;
            if (vertexIndexB >= 0) vertexNormals[vertexIndexB] += triangleNormal;
            if (vertexIndexC >= 0) vertexNormals[vertexIndexC] += triangleNormal;
        }

        foreach (Vector3 normal in vertexNormals)
        {
            normal.Normalize();
        }

        bakedNormals = vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 a = indexA >= 0 ? vertices[indexA] : borderVertices[-indexA - 1];
        Vector3 b = indexB >= 0 ? vertices[indexB] : borderVertices[-indexB - 1];
        Vector3 c = indexC >= 0 ? vertices[indexC] : borderVertices[-indexC - 1];

        Vector3 ab = b - a;
        Vector3 ac = c - a;

        return Vector3.Cross(ab, ac).normalized;
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            // border vertex
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            // mesh vertex
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;

        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            // border triangle
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;

        }
        else
        {
            // mesh triangle
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;

        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.normals = bakedNormals;
        return mesh;
    }
}