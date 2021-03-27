using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GenerateNoiseMap(int width, int height, NoiseSettings settings, Vector2 sampleCenter)
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(settings.seed);
        float maxPossibleHeight = 0;
        float frequency = 1;
        float amplitude = 1;

        Vector2[] sampleOffsets = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            sampleOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }


        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + sampleOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + sampleOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }


                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }

                float fallOffValue = ComputeFallOffValue(x - halfWidth + settings.offset.x, y - halfHeight - settings.offset.y, (float)settings.fallOffRadius);
                float normalizedHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.5f);
                noiseMap[x, y] = normalizedHeight * fallOffValue;
            }
        }

        return noiseMap;
    }

    private static float ComputeFallOffValue(float x, float y, float mapSize)
    {
        float variance = mapSize * 10;
        float gaussValue = mapSize * Mathf.Exp(-(Mathf.Pow(x, 2) / variance + Mathf.Pow(y, 2) / variance) / 2) / (2 * Mathf.PI);
        return gaussValue > 1 ? 1 : gaussValue;

    }
}

[System.Serializable]
public class NoiseSettings
{
    public float scale;
    public float persistence;
    public float lacunarity;
    public int octaves;
    public int seed;
    public Vector2 offset;
    public int fallOffRadius;

    public void Validate()
    {
        scale = Mathf.Max(scale, 0.001f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}