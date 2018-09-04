using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof (MapGenerator))] // Required for custom editor scripts
public class MapGenEditor : Editor {
	// Make a custom editor button to generate the noise map
	public override void OnInspectorGUI () {
		// target is object that editor is inspecting
		MapGenerator mapGen = (MapGenerator) target;

		if (DrawDefaultInspector ()) { // If a value is changed update the map
			if (mapGen.autoUpdate) {
				mapGen.DrawMapInEditor ();
			}
		}

		// Draw a custom button to the editor to generate maps
		if(GUILayout.Button("Generate Map")) {
			mapGen.DrawMapInEditor();
		}
	}
}
#endif
