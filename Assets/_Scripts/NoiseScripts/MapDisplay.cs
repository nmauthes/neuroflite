using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
	public Renderer textureRender; // Reference to renderer to noise texture

	public MeshFilter meshFilter; // For rendering the mesh
	public MeshRenderer meshRender;

	public void DrawTexture(Texture2D texture) { // Draws a texture to the screen
		// Use shared material so texture can be applied before runtime
		textureRender.sharedMaterial.mainTexture = texture;
		// Set size of plane to same size as map
		textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
	}

	public void DrawMesh (MeshData meshData, Texture2D texture) {
		meshFilter.sharedMesh = meshData.CreateMesh ();
		meshRender.sharedMaterial.mainTexture = texture;
	}
}
