using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class to blink a sprite.
/// </summary>
public class Blinking : MonoBehaviour {

	#region Blinking Vars

	[Tooltip("The color of the image at the peak of its blink cycle.")]
	public Color peakColor;

	[Tooltip("The rate at which the image blinks.")]
	[Range(0.01f, 0.5f)]
	public float blinkInterval;

	[Tooltip("The depth of the alpha dip.")]
	[Range(0f, 1f)]
	public float blinkDepth = 0.5f;

	[SerializeField]
	[Range(0f, 2f*Mathf.PI)]
	float progress = 0f;

	#endregion
	#region Unity Callbacks

	void Update () {
		if (progress >= 2f * Mathf.PI) progress = 0;
		Color color = peakColor;
		color.a = 1f - blinkDepth + blinkDepth * Mathf.Sin(progress);
		GetComponent<Image>().color = color;
		progress += blinkInterval;
	}

	#endregion
}
