using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise { // Static class for generating noise

	public enum NormalizeMode {Local, Global}

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale,
		int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {

		float maxPossibleHeight = 0; // The maximum possible height of the terrain
		float amplitude = 1;
		float frequency = 1;

		// Generates random noise seed
		System.Random prng = new System.Random (seed); // prng = Pseudo-random number generator
		// Each octave is sampled from a different location
		Vector2[] octaveOffsets = new Vector2[octaves];
		for (int i = 0; i < octaves; i++) {
			// Offset allows you to scroll through noise
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y; // Subtract y to simulate forward motion
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistence;
		}

		float[,] noiseMap = new float[mapWidth, mapHeight];

		if (scale <= 0) {
			scale = 0.0001f;
		}

		// Keep track of min and max noise height for normalization later
		float minLocalNoiseHeight = float.MaxValue;
		float maxNoiseHeight = float.MinValue;

		// Allows you to scale the noise towards the center
		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) {
					// Point from which height values are sampled
					// Higher frequency means height will change more rapidly
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

					// Determine the height for each point and assign to noise map
					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					// Increase noise height by perlin value of each octave
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistence;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight) {
					maxNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}

				noiseMap [x, y] = noiseHeight; // Apply noise height to map at x, y
			}
		}

		// Normalize range of the noise map to between 0, 1
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (normalizeMode == NormalizeMode.Local) {
					// Preferred method if not doing endless terrain
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
				}
				else {
					float normalizedHeight = (noiseMap [x, y] + 1) / maxPossibleHeight;
					noiseMap [x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
				}
			}
		}

		return noiseMap;
	}
}
