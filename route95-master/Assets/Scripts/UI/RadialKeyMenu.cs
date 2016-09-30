using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to handle the radial key selection menu.
/// </summary>
public class RadialKeyMenu : MonoBehaviour {

	#region RadialKeyMenu Vars

	public static RadialKeyMenu instance; // Quick reference to this instance

	List<GameObject> objects;             // All buttons currently instantiated

	private float radius;                 // Current button placement radius
	private float scale;                  // Current button scale

	[Tooltip("Base button scale.")]
	public float baseScale;

	[Tooltip("Base button scale factor.")]
	public float scaleFactor;

	[Tooltip("Confirm button.")]
	public GameObject confirmButton;

	Color gray = new Color (0.8f, 0.8f, 0.8f, 0.8f);

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
		objects = new List<GameObject>();
	}

	void Start () {
		Refresh();
	}

	#endregion
	#region RadialKeyMenu Vars

	/// <summary>
	/// Refreshes the radial key menu.
	/// </summary>
	public void Refresh () {

		// Clear old buttons
		foreach (GameObject obj in objects) Destroy (obj);
		objects.Clear();

		// Init radius and scale
		radius = (gameObject.RectTransform().rect.width - baseScale) / 2f;
		scale = baseScale;

		// Layer one -- keys
		int numKeys = Enum.GetValues(typeof(Key)).Length;
		for (int i=1; i < numKeys; i++) { // i=1 so that it skips Key.None
			Key key = (Key)i;
			float angle = (float)i / (float)(numKeys-1) * 2f * Mathf.PI;

			// Create button
			GameObject button = UIHelpers.MakeTextButton(key.ToString());
			RectTransform tr = button.RectTransform();
			tr.SetParent (gameObject.RectTransform());
			tr.SetSideWidth (scale);
			tr.AnchorAtPoint(0.5f, 0.5f);
			tr.anchoredPosition3D = new Vector3 ( radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), 0f);
			tr.ResetScaleRot();

			// Set button text
			Text text = button.GetComponentInChildren<Text>();
			if (key.ToString().Contains("Sharp")) text.text = key.ToString()[0] + "#";
			text.font = GameManager.instance.font;
			text.fontSize = (int)(scale/2f);
			text.color = gray;

			// Set button image
			Image img = button.Image();
			img.sprite = GameManager.instance.circleIcon;
			img.color =  gray;

			// Highlight if selected key
			if (key == MusicManager.instance.currentSong.key) {
				text.color = Color.white;
				img.color = Color.white;

				GameObject hl = UIHelpers.MakeImage (key.ToString() +"_SelectedHighlight");
				tr = hl.RectTransform();
				tr.SetParent (button.RectTransform());
				tr.sizeDelta = ((RectTransform)(tr.parent)).sizeDelta;
				tr.AnchorAtPoint(0.5f, 0.5f);
				tr.anchoredPosition3D = Vector3.zero;
				tr.ResetScaleRot();

				img = hl.Image();
				img.sprite = GameManager.instance.circleIcon;
				img.color = Color.white;
			}

			// Set button functionality
			button.Button().onClick.AddListener (delegate {
				GameManager.instance.MenuClick();
				MusicManager.instance.currentSong.key = key;
				Refresh();
			});

			// Set button show/hide
			ShowHide sh = button.AddComponent<ShowHide>();
			GameObject highlight = UIHelpers.MakeImage (key.ToString() + "_Highlight");
			tr = highlight.RectTransform();
			tr.SetParent (button.RectTransform());
			tr.sizeDelta = ((RectTransform)(tr.parent)).sizeDelta;
			tr.AnchorAtPoint(0.5f, 0.5f);
			tr.ResetScaleRot();
			tr.anchoredPosition3D = Vector3.zero;
			highlight.Image().sprite = GameManager.instance.volumeIcon;
			highlight.Image().color = Color.white;

			sh.objects = new List<GameObject>();
			sh.objects.Add (highlight);

			highlight.SetActive(false);

			objects.Add (button);
		}

		// Layer two -- scales
		radius *= scaleFactor;
		scale *= scaleFactor;
		int numScales = ScaleInfo.AllScales.Count;
		for (int i=0; i < numScales; i++) {
			ScaleInfo scalei = ScaleInfo.AllScales[i];
			float angle = (float)i / (float)numScales * 2f * Mathf.PI;

			// Make scale button
			GameObject button = UIHelpers.MakeTextButton(scalei.name);
			RectTransform tr = button.RectTransform();
			tr.SetParent (gameObject.RectTransform());
			tr.SetSideWidth(scale);
			tr.AnchorAtPoint(0.5f, 0.5f);
			tr.ResetScaleRot();
			tr.anchoredPosition3D = new Vector3 (radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), 0f);

			// Set button text
			Text text = button.GetComponentInChildren<Text>();
			text.font = GameManager.instance.font;
			text.fontSize = (int)(baseScale/6f);
			text.color = gray;

			// Set button image
			Image img = button.Image();
			img.sprite = GameManager.instance.circleIcon;
			img.color = gray;

			// Set highlighted button
			if (i == MusicManager.instance.currentSong.scale) {
				text.color = Color.white;
				img.color = Color.white;
			}

			// Set button functionality
			button.Button().onClick.AddListener (delegate {
				GameManager.instance.MenuClick();
				MusicManager.instance.currentSong.scale = scalei.scaleIndex;
				Refresh();
			});

			// Set show/hide
			ShowHide sh = button.AddComponent<ShowHide>();
			GameObject highlight = UIHelpers.MakeImage (scalei.name + "_Highlight");
			tr = highlight.RectTransform();
			tr.SetParent (button.RectTransform());
			tr.sizeDelta = ((RectTransform)(tr.parent)).sizeDelta;
			tr.AnchorAtPoint(0.5f, 0.5f);
			tr.ResetScaleRot();
			tr.anchoredPosition3D = Vector3.zero;
			highlight.Image().sprite = GameManager.instance.volumeIcon;
			highlight.Image().color = Color.white;

			sh.objects = new List<GameObject>();
			sh.objects.Add (highlight);

			highlight.SetActive(false);

			objects.Add (button);
				
		}

		// Confirm button
		if (MusicManager.instance.currentSong.key != Key.None &&
			MusicManager.instance.currentSong.scale != -1) {
			confirmButton.Button().interactable = true;
			confirmButton.SetActive (true);
		} else confirmButton.SetActive (false);
	}

	#endregion
}
