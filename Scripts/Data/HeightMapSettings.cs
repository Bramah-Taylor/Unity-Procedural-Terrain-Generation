using UnityEngine;
using System.Collections;

// Class for storing data related to the heightmap - fBm values, Perlin modifying values, etc.
[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{
	public NoiseSettings noiseSettings;
	public bool useFalloff;                 // Primitive island generation method
	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public float minHeight
    {
		get
        {
			return heightMultiplier * heightCurve.Evaluate(0);
		}
	}

	public float maxHeight
    {
		get
        {
			return heightMultiplier * heightCurve.Evaluate(1);
		}
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
    {
		noiseSettings.ValidateValues();
		base.OnValidate();
	}
	#endif

}
