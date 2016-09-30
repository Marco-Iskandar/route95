using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to handle instrument display on the back of the car.
/// </summary>
public class InstrumentDisplay : MonoBehaviour {

	#region InstrumentDisplay Vars

	public static InstrumentDisplay instance;

	public Image glow;                        // Sprite to change for glow
	public float fadeSpeed;                   // Speed of fade
	public List<Fadeable> glows;                 // Instrument icons glows

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
	}

	void FixedUpdate () {

		if (GameManager.instance.currentState != GameManager.State.Live) return;
		if (GameManager.instance.paused) return;

		Color color = glow.color;
		color.a -= fadeSpeed;
		glow.color = color;

	}

	#endregion
	#region InstrumentDisplay Methods

	/// <summary>
	/// Refreshes the display, changing art if necessary.
	/// </summary>
	public void Refresh () {
		gameObject.Image().sprite = MusicManager.instance.currentInstrument.icon;
		glow.sprite = MusicManager.instance.currentInstrument.glow;
	}

	/// <summary>
	/// Sets glow to full.
	/// </summary>
	public void WakeGlow () {
		Color color = glow.color;
		color.a = 1f;
		glow.color = color;
	}

	public void WakeGlow (int index) {
		glows[index].UnFade();
	}

	public void FadeGlow (int index) {
		glows[index].Fade();
	}

	#endregion
}
