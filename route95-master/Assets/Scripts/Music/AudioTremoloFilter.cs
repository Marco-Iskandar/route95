using UnityEngine;
using System.Collections;

/// <summary>
/// Custom audio tremolo filter.
/// </summary>
public class AudioTremoloFilter : MonoBehaviour {

	#region AudioTremoloFilter Vars

	[Tooltip("Tremolo oscillation rate.")]
	[Range(Mathf.PI/32f, Mathf.PI/16f)]
	public float rate = Mathf.PI/32f;

	[Tooltip("Tremolo oscillation depth.")]
	[Range(0f, 1f)]
	public float depth;

	[Tooltip("Current oscillation theta.")]
	[SerializeField]
	[Range(0f, Mathf.PI*2f)]
	float r;

	#endregion
	#region Unity Callbacks

	void FixedUpdate () {

		// Add rate to r
		r += rate;

		// Wrap r if past 2PI
		if (r > Mathf.PI * 2f) r -= (Mathf.PI * 2f);
	}

	void OnEnable() {

		// Start r at 0
		r = 0f;
	}

	void OnDisable () {

		// Reset volume while disabled
		GetComponent<AudioSource>().volume = 1f;
	}

	void OnAudioFilterRead (float[] data, int channels) {

		// Multiply data amplitude by current cos value
		for (int i=0; i<data.Length; i++)
			data[i] = data[i] *( 1f - (1f-depth)/2f + 0.5f*Mathf.Cos(r));
	}

	#endregion
}
