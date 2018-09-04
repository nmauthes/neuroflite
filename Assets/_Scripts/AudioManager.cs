using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
	public float maxVolume;
	public float fadeSpeed;

	public AnimationCurve fadeCurve;

	public bool fade;

	public AudioSource audioSource;

	// Use this for initialization
	void Start () {
		audioSource.volume = 0f;
	}

	// Update is called once per frame
	void Update () {
		if (!GameMechanics.paused) {
			if (!audioSource.isPlaying)
				audioSource.Play ();

			if (fade)
				FadeIn (audioSource);
		}
		else {
			if (fade && audioSource.volume > 0)
				FadeOut (audioSource);
			else
				audioSource.Stop ();
		}
	}

	public void FadeIn(AudioSource source) {
		if (source.volume < maxVolume) {
			source.volume += fadeCurve.Evaluate (Time.deltaTime * fadeSpeed);
		}
	}

	public void FadeOut(AudioSource source) {
		if (source.volume > 0) {
			source.volume -= fadeCurve.Evaluate (Time.deltaTime * fadeSpeed);
		}
	}
}
