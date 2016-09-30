using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Special use of DraggableButton for riff volume slider.
/// </summary>
public class RiffVolumeButton : DraggableButton {

	#region RiffVolumeButton Vars

	const float vDragDistance = 128f;
	const float hDragDistance = 128f;

	float oldVolume; // copy of old note volume

	Riff targetRiff; // riff to edit

	#endregion
	#region Unity Callbacks

	void Awake () {
		maxDragDistanceUp = vDragDistance;
		maxDragDistanceDown = vDragDistance;
		maxDragDistanceLeft = hDragDistance;
		maxDragDistanceRight = hDragDistance;
	}

	#endregion
	#region DraggableButton Overrides

	public override void OnMouseDown() {
		targetRiff = InstrumentSetup.currentRiff;
		oldVolume = targetRiff.volume;
	}

	public override void DragDown (float actionRatio) {
		targetRiff.volume = Mathf.Clamp01 (oldVolume - actionRatio);
		gameObject.Image().fillAmount = targetRiff.volume;
	}

	public override void DragUp (float actionRatio) {
		targetRiff.volume = Mathf.Clamp01 (oldVolume + actionRatio);
		gameObject.Image().fillAmount = targetRiff.volume;
	}

	#endregion
}
