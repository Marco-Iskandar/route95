using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class to fade all maskable graphics on and in a GameObject
/// </summary>
public class Fadeable : MonoBehaviour {

	#region Fadeable Vars

	[Tooltip("Does the graphic start faded?")]
	public bool startFaded = true;

	[Tooltip("Rate of fade/unfade in percent per cycle.")]
	public float fadeSpeed = 0.05f;         // Speed at which to perform fade

	[Tooltip("Fade all children graphics?")]
	public bool fadeAllChildren = false;

	[Tooltip("Prevent being faded by parents?")]
	public bool blockParents = false;

	[Tooltip("Disable GameObject after fading?")]
	public bool disableAfterFading = false;

	float alpha;                                       // Current fade alpha
	public bool busy = false;
	Dictionary<MaskableGraphic, Color> originalColors; // Stores references to each graphic and its original color

	#endregion
	#region Unity Callbacks

	public void Awake () {

		// Build dicts of original colors
		originalColors = new Dictionary<MaskableGraphic, Color>();
		if (fadeAllChildren) {
			List<MaskableGraphic> allGraphics = GetComponentsInChildren<MaskableGraphic>().ToList<MaskableGraphic>();
			foreach (MaskableGraphic graphic in allGraphics)
				if (graphic.GetComponent<Fadeable>() == null ||
					!graphic.GetComponent<Fadeable>().blockParents) originalColors.Add (graphic, graphic.color);
		} else if (GetComponent<MaskableGraphic>() != null)
			originalColors.Add (GetComponent<MaskableGraphic>(), GetComponent<MaskableGraphic>().color);

		// Initially fade if necessary
		if (startFaded) {
			alpha = 0f;
			ColorAll();
			if (disableAfterFading) gameObject.SetActive(false);
		}
	}

	#endregion
	#region Fadeable Methods

	/// <summary>
	/// Returns true if the object is done fading.
	/// </summary>
	public bool DoneFading {
		get {
			return alpha == 0f && !busy;
		}
	}

	/// <summary>
	/// Returns true if the object is done unfading.
	/// </summary>
	public bool DoneUnfading {
		get {
			return alpha == 1f && !busy;
		}
	}

	public bool NotFading {
		get {
			return DoneFading || DoneUnfading;
		}
	}

	/// <summary>
	/// Starts fading the object.
	/// </summary>
	public void Fade () {
		if (gameObject.activeSelf) StopCoroutine ("DoUnFade");
		else return;
		busy = true;
		StartCoroutine("DoFade");
	}

	/// <summary>
	/// Starts unfading the object.
	/// </summary>
	public void UnFade () {
		if (gameObject.activeSelf) StopCoroutine("DoFade");
		else gameObject.SetActive(true);
		busy = true;
		StartCoroutine("DoUnFade");
	}

	/// <summary>
	/// Fades an object.
	/// </summary>
	/// <returns></returns>
	IEnumerator DoFade () {
		while (true) {
			if (alpha >= fadeSpeed) {

				alpha -= fadeSpeed;
				ColorAll();
					
				yield return null;
			} else {
				busy = false;
				if (disableAfterFading) gameObject.SetActive(false);
				alpha = 0f;
				ColorAll();
				yield break;
			}
		}
	}

	/// <summary>
	/// Unfades an object.
	/// </summary>
	/// <returns></returns>
	IEnumerator DoUnFade () {
		while (alpha <= 1f-fadeSpeed) {

			alpha += fadeSpeed;
			ColorAll();

			yield return null;
		}
		busy = false;
		alpha = 1f;
		ColorAll();
		yield break;
	}

	/// <summary>
	/// Updates the alphas of parent and all children.
	/// </summary>
	void ColorAll () {
		foreach (MaskableGraphic graphic in originalColors.Keys) {
			Color newColor = originalColors[graphic];
			newColor.a *= alpha;
			graphic.color = newColor;
		}
	}

	#endregion
}
