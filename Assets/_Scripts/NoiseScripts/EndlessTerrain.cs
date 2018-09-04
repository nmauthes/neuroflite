using System.Collections;
using System.Collections.Generic; // Import this for C# dictionary
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
	const float scale = 1f; // Allows you to scale the map uniformly

	const float viewerMoveThresholdForChunkUpdate = 25f; // Chunks are updated once viewer moves a certain distance rather than every frame
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate; // Getting the square distance is quicker than actual distance (no sqrt op)

	public LODInfo[] detailLevels;
	// The distance the viewer can see
	public static float maxViewDist; // Static variable can be changed at runtime

	public Transform viewer;
	public Material mapMaterial;

	public static Vector2 viewerPosition;
	public static Vector2 viewerPositionOld;
	static MapGenerator mapGenerator; // Reference to the MapGenerator class
	int chunkSize;
	int chunksVisibleInViewDist; // The number of map chunks currently visible

	// Ensures that no duplicates are created at a coord if chunk already created there
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	// Ensures that chunks beyond the offset are removed
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() {
		mapGenerator = FindObjectOfType<MapGenerator> ();

		// Max view dist is last element of LOD array
		maxViewDist = detailLevels [detailLevels.Length - 1].visibleDistThreshold;
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);

		UpdateVisibleChunks ();
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale; // Update viewer pos

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	void UpdateVisibleChunks() {
		// Sets invisible all chunks that have moved outside the visible distance
		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();

		// Gets the coordinate of the chunk (e.g. (0, 0), (1, 0), (-1, -1)
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		// Cycle through each visible chunk
		for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset,
					currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					// Update terrain chunk
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
				}
				else { // Make a new terrain chunk
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}
			}
		}
	}

	// Class representing a terrain chunk object
	public class TerrainChunk {
		GameObject meshObject;
		Vector2 position;
		Bounds bounds; // Represents a bounding box that completely surrounds the object

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes; // Meshes at various LOD's

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1; // Keep track of previous LOD so no update is done if LOD level remains the same

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y); // Position in 3D space
		
			// Generate the terrain chunk mesh
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;

			meshObject.transform.position = positionV3 * scale;
			meshObject.transform.parent = parent; // New chunks are children of this transform
			meshObject.transform.localScale = Vector3.one * scale;
			SetVisible(false); // Default state of the chunk is not visible

			lodMeshes = new LODMesh[detailLevels.Length];
			for(int i = 0; i < lodMeshes.Length; i++) {
				// Create a new LOD mesh
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}

		// First the map data is fetched, then used to generate the meshes at various LOD
		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataReceived = true;

			Texture2D texture = TextureGenerator.TextureFromColorMap (mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk ();
		}
			
		// Enables mesh object if dist between viewer and chunk is less than max view dist
		public void UpdateTerrainChunk() {
			if(mapDataReceived) { // Don't update if map data hasn't been received
				float viewDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewDistFromNearestEdge <= maxViewDist;
				SetVisible (visible);

				if (visible) {
					int lodIndex = 0;

					// Look at the terrain chunk and choose LOD based on dist from viewer
					for (int i = 0; i < detailLevels.Length - 1; i++) { // Don't have to look at last index because visible is false
						if (viewDistFromNearestEdge > detailLevels [i].visibleDistThreshold) {
							lodIndex = i + 1;
						}
						else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes [lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh (mapData);
						}
					}

					terrainChunksVisibleLastUpdate.Add (this); // Adds self to list
				}
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() { // Determines whether a chunk is visible
			return meshObject.activeSelf;
		}
	}

	// Each terrain chunk will have an array of LODMeshes, class fetches appropriate mesh
	class LODMesh {
		public Mesh mesh;
		public bool hasRequestedMesh; // Has the mesh been requested from MapGenerator
		public bool hasMesh; // Has the mesh been received
		int lod;
		System.Action updateCallback; // Chunks must be updated manually

		public LODMesh(int lod, System.Action updateCallback) {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh ();
			hasMesh = true;

			updateCallback();
		}

		// Request specific LOD mesh when required
		public void RequestMesh(MapData mapData) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData (mapData, lod, OnMeshDataReceived);
		}
	}

	[System.Serializable] // Shows up in inspector
	public struct LODInfo {
		public int lod;
		public float visibleDistThreshold;
	}
}
