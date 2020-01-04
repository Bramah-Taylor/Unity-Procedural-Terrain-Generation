using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for generating heightmaps
// This class should be thread safe
public static class HeightMapGenerator
{
	public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        // Get the data from the Noise class using the input noise settings
		float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        // AnimationCurves are not thread safe even for read access - this means we have to create a new curve for each heightmap
        // by using the data stored in the settings object's AnimationCurve
		AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

		float minValue = float.MaxValue;
		float maxValue = float.MinValue;

        // Find the maximum and minimum values within this heightmap
		for (int i = 0; i < width; i++)
        {
			for (int j = 0; j < height; j++)
            {
				values [i, j] *= heightCurve_threadsafe.Evaluate(values [i, j]) * settings.heightMultiplier;

				if (values [i, j] > maxValue)
                {
					maxValue = values [i, j];
				}
				if (values [i, j] < minValue)
                {
					minValue = values [i, j];
				}
			}
		}

		return new HeightMap (values, minValue, maxValue);
	}

}

public struct HeightMap
{
	public readonly float[,] values;
    // #TODO: These values only get used for greyscale colour interpolation in the map previewing, so they can probably be removed at some point
	public readonly float minValue;
	public readonly float maxValue;

	public HeightMap(float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}

