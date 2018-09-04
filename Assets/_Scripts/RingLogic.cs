using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingLogic : MonoBehaviour {
	public bool spin;
	public float spinSpeed;

	public Material hitMaterial;
	public Material flashMaterial;

	bool hit;

	GameObject ringGenerator;

	AudioSource hitSound;
	AudioSource pingSound;

	public float pingInterval;

	// Use this for initialization
	void Start () {
		ringGenerator = GameObject.Find ("Ring Generator");

		hitSound = ringGenerator.GetComponent<AudioSource> ();
		pingSound = GetComponent<AudioSource>();

		hit = false;

		float pingTimeStart = Time.time;
	}

	float pingTimeStart;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (spinSpeed * Vector3.forward * Time.deltaTime, Space.World);

		if (!GameMechanics.paused && !hit && Time.time - pingTimeStart >= pingInterval) {
			if (!pingSound.isPlaying) {
				pingSound.Play ();
				pingTimeStart = Time.time;
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			foreach (Transform child in transform) {
				Renderer rend = child.GetComponent<Renderer> ();
				rend.material = hitMaterial;
			}

			hit = true;
			hitSound.Play ();
			GameMechanics.score += RingGenerator.RingPoints;
		}
	}
}
