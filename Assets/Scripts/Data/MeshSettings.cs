using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    public const int numBorderVertices = 2;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    public float scale;
    public float maxViewDistance;

    public int numVertsPerLine
    {
        get
        {
            return supportedChunkSizes[chunkSizeIndex] + 1 + numBorderVertices;
        }
    }

    public float meshWorldSize
    {
        get
        {
            return (numVertsPerLine - 1 - numBorderVertices) * scale;
        }
    }
}
