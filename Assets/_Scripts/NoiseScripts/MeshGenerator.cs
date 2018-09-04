using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Steps to make a mesh:

// Store all the vertices in an array
// Connect the vertices to form triangles
// Triangles are formed in clockwise order (e.g. 0, 4, 3, 4, 0, 1...)

// Number of vertices = width * height
// Number of triangles = Number of squares * 2 * 3 = (w - 1)(h - 1) * 6

public static class MeshGenerator {
	// Creates a mesh from a 2D height map
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve,
		int levelOfDetail, bool useFlatShading) {
		// Each thread will have its own height curve object
		AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys); // Create a new animation curve at the start to avoid errors

		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f; // Scales the lefmost point so that the mesh is centered on the screen
		float topLeftZ = (height - 1) / 2f;

		// The number of vertices to skip over (1 draws all vertices)
		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1: levelOfDetail * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData (verticesPerLine, verticesPerLine, useFlatShading);
		int vertexIndex = 0; // Current index of the vertices array

		for (int y = 0; y < height; y += meshSimplificationIncrement) { // Loop through the height map
			for (int x = 0; x < width; x += meshSimplificationIncrement) {
				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate(heightMap [x, y]) * heightMultiplier, topLeftZ - y);
				// Each vertex's position in the map is represented as a percentage
				meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

				// Ignore indexes at the right and bottom of the mesh
				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine); // First triangle
					meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1); // Second triangle
				}

				vertexIndex++;
			}
		}

		meshData.ProcessMesh ();

		return meshData;
	}
}

public class MeshData { // Convenience class for storing the mesh data
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs; // UV mapping means projecting a 2D image to the surface of a 3D obj

	int triangleIndex; // Current index of the triangles array

	bool useFlatShading;

	public MeshData(int meshWidth, int meshHeight, bool useFlatShading) {
		this.useFlatShading = useFlatShading;

		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight]; // Need one UV per vertex
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

	// Convenience method for adding triangles to the triangles array
	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public void ProcessMesh() {
		if (useFlatShading) {
			FlatShading ();
		}
		else {
			// Use normal shading
		}
	}

	void FlatShading() {
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++) {
			flatShadedVertices [i] = vertices [triangles [i]];
			flatShadedUvs [i] = uvs [triangles [i]];
			triangles [i] = i; // Update triangles array to refer to flat shaded index
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUvs;
	}

	// Method for getting a mesh from the mesh data
	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals (); // Ensures lighting works correctly

		return mesh;
	}
}
