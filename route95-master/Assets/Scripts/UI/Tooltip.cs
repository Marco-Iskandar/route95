using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class to handle generic tooltips.
/// </summary>
public class Tooltip : MonoBehaviour {

	#region Tooltip Vars

	public static Tooltip instance;

	Text textObj;

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
		textObj = GetComponentInChildren<Text>();
	}

	#endregion
	#region Tooltip Methods

	public void SetText (string text) {
		textObj.text = text;
	}

	#endregion
}
