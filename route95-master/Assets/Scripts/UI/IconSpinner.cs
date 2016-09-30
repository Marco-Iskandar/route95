using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to handle an animated icon spinner effect.
/// </summary>
public class IconSpinner : MonoBehaviour {

	#region IconSpinner Vars

	[Tooltip("List of images to move.")]
	public List<RectTransform> images;

	[Tooltip("Radius of spinning area.")]
	public float spinRadius;

	[Tooltip("Spinning speed.")]
	public float spinRate;

	int imageCount; // Number of images
	float r = 0f;   // Current theta

	#endregion
	#region Unity Callbacks

	void Awake () {
		// Get number of images
		imageCount = images.Count;
	}

	void Update () {

		// Update theta
		r += spinRate * Time.deltaTime;

		// Move all images
		for (int i=0; i<imageCount; i++) {
			float angleOffset = (float)i / (float)images.Count * Mathf.PI * 2f;
			images[i].anchoredPosition3D = 
				new Vector3 (Mathf.Cos (r + angleOffset), Mathf.Sin (r + angleOffset), 0f) * spinRadius;
		}
	}

	#endregion

}
