using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour { // Attach to camera
	Camera cam;

	public float lerpTime;
	public Color[] colors;

	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera> ();
		cam.clearFlags = CameraClearFlags.SolidColor;

		colorIndex = 0;
		cam.backgroundColor = colors [colorIndex];

		startTime = Time.time;
	}

	int colorIndex;
	float startTime;
	
	// Update is called once per frame
	void Update () {
		float t = Mathf.Clamp01((Time.time - startTime) / lerpTime);

		cam.backgroundColor = Color.Lerp(colors[colorIndex % colors.Length], 
			colors[(colorIndex + 1) % colors.Length], t);

		if (t >= 1) {
			colorIndex++;
			startTime = Time.time;
		}
	}
}
