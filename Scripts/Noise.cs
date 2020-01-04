using UnityEngine;
using System.Collections;

// Class for handling the generation of noise maps using fractional Brownian motion
public static class Noise
{
    // Enum for normalizing the heightmap values within a range of 0-1
	public enum NormalizeMode{Local, Global};

    // Main function - output 2D array of data to represent as our noise map
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random(settings.seed);
		Vector2[] octaveOffsets = new Vector2[settings.octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

        // Generate a set of offsets for each Perlin noise iteration
		for (int i = 0; i < settings.octaves; i++)
        {
            // Perlin noise behaves strangely beyond 100k, so keep offsets within that number
            // Bear in mind that this may have implications for extremely large worlds generated using Perlin noise
			float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
			float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= settings.persistance;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2.0f;
		float halfHeight = mapHeight / 2.0f;

		for (int y = 0; y < mapHeight; y++)
        {
			for (int x = 0; x < mapWidth; x++)
            {
				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

                // Perform the actual fractional Brownian motion algorithm here
				for (int i = 0; i < settings.octaves; i++)
                {
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= settings.persistance;
					frequency *= settings.lacunarity;
				}

                // Find the min and max height
				if (noiseHeight > maxLocalNoiseHeight)
                {
					maxLocalNoiseHeight = noiseHeight;
				} 
				if (noiseHeight < minLocalNoiseHeight)
                {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;

                // For endless terrainm normalise the height manually using an approximation
                // We do this to prevent seams appearing between the terrain chunks due to difference in min/max height values
                // #TODO: This can potentially be removed depending on whether having the output noise data be clamped between 0 and 1 is desired or not
                //        Right now this prevents the formation of lots of water areas
				if (settings.normalizeMode == NormalizeMode.Global)
                {
					float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
					noiseMap [x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
				}
			}
		}

        // Normalize for a single noise map
        // This is effectively only used for the preview mesh
		if (settings.normalizeMode == NormalizeMode.Local)
        {
			for (int y = 0; y < mapHeight; y++)
            {
				for (int x = 0; x < mapWidth; x++)
                {
					noiseMap [x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				}
			}
	    }

		return noiseMap;
	}
}

[System.Serializable]
public class NoiseSettings
{
	public Noise.NormalizeMode normalizeMode;

    // Noise scale - greater scale creates larger features and is more 'zoomed in' in a sense
	public float scale = 200;

    // fBm values
	public int octaves = 6;
	[Range(0,1)]
	public float persistance =.6f;
	public float lacunarity = 2;

    // PRNG seed
	public int seed;
    // Terrain offset from origin
	public Vector2 offset;

	public void ValidateValues()
    {
		scale = Mathf.Max(scale, 0.01f);
		octaves = Mathf.Max(octaves, 1);
		lacunarity = Mathf.Max(lacunarity, 1);
		persistance = Mathf.Clamp01(persistance);
	}
}