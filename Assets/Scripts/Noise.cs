using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GenerateNoiseMap(int width, int height, float scale, int seed, int octaves, float lacunarity, float persistence, Vector2 offset, int mapSize)
    {
        float[,] noiseMap = new float[width, height];

        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        System.Random prng = new System.Random(seed);
        float maxPossibleHeight = 0;
        float frequency = 1;
        float amplitude = 1;

        Vector2[] sampleOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            sampleOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
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

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + sampleOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + sampleOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = noiseHeight;

                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }

            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float fallOffValue = ComputeFallOffValue(x - halfWidth + offset.x, y - halfHeight - offset.y, (float)mapSize);

                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.5f);
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
