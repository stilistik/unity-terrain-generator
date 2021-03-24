using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    public Layer[] layers;
    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("tints", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("tintStrengths", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("startHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("blendStrengths", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("textureScales", layers.Select(x => x.textureScale).ToArray());
        material.SetTexture("textures", GenerateTexture2DArray(layers.Select(x => x.texture).ToArray()));

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    private Texture2DArray GenerateTexture2DArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                textureArray.SetPixels(textures[i].GetPixels(), i);
            }
        }
        textureArray.Apply();
        return textureArray;
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }
}

[System.Serializable]
public class Layer
{
    public Texture2D texture;
    public float textureScale;
    public Color tint;
    [Range(0, 1)]
    public float tintStrength;
    [Range(0, 1)]
    public float startHeight;
    [Range(0, 1)]
    public float blendStrength;
}
