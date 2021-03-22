using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public float noiseScale;
    public float persistence;
    public float lacunarity;
    public int octaves;
    public int seed;
    public Vector2 offset;

    protected override void OnValidate()
    {
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        base.OnValidate();
    }
}
