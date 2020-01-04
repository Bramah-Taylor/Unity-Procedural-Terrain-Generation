using UnityEngine;
using System.Collections;

// Class that stores mesh-related data - what size we want it to be, chunk resolution, etc.
[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
	public const int numSupportedLODs = 5;
	public const int numSupportedChunkSizes = 9;
	public const int numSupportedFlatshadedChunkSizes = 3;
	public static readonly int[] supportedChunkSizes = {48,72,96,120,144,168,192,216,240}; // Chunk sizes supported by the LOD generation scheme
	
	public float meshScale = 2.5f;
	public bool useFlatShading;

	[Range(0,numSupportedChunkSizes-1)]
	public int chunkSizeIndex;
	[Range(0,numSupportedFlatshadedChunkSizes-1)]
	public int flatshadedChunkSizeIndex;


	// Num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals
	public int numVertsPerLine
    {
		get
        {
			return supportedChunkSizes[(useFlatShading) ? flatshadedChunkSizeIndex : chunkSizeIndex] + 1;
		}
	}

	public float meshWorldSize
    {
		get
        {
			return (numVertsPerLine - 3) * meshScale;
		}
	}
}
