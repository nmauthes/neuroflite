using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingGenerator : MonoBehaviour {
	public GameObject ringPrefab;

	public float maxHorizontalOffset;
	public float maxVerticalOffset;
	public float ringsDistance;

	public const float regenerationTime = 5;

	public int numberOfRings;
	public List<GameObject> rings;

	public const int RingPoints = 10;

	void Awake() {
		rings = new List<GameObject>();
	}

	float zPos;

	// Use this for initialization
	void Start () {
		zPos = 0;
		for (int i = 0; i < numberOfRings; i++) {
			GenerateRing ();
		}
	}


	// Update is called once per frame
	void Update () {

//		for (int i = rings.Count - 1; i >= 0; i--) {
//			if (!rings [i].activeSelf && !rings [(i + 1) % rings.Count].activeSelf) {
//				Vector3 newPos = new Vector3 (transform.position.x + Random.Range(-maxHorizontalOffset, maxHorizontalOffset), transform.position.y + Random.Range(-maxVerticalOffset, maxVerticalOffset), transform.position.z + (zPos += ringsDistance));
//				rings[i] = Instantiate (ringPrefab, newPos, transform.rotation, this.transform);
//			}
//		}
	}

	public void GenerateRing() {
		Vector3 ringPos = new Vector3 (transform.position.x + Random.Range(-maxHorizontalOffset, maxHorizontalOffset), transform.position.y + Random.Range(-maxVerticalOffset, maxVerticalOffset), transform.position.z + (zPos += ringsDistance));
		GameObject newRing = Instantiate (ringPrefab, ringPos, transform.rotation, this.transform);
		rings.Add (newRing);
	}
}
