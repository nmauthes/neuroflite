using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class GameMechanics : MonoBehaviour {
	public GameObject rotator;
	public GameObject player;

	public static Vector3 initialPos;

	public float forwardSpeed = 5;
	public float rotateSpeed = 25;

	public bool spin = true;
	public float spinSpeed = 25;

	public float rotationSmoothing;

	RingGenerator ringGenerator;

	AudioSource pulseSound;
	public Vector2 pulsePitchRange;

	public GameObject cam;
	Blur blur;

	public GameObject menu;
	CanvasGroup canvasGroup;
	MuseConnection museConnection;

	public static int score;
	public Text scoreText;

	public Text timerText;
	float timerStart;

	// Use this for initialization
	void Start () {
		museConnection = menu.GetComponent<MuseConnection> ();
		canvasGroup = menu.GetComponent<CanvasGroup> ();
		cam = GameObject.Find ("Main Camera");
		blur = cam.GetComponent<Blur>();

		ringGenerator = GameObject.Find ("Ring Generator").GetComponent<RingGenerator> ();

		initialPos = transform.position;
		pulseSound = player.GetComponent<AudioSource> ();

		score = 0;
		timerStart = 0;
		velocity = Vector3.zero;
		paused = true;
	}
		
	public float smoothTime = 0.8f;
	Vector3 velocity;
	float velocityf;

	public float timeToPause = 3;
	public static bool paused;
	bool stateChanged;
	float mouseHeldStartTime;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			mouseHeldStartTime = Time.time;
			stateChanged = false;
		} 
		else if (!stateChanged && Input.GetMouseButton (0) && Time.time - mouseHeldStartTime >= timeToPause) {
			paused = !paused;
			stateChanged = true;

			if (!paused) {
				score = 0;
				timerStart = Time.time;
			}
		}

		SetScoreText (score);
		SetTimerText (Time.time - timerStart);

		// Hide the menu and pause the game if mouse is held for pauseTime
		if (!paused) {
			MovePlayer ();
			MakeMenuVisible (false);
			blur.enabled = false;

			float newPitch = Mathf.Lerp (pulsePitchRange.x, pulsePitchRange.y, museConnection.mellowScore);
			pulseSound.pitch = Mathf.SmoothDamp (pulseSound.pitch, newPitch, ref velocityf, smoothTime);

			if (!pulseSound.isPlaying) {
				pulseSound.Play ();
			}
		} 
		else {
			MakeMenuVisible (true);
			blur.enabled = true;

			pulseSound.Stop ();
		}
	}
		
	int ringIndex = 0;

	public void MovePlayer() {
		Vector3 targetRingPos = ringGenerator.rings [ringIndex].transform.position;

		// Player will move towards each ring in sequence
		if (transform.position != targetRingPos) {
			transform.position = Vector3.MoveTowards (transform.position, targetRingPos, forwardSpeed * Time.deltaTime);
		}
		else {
			Destroy(ringGenerator.rings [ringIndex], RingGenerator.regenerationTime);
			ringGenerator.GenerateRing ();
			ringIndex = (ringIndex + 1) % ringGenerator.rings.Count;
		}

		// Rotate player while maintaining orientation
		rotator.transform.RotateAround (transform.position, transform.forward, rotateSpeed * Time.deltaTime);
		player.transform.Rotate (-rotateSpeed * transform.forward * Time.deltaTime, Space.World);

		// Move the player laterally according to Muse score
		Vector3 newPos = Vector3.Lerp(transform.position, rotator.transform.position, 1 - museConnection.mellowScore);
		player.transform.position = Vector3.SmoothDamp (player.transform.position, newPos, ref velocity, smoothTime);

		// Spin the player mesh on the y axis
		if (spin) {
			player.transform.Rotate (spinSpeed * Vector3.up * Time.deltaTime, Space.World);
		}
	}

	void SetScoreText(float score) {
		scoreText.text = "Score " + score;
	}

	void SetTimerText(float time) {
		int minutes = (int) time / 60;
		int seconds = (int) time % 60;

		timerText.text = "Time " + minutes.ToString("00") + ":" + seconds.ToString("00");
	}

	public void MakeMenuVisible(bool visible) {
		if (visible) {
			canvasGroup.interactable = true;
			canvasGroup.alpha = 1;
		}
		else {
			canvasGroup.interactable = false;
			canvasGroup.alpha = 0;
		}
	}
}
