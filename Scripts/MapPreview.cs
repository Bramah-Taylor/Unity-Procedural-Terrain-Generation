﻿using UnityEngine;
using System.Collections;

// Script class to handle the in-editor previews, either a mesh or a texture depending on user input
public class MapPreview : MonoBehaviour
{
	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
    
	public enum DrawMode {NoiseMap, Mesh};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureData;

	public Material terrainMaterial;
    
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;
    
    // Main function. Generates the height map and applies it to the desired preview object.
	public void DrawMapInEditor()
    {
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

		if (drawMode == DrawMode.NoiseMap)
        {
			DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
		}
        else if (drawMode == DrawMode.Mesh)
        {
			DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values,meshSettings));
		}
	}
    
	public void DrawTexture(Texture2D texture)
    {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10.0f; // Shrink the texture down so that we get a reasonably sized preview

		textureRender.gameObject.SetActive(true);
		meshFilter.gameObject.SetActive(false);
	}
    
	public void DrawMesh(MeshData meshData)
    {
		meshFilter.sharedMesh = meshData.CreateMesh();

		textureRender.gameObject.SetActive(false);
		meshFilter.gameObject.SetActive(true);
	}
    
	void OnValuesUpdated()
    {
		if (!Application.isPlaying)
        {
			DrawMapInEditor();
		}
	}

	void OnTextureValuesUpdated()
    {
		textureData.ApplyToMaterial(terrainMaterial);
	}

	void OnValidate()
    {
        // Maintain subscriptions to settings objects delegates so that we're not making repeated calls to functions in this class
		if (meshSettings != null)
        {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (heightMapSettings != null)
        {
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null)
        {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
	}
}
