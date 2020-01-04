using UnityEngine;
using System.Collections;

// Class for generating textures from input heightmaps
// Currently used for previewing noise maps and falloff maps in the MapPreview class
public static class TextureGenerator
{
    // Generate a 2D texture from an input array of colour data
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colourMap);
		texture.Apply();
		return texture;
	}
    
    // Convert an input heightmap into an array of colour data, then call TextureFromColourMap
	public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
		int width = heightMap.values.GetLength(0);
		int height = heightMap.values.GetLength(1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++)
        {
			for (int x = 0; x < width; x++)
            {
                // Get the interpolant parameter t using InverseLerp and use this to produce a greyscale colour texture
				colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
			}
		}

		return TextureFromColourMap(colourMap, width, height);
	}
}
