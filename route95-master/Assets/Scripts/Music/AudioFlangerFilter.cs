using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Custom audio flanger filter.
/// </summary>
public class AudioFlangerFilter : MonoBehaviour {

	#region AudioFlangerFilter Vars

	[Tooltip("Rate at which to oscillate flanger.")]
	[Range(Mathf.PI/32f, Mathf.PI/16f)]
	public float rate = Mathf.PI/32f;

	[Tooltip("Ratio of dry/wet signal.")]
	[Range(0f,1f)]
	public float dryMix = 0.5f;

	[SerializeField]
	[Tooltip("Current signal delay.")]
	[Range(0.005f, 0.025f)]
	float delay = 0.005f;

	float r = 0f;           // Current oscillation theta
	float time;             // Time between frames
	List<float[]> oldDatas; // Old audio data arrays
	float[] mixed;          // Array of mixed old audio inputs
	int len;                // Length of data arrays

	#endregion
	#region Unity Callbacks

	void FixedUpdate () {

		// Add rate
		r += rate;

		// Wrap r if above 2PI
		if (r > Mathf.PI * 2f) r -= (Mathf.PI * 2f);

		// Oscillate delay
		delay = 0.015f + 0.01f * Mathf.Sin(r);

	}

	void OnEnable () {
		r = 0f;
		time = 0.02f * Application.targetFrameRate;
	}

	public void OnAudioFilterRead (float[] data, int channels) {

		// Initllist of old data if necessary
		if (oldDatas == null) oldDatas = new List<float[]>();

		// Add up to 5 old audio inputs
		while (oldDatas.Count < 5) oldDatas.Add (data);
	
		// Mix old signals
		MixSignals ();

		// Copy raw audio data
		float[] copy = new float[len];

		// Mix old and new signals
		float oneMinusMix = 1f - dryMix;
		for (int i=0; i<data.Length; i++) {
			copy[i] = data[i] * dryMix + mixed[i] * oneMinusMix * 0.95f;
			data[i] = copy[i];
		}

		// Remove oldest data
		oldDatas.RemoveAt(0);

		// Add current data
		oldDatas.Add(copy);
	}

	#endregion
	#region AudioFlangerFilter Methods

	/// <summary>
	/// Mixes current audio signal with past signals.
	/// </summary>
	void MixSignals () {

		// Get length of audio signal
		len = oldDatas[0].Length;

		// Init mixed array if necessary
		if (mixed == null) mixed = new float[len];

		// Calculate mixing value between old inputs
		float val = delay / time;

		// Get indices of old inputs to use
		int hi = Mathf.CeilToInt(val);
		int lo = Mathf.FloorToInt(val);

		// Stop if indices are invalid
		if (hi < 0 || lo < 0 || hi >= 5 || lo >= 5) return;

		// Mix old inputs
		float mix = (val - (float)lo);
		float oneMinusMix = 1f - mix;

		// Populate mixed array
		for (int i=0; i<len; i++) {
			float high = oldDatas[hi][i];
			float low = oldDatas[lo][i];

			try {
				mixed[i] = (high * mix) + (low * oneMinusMix);
			} catch (IndexOutOfRangeException e) {
				Debug.LogError("i: " + i + " len: " + len + e.Message);
			}
		}
		
	}

	#endregion
}
