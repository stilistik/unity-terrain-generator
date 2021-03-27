using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, MapGenerator.LOD lod)
    {
        int skipIncrement = (int)lod;
        int verticesPerLine = meshSettings.numVertsPerLine;
        float meshWorldSize = meshSettings.meshWorldSize;
        MeshData meshData = new MeshData(verticesPerLine);

        Vector2 topLeft = new Vector2(-1, 1) * meshWorldSize / 2f;


        int[,] vertexIndicesMap = new int[verticesPerLine, verticesPerLine];
        int borderVertexIndex = -1;
        int meshVertexIndex = 0;

        for (int y = 0; y < verticesPerLine; y++)
        {
            for (int x = 0; x < verticesPerLine; x++)
            {
                bool isBorderVertex = IsBorderVertex(x, y, verticesPerLine);
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


        for (int y = 0; y < verticesPerLine; y++)
        {
            for (int x = 0; x < verticesPerLine; x++)
            {

                bool isBorderVertex = IsBorderVertex(x, y, verticesPerLine);
                bool isSkippedVertex = !isBorderVertex && ((x - 1) % skipIncrement != 0 || (y - 1) % skipIncrement != 0);

                if (!isSkippedVertex)
                {
                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / meshWorldSize;
                    Vector3 vertexPosition = new Vector3((topLeft.x + percent.x * meshWorldSize) * meshSettings.scale, heightMap[x, y], (topLeft.y - percent.y * meshWorldSize) * meshSettings.scale);
                    meshData.AddVertex(vertexPosition, percent, vertexIndex);

                    bool shouldCreateTriangle = x < verticesPerLine - 2 && y < verticesPerLine - 2;

                    if (shouldCreateTriangle)
                    {
                        int xIncrement = y == 0 ? 1 : skipIncrement;
                        int yIncremment = x == 0 ? 1 : skipIncrement;
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + xIncrement, y];
                        int c = vertexIndicesMap[x, y + yIncremment];
                        int d = vertexIndicesMap[x + xIncrement, y + yIncremment];
                        meshData.AddTriangle(d, a, b);
                        meshData.AddTriangle(a, d, c);
                    }
                }
            }
        }

        meshData.BakeNormals();

        return meshData;
    }

    private static bool IsBorderVertex(int x, int y, int verticesPerLine)
    {
        return x == 0 || x == verticesPerLine - 1 || y == 0 || y == verticesPerLine - 1;
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