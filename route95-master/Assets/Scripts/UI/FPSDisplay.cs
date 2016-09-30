// Script by Dave Hampson

using UnityEngine;
using System.Collections;

/// <summary>
/// Class to display current performance data on screen.
/// </summary>
public class FPSDisplay : MonoBehaviour {

	#region FPSDisplay Vars

	float deltaTime = 0.0f;

	#endregion
	#region Unity Callbacks

	void Awake () {
		if (!Debug.isDebugBuild) gameObject.SetActive(false);
	}

	void Update () {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void OnGUI() {
		if (CameraControl.instance.state != CameraControl.State.Free) {
			int w = Screen.width, h = Screen.height;

			GUIStyle style = new GUIStyle();

			Rect rect = new Rect (0, 0, w, h * 2 /100);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 /100;
			style.normal.textColor = new Color (0f, 0f, 1f, 1f);
			float fps = 1f / deltaTime;
			string text = fps.ToString ("0000.") + "fps";
			GUI.Label(rect, text, style);
		}
	}

	#endregion
}
