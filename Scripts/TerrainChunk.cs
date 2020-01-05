using UnityEngine;

public class TerrainChunk
{
	const float colliderGenerationDistanceThreshold = 100;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	 
	GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

    TerrainMesh terrainMesh;

	HeightMap heightMap;
	bool heightMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider;
	float maxViewDst;

	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	Transform viewer;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, float visibleDstThreshold, Transform parent, Transform viewer, Material material)
    {
		this.coord = coord;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.viewer = viewer;
        maxViewDst = visibleDstThreshold;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize);

		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

        terrainMesh = new TerrainMesh();
        terrainMesh.updateCallback += UpdateTerrainChunk;
		terrainMesh.updateCallback += UpdateCollisionMesh;
	}

	public void Load()
    {
        // Pass in lambda as function object to preserve function parameters
		ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
	}

    // Update the terrain chunk once the heightmap has been generated
	void OnHeightMapReceived(object heightMapObject)
    {
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;

		UpdateTerrainChunk();
	}

	Vector2 viewerPosition
    {
		get
        {
			return new Vector2(viewer.position.x, viewer.position.z);
		}
	}

	public void UpdateTerrainChunk()
    {
		if (heightMapReceived)
        {
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

			bool wasVisible = IsVisible();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			if (visible)
            {
                // LOD stuff
				int lodIndex = 0;

				for (int i = 0; i < 1; i++)
                {
					if (viewerDstFromNearestEdge > maxViewDst)
                    {
						lodIndex = i + 1;
					}
                    else
                    {
						break;
					}
				}

				if (lodIndex != previousLODIndex)
                {
                    TerrainMesh lodMesh = terrainMesh;
					if (lodMesh.hasMesh)
                    {
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					}
                    else if (!lodMesh.hasRequestedMesh)
                    {
						lodMesh.RequestMesh(heightMap, meshSettings);
					}
				}
			}

			if (wasVisible != visible)
            {
				SetVisible(visible);
				if (onVisibilityChanged != null)
                {
					onVisibilityChanged(this, visible);
				}
			}
		}
	}

	public void UpdateCollisionMesh()
    {
		if (!hasSetCollider)
        {
			float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

			if (sqrDstFromViewerToEdge < maxViewDst)
            {
				if (!terrainMesh.hasRequestedMesh)
                {
                    terrainMesh.RequestMesh(heightMap, meshSettings);
				}
			}

			if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
				if (terrainMesh.hasMesh)
                {
					meshCollider.sharedMesh = terrainMesh.mesh;
					hasSetCollider = true;
				}
			}
		}
	}

	public void SetVisible(bool visible)
    {
		meshObject.SetActive(visible);
	}

	public bool IsVisible()
    {
		return meshObject.activeSelf;
	}
}

class TerrainMesh
{
	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	public event System.Action updateCallback;

	public TerrainMesh()
    {
	}

	void OnMeshDataReceived(object meshDataObject)
    {
		mesh = ((MeshData)meshDataObject).CreateMesh();
		hasMesh = true;

		updateCallback();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
		hasRequestedMesh = true;
        // Pass in lambda as function object to preserve function parameters
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings), OnMeshDataReceived);
	}

}