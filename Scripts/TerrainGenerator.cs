using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class for managing the generation and visibility of terrain chunks.
public class TerrainGenerator : MonoBehaviour
{
	const float viewerMoveThresholdForChunkUpdate = 25.0f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;

	public Transform viewer;
	public Material mapMaterial;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;

    // Container for all of the terrain chunks
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    // Container for all of the visible terrain chunks
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	void Start()
    {
		textureSettings.ApplyToMaterial(mapMaterial);
		textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

		UpdateVisibleChunks();
	}

	void Update()
    {
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
			foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                // This may need changing. Currently we're trying to update each visible chunk's collision mesh every time the player moves.
				chunk.UpdateCollisionMesh();
			}
		}

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}
		
	void UpdateVisibleChunks()
    {
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
		for (int i = visibleTerrainChunks.Count-1; i >= 0; i--)
        {
            // Update visible terrain chunks.
			alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
			visibleTerrainChunks[i].UpdateTerrainChunk();
		}
			
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        // Now check if any non-visible chunks should be updated to be visible.
		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    // Make previously generated chunk visible.
					if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
						terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					}
                    // Else generate a new terrain chunk.
                    else
                    {
						TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord,heightMapSettings,meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
						terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
						newChunk.Load();
					}
				}

			}
		}
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
		if (isVisible)
        {
			visibleTerrainChunks.Add(chunk);
		}
        else
        {
			visibleTerrainChunks.Remove(chunk);
		}
	}

}

[System.Serializable]
public struct LODInfo
{
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float visibleDstThreshold;
    
	public float sqrVisibleDstThreshold
    {
		get
        {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}
