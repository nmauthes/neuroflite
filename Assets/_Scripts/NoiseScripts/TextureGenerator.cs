using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator { // Class for generating textures for noise map
	// Creates a texture from a 1D color map
	public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point; // Fixes blurriness
		texture.wrapMode = TextureWrapMode.Clamp; // Prevents wrapping of texture
		texture.SetPixels (colorMap);
		texture.Apply ();

		return texture;
	}

	// Get a texture based on a 2D height map
	public static Texture2D TextureFromHeightMap(float[,] heightMap) {
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);

		// Generate an array of pixel colors then apply them to the map
		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				// Give each pixel a color value between black and white
				colorMap [y * width + x] = Color.Lerp (Color.black, Color.white,
					heightMap[x, y]);
			}
		}

		return TextureFromColorMap (colorMap, width, height);
	}
}
