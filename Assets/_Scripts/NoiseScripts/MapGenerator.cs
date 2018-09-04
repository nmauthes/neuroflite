using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {
	public enum DrawMode {NoiseMap, ColorMap, Mesh} // Draw map in color or grayscale
	public DrawMode drawMode;

	public Noise.NormalizeMode normalizeMode;

	// Unity has a limit of 65025 vertices per mesh for a square mesh
	// Default width is 241 because w - 1 = 240 (makes LOD drawing easier)
	public const int mapChunkSize = 97; // The width of each map chunk

	public bool useFlatShading;

	[Range(0, 6)]
	public int editorPreviewLOD; // Skip vertices in increments from 1-12
	public float noiseScale;

	public int octaves;
	[Range(0, 1)] // Changes variable to a slider in the editor
	public float persistence; // Controls decrease in amplitude of octaves (larger = small rocks are less prominent)
	public float lacunarity; // Controls increase in frequency of octaves (larger = smaller rocks)

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve; // How much each height val is affected by noise

	public bool autoUpdate;

	public TerrainType[] regions;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void DrawMapInEditor() {
		MapData mapData = GenerateMapData (Vector2.zero);

		// Reference to the MapDisplay object
		MapDisplay display = FindObjectOfType<MapDisplay> ();

		if (drawMode == DrawMode.NoiseMap) {
			// Draw the noise map on the MapDisplay object
			display.DrawTexture (TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		}
		else if (drawMode == DrawMode.ColorMap) {
			// Draw the color map on the MapDisplay object
			display.DrawTexture (TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
		}
		else if (drawMode == DrawMode.Mesh) {
			// Draw the mesh
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD, useFlatShading), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
		}
	}

	// MapData is calculated over several frames to avoid freezing game
	public void RequestMapData(Vector2 center, Action<MapData> callback) { // Expects parameter of type MapData
		ThreadStart threadStart = delegate { // delegate is used to pass methods as arguments to other methods
			MapDataThread(center, callback);
		};

		new Thread (threadStart).Start ();
	}

	// Calculates the map data in a new thread
	void MapDataThread(Vector2 center, Action<MapData> callback) {
		MapData mapData = GenerateMapData (center);
		// Avoids queue being accessed from multiple places
		lock(mapDataThreadInfoQueue) { // While a thread is executing this code no other thread can do so
			mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData>(callback, mapData));
		}
	}

	// Handles threading for the terrain meshes
	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
		ThreadStart threadStart = delegate { // delegate is used to pass methods as arguments to other methods
			MeshDataThread(mapData, lod, callback);
		};

		new Thread (threadStart).Start ();
	}

	void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
		MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier,
			meshHeightCurve, lod, useFlatShading);
		lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue (new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	void Update() {
		if (mapDataThreadInfoQueue.Count > 0) { // If there is something in the queue
			for(int i = 0; i < mapDataThreadInfoQueue.Count; i++) { // Loop through queue
				// Dequeue next object and execute callback function
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback (threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0) { // If there is something in the queue
			for(int i = 0; i < meshDataThreadInfoQueue.Count; i++) { // Loop through queue
				// Dequeue next object and execute callback function
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback (threadInfo.parameter);
			}
		}
	}

	MapData GenerateMapData(Vector2 center) { // Center ensures cells are unique
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale,
			octaves, persistence, lacunarity, center + offset, normalizeMode);

		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) { // Assign each pixel a color value
			for (int x = 0; x < mapChunkSize; x++) {
				float currentHeight = noiseMap [x, y];

				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight >= regions [i].height) {
						colorMap [y * mapChunkSize + x] = regions [i].color;
					}
					else {
						break; // Only break once you reach a value less than regions height
					}
				}
			}
		}

		return new MapData (noiseMap, colorMap);
	}

	// Called automatically whenever a variable is changed in the editor
	void OnValidate() { // Constrains variable values
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

	// Holds the map data/mesh data and callback variables
	struct MapThreadInfo<T> {
		// readonly: Assignments can only occur in the constructor
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}

[System.Serializable] // Will show up in the inspector
public struct TerrainType { // Allows you to assign colors to specific height values
	public string name;
	public float height;
	public Color color;
}

// Holds the height map and color map generated in GenerateMap
public struct MapData {
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;

	public MapData (float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}

}
